param([string]$filewildCard, [string]$version)

#returns FileInfo
function findFile([string]$filewildCard)  {

    $files = @(Get-ChildItem $filewildCard -Recurse);
    if ($files.Length -gt 1) {
        throw "Found $($files.length) files. Stop, we need an unique pattern"
    }
    if ($files.Length -eq 0) {
        throw "Find not found with pattern $filewildCard. Stop"
    }
    return $files[0];
}

function patchAssemblyFileVersion([System.IO.FileInfo]$file , [string]$version) {

    $xmlPath = $file.FullName;

    $xml = [xml](get-content $xmlPath)

    $propertyGroup = $xml.Project.PropertyGroup

    Write-Host "patch $xmlPath to $version"

    # FileVersion = AssemblyFileVersionAttribute
    $propertyGroup[0].FileVersion = $version

    $xml.Save($xmlPath)
}

$file = findFile $filewildCard -ErrorAction Stop 

patchAssemblyFileVersion $file $version -ErrorAction Stop



trap 
{ 
  write-output $_ 
  exit 1 
} 