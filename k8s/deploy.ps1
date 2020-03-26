<# .SYNOPSIS #>
param(
    [switch]$All=$False # Deploy all apps
   ,[string]$Context="docker-desktop"
   ,[string]$Environment="Local"
   ,[Parameter(ValueFromRemainingArguments, Position = 0)][string[]]$Apps
)

Write-Host "Deploying to $Context with $Environment environment"

if ($Context -ne "docker-desktop" -and $Environment -eq "local") {
    $Answer = Read-Host -Prompt "Are you sure that the environment $Environment should be used on $Context? [y/n]"

    if ($Answer -ne "y") {
        return 1;
    }
}

if ($All) {
    $Apps = Get-ChildItem .\Apps -Filter *.yaml | Where-Object {$_.Length -gt 0} | Foreach-Object {$_.BaseName}
}

kubectl apply --context $Context -f Namespace.yaml
kubectl apply --context $Context -f Environments/$Environment
kubectl apply --context $Context -f Services
foreach ($yaml in Get-ChildItem .\Network -Filter *.yaml) {
    (Get-Content $yaml.FullName) -Replace "cloudspool.dk", "cloudspool.localhost" | kubectl apply --context $Context -f -
}

foreach ($App in $Apps) {
    $AppLower = $App.ToLower()
    $Deployment = $AppLower
    Write-Host "Deploying $Deployment"

    $Images = Select-String -Path Apps/$App.yaml -Pattern "image: (rmjac/.*)" | ForEach-Object {$_.Matches.Groups[1].Value}
    foreach ($Image in $Images) {
        docker push $Image
        if (!$?) {
            exit $?;
        }
    }

    kubectl scale --replicas=0 deployment $Deployment -n cloudspool --context $Context # This may fail if this is the first deploy to the cluster
    kubectl apply --context $Context -f Apps/$App.yaml
}