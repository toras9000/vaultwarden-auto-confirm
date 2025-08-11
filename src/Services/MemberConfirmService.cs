using Lestaly;
using Microsoft.Extensions.Logging;
using vaultwarden_auto_confirm.Settings;
using VwConnector;
using VwConnector.Agent;

namespace vaultwarden_auto_confirm.Services;

/// <summary>確認処理対象組織</summary>
/// <param name="ID">組織ID</param>
/// <param name="Name">組織名</param>
public record MemberConfirmOrg(string ID, string Name);

/// <summary>確認処理コンテキスト</summary>
/// <param name="Org">組織</param>
/// <param name="EditPermissions">パーミッションの設定を行うか否か</param>
public record MemberConfirmContext(MemberConfirmOrg Org, bool EditPermissions);

/// <summary>確認処理</summary>
public class MemberConfirmService : IDisposable
{
    // 構築
    #region コンストラクタ
    /// <summary>依存データを受け取るコンストラクタ</summary>
    /// <param name="settings">設定</param>
    /// <param name="logger">ロガー</param>
    public MemberConfirmService(AppSettings settings, ILogger logger)
    {
        this.logger = logger;
        this.appSettings = settings;
        this.vwConnector = new VaultwardenConnector(new Uri(settings.Vaultwarden.Server.Url));
        this.context = new MemberConfirmContext(
            Org: new(this.appSettings.Vaultwarden.Organization.OrgId, "-"),
            EditPermissions: false
        );
    }
    #endregion

    // 公開プロパティ
    #region コンストラクタ
    /// <summary>確認処理コンテキスト</summary>
    public MemberConfirmContext Context => this.context;
    #endregion

    // 公開メソッド
    #region 初期化
    /// <summary>確認処理用のデータを準備する初期化処理</summary>
    /// <param name="breaker">中止トークン</param>
    public async ValueTask InitAsync(CancellationToken breaker)
    {
        this.logger.LogInformation("Initialization of confirmation");

        // Vaultwardenクライアント生成
        using var agent = await createAgentAsync(breaker);

        // 対象組織情報を取得
        var orgId = this.appSettings.Vaultwarden.Organization.OrgId;
        var org = agent.Profile.organizations.FirstOrDefault(o => string.Equals(o.id, orgId, StringComparison.OrdinalIgnoreCase));
        if (org == null) throw new ArgumentException($"Not found organization '{orgId}'");

        // 取得した情報をコンテキスト情報として保持
        var permSettings = this.appSettings.Vaultwarden.Permissions;
        this.context = new MemberConfirmContext(Org: new(org.id, org.name), EditPermissions: permSettings.Enabled);

        // パーミッション適用が有効であれば、設定用データを作る
        if (permSettings.Enabled)
        {
            // 組織のコレクション情報を取得
            var orgCollections = await agent.GetCollectionsAsync(org.id, breaker);

            // 許可設定を元に、パーミッション設定情報を作る
            // 同一コレクションに対する設定を複数作ってしまわないように、取得したコレクションを基準にして構築する
            var permCollections = new List<VwCollection>();
            foreach (var orgCol in orgCollections)
            {
                // コレクションIDを指定した設定があればそれを優先
                var matchId = permSettings.Collections.FirstOrDefault(c => c.ID.IsNotWhite() && string.Equals(c.ID, orgCol.Id, StringComparison.OrdinalIgnoreCase));
                if (matchId != null)
                {
                    permCollections.Add(makeCollectionPermission(orgCol.Id, matchId));
                    continue;
                }

                // 上記でマッチしなければ、名前の一致する設定を検索
                var matchName = permSettings.Collections.FirstOrDefault(c => c.Name.IsNotWhite() && c.Name == orgCol.Name);
                if (matchName != null)
                {
                    permCollections.Add(makeCollectionPermission(orgCol.Id, matchName));
                    continue;
                }
            }

            // ユーザロールをパーミッション設定用に変換
            var permRole = permSettings.Role switch
            {
                VaultwardenMemberRole.Owner => EditMembershipType.Owner,
                VaultwardenMemberRole.Manager => EditMembershipType.Manager,
                _ => EditMembershipType.User,
            };

            // パーミッション設定要求のパラメータデータを構築。
            this.editArgs = new(permRole, collections: permCollections.ToArray(), groups: [], access_all: false, new(false, false, false));
        }
    }
    #endregion

