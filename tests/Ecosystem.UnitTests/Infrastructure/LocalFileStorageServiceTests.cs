using System.Text;
using Ecosystem.Application.Common;
using Ecosystem.Application.DTOs;
using Ecosystem.Infrastructure.Files;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;

namespace Ecosystem.UnitTests.Infrastructure;

public class LocalFileStorageServiceTests : IDisposable
{
    private static readonly byte[] ValidPngHeader = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D };
    private static readonly byte[] ValidJpegHeader = { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46 };
    private static readonly byte[] ExeMzHeader = { 0x4D, 0x5A, 0x90, 0x00, 0x03, 0x00, 0x00, 0x00 };

    private readonly string _tempRoot;
    private readonly FileStorageOptions _options;
    private readonly LocalFileStorageService _service;

    public LocalFileStorageServiceTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), $"eco_file_unit_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempRoot);

        _options = new FileStorageOptions
        {
            MaxFileSizeBytes = 5 * 1024 * 1024,
            MaxRequestTotalBytes = 15 * 1024 * 1024,
            MaxFilesPerRequest = 5,
            UploadFolder = "wwwroot/uploads",
            VerifyMagicBytes = true
        };

        var snapshotMock = new Mock<IOptionsSnapshot<FileStorageOptions>>();
        snapshotMock.Setup(s => s.Value).Returns(() => _options);

        var envMock = new Mock<IHostEnvironment>();
        envMock.Setup(e => e.ContentRootPath).Returns(_tempRoot);

        _service = new LocalFileStorageService(snapshotMock.Object, envMock.Object);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, recursive: true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task UploadAsync_ValidPng_SavesWithGuidFileNameAndComputesChecksum()
    {
        // Arrange
        await using var stream = new MemoryStream(ValidPngHeader);
        var input = new FileUploadInput
        {
            Content = stream,
            OriginalFileName = "../../nested/diagram.PNG",
            ContentType = "image/png",
            Length = ValidPngHeader.Length
        };

        // Act
        var result = await _service.UploadAsync(input);

        // Assert
        Assert.EndsWith(".png", result.StoredFileName);
        Assert.DoesNotContain("..", result.StoredFileName);
        Assert.Equal("diagram.PNG", result.OriginalFileName);
        Assert.Equal("image/png", result.ContentType);
        Assert.Equal(ValidPngHeader.Length, result.SizeBytes);
        Assert.Equal(64, result.Sha256Checksum.Length);
        Assert.Equal($"/api/v1/files/{result.StoredFileName}", result.DownloadUrl);
        Assert.Equal($"/uploads/{result.StoredFileName}", result.StaticUrl);

        var physicalPath = Path.Combine(_tempRoot, "wwwroot", "uploads", result.StoredFileName);
        Assert.True(File.Exists(physicalPath));
    }

    [Fact]
    public async Task UploadAsync_ValidMarkdownAndPdf_Succeed()
    {
        // Arrange
        var mdBytes = Encoding.UTF8.GetBytes("# Kiến trúc Clean Architecture\n\nNội dung tiếng Việt UTF-8.");
        var pdfBytes = Encoding.ASCII.GetBytes("%PDF-1.7\n1 0 obj\n<<>>\nendobj\n");

        await using var mdStream = new MemoryStream(mdBytes);
        await using var pdfStream = new MemoryStream(pdfBytes);

        // Act
        var mdResult = await _service.UploadAsync(new FileUploadInput
        {
            Content = mdStream,
            OriginalFileName = "notes.md",
            ContentType = "text/markdown; charset=utf-8",
            Length = mdBytes.Length
        });

        var pdfResult = await _service.UploadAsync(new FileUploadInput
        {
            Content = pdfStream,
            OriginalFileName = "slides.pdf",
            ContentType = "application/pdf",
            Length = pdfBytes.Length
        });

        // Assert
        Assert.EndsWith(".md", mdResult.StoredFileName);
        Assert.EndsWith(".pdf", pdfResult.StoredFileName);
    }

    [Fact]
    public async Task UploadAsync_ZeroLengthOrOverMaxSize_ThrowsArgumentException()
    {
        // Arrange & Act & Assert 1: 0 byte
        await using var emptyStream = new MemoryStream();
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UploadAsync(new FileUploadInput
        {
            Content = emptyStream,
            OriginalFileName = "empty.png",
            ContentType = "image/png",
            Length = 0
        }));

        // Arrange & Act & Assert 2: Over 5MB
        await using var dummyStream = new MemoryStream(ValidJpegHeader);
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UploadAsync(new FileUploadInput
        {
            Content = dummyStream,
            OriginalFileName = "huge.jpg",
            ContentType = "image/jpeg",
            Length = (5 * 1024 * 1024) + 1
        }));
    }

    [Theory]
    [InlineData("malware.exe", "application/octet-stream")]
    [InlineData("xss.svg", "image/svg+xml")]
    [InlineData("page.html", "text/html")]
    [InlineData("noextension", "image/png")]
    public async Task UploadAsync_DisallowedExtensionOrMimeType_ThrowsArgumentException(
        string fileName,
        string contentType)
    {
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes("<svg onload=alert(1)/>"));
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UploadAsync(new FileUploadInput
        {
            Content = stream,
            OriginalFileName = fileName,
            ContentType = contentType,
            Length = stream.Length
        }));
    }

    [Fact]
    public async Task UploadAsync_ExecutableDisguisedAsJpgOrMarkdown_BlockedByMagicBytesAndNothingSaved()
    {
        // Act & Assert 1: virus.exe renamed to avatar.jpg with image/jpeg
        await using var exeAsJpg = new MemoryStream(ExeMzHeader);
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UploadAsync(new FileUploadInput
        {
            Content = exeAsJpg,
            OriginalFileName = "avatar.jpg",
            ContentType = "image/jpeg",
            Length = ExeMzHeader.Length
        }));

        // Act & Assert 2: virus.exe renamed to note.md
        await using var exeAsMd = new MemoryStream(ExeMzHeader);
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UploadAsync(new FileUploadInput
        {
            Content = exeAsMd,
            OriginalFileName = "note.md",
            ContentType = "text/markdown",
            Length = ExeMzHeader.Length
        }));

        // Assert: Zero files created in uploads folder
        var uploadDir = Path.Combine(_tempRoot, "wwwroot", "uploads");
        Assert.False(Directory.Exists(uploadDir) && Directory.GetFiles(uploadDir).Length > 0);
    }

    [Theory]
    [InlineData("../appsettings.json")]
    [InlineData("..\\secret.txt")]
    [InlineData("subdir/image.png")]
    [InlineData("")]
    public async Task OpenDownloadAndDelete_PathTraversalAttempt_ThrowsArgumentException(string maliciousName)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.OpenDownloadAsync(maliciousName));
        await Assert.ThrowsAsync<ArgumentException>(() => _service.DeleteAsync(maliciousName));
    }

    [Fact]
    public async Task UploadMultipleAsync_PartialSuccess_SavesValidAndCollectsInvalidErrors()
    {
        // Arrange
        await using var validPng = new MemoryStream(ValidPngHeader);
        await using var spoofedExe = new MemoryStream(ExeMzHeader);

        var inputs = new List<FileUploadInput>
        {
            new() { Content = validPng, OriginalFileName = "good.png", ContentType = "image/png", Length = ValidPngHeader.Length },
            new() { Content = spoofedExe, OriginalFileName = "fake.png", ContentType = "image/png", Length = ExeMzHeader.Length }
        };

        // Act
        var result = await _service.UploadMultipleAsync(inputs);

        // Assert
        Assert.Equal(1, result.SuccessCount);
        Assert.Equal(1, result.ErrorCount);
        Assert.Single(result.UploadedFiles);
        Assert.Single(result.Errors);
        Assert.Equal("fake.png", result.Errors[0].OriginalFileName);
    }

    [Fact]
    public async Task UploadMultipleAsync_ExceedsMaxFilesPerRequest_ThrowsArgumentException()
    {
        _options.MaxFilesPerRequest = 2;

        await using var s1 = new MemoryStream(ValidPngHeader);
        await using var s2 = new MemoryStream(ValidPngHeader);
        await using var s3 = new MemoryStream(ValidPngHeader);

        var inputs = new List<FileUploadInput>
        {
            new() { Content = s1, OriginalFileName = "1.png", ContentType = "image/png", Length = ValidPngHeader.Length },
            new() { Content = s2, OriginalFileName = "2.png", ContentType = "image/png", Length = ValidPngHeader.Length },
            new() { Content = s3, OriginalFileName = "3.png", ContentType = "image/png", Length = ValidPngHeader.Length }
        };

        await Assert.ThrowsAsync<ArgumentException>(() => _service.UploadMultipleAsync(inputs));
    }
}
