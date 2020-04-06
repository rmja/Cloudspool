FROM node:12 AS npm
WORKDIR "/src"
COPY ["Api/package*.json", "./"]
RUN npm install

# alpine cannot run ChakraCore as it requires glibc
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR "/src"
COPY --from=npm /src/node_modules Api/node_modules
COPY ["Api/Api.csproj", "Api/"]
RUN dotnet restore "Api/Api.csproj"
COPY ["Shared", "Shared/"]
COPY . .
WORKDIR "/src/Api"
RUN dotnet build --no-restore "Api.csproj" -c ${DOTNET_CONFIG:-Release} -o /app/build

FROM build AS publish
RUN dotnet publish --no-restore "Api.csproj" -c ${DOTNET_CONFIG:-Release} -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]