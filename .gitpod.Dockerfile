FROM gitpod/workspace-dotnet:latest

USER gitpod

ENV DOTNET_ROOT="/workspace/.dotnet"
ENV PATH=$PATH:$DOTNET_ROOT
ENV DOTNET_CLI_TELEMETRY_OPTOUT=true