using System.Runtime.Serialization;

namespace NxEditor.Core.Exceptions;

public class LocalizationException : Exception
{
    public LocalizationException() { }
    public LocalizationException(string? message) : base(message) { }
    public LocalizationException(string? message, Exception? innerException) : base(message, innerException) { }
    protected LocalizationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
