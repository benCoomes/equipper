# Dev Env Setup
Setup instructions for a development environment, while a proper codespaces devcontainer.json is pending. Microsoft has [documentation](https://learn.microsoft.com/en-us/azure/static-web-apps/local-development) with additional details.

## Essentials
Create a codespace on the main branch.

### Start Cosmos DB Emulator
Start the emulator:
```sh
script/cosmosdb
```

Once started, install the emulator's certificate:
```sh
script/cosmoscert
```

## Running the Static Web App
```sh
script/server
```