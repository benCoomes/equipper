FROM mcr.microsoft.com/devcontainers/universal:2-linux

RUN apt-get update

# static web apps emulator
RUN sudo npm install -g @azure/static-web-apps-cli

# docker emulator
RUN docker pull mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator