using System.Security.Principal;

namespace AceSmokeShop
{
    public class AuthenticatedUser : IIdentity
    {
        public AuthenticatedUser(string AuthType, bool IsAuth, string name)
        {
            AuthenticationType = AuthType;
            IsAuthenticated = IsAuth;
            Name = name;
        }

        public string AuthenticationType { get; }

        public bool IsAuthenticated { get; }

        public string Name { get; }
    }
}
