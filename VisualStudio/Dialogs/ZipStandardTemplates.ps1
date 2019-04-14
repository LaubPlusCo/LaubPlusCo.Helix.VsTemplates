param($ProjectDir, $SolutionDir)

function Zip-TemplateFolders($zipFile, $rootDir) {
  Remove-Item $zipFile
  $templateFolders = Get-ChildItem $rootDir | ?{ $_.PSIsContainer } | Select-Object FullName
  foreach ($templateFolder in $templateFolders) { Compress-Archive -Path $templateFolder.FullName -DestinationPath $zipFile -Update }
}

Zip-TemplateFolders "$ProjectDir\\Resources\\ModuleTemplates.zip" "$SolutionDir\\StandardTemplates\\Modules" 
Zip-TemplateFolders "$ProjectDir\\Resources\\SolutionTemplates.zip" "$SolutionDir\\StandardTemplates\\Solutions" 
