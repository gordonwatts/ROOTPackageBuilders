#
# Given a version of ROOT, and a package version, go ahead and create a set of C++ nuget packages for it.
#
[CmdletBinding()]
param (
  [Parameter(mandatory=$true)]
  [string] $ROOTUrl,
  [Parameter(mandatory=$true)]
  [string] $bundleVersion,
  [Parameter(mandatory=$true)]
  [string] $scratchDir
  )

Import-Module -Force -DisableNameChecking .\ROOTRepositoryAccess

#
# Download the root package from the web.
#
function DownloadFromWeb($url, $local)
{
    if (-not (test-path $local))
    {
        if (-not $url.Contains("//"))
        {
            copy-item $url $local
        }
        else
        {
            Write-Host "Downloading $url"
            $client = new-object System.Net.WebClient
            $client.DownloadFile( $url, $local )
        }
    }
}

#
# Uncompress a file
#
$SevenZipExe = "C:\Program Files\7-Zip\7z.exe"
function Uncompress($path, $logDir)
{
    if (-not $path.EndsWith(".tar.gz"))
    {
        throw "Only know how to uncompress .tar.gz files - $path"
    }

    $logFile = "$logDir/uncompress.log"
    $uncompressFlag = "$path-uncompressed"
    
    if (-not (test-path $uncompressFlag)) {
      Write-Host "Uncompressing $path"
      "Uncompressing $path" | out-file -filepath $logFile -append

      $tarfileName = join-path $(split-path $path) $(split-path -leaf ($path.SubString(0, $path.Length-3)))

      $outputDirectory = split-path $path
      $switch = "-o" + $outputDirectory
      if (-not (test-path $tarfileName)) {
        
        & "$SevenZipExe" $switch x -y $path | out-file -filepath $logFile -append
      }
      if (-not (test-path $tarfileName)) {
        throw "Could not find the tar file $tarfileName after uncompressoing $path"
      }

      & "$SevenZipExe" $switch x -y $tarfileName | out-file -filepath $logfile -append

      $bogus = new-item $uncompressFlag -type file
    }
}

#
# Get the root release, if we haven't already. If the download was interrupted, then
# the below code will likely fail.
#

$versionInfo = $ROOTUrl | parse-root-filename
$baseDirectoryName = join-path $scratchDir (($ROOTUrl -split ".win32")[0] -split "/")[-1]
$localFileName = join-path $baseDirectoryName ($ROOTUrl -split "/")[-1]

if (-not (test-path $baseDirectoryName)) {
    mkdir $baseDirectoryName
}
#set-location $baseDirectoryName
DownloadFromWeb $ROOTUrl $localFileName

#
# Next, unzip it.
#

$rootInstallDir = $baseDirectoryName + "/root";
Uncompress $localFileName $baseDirectoryName

#
# Now, write out the nuget package file.
#

$version = $versionInfo.VersionMajor + "." + $versionInfo.VersionMinor + "." + $versionInfo.VersionSubMinor
$class = $versionInfo.DownloadType

$nugetFile = join-path $baseDirectoryName "ROOT.autopkg"
"nuget {" | out-file $nugetFile
"    nuspec {" | out-file -append $nugetFile
"        id = ROOT-$version-$class;" | out-file -append $nugetFile
"        version = $version.$bundleVersion;" | out-file -append $nugetFile
"        title = ROOT $version ""($class)"";" | out-file -append $nugetFile
"        authors = {CERN};" | out-file -append $nugetFile
"        owners = {G. Watts};" | out-file -append $nugetFile
"        licenseUrl: ""http://root.cern.ch/drupal/content/license"";" | out-file -append $nugetFile
"        projectUrl: ""http://root.cern.ch"";" | out-file -append $nugetFile
"        iconUrl: ""http://root.cern.ch/drupal/sites/default/files/rootdrawing-logo.png"";" | out-file -append $nugetFile
"        requireLicenseAcceptance: false;" | out-file -append $nugetFile
"        summary: ""The complete ROOT data analysis toolkit for C++, for ROOT $version and compiled for $class"";" | out-file -append $nugetFile
"        description: Contains all libraries needed for the full set of ROOT utilities;" | out-file -append $nugetFile
"        releaseNotes: Release $version of ROOT;" | out-file -append $nugetFile
"        tags: data;" | out-file -append $nugetFile
"    };" | out-file -append $nugetFile
"    " | out-file -append $nugetFile
"    files {" | out-file -append $nugetFile
"        include: { ""root\include\*""};" | out-file -append $nugetFile
"        [x86,dynamic] {" | out-file -append $nugetFile
"            lib: { ""root\lib\*.lib"" };" | out-file -append $nugetFile
"            bin: { ""root\bin\*.dll"" };" | out-file -append $nugetFile
"        };" | out-file -append $nugetFile
"    }" | out-file -append $nugetFile
"    " | out-file -append $nugetFile
"    props {" | out-file -append $nugetFile
"        ItemDefinitionGroup.ClCompile.ForcedIncludeFiles += ""w32pragma.h"";" | out-file -append $nugetFile
"    }" | out-file -append $nugetFile
"    " | out-file -append $nugetFile
"}" | out-file -append $nugetFile

#
# And generate the packages!
#

Write-NuGetPackage $nugetFile -PackageDirectory $baseDirectoryName

