#!/usr/bin/env dotnet-script
#r "nuget: VwConnector, 1.34.3-rev.1"
#r "nuget: Lestaly.General, 0.102.0"
#r "nuget: Kokuban, 0.2.0"
#load ".settings.csx"
#nullable enable
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Kokuban;
using Lestaly;
using Lestaly.Cx;
using VwConnector;
using VwConnector.Agent;

var settings = new
{
    VwliConfigFile = ThisSource.RelativeFile("./assets/vwac/settings.json"),
};

public record OperationSettings(int StartupWaitSeconds, int IntervalMinutes);
public record VaultwardenServerSettings(string Url);
public record VaultwardenOrganizationSettings(string OrgId);
public record VaultwardenConfirmUserSettings(string Mail, string MasterPassword);
[JsonConverter(typeof(JsonStringEnumConverter))] public enum VaultwardenCollectionPrivilege { Show, ShowHidePassword, Edit, EditHidePassword, Manage, }
public record VaultwardenCollectionSettings(string? ID, string? Name, VaultwardenCollectionPrivilege Privilege);
[JsonConverter(typeof(JsonStringEnumConverter))] public enum VaultwardenMemberRole { User, Manager, Owner, }
public record VaultwardenPermissionsSettings(bool Enabled, VaultwardenMemberRole Role, VaultwardenCollectionSettings[] Collections);
public record VaultwardenClientSettings(string DeviceName, string DeviceIdentifier);
public record VaultwardenSettings(VaultwardenServerSettings Server, VaultwardenOrganizationSettings Organization, VaultwardenConfirmUserSettings ConfirmUser, VaultwardenPermissionsSettings Permissions, VaultwardenClientSettings Client);
public record AppSettings(OperationSettings Operation, VaultwardenSettings Vaultwarden);

return await Paved.ProceedAsync(noPause: Args.RoughContains("--no-pause"), async () =>
{
    using var signal = new SignalCancellationPeriod();

    var testUser = vwSettings.Setup.TestUser;
    var testOrg = vwSettings.Setup.TestOrg;

    WriteLine("Get info");
    using var agent = await VaultwardenAgent.CreateAsync(vwSettings.Service.Url, new(testUser.Mail, testUser.Password), signal.Token);
    var userProfile = agent.Profile;
    var orgProfile = agent.Profile.organizations.FirstOrDefault(o => o.name == testOrg.Name) ?? throw new Exception("Not found org");
    var collections = await agent.GetCollectionsAsync(orgProfile.id, signal.Token);
    if (collections.Length <= 0) throw new Exception("No collections");

    WriteLine("Update vaultwarden-ldap-import config");
    var config = await settings.VwliConfigFile.ReadJsonAsync<AppSettings>() ?? throw new Exception("Cannot load config");
    config = config with
    {
        Vaultwarden = config.Vaultwarden with
        {
            Organization = new(OrgId: orgProfile.id),
            Permissions = config.Vaultwarden.Permissions with
            {
                Collections = collections.Select((c, i) =>
                {
                    var id = (i % 2 == 0) ? c.Id : default;
                    var name = (id == null) ? c.Name : default;
                    var priv = (i % 3) switch
                    {
                        1 => VaultwardenCollectionPrivilege.Edit,
                        2 => VaultwardenCollectionPrivilege.Manage,
                        _ => VaultwardenCollectionPrivilege.Show,
                    };
                    return new VaultwardenCollectionSettings(id, name, priv);
                }).ToArray(),
            },
        },
    };

    var options = new JsonSerializerOptions
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };
    await settings.VwliConfigFile.WriteJsonAsync(config, options);

    WriteLine("Restart containers.");
    var composeFile = ThisSource.RelativeFile("./compose.yml");
    await "docker".args("compose", "--file", composeFile, "down", "confirmer");
    await "docker".args("compose", "--file", composeFile, "up", "-d", "--wait", "confirmer").result().success();

});
