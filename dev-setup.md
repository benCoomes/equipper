# Dev Env Setup
Setup instructions for a development environment, while a proper codespaces devcontainer.json is pending. 

## Essentials
Create default codespace (as of Oct 2022). Should be Ubuntu 20.04 (`cat /etc/os-release` to check)

Install Function App tools
```sh
wget -q https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install azure-functions-core-tools-4
```

Install SWA tools
```sh
npm install -g @azure/static-web-apps-cli
```

TODO: 
* Install cosmos emulator or container for tests

## Running the Static Web App
```sh
swa start ./Website --api-location ./FunctionApp/
```