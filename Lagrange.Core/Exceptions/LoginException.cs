namespace Lagrange.Core.Exceptions;

public class LoginException(short retCode, string tag, string message) : Exception($"{tag}({retCode}) | {message}")
{
    public short RetCode { get; } = retCode; 
    
    public string Tag { get; } = tag;
    
    public string ErrorMsg { get; } = message;
}