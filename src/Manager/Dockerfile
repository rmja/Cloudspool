FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR "/src"
COPY ["Manager/Manager.csproj", "Manager/"]
RUN dotnet restore "Manager/Manager.csproj"
COPY . .
WORKDIR "/src/Manager"
RUN dotnet build --no-restore "Manager.csproj" -c ${DOTNET_CONFIG:-Release} -o /app/build

FROM build AS publish
RUN dotnet publish --no-restore "Manager.csproj" -c ${DOTNET_CONFIG:-Release} -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Manager.dll"]