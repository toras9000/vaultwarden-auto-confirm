using System.Text.Json;
using Lestaly;
using Microsoft.Extensions.Logging;
using vaultwarden_auto_confirm.Services;
using vaultwarden_auto_confirm.Settings;

// 停止トークン作成
using var signal = new SignalCancellationPeriod();

// ロガーを作成
using var loggerFactory = LoggerFactory.Create(builder => builder.AddSimpleConsole(conf => conf.SingleLine = true));
var logger = loggerFactory.CreateLogger("VWAC");

// 設定ファイル読み出し
var settingsFile = EnvVers.AppRelativeFile("VWAC_SETTINGS_FILE", "Data/settings.json");
logger.LogInformation($"Load settings file: {settingsFile.FullName}");
var settingsOptions = new JsonSerializerOptions { AllowTrailingCommas = true, ReadCommentHandling = JsonCommentHandling.Skip, };
var settings = await settingsFile.ReadJsonAsync<AppSettings>(settingsOptions, cancelToken: signal.Token) ?? throw new Exception("Fialed to load settings");

// 未設定の状態を簡易的に検出
if (settings.Vaultwarden.Server.Url.IsWhite())
{
    logger.LogError("No server settings");
    await Task.Delay(Timeout.Infinite, signal.Token);
    return;
}

// 確認処理生成
using var confirmer = new MemberConfirmService(settings, logger);
await confirmer.InitAsync(signal.Token);

// 処理対象表示
logger.LogInformation($"Settings:");
logger.LogInformation($".. Vaultwarden        : {settings.Vaultwarden.Server.Url}");
logger.LogInformation($"   .. ConfirmUser     : {settings.Vaultwarden.ConfirmUser.Mail}");
logger.LogInformation($"   .. Organization    : {confirmer.Context.Org.ID} ({confirmer.Context.Org.Name})");


// 起動時待機
if (0 < settings.Operation.StartupWaitSeconds)
{
    logger.LogInformation("Startup waiting...");
    var waitTime = TimeSpan.FromSeconds(settings.Operation.StartupWaitSeconds);
    await Task.Delay(waitTime, signal.Token);
}

// 定期的な確認処理
var intervalTime = TimeSpan.FromMinutes(settings.Operation.IntervalMinutes);
while (true)
{
    try
    {
        await confirmer.ConfirmAsync(signal.Token);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to confirm");
    }
    await Task.Delay(intervalTime, signal.Token);
}
