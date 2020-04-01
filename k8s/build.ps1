<# .SYNOPSIS #>
param(
    [switch]$All=$False # Build all apps
   ,[switch]$Release=$False
   ,[string]$Tag="latest"
   ,[Parameter(ValueFromRemainingArguments, Position = 0)][string[]]$Apps
)

if ($Release) {
    $DotnetConfig = "Release"
    Write-Host "Using RELEASE configuration"
}
else {
    $DotnetConfig = "Debug"
    Write-Host "Using DEBUG configuration"
}

if ($All) {
    $Apps = Get-ChildItem .\Apps -Filter *.yaml | Foreach-Object {$_.BaseName}
}

if ($Apps.Length -eq 0) {
    Write-Host -ForegroundColor Red "No apps specified, use -All to build all apps"
    return
}

foreach ($App in $Apps) {
    Write-Host "Generating ../src/$App/Dockerfile.generated"
    .\process-dockerfile ../src/$App/Dockerfile
}

foreach ($App in $Apps) {
    $AppLower = $App.ToLower()
    $Image = "rmjac/cloudspool-${AppLower}"
    Write-Host "Building ${Image}:$Tag"
    docker build -f "../src/$App/Dockerfile.generated" `
        -t ${Image}:$Tag `
        --build-arg DOTNET_CONFIG=$DotnetConfig `
        ../src

    if (!$?) {
        exit $?
    }
}