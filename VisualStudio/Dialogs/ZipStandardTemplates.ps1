param($ProjectDir, $SolutionDir)

$templatesZipFile = $ProjectDir + "\\Resources\\StandardTemplates.zip"
$standardTemplatesRoot =  $SolutionDir + "\\StandardTemplates\\"
Write-Host $templatesZipFile
Write-Host $standardTemplatesRoot

Remove-Item $templatesZipFile

$templateFolders = Get-ChildItem $SolutionDir\\StandardTemplates\\ | ?{ $_.PSIsContainer } | Select-Object FullName


foreach ($templateFolder in $templateFolders) { Compress-Archive -Path $templateFolder.FullName -DestinationPath $templatesZipFile -Update }

