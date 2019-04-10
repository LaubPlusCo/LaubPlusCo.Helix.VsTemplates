# TODO: Move getting params to separate psm
function Read-ValueFromPrompt($Text, $DefaultValue) {
    if (($InputValue = Read-Host -Prompt "$($Text) (press enter for default: [$($DefaultValue)])") -eq "") { $InputValue = $DefaultValue }
    Write-Host ""
    return $InputValue;
}

do {
    # The prefix that will be used on the SOLR, Website, and Database instances.
    $Postfix = Read-ValueFromPrompt "Type solution postfix or press enter" "sitecore-9-1.localhost"
    # The folder that Solr has been installed in.
    $SolrRoot = Read-ValueFromPrompt "Type the full path to your local solr_home directory" "C:\solr\7-2-1\server\solr"
    # Root folder for instances
    $InstanceRootFolder = Read-ValueFromPrompt "Type the instance root folder" "C:\Websites\"
    # Root folder for instances
    $SqlServer = Read-ValueFromPrompt "Type name of SQL instance" "."
    # Root folder for instances
    $SqlAdminUser = Read-ValueFromPrompt "Type the SQL instance admin user name" "sif"
    # Root folder for instances
    $SqlAdminPassword = Read-ValueFromPrompt "Type the SQL instance admin user password" "SomeSecretSqlPassword"
    # The URL of the Solr Server.
    $SolrUrl = Read-ValueFromPrompt "Type the Solr URL" "https://localhost:8983/solr"
    # Sitecore admin password
    $SitecoreAdminPassword = Read-ValueFromPrompt "Type the password to use for Sitecore admin" "b"

    Write-Host "[ Review input before install ]`n" -ForegroundColor DarkRed -BackgroundColor white

    Write-Host @"
Solution postfix: $Postfix
solr_home directory: $SolrRoot
Instance root folder: $InstanceRootFolder
Sql server: $SqlServer
Sql Admin user: $SqlAdminUser
Sql Admin password: $SqlAdminPassword
Solr URL: $SolrUrl
Sitecore admin password: $SitecoreAdminPassword
***********

"@ -ForegroundColor DarkCyan -BackgroundColor white

} while (-not( ($valid=(Read-Host "Is the above correct y/n, x exits..")) -match "[yYxX]"))
if ($valid -match "[xX]") { return }

# The root folder with the license file and the WDP files.
$SCInstallRoot = $PSScriptRoot
# The name of the XConnect service.
$XConnectSiteName = "xc-$Postfix"
# The Sitecore site instance name.
$SitecoreSiteName = "$Postfix"
# The Identity Server site name.
$IdentityServerSiteName = "id-$Postfix"
# The path to the license file.
$LicenseFile = "$SCInstallRoot\Assets\license.xml"

$WebsiteRootFolder = Join-Path $InstanceRootFolder "$Postfix\Website"
$XConnectRootFolder = Join-Path $InstanceRootFolder "$Postfix\XConnect"
$IdentityRootFolder = Join-Path $InstanceRootFolder "$Postfix\Identity"

# The path to the XConnect Package to Deploy.
$XConnectPackage = (Get-ChildItem "$SCInstallRoot\Assets\Sitecore 9.1.0 rev. * (OnPrem)_xp0xconnect.scwdp.zip").FullName
# The path to the Sitecore Package to Deploy.
$SitecorePackage = (Get-ChildItem "$SCInstallRoot\Assets\Sitecore 9.1.0 rev. * (OnPrem)_single.scwdp.zip").FullName
# The path to the Identity Server Package to Deploy.
$IdentityServerPackage = (Get-ChildItem "$SCInstallRoot\Assets\Sitecore.IdentityServer 2.0.0 rev. * (OnPrem)_identityserver.scwdp.zip").FullName
# The Identity Server password recovery URL, this should be the URL of the CM Instance
$PasswordRecoveryUrl = "http://$SitecoreSiteName"
# The URL of the Identity Server
$SitecoreIdentityAuthority = "https://$IdentityServerSiteName"
# The URL of the XconnectService
$XConnectCollectionService = "https://$XConnectSiteName"
# The random string key used for establishing connection with IdentityService. This will be regenerated if left on the default.
$ClientSecret = "SIF-Default"
# Pipe-separated list of instances (URIs) that are allowed to login via Sitecore Identity.
$AllowedCorsOrigins = "http://$SitecoreSiteName"

# Install XP0 via combined partials file.
$singleDeveloperParams = @{
    Path                          = "$SCInstallRoot\XP0-SingleDeveloper.json"
    SqlServer                     = $SqlServer
    SqlAdminUser                  = $SqlAdminUser
    SqlAdminPassword              = $SqlAdminPassword
    SitecoreAdminPassword         = $SitecoreAdminPassword
    SolrUrl                       = $SolrUrl
    SolrRoot                      = $SolrRoot
    Prefix                        = $Postfix
    XConnectCertificateName       = $XConnectSiteName
    IdentityServerCertificateName = $IdentityServerSiteName
    IdentityServerSiteName        = $IdentityServerSiteName
    LicenseFile                   = $LicenseFile
    XConnectPackage               = $XConnectPackage
    SitecorePackage               = $SitecorePackage
    IdentityServerPackage         = $IdentityServerPackage
    XConnectSiteName              = $XConnectSiteName
    SitecoreSitename              = $SitecoreSiteName
    PasswordRecoveryUrl           = $PasswordRecoveryUrl
    SitecoreIdentityAuthority     = $SitecoreIdentityAuthority
    XConnectCollectionService     = $XConnectCollectionService
    ClientSecret                  = $ClientSecret
    AllowedCorsOrigins            = $AllowedCorsOrigins
    WebsiteRootFolder             = $WebsiteRootFolder
    XConnectRootFolder            = $XConnectRootFolder
    IdentityRootFolder            = $IdentityRootFolder
}

# Write uninstall script
$UninstallScriptPath = Join-Path $PSScriptRoot "Uninstall-$Postfix.ps1"
$PropsAsString = "@{`n"
$singleDeveloperParams.GetEnumerator() | ForEach-Object { $PropsAsString += "$($_.key)=`"$($_.value)`"`n" }
$PropsAsString += "}"
(Get-Content "$SCInstallRoot\Assets\Uninstall.ps1.template").Replace("[ParamsObject]", $PropsAsString) | Set-Content $UninstallScriptPath

Push-Location $SCInstallRoot

Install-SitecoreConfiguration @singleDeveloperParams *>&1 | Tee-Object XP0-SingleDeveloper.log

Pop-Location


# TODO: Move below to separate ps1 module...

# Update serialization config source folder
$SolutionDir = (Get-Item $PSScriptRoot ).parent.FullName
$SourceFolder = Join-Path $SolutionDir "/src/"
$SerializationConfigPath = Join-Path $WebsiteRootFolder "/App_Config/Include/z.Foundation.Serialization.SourceFolder.config"
(Get-Content "$SCInstallRoot\Assets\z.Foundation.Serialization.SourceFolder.config.template").Replace("[SourceFolder]", $SourceFolder) | Set-Content $SerializationConfigPath

# Update hostName variable used in site definitions - Note; all sites use virtual folders so share host name.
$HostNameConfigPath = Join-Path $WebsiteRootFolder "/App_Config/Include/z.Environment.Hostname.config"
(Get-Content "$SCInstallRoot\Assets\z.Environment.Hostname.config.template").Replace("[Hostname]", $SitecoreSiteName) | Set-Content $HostNameConfigPath

# Write user publish properties file to build/props
$UserPropsFile = Join-Path $SolutionDir "/Build/props/Publish.Properties.props.user"
(Get-Content "$SCInstallRoot\Assets\Publish.Properties.props.user.template").Replace("[WebsiteRootFolder]", $WebsiteRootFolder) | Set-Content $UserPropsFile