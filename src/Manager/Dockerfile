FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["src/Manager/Manager.csproj", "src/Manager/"]
RUN dotnet restore "src/Manager/Manager.csproj"
COPY . .
WORKDIR "/src/src/Manager"
RUN dotnet build "Manager.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Manager.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Manager.dll"]