    #region 確認処理
    /// <summary>ユーザの確認を行う</summary>
    /// <param name="breaker">中止トークン</param>
    public async ValueTask ConfirmAsync(CancellationToken breaker)
    {
        this.logger.LogInformation("Check members to confirm");

        // Vaultwardenクライアント生成
        using var agent = await createAgentAsync(breaker);

        // 組織メンバの取得
        var orgID = this.context.Org.ID;
        var members = await agent.Connector.Organization.GetMembersAsync(agent.Token, orgID, cancelToken: breaker);

        // 確認が必要なメンバの抽出
        var accepted = members.data.Where(m => m.status == MembershipStatus.Accepted).ToArray();
        if (accepted.Length <= 0)
        {
            this.logger.LogInformation(".. No targets");
            return;
        }
        this.logger.LogInformation($".. {accepted.Length} members");

        // 各メンバの確認処理
        foreach (var member in accepted)
        {
            this.logger.LogInformation($".. {member.email}");
            try
            {
                this.logger.LogInformation($".... Confirm");
                var confirmArgs = new AgentConfirmMemberArgs(member.id, member.userId);
                await agent.Affect.ConfirmMemberAsync(orgID, confirmArgs, breaker);

                if (this.editArgs != null)
                {
                    this.logger.LogInformation($".... Edit Permissions");
                    await agent.Connector.Organization.EditMemberAsync(agent.Token, orgID, member.id, this.editArgs, breaker);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $".... Failed.");
            }
        }
    }
    #endregion

    #region インポート処理
    /// <summary>リソース破棄</summary>
    public void Dispose()
    {
        this.vwConnector.Dispose();
    }
    #endregion


    // 非公開フィールド
    #region 確認処理
    /// <summary>設定</summary>
    private AppSettings appSettings;

    /// <summary>Vaultwardenアクセサ</summary>
    private VaultwardenConnector vwConnector;

    /// <summary>確認コンテキスト情報</summary>
    private MemberConfirmContext context;

    /// <summary>ユーザへのパーミッション付与設定</summary>
    private EditOrgMemberArgs? editArgs;
    #endregion

    #region 依存サービス
    /// <summary>ロガー</summary>
    private ILogger logger;
    #endregion

    // 非公開メソッド
    #region Vaultwardenアクセス
    /// <summary>Vaultwardenクライアントエージェントを生成する</summary>
    /// <param name="breaker"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private async ValueTask<VaultwardenAgent> createAgentAsync(CancellationToken breaker)
    {
        // 設定されたユーザによるクライアントエージェントを生成
        var vwSettings = this.appSettings.Vaultwarden;
        var vwClient = new ClientInfo(vwSettings.Client.DeviceName, vwSettings.Client.DeviceIdentifier);
        var vwUser = new UserContext(vwSettings.ConfirmUser.Mail, vwSettings.ConfirmUser.MasterPassword, vwClient);
        var agent = await VaultwardenAgent.CreateAsync(this.vwConnector, vwUser, breaker);
        // このエージェントがサポートしないKDFアルゴリズムであればエラーにする
        if (agent.Kdf.kdf != KdfType.Pbkdf2) throw new NotImplementedException("Not supported KDF algorithm");
        return agent;
    }

    /// <summary>コレクションへのパーミッション設定データを構築する</summary>
    /// <param name="id">コレクションID</param>
    /// <param name="perm">コレクションへの許可設定</param>
    /// <returns>パーミッション設定用データ</returns>
    private VwCollection makeCollectionPermission(string id, VaultwardenCollectionSettings perm)
        => perm.Privilege switch
        {
            VaultwardenCollectionPrivilege.Manage => new(id, readOnly: false, hidePasswords: false, manage: true),
            VaultwardenCollectionPrivilege.Edit => new(id, readOnly: false, hidePasswords: false, manage: false),
            VaultwardenCollectionPrivilege.EditHidePassword => new(id, readOnly: false, hidePasswords: true, manage: false),
            VaultwardenCollectionPrivilege.ShowHidePassword => new(id, readOnly: true, hidePasswords: true, manage: false),
            _ => new(id, readOnly: true, hidePasswords: false, manage: false),
        };
    #endregion

}
