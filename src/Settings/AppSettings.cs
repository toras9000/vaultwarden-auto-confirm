using System.Text.Json.Serialization;

namespace vaultwarden_auto_confirm.Settings;

/// <summary>動作設定</summary>
/// <param name="StartupWaitSeconds">開始時の待機時間[秒]</param>
/// <param name="IntervalMinutes">インポート処理間隔[分]</param>
public record OperationSettings(int StartupWaitSeconds, int IntervalMinutes);

/// <summary>Vaultwardenサーバ設定</summary>
/// <param name="Url">Vaultwarden URL</param>
public record VaultwardenServerSettings(string Url);

/// <summary>Vaultwarden組織情報</summary>
/// <param name="OrgId">対象組織ID</param>
public record VaultwardenOrganizationSettings(string OrgId);

/// <summary>Vaultwarden確認ユーザ情報</summary>
/// <param name="Mail">確認ユーザのメールアドレス</param>
/// <param name="MasterPassword">確認ユーザのマスターパスワード</param>
public record VaultwardenConfirmUserSettings(string Mail, string MasterPassword);

/// <summary>Vaultwardenコレクション権限</summary>
[JsonConverter(typeof(JsonStringEnumConverter<VaultwardenCollectionPrivilege>))]
public enum VaultwardenCollectionPrivilege
{
    /// <summary>アイテム表示</summary>
    Show,
    /// <summary>アイテム表示 (パスワード非表示)</summary>
    ShowHidePassword,
    /// <summary>アイテム編集</summary>
    Edit,
    /// <summary>アイテム編集 (パスワード非表示)</summary>
    EditHidePassword,
    /// <summary>管理</summary>
    Manage,
}

/// <summary>Vaultwardenコレクションへの許可設定</summary>
/// <param name="ID">コレクションID。コレクション名とは排他設定。</param>
/// <param name="Name">コレクション名。ID未設定の場合のみ参照する。名称が一致する全てのコレクションが対象となる。</param>
/// <param name="Privilege">コレクションへの権限</param>
public record VaultwardenCollectionSettings(string? ID, string? Name, VaultwardenCollectionPrivilege Privilege);

/// <summary>Vaultwardenメンバーロール</summary>
[JsonConverter(typeof(JsonStringEnumConverter<VaultwardenMemberRole>))]
public enum VaultwardenMemberRole
{
    /// <summary>ユーザ</summary>
    User,
    /// <summary>管理者</summary>
    Manager,
    /// <summary>所有者</summary>
    Owner,
}

/// <summary>Vaultwardenメンバーへの許可設定</summary>
/// <param name="Enabled">パーミッション設定を行うか否か</param>
/// <param name="Role">ロール</param>
/// <param name="Collections">コレクションへのアクセス権</param>
public record VaultwardenPermissionsSettings(bool Enabled, VaultwardenMemberRole Role, VaultwardenCollectionSettings[] Collections);

/// <summary>Vaultwardenアクセスクライアント情報</summary>
/// <param name="DeviceName">デバイス名</param>
/// <param name="DeviceIdentifier">デバイス識別子</param>
public record VaultwardenClientSettings(string DeviceName, string DeviceIdentifier);

/// <summary>Vaultwarden設定</summary>
/// <param name="Server">Vaultwardenサーバ設定</param>
/// <param name="Organization">Vaultwarden組織情報</param>
/// <param name="ConfirmUser">Vaultwarden確認ユーザ情報</param>
/// <param name="Permissions">Vaultwardenメンバー許可設定</param>
/// <param name="Client">Vaultwardenアクセスクライアント情報</param>
public record VaultwardenSettings(
    VaultwardenServerSettings Server,
    VaultwardenOrganizationSettings Organization,
    VaultwardenConfirmUserSettings ConfirmUser,
    VaultwardenPermissionsSettings Permissions,
    VaultwardenClientSettings Client
 );

/// <summary>アプリケーション設定のルート</summary>
/// <param name="Operation">動作設定</param>
/// <param name="Ldap">LDAP設定</param>
/// <param name="Vaultwarden">Vaultwarden設定</param>
public record AppSettings(OperationSettings Operation, VaultwardenSettings Vaultwarden);
