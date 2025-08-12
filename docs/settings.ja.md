# configuration file

vaultwarden-auto-confirm はデフォルトで実行ファイルの場所からの相対パス `Data/settings.json` を設定ファイルとして読み込みます。  
この設定ファイルでは対象の組織、確認を行うユーザ、コレクションへの許可などの設定を行います。  

## ファイル内容

ファイルはJSON形式で、全容は以下のようになります。  
個々の設定については後述します。  

```
{
  "Operation": {
    "StartupWaitSeconds": 60,
    "IntervalMinutes": 60
  },
  "Vaultwarden": {
    "Server": {
      "Url": "http://vaultwarden-server"
    },
    "Organization": {
      "OrgId": "organization-id",
    },
    "ConfirmUser": {
      "Mail": "owner-user@example.com",
      "MasterPassword": "owner-master-password"
    },
    "Permissions": {
      "Enabled": true,
      "Role": "User",
      "Collections": [
        {
          "ID": "collection-id",
          "Privilege": "Show"
        },
        {
          "Name": "collection-name",
          "Privilege": "Edit"
        }
      ]
    },
    "Client": {
      "DeviceName": "vaultwarden-auto-confirm",
      "DeviceIdentifier": "vaultwarden-auto-confirm"
    }
  }
}
```

## 設定の詳細

| パス                                              | 型      | 説明                                                                                                                                       |
|---------------------------------------------------|---------|--------------------------------------------------------------------------------------------------------------------------------------------|
| `Operation.StartupWaitSeconds`                    | number  | 起動時の処理開始前待機時間[秒]                                                                                                             |
| `Operation.IntervalMinutes`                       | number  | 処理実行の間隔[分]                                                                                                                         |
| `Vaultwarden.Server.Url`                          | string  | VaultwardenサーバーのURL                                                                                                                   |
| `Vaultwarden.Organization.OrgId`                  | string  | 対象組織のID                                                                                                                               |
| `Vaultwarden.ConfirmUser.Mail`                    | string  | 確認を行うユーザのメールアドレス                                                                                                           |
| `Vaultwarden.ConfirmUser.MasterPassword`          | string  | 上記ユーザのマスターパスワード                                                                                                             |
| `Vaultwarden.Permissions.Enabled`                 | boolean | 確認したユーザに対する権限設定を行うかどうか。<br>有効な場合のみ以下の権限設定を利用する。                                                 |
| `Vaultwarden.Permissions.Role`                    | string  | 権限付与するロール。<br>有効な値は `User`, `Manager`, `Owner` のいずれか。                                                                 |
| `Vaultwarden.Permissions.Collections[].ID`        | string  | 設定対象コレクションのID。<br>名前指定とは排他指定となる。空やundefinedでない場合はID指定が優先され、IDが一致するものが対象となる。        |
| `Vaultwarden.Permissions.Collections[].Name`      | string  | 設定対象コレクションの名前。<br>IDが指定されない場合のみ参照され、名前が一致するコレクションが対象となる。もし複数あればすべて対象となる。 |
| `Vaultwarden.Permissions.Collections[].Privilege` | string  | コレクションに対するアクセス権限。<br>有効な値は `Show`, `ShowHidePassword`, `Edit`, `EditHidePassword`, `Manage` のいずれか               |
| `Vaultwarden.Client.DeviceName`                   | string  | デバイスの表示名<br>これはVaultwardenで接続元の記録に用いられる。                                                                          |
| `Vaultwarden.Client.DeviceIdentifier`             | string  | デバイス識別子。同上。                                                                                                                     |
