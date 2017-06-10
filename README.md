# Sitecore Helix Module & Solution Templates for VisualStudio

Visual Studio extension for accelerating creating new Visual Studio solutions and projects so these follow the conventions described by Sitecore Helix.

## Installation
The extension can be installed via Visual Studio Extensions and Updates or by downloading the vsix ([latest version](https://laubplusco-my.sharepoint.com/personal/anders_laub_laubplusco_net/_layouts/15/guestaccess.aspx?docid=0a66fed6f90534b2d8d138f229da9969b&authkey=AYFoh7rn2SYhPMfjsIghqHk)).

Visual Studio 2015 and 2017 are both supported but the built-in module templates does not support being loaded in Visual Studio 2015. You will need to make your own templates or upgrade VS.

> **Note:** You need to run Visual Studio as administrator to use this extension.   
> *Always start Visual Studio as administrator*
> * Right click a shortcut to Visual Studio  
> * Select "Properties" 
> * Click "Open File Location"  
> * Right click "devenv.exe"  
> * Select "Troubleshoot compatibility"  
> * Select "Troubleshoot program"  
> * Check "The program requires additional permissions"  
> * Next, Test and Finish  

When the extension has been installed a new project template is available under Visual C# templates.

[Image]

When the template is used for the first time you will be asked to select a root directory for storing your Helix templates. This directory can be changed at any time from the Settings dialog. When an empty root folder is selected you will be asked if you want to install the built-in templates.

### Creating new templates
The extension comes with a set of built-in examples that can be installed on first use and updated from the settings dialog.

All templates for both solutions and modules are simply folders beneath a selected root folder. The root folder can be changed so different set of templates can be used for different solution setups. 
Template folders includes a manifest file (see description below) that defines the template and how it should be parsed and attached to Visual Studio.

Please share templates that you create that can be used by others. A public Github repo will be made available for this soon.


#### Template manifest
All templates are described in xml format in a manifes, template.manifest.xml. This template should be in the root of the template folder.


[DESCRIPTION OF MANIFEST]

#### Token replacement

##### Token validation (IValidateToken)

##### Token suggestions (ISuggestToken)

#### The TemplateEngine


## Contributing / Development



All code is released as open source under the ISC license. Please remember to include the original license in any derivatives.
Anders Laub - contact@laubplusco.net