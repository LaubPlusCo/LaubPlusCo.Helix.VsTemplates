[newprojectdialog]: https://laubplusco-my.sharepoint.com/personal/anders_laub_laubplusco_net/_layouts/15/guestaccess.aspx?docid=05a5d33f484d2412fa9879d837c14a9aa&authkey=Aab9UNxwW3ZIfC47SA8hjsU "Sitecore Helix Modules & Solutions"

# Sitecore Helix Module & Solution Templates for VisualStudio

Visual Studio extension that accelerates creating new Visual Studio solutions and projects that follow the conventions described in Sitecore Helix.

> THE EXTENSION IS IN PRE-RELEASE MODE. DOCUMENTATION WILL BE UPDATED SOON.    
> 
> *Please send feedback, comments and issues to @anderslaub on the Sitecore community Slack*


## Installation
The extension can be installed via Visual Studio Extensions and Updates

Visual Studio 2015 and 2017 are both supported but the built-in module templates does not support being loaded in Visual Studio 2015. You will need to make your own templates or upgrade VS.

> **Important Note:** You need to run Visual Studio as administrator to use this extension.   
> *Guide to always start Visual Studio as administrator*
> * Right click a shortcut to Visual Studio  
> * Select "Properties" 
> * Click "Open File Location"  
> * Right click "devenv.exe"  
> * Select "Troubleshoot compatibility"  
> * Select "Troubleshoot program"  
> * Check "The program requires additional permissions"  
> * Next, Test and Finish
> * -or install the extension called *RunAsAdmin** 

When the extension has been installed a new project template is available under Visual C# templates.

![alt text][newprojectdialog]

When the template is used for the first time you will be asked to select a root directory for storing your Helix templates. This directory can be changed at any time from the Settings dialog. When an empty root folder is selected you will be asked if you want to install the built-in templates.

[Image]

When selecting File > New Project the extension will presume that you are creating a new solution and filter the available templates so only solution templates are shown.

To add a new module click Add New project in the solution explorer. Note that you do not have to right-click the location you want the module placed. It is the template folder structure that dictates where the module is placed.

[Image]

Always select the solution root folder when creating new modules. The template folder structure ensures that the module is placed correctly and will generate any missing folders.

[Image]




### Creating new templates
The extension comes with a set of built-in examples that can be installed on first use and updated from the settings dialog.

All templates for both solutions and modules are simply folders beneath a selected root folder. The root folder can be changed so different set of templates can be used for different solution setups. 
Template folders includes a manifest file (see description below) that defines the template and how it should be parsed and attached to Visual Studio.

Please share templates that you create that can be used by others. A public Github repo will be made available for this soon.

#### Template manifest
All templates are described in a manifest file, template.manifest.xml. The manifest location is used as the root of the template folder structure.


#### Token replacement

##### Token validation (IValidateToken)

##### Token suggestions (ISuggestToken)

#### The TemplateEngine


## Contributing / Development



All code is released as open source under the ISC license.  
Please remember to include the original license in any derivatives.  
Anders Laub - contact@laubplusco.net