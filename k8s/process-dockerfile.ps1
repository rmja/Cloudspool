Function Get-ProjectReferences($csprojFile) {
    $csproj = Get-Item $csprojFile
    $projectXml = [xml](Get-Content $csproj)
    $dependencies = $projectXml.Project.ItemGroup.ProjectReference | ForEach-Object {$_.Include} | Where-Object {$_}

    return $dependencies | ForEach-Object {Get-Item (Join-Path -Resolve $csproj.DirectoryName $_)}
}

Function Get-ProjectReferencesRecursive($csprojFile, [ref]$traversed = ([ref]@())) {
    $initLength = $traversed.value.Length
    $csproj = Get-Item $csprojFile
    $traversed.value += $csproj.FullName
    $dependencies = Get-ProjectReferences $csproj

    foreach ($dependency in $dependencies) {
        if (!$traversed.value.Contains($dependency.FullName)) {
            Get-ProjectReferencesRecursive $dependency $traversed
        }
    }

    if ($initLength -eq 0) {
        # Remove the first traversed item which is the starting point csproj file
        return $traversed.value | Select-Object -Skip 1 | ForEach-Object {Get-Item $_}
    }
}

$dockerfile = $args[0]
$srcroot = "../src"
$allCopyLines = @()
$allProjects = @()

$lines = @()
foreach ($line in Get-Content $dockerfile) {
    if ($line -match "^RUN dotnet restore ""(.*/.*\.csproj)""$") {
        $csprojFile = Join-Path -Resolve $srcroot $Matches.1
        $dependencyProjects = Get-ProjectReferencesRecursive $csprojFile | ForEach-Object {$_.BaseName}
        $dependencyCopyLines = $dependencyProjects | ForEach-Object { "COPY [""$_/$_.csproj"", ""$_/""]" }
        $lines += "COPY [""Directory.Build.props"", ""/""]"
        $lines += $dependencyCopyLines
        $lines += $line

        $allCopyLines += $dependencyCopyLines
        $allProjects += $dependencyProjects
        $allProjects += (Get-Item $csprojFile).BaseName
    }
    elseif ($line -eq "COPY . .") {
        $lines += $allProjects | ForEach-Object { "COPY [""$_/"", ""$_/""]" }
    }
    elseif (!$allCopyLines.Contains($line)) {
        $lines += $line
    }
}

Set-Content "$dockerfile.generated" $lines