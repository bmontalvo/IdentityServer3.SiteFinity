using IdentityServer3.Core;
using IdentityServer3.Core.Services.InMemory;
using System.Collections.Generic;
using System.Security.Claims;

namespace Host.Config
{
    static class Users
    {
        public static List<InMemoryUser> Get()
        {
            return new List<InMemoryUser>
            {
                new InMemoryUser{Subject = "1", Username = "Admin", Password = "secret",
                    Claims = new Claim[]
                    {
                        new Claim(Constants.ClaimTypes.GivenName, "Admin"),
                        new Claim(Constants.ClaimTypes.FamilyName, "Admin")
                    }
                },
            };
        }
    }
}
