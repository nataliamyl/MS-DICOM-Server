﻿$CurrentDirectory = (pwd).path

$CommonModule = -join($CurrentDirectory, '\', 'Common.psm1')
Import-Module $CommonModule -Force

$txt = '.txt'
$topicName = 'qido'

$ResourceGroup = Read-Host -Prompt 'Input resource group name'
$ConcurrentThreads = Read-Host -Prompt 'Input threads to run simultaneously for upload'

build($QueryGeneratorProject)
for($i = 0; $i -lt $ConcurrentThreads; $i++)
{
	$fileName = -join($CurrentDirectory, '\', $i, $txt)
	$outputFileName = -join($CurrentDirectory, '\', $i, 'queries', $txt)
	Start-Process -FilePath $QueryGeneratorApp -ArgumentList "$fileName $outputFileName" -RedirectStandardError "log.txt"
}

$null = Read-Host 'Press any key to continue once the QueryGenerator processes are completed.'

build($MessageUploaderProject)
for($i = 0; $i -lt $ConcurrentThreads; $i++)
{    
	$fileName = -join($CurrentDirectory, '\', $i, $txt)
    $TotalCount = Get-Content $fileName | Measure-Object –Line
	Start-Process -FilePath $MessageUploaderApp -ArgumentList "$topicName $fileName 0 $TotalCount" -RedirectStandardError "log.txt"
}