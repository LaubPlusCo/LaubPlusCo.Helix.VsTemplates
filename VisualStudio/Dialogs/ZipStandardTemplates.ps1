param($ProjectDir, $RootDir)

function Zip-TemplateFolders($zipFile, $folderToZip) {
  Remove-Item $zipFile
  $templateFolders = Get-ChildItem $folderToZip | ?{ $_.PSIsContainer } | Select-Object FullName
  foreach ($templateFolder in $templateFolders) { Compress-Archive -Path $templateFolder.FullName -DestinationPath $zipFile -Update }
}

Zip-TemplateFolders "$ProjectDir\\Resources\\ModuleTemplates.zip" "$RootDir\\Modules"
Zip-TemplateFolders "$ProjectDir\\Resources\\SolutionTemplates.zip" "$RootDir\\Solutions"
