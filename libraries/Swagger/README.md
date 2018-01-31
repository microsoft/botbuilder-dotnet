# BotFramework Connector

> see https://aka.ms/autorest

Configuration for generating BotFramework Connector SDK.

``` yaml
add-credentials: true
openapi-type: data-plane
```
The current release for the BotFramework Connector is v3.0.

# Releases

## Connector API 3.0

``` yaml
input-file: ConnectorAPI.json
```

### Connector API 3.0 - CSharp Settings
These settings apply only when `--csharp` is specified on the command line.
``` yaml $(csharp)
csharp:
  namespace: Microsoft.Bot.Connector
  override-client-name: ConnectorClient
  use-datetimeoffset: true
  sync-methods: None
  license-header: MICROSOFT_MIT_NO_VERSION
  azure-arm: false
  output-folder: Generated
  clear-output-folder: true
```