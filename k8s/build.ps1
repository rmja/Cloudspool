<# .SYNOPSIS #>
param(
    [switch]$All=$False # Build all apps
   ,[switch]$Release=$False
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

foreach ($App in $Apps) {
    Write-Host "Generating ../src/$App/Dockerfile.generated"
    .\process-dockerfile ../src/$App/Dockerfile
}

foreach ($App in $Apps) {
    $AppLower = $App.ToLower()
    $Tag = "rmjac/cloudspool-${AppLower}:latest"
    Write-Host "Building $App with tag $Tag"
    docker build -f "../src/$App/Dockerfile.generated" `
        -t $Tag `
        --build-arg DOTNET_CONFIG=$DotnetConfig `
        ../src

    if (!$?) {
        exit $?
    }
}