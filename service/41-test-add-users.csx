#!/usr/bin/env dotnet-script
#r "nuget: VwConnector, 1.34.3-rev.1"
#r "nuget: Lestaly.General, 0.102.0"
#r "nuget: Kokuban, 0.2.0"
#load ".settings.csx"
#nullable enable
using System.Net;
using Kokuban;
using Lestaly;
using VwConnector;
using VwConnector.Agent;

return await Paved.ProceedAsync(noPause: Args.RoughContains("--no-pause"), async () =>
{
    using var signal = new SignalCancellationPeriod();

    var orgName = vwSettings.Setup.TestOrg.Name;
    var user = new UserContext(vwSettings.Setup.TestUser.Mail, vwSettings.Setup.TestUser.Password);
    var agent = await VaultwardenAgent.CreateAsync(vwSettings.Service.Url, user, signal.Token);
    var org = agent.Profile.organizations.FirstOrDefault(o => o.name == orgName) ?? throw new Exception("Org not found");

    WriteLine("Register test user");
    while (true)
    {
        Write(">");
        var input = ReadLine()?.Trim();
        if (input == null) break;
        if (input.IsWhite()) continue;

        try
        {
            var name = input.Trim() == "*" ? $"user-{DateTime.Now.Ticks:X16}" : input;
            var mail = $"{name}@myserver.home";
            var pass = $"{name}-pass";
            WriteLine("Register");
            WriteLine($"  Mail: {mail}");
            WriteLine($"  Pass: {pass}");
            await agent.Connector.Account.RegisterUserNoSmtpAsync(new(mail, pass), signal.Token);
            WriteLine(Chalk.Green["  .. Completed"]);

            WriteLine($"Invite to {org.name}");
            var inviteArgs = new InviteOrgMemberArgs(VwConnector.InviteMembershipType.User, emails: [mail], groups: []);
            await agent.Connector.Organization.InviteUserAsync(agent.Token, org.id, inviteArgs, signal.Token);
            WriteLine(Chalk.Green["  .. Completed"]);
        }
        catch (Exception ex)
        {
            WriteLine(Chalk.Red[$"Error: {ex.Message}"]);
        }
    }
});
