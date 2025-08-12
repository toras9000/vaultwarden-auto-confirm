# vaultwarden-auto-confirm

Vaultwarden 上で定期的に組織メンバーの確認(承認)を行うためのツールです。  

## ツールの動作

本ツールは [設定ファイル](./docs/settings.ja.md) の内容に基づき、一定間隔で Vaultwarden の組織メンバーに対する確認(承認)を自動的に実行します。  
対象となるのは、組織に招待されて（設定によってはユーザ自身が招待を承認したうえで）確認待ちとなっているユーザです。  
また、ユーザの確認と同時に、コレクションへのアクセス許可設定を行うこともできます。  

## ツールの実行

実行バイナリは [Docker イメージ](https://github.com/toras9000/vaultwarden-auto-confirm/pkgs/container/vaultwarden-auto-confirm) として提供しています。  
このイメージは、デフォルトで `/app/Data/settings.json` を設定ファイルとして参照します。  
設定ファイルのパスは、環境変数 `VWAC_SETTINGS_FILE` によって変更可能です。  

以下は、環境変数を利用した docker-compose の例です。

```yaml
services:
  importer:
    image: ghcr.io/toras9000/vaultwarden-auto-confirm:0.1.0
    restart: unless-stopped
    volumes:
      - type: bind
        source: ./assets/vwac/settings.json
        target: /vwac/settings.json
        read_only: true
        bind:
          create_host_path: false
    environment:
      - VWAC_SETTINGS_FILE=/vwac/settings.json
```
