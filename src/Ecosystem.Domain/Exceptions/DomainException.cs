namespace Ecosystem.Domain.Exceptions;

/// <summary>
/// Ngoại lệ miền xảy ra khi vi phạm quy tắc nghiệp vụ cốt lõi.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
