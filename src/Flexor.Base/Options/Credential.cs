using System.Runtime.InteropServices;
using System.Security;

namespace Flexor.Options;

public record Credential(string Username, SecureString Password)
{
    public string StringPassword 
        => Marshal.PtrToStringUni(Marshal.SecureStringToGlobalAllocUnicode(Password)) 
           ?? string.Empty;
}
