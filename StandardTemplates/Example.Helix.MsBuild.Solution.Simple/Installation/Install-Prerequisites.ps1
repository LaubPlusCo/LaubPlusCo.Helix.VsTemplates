#     Set-PSRepository -Name SitecoreGallery -InstallationPolicy Trusted

# TODO: Check if packages are in assets folder.
# TODO: Check if license file is in assets folder.

$SitecoreGalleryRepo = Get-PSRepository | Where-Object Name -Match "^SitecoreGallery*" | Select-Object

if ($null -eq $SitecoreGalleryRepo) {
    Write-Host "SitecoreGallery PSRepository not already registered.." -ForegroundColor Red
    Register-PSRepository -Name SitecoreGallery -InstallationPolicy Trusted
    Set-PSRepository -Name SitecoreGallery -InstallationPolicy Trusted
    Write-Host "SitecoreGallery PSRepository is now registered" -ForegroundColor Green
}

$error.clear()

Import-Module SitecoreInstallFramework -ErrorAction SilentlyContinue 

if ($error) {
    $error.clear()
    Write-Host "Sitecore Installation Framework not installed.." -ForegroundColor Red
    Install-Module SitecoreInstallFramework -Scope CurrentUser -Repository SitecoreGallery
    if ($error) {
      Write-Host "Could not install Sitecore Installation Framework." -ForegroundColor Red
      return
    }
    Write-Host "Sitecore Installation Framework has been installed" -ForegroundColor Green
}

$SifModule = Get-Module SitecoreInstallFramework

if ($SifModule.Version.ToString() -lt "2.0.0") {
    Write-Host "Old version of Sitecore Installation Framework found.." -ForegroundColor Red
    Remove-Module $SifModule.Name
    Remove-Item $SifModule.ModuleBase -Recurse -Force
    Install-Module SitecoreInstallFramework -Scope CurrentUser -Repository SitecoreGallery
    Write-Host "Updated Sitecore Installation Framework to latest.." -ForegroundColor Green
}

$PreReqParams = Join-Path $PSScriptRoot "Prerequisites.json"

Install-SitecoreConfiguration $PreReqParams | Tee-Object Check-Prerequisites.log

