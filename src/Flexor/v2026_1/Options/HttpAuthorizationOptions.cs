using System.Runtime.InteropServices;
using System.Security;
using Azure.Bicep.Types.Concrete;
using Bicep.Local.Extension.Types.Attributes;
using Flexor.Options;

namespace Flexor.v2026_1.Options;

public class HttpAuthorizationOptions
{
    [TypeProperty("A token used for `Bearer` authentication in the HTTP request", ObjectTypePropertyFlags.None)]
    public SecureString? BearerToken { get; set; }

    [TypeProperty("Credentials used for `Basic` authentication in the HTTP request", ObjectTypePropertyFlags.None)]
    public Credential? Credential { get; set; }

    internal string StringBearerToken 
        => BearerToken is null 
         ? string.Empty 
         : Marshal.PtrToStringUni(Marshal.SecureStringToGlobalAllocUnicode(BearerToken)) 
           ?? string.Empty;
}

