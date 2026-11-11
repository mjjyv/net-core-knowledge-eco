using System.Buffers;
using System.Security.Cryptography;
using Ecosystem.Application.Common;
using Ecosystem.Application.DTOs;
using Ecosystem.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Ecosystem.Infrastructure.Files;

/// <summary>
/// Dịch vụ lưu trữ tệp cục bộ áp dụng IOptionsSnapshot&lt;FileStorageOptions&gt;,
/// kiểm tra 4 tầng (Null/Size, Extension, MIME Type, Magic Bytes) và chống Path Traversal (CWE-22).
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private const int HeaderInspectLength = 512;
    private const int CopyBufferSize = 81_920;

    private static readonly Dictionary<string, string> SafeMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".png"] = "image/png",
        [".webp"] = "image/webp",
        [".gif"] = "image/gif",
        [".pdf"] = "application/pdf",
        [".md"] = "text/markdown; charset=utf-8"
    };

    private static readonly Dictionary<string, byte[]> BinaryMagicSignatures = new(StringComparer.OrdinalIgnoreCase)
    {
        [".png"] = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A },
        [".jpg"] = new byte[] { 0xFF, 0xD8, 0xFF },
        [".jpeg"] = new byte[] { 0xFF, 0xD8, 0xFF },
        [".gif"] = new byte[] { 0x47, 0x49, 0x46, 0x38 }, // GIF8
        [".pdf"] = new byte[] { 0x25, 0x50, 0x44, 0x46 }  // %PDF
    };

    private static readonly byte[][] DisallowedMarkdownHeaders =
    {
        new byte[] { 0x4D, 0x5A },             // MZ (Windows PE/EXE/DLL)
        new byte[] { 0x7F, 0x45, 0x4C, 0x46 }, // ELF (Linux Binary)
        new byte[] { 0x25, 0x50, 0x44, 0x46 }, // %PDF
        new byte[] { 0x89, 0x50, 0x4E, 0x47 }  // PNG
    };

    private readonly FileStorageOptions _options;
    private readonly IHostEnvironment _hostEnvironment;

    public LocalFileStorageService(
        IOptionsSnapshot<FileStorageOptions> optionsSnapshot,
        IHostEnvironment hostEnvironment)
    {
        _options = optionsSnapshot.Value;
        _hostEnvironment = hostEnvironment;
    }

    public FileLimitsDto GetLimits()
    {
        return new FileLimitsDto
        {
            MaxFileSizeBytes = _options.MaxFileSizeBytes,
            MaxFileSizeMegabytes = Math.Round(_options.MaxFileSizeBytes / (1024.0 * 1024.0), 2),
            MaxRequestTotalBytes = _options.MaxRequestTotalBytes,
            MaxFilesPerRequest = _options.MaxFilesPerRequest,
            AllowedExtensions = _options.AllowedExtensions,
            AllowedContentTypes = _options.AllowedContentTypes,
            VerifyMagicBytes = _options.VerifyMagicBytes
        };
    }

    public async Task<FileUploadResultDto> UploadAsync(
        FileUploadInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.Length <= 0)
        {
            throw new ArgumentException("Tệp tải lên rỗng hoặc chưa được chọn.");
        }

        if (input.Length > _options.MaxFileSizeBytes)
        {
            var maxMb = _options.MaxFileSizeBytes / (1024.0 * 1024.0);
            throw new ArgumentException(
                $"Dung lượng tệp '{input.OriginalFileName}' vượt quá giới hạn tối đa {maxMb:0.##}MB ({_options.MaxFileSizeBytes} bytes).");
        }

        var extension = ValidateExtensionAndContentType(input.OriginalFileName, input.ContentType);

        var headerBuffer = new byte[HeaderInspectLength];
        var headerBytesRead = await ReadPrefixAsync(input.Content, headerBuffer, cancellationToken);
        if (headerBytesRead == 0)
        {
            throw new ArgumentException("Luồng dữ liệu của tệp tải lên rỗng.");
        }

        if (_options.VerifyMagicBytes)
        {
            ValidateSignature(extension, headerBuffer.AsSpan(0, headerBytesRead), input.OriginalFileName);
        }

        var uploadRoot = ResolveUploadRootDirectory();
        Directory.CreateDirectory(uploadRoot);

        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var destinationPath = Path.Combine(uploadRoot, storedFileName);

        var (actualSize, sha256Hex) = await WriteFileAndComputeSha256Async(
            destinationPath,
            headerBuffer,
            headerBytesRead,
            input.Content,
            cancellationToken);

        return new FileUploadResultDto
        {
            StoredFileName = storedFileName,
            OriginalFileName = Path.GetFileName(input.OriginalFileName),
            ContentType = SafeMimeTypes.GetValueOrDefault(extension, input.ContentType),
            SizeBytes = actualSize,
            DownloadUrl = $"/api/v1/files/{storedFileName}",
            StaticUrl = BuildStaticUrl(storedFileName),
            Sha256Checksum = sha256Hex,
            UploadedAtUtc = DateTime.UtcNow
        };
    }

    public async Task<MultiFileUploadResultDto> UploadMultipleAsync(
        IReadOnlyList<FileUploadInput> inputs,
        CancellationToken cancellationToken = default)
    {
        if (inputs == null || inputs.Count == 0)
        {
            throw new ArgumentException("Vui lòng chọn ít nhất 1 tệp để tải lên.");
        }

        if (inputs.Count > _options.MaxFilesPerRequest)
        {
            throw new ArgumentException(
                $"Chỉ được phép tải lên tối đa {_options.MaxFilesPerRequest} tệp trong một lần gửi.");
        }

        var totalBytes = inputs.Sum(f => Math.Max(0, f.Length));
        if (totalBytes > _options.MaxRequestTotalBytes)
        {
            var maxTotalMb = _options.MaxRequestTotalBytes / (1024.0 * 1024.0);
            throw new ArgumentException(
                $"Tổng dung lượng các tệp vượt quá giới hạn {maxTotalMb:0.##}MB.");
        }

        var result = new MultiFileUploadResultDto();
        foreach (var item in inputs)
        {
            try
            {
                var uploaded = await UploadAsync(item, cancellationToken);
                result.UploadedFiles.Add(uploaded);
            }
            catch (ArgumentException ex)
            {
                result.Errors.Add(new FileUploadErrorDto
                {
                    OriginalFileName = Path.GetFileName(item.OriginalFileName),
                    Reason = ex.Message
                });
            }
        }

        result.SuccessCount = result.UploadedFiles.Count;
        result.ErrorCount = result.Errors.Count;
        return result;
    }

    public Task<FileDownloadResult?> OpenDownloadAsync(
        string fileName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var fullPath = ResolveSafeTargetFilePath(fileName);
        if (!File.Exists(fullPath))
        {
            return Task.FromResult<FileDownloadResult?>(null);
        }

        var fileInfo = new FileInfo(fullPath);
        var extension = Path.GetExtension(fullPath).ToLowerInvariant();
        var contentType = SafeMimeTypes.GetValueOrDefault(extension, "application/octet-stream");

        var stream = new FileStream(
            fullPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            useAsync: true);

        return Task.FromResult<FileDownloadResult?>(new FileDownloadResult
        {
            FileStream = stream,
            StoredFileName = fileInfo.Name,
            ContentType = contentType,
            SizeBytes = fileInfo.Length
        });
    }

    public Task<bool> DeleteAsync(
        string fileName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var fullPath = ResolveSafeTargetFilePath(fileName);
        if (!File.Exists(fullPath))
        {
            return Task.FromResult(false);
        }

        File.Delete(fullPath);
        return Task.FromResult(true);
    }

    private string ValidateExtensionAndContentType(string originalFileName, string rawContentType)
    {
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        var allowedExt = _options.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(extension) || !allowedExt)
        {
            throw new ArgumentException(
                $"Định dạng '{extension}' không được phép. Chỉ hỗ trợ: {string.Join(", ", _options.AllowedExtensions)}.");
        }

        var normalizedMime = rawContentType.Split(';')[0].Trim();
        var allowedMime = _options.AllowedContentTypes.Contains(normalizedMime, StringComparer.OrdinalIgnoreCase);
        if (!allowedMime)
        {
            throw new ArgumentException(
                $"MIME Content-Type '{normalizedMime}' không nằm trong danh sách cho phép.");
        }

        return extension;
    }

    private static async Task<int> ReadPrefixAsync(Stream stream, byte[] buffer, CancellationToken cancellationToken)
    {
        var totalRead = 0;
        while (totalRead < buffer.Length)
        {
            var bytesRead = await stream.ReadAsync(
                buffer.AsMemory(totalRead, buffer.Length - totalRead),
                cancellationToken);
            if (bytesRead == 0)
            {
                break;
            }

            totalRead += bytesRead;
        }

        return totalRead;
    }

    private static void ValidateSignature(string extension, ReadOnlySpan<byte> header, string originalFileName)
    {
        if (extension == ".md")
        {
            if (header.IndexOf((byte)0) >= 0 || HasDisallowedBinaryHeader(header))
            {
                throw new ArgumentException(
                    $"Tệp Markdown '{Path.GetFileName(originalFileName)}' chứa dữ liệu nhị phân hoặc chữ ký thực thi không hợp lệ.");
            }

            return;
        }

        if (extension == ".webp")
        {
            var isRiff = header.Length >= 12
                         && header[..4].SequenceEqual("RIFF"u8)
                         && header[8..12].SequenceEqual("WEBP"u8);
            if (!isRiff)
            {
                throw new ArgumentException(
                    $"Chữ ký nhị phân (Magic Bytes) của tệp '{Path.GetFileName(originalFileName)}' không khớp định dạng WEBP.");
            }

            return;
        }

        if (BinaryMagicSignatures.TryGetValue(extension, out var expectedSignature)
            && !header.StartsWith(expectedSignature))
        {
            throw new ArgumentException(
                $"Chữ ký nhị phân (Magic Bytes) của tệp '{Path.GetFileName(originalFileName)}' không khớp với phần mở rộng '{extension}'.");
        }
    }

    private static bool HasDisallowedBinaryHeader(ReadOnlySpan<byte> header)
    {
        foreach (var signature in DisallowedMarkdownHeaders)
        {
            if (header.StartsWith(signature))
            {
                return true;
            }
        }

        return false;
    }

    private async Task<(long SizeBytes, string Sha256Hex)> WriteFileAndComputeSha256Async(
        string destinationPath,
        byte[] prefixBuffer,
        int prefixLength,
        Stream remainingStream,
        CancellationToken cancellationToken)
    {
        using var hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        long totalBytesWritten = 0;

        try
        {
            await using var target = new FileStream(
                destinationPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: CopyBufferSize,
                useAsync: true);

            await target.WriteAsync(prefixBuffer.AsMemory(0, prefixLength), cancellationToken);
            hasher.AppendData(prefixBuffer, 0, prefixLength);
            totalBytesWritten += prefixLength;

            var copyBuffer = ArrayPool<byte>.Shared.Rent(CopyBufferSize);
            try
            {
                int read;
                while ((read = await remainingStream.ReadAsync(copyBuffer.AsMemory(), cancellationToken)) > 0)
                {
                    totalBytesWritten += read;
                    if (totalBytesWritten > _options.MaxFileSizeBytes)
                    {
                        throw new ArgumentException(
                            $"Kích thước luồng dữ liệu thực tế vượt quá giới hạn {_options.MaxFileSizeBytes} bytes.");
                    }

                    await target.WriteAsync(copyBuffer.AsMemory(0, read), cancellationToken);
                    hasher.AppendData(copyBuffer, 0, read);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(copyBuffer);
            }
        }
        catch
        {
            if (File.Exists(destinationPath))
            {
                File.Delete(destinationPath);
            }

            throw;
        }

        return (totalBytesWritten, Convert.ToHexString(hasher.GetHashAndReset()).ToLowerInvariant());
    }

    private string ResolveUploadRootDirectory()
    {
        var combined = Path.Combine(_hostEnvironment.ContentRootPath, _options.UploadFolder);
        return Path.TrimEndingDirectorySeparator(Path.GetFullPath(combined));
    }

    private string ResolveSafeTargetFilePath(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)
            || fileName.Contains('/')
            || fileName.Contains('\\')
            || fileName.Contains("..")
            || Path.GetFileName(fileName) != fileName
            || fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            throw new ArgumentException("Tên tệp không hợp lệ hoặc vi phạm bảo mật đường dẫn (Path Traversal).");
        }

        var rootDir = ResolveUploadRootDirectory();
        var fullTarget = Path.GetFullPath(Path.Combine(rootDir, fileName));
        if (!fullTarget.StartsWith(rootDir + Path.DirectorySeparatorChar, StringComparison.Ordinal))
        {
            throw new ArgumentException("Đường dẫn tệp vượt ra ngoài phạm vi thư mục lưu trữ được cấp phép.");
        }

        return fullTarget;
    }

    private string BuildStaticUrl(string storedFileName)
    {
        var normalizedFolder = _options.UploadFolder.Replace('\\', '/').Trim('/');
        if (normalizedFolder.StartsWith("wwwroot/", StringComparison.OrdinalIgnoreCase))
        {
            var relative = normalizedFolder["wwwroot/".Length..];
            return $"/{relative}/{storedFileName}";
        }

        return $"/api/v1/files/{storedFileName}";
    }
}
