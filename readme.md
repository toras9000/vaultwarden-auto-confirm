# vaultwarden-auto-confirm

A tool for periodically confirming (approving) organization members in Vaultwarden.

## How It Works

This tool automatically performs member confirmation (approval) for Vaultwarden organizations at fixed intervals, based on the settings defined in the [configuration file](./docs/settings.md).  
It targets users who have been invited to the organization and are in a “pending confirmation” state — including cases where the invited user must approve the invitation themselves, depending on your settings.  
In addition to confirming users, the tool can also assign collection access permissions at the same time.

## Running the Tool

The executable binary is provided as a [Docker image](https://github.com/toras9000/vaultwarden-auto-confirm/pkgs/container/vaultwarden-auto-confirm).  
By default, the image uses `/app/Data/settings.json` as the configuration file.  
You can change the configuration file path by setting the `VWAC_SETTINGS_FILE` environment variable.

Below is an example docker-compose configuration using an environment variable:

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
