namespace SmartPrompt.Application.Common.Exceptions;

public class AIProviderException : Exception
{
    public string Provider { get; }

    public AIProviderException(string provider, string message)
        : base(message)
    {
        Provider = provider;
    }

    public AIProviderException(string provider, string message, Exception innerException)
        : base(message, innerException)
    {
        Provider = provider;
    }
}
