
# Get the directory where this script was invoked.
function Get-ScriptDirectory ()
{
    Split-Path $ExecutionContext.SessionState.Module.Path
}


# Build a binary nuget package for this particular version of root.
# This pacakge will download or use the locally installed ROOT, and in MSBUILD
# will correctly setup whatever is required for later use.
#
# Should be run in a location that can deal with creating whatever is needed.
#
function New-ROOTNugetPackage ($rootVersion, $nugetVersion)
{
    # Get the directory ready for the various nuget files.
    $basedir = "$PWD/$rootVersion"
    if (Test-Path $basedir) {
        Remove-Item -Recurse $basedir
    }
    $v = New-Item $basedir -ItemType Directory

    # Create the targets file, and copy them over to the new directory
    $scriptDirectory = Get-ScriptDirectory
    $(Get-Content "$scriptDirectory/ROOT.nuget/FullROOT.nuspec") -replace "5.34.20",$rootVersion -replace "534.20",$nugetVersion > "$basedir/FullROOT.nuspec"
    $(Get-Content "$scriptDirectory/ROOT.nuget/ROOT.props") -replace "5.34.20",$rootVersion | set-content "$basedir/ROOT.props"
	Copy-Item $scriptDirectory/ROOT.nuget/ROOT.targets $basedir/ROOT.targets

    # Run nuget.
    #cd $basedir
    nuget pack $basedir/FullROOT.nuspec
}

# What we will give to the world
Export-ModuleMember New-ROOTNugetPackage