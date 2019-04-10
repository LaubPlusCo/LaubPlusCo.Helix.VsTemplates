# $projectname$  

Lorem ipsum  

## Local development installation

> _Note;_ the following parameters are needed for installation:
> 
> - Postfix: The solution postfix _default sitecore-9-1.localhost"_  
> - SolrRoot: Path to your local solr home directory _default C:/solr/7-2-1/server/solr_  
>   _The Solr home on a clean Solr install is /{path to solr instance root}/server/solr/_
> - InstanceRootFolder: The instance root folder _default C:/Websites/_  
> - SqlServer: SQL instance name _default ._  
> - SqlAdminUser: SQL instance admin user name _default sif_  
> - SqlAdminPassword: SQL instance admin user password _default SomeSecretSqlPassword_  
> - SolrUrl: Solr URL _default https://localhost:8983/solr_  
> - SitecoreAdminPassword: Password to use for Sitecore admin _default b_  

1. Download Sitecore XP0 installation files and unzip in `.\Installation\Assets`
2. Copy in a valid Sitecore licence.xml to `.\Installation\Assets`
3. Open a Powershell console as administrator
4. If first time setup of Sitecore 9.1 on the local machine
   1. Run `.\Installation\Install-Prerequisites.ps1`
   2. Wait until scripts finishes
   3. Restart Powershell console
5. Run `.\Installation\XP0-SingleDeveloper.ps1` from Powershell console as administrator
   1. When prompted; Type in the values where your local setup parameters defer from the default.
   2. Wait - installation takes approx. 10 min to finish
      - If an error occurs run the generated Uninstall script ex. `.\Installation\Uninstall-sitecore-9-1.localhost.ps1`
      - Fix the cause of the error and run `.\Installation\XP0-SingleDeveloper.ps1` again
6. Open solution in Visual Studio and run a solution Rebuild
7. Open a browser and navigate to `[$postfix]/unicorn.aspx?Verb=Sync` ex. http://sitecore-9-1.localhost/unicorn.aspx?verb=sync
   1. Wait for all Unicorn configurations to be synced
   2. Go to "Control Panel" > "Indexing\Populate Solr Managed Schema" > Select all indexes > Click "Populate"
   3. From the Control panel > Indexing Manager > rebuild all indexes
   4. Run Database\Rebuild the link database
8. Enjoy!

### Uninstall the Instance and xConnect Services

> *BEWARE;* this deletes your local databases. Make sure all your definition item changes has been serialized.

1. Run the uninstall script that was generated during install  
   - ex. `.\Installation\Uninstall-sitecore-9-1.localhost.ps1`
