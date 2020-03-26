FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR "/src"
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