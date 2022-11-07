# Dev Env Setup
Setup instructions for a development environment, while a proper codespaces devcontainer.json is pending. Microsoft has [documentation](https://learn.microsoft.com/en-us/azure/static-web-apps/local-development) with additional details.

## Essentials
Create default codespace (as of Oct 2022). Should be Ubuntu 20.04 (`cat /etc/os-release` to check)

### Install Function App tools
```sh
wget -q https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install azure-functions-core-tools-4
```

### Install SWA tools
```sh
npm install -g @azure/static-web-apps-cli
```

### Install Cosmos DB Emulator
```sh
docker pull mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator

ipaddr="`ifconfig | grep "inet " | grep -Fv 127.0.0.1 | awk '{print $2}' | head -n 1`"
docker run \
    --publish 8081:8081 \
    --publish 10251-10254:10251-10254 \
    --memory 3g --cpus=2.0 \
    --name=test-linux-emulator \
    --env AZURE_COSMOS_EMULATOR_PARTITION_COUNT=10 \
    --env AZURE_COSMOS_EMULATOR_ENABLE_DATA_PERSISTENCE=true \
    --env AZURE_COSMOS_EMULATOR_IP_ADDRESS_OVERRIDE=$ipaddr \
    --interactive \
    --tty \
    mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator
```

After the emulator is running, execute the following in a new terminal to register the emulator's certificate:
```sh
ipaddr="`ifconfig | grep "inet " | grep -Fv 127.0.0.1 | awk '{print $2}' | head -n 1`"
curl -k https://$ipaddr:8081/_explorer/emulator.pem > ~/emulatorcert.crt
sudo cp ~/emulatorcert.crt /usr/local/share/ca-certificates/
sudo update-ca-certificates
```

TODO: 
* Emulator still does not work with tests, and likely not with a dev instance of the app either:
  > Error Message:
   Test method Coomes.Equipper.CosmosStorage.Test.TokenStorageTests.TokenStorage_AddOrUpdate_UpdatesExistingTokens threw exception: 
   System.Net.Http.HttpRequestException: The SSL connection could not be established, see inner exception. ---> System.Security.Authentication.AuthenticationException: The remote certificate is invalid because of errors in the certificate chain: UntrustedRoot

## Running the Static Web App
```sh
swa start ./Website --api-location ./FunctionApp/
```