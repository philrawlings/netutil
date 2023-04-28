New-Variable -Name "projectFolder" -Value (Join-Path (Resolve-Path ..) 'src/NetUtil')
$xml = [Xml] (Get-Content $projectFolder\NetUtil.csproj)
$version = [Version] $xml.Project.PropertyGroup.Version
New-Variable -Name "publishFolder" -Value (Join-Path (Resolve-Path ..) 'builds/global-tool' $version)

# Remove destination folder if exists
if(Test-Path $publishFolder -PathType Container) { 
    rm -r $publishFolder
}

# Publish application
Write-Host "Publishing project..." -ForegroundColor yellow
Write-Host -> $publishFolder -ForegroundColor DarkYellow
dotnet pack $projectFolder/NetUtil.csproj --output $publishFolder
if ($LastExitCode -ne 0) { break }
Write-Host ""

Write-Host Build Complete -ForegroundColor green
Write-Host -> $publishFolder -ForegroundColor DarkYellow