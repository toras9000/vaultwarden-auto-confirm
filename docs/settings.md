# Configuration File

By default, **vaultwarden-auto-confirm** loads its configuration file from the relative path `Data/settings.json` based on the location of the executable.  
This configuration file defines settings such as the target organization, the user to perform confirmations, and the permissions for collections.

## File Contents

The file is in JSON format, with the complete structure shown below.  
Details of each setting are provided in the next section.

```json
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
      "OrgId": "organization-id"
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

## Configuration Details

| Path                                              | Type    | Description                                                                                                                                                                                                 |
|---------------------------------------------------|---------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `Operation.StartupWaitSeconds`                    | number  | Wait time in seconds before starting the process after application startup.                                                                                                                                 |
| `Operation.IntervalMinutes`                       | number  | Interval in minutes between process executions.                                                                                                                                                             |
| `Vaultwarden.Server.Url`                          | string  | URL of the Vaultwarden server.                                                                                                                                                                              |
| `Vaultwarden.Organization.OrgId`                  | string  | ID of the target organization.                                                                                                                                                                              |
| `Vaultwarden.ConfirmUser.Mail`                    | string  | Email address of the user performing confirmations.                                                                                                                                                         |
| `Vaultwarden.ConfirmUser.MasterPassword`          | string  | Master password of the above user.                                                                                                                                                                          |
| `Vaultwarden.Permissions.Enabled`                 | boolean | Whether to assign permissions to the confirmed user.<br>When enabled, the following permission settings will be applied.                                                                                    |
| `Vaultwarden.Permissions.Role`                    | string  | Role to assign.<br>Valid values: `User`, `Manager`, or `Owner`.                                                                                                                                             |
| `Vaultwarden.Permissions.Collections[].ID`        | string  | ID of the target collection.<br>Mutually exclusive with Name-based specification. If non-empty and not undefined, ID-based selection takes precedence and targets the collection with the matching ID.     |
| `Vaultwarden.Permissions.Collections[].Name`      | string  | Name of the target collection.<br>Used only if no ID is specified. All collections with matching names are targeted.                                                                                        |
| `Vaultwarden.Permissions.Collections[].Privilege` | string  | Access privilege for the collection.<br>Valid values: `Show`, `ShowHidePassword`, `Edit`, `EditHidePassword`, or `Manage`.                                                                                  |
| `Vaultwarden.Client.DeviceName`                   | string  | Display name of the device.<br>This is used by Vaultwarden for recording the connection source.                                                                                                             |
| `Vaultwarden.Client.DeviceIdentifier`             | string  | Device identifier. Same usage as above.                                                                                                                                                                     |

