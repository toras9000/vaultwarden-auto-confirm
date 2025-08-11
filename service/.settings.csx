using System.Net;
using Lestaly;
#nullable enable

var vwSettings = new
{
    // Vaultwarden service
    Service = new
    {
        // Vaultwarden URL
        Url = new Uri("http://localhost:8240"),
    },

    Setup = new
    {
        Admin = new
        {
            Password = "admin-pass",
        },

        TestUser = new
        {
            Mail = "tester@myserver.home",
            Password = "tester-password",
        },

        TestOrg = new
        {
            Name = "TestOrg",
            Collections = new[]
            {
                "Collec1",
                "Collec2",
            },
        },
    },
};
