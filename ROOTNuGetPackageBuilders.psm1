
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
function New-ROOTNugetPackage ($version)
{
    # Get the directory ready for the various nuget files.
    $basedir = "$PWD/$version"
    if (Test-Path $basedir) {
        Remove-Item -Recurse $basedir
    }
    $v = New-Item $basedir -ItemType Directory

    # Create the targets file
    $scriptDirectory = Get-ScriptDirectory
    $(Get-Content "$scriptDirectory/ROOT.targets") -replace "5.34.20",$version > "$basedir/ROOT-Binaries.targets"
    $(Get-Content "$scriptDirectory/ROOT-Binaries.nuspec") -replace "5.34.20",$version > "$basedir/ROOT-Binaries.nuspec"

    # Run nuget.
    cd $basedir
    nuget pack $scriptDirectory/ROOT-Binaries.nuspec
}

# What we will give to the world
Export-ModuleMember New-ROOTNugetPackage