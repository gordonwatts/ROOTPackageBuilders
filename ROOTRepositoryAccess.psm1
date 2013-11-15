#
# Some functions to get access to the
# stuff in the ROOT repository.
#
# G. Watts
#

filter extract-root-link
{
	return $_.href
}

#
# Parse a root version number and return the bits
#
function Parse-ROOT-Version ($rootVersion)
{
	$vinfo = $rootVersion.Split(".")
	if ($vinfo.length -ne 3)
	{
		throw New-Object System.ArgumentException "Root version $rootVersion is not a legal version number like 5.33.02-rc1"
	}
	
	$r = New-Object System.Object
	$r | Add-Member -type NoteProperty -Name "VersionMajor" -Value $vinfo[0]
	$r | Add-Member -type NoteProperty -Name "VersionMinor" -Value $vinfo[1]
	
	$v3Extra = $vinfo[2].SubString(2)
	$v3 = $vinfo[2].SubString(0,2)
	$r | Add-Member -type NoteProperty -Name "VersionSubMinor" -Value $v3
	$r | Add-Member -type NoteProperty -Name "VersionExtraInfo" -Value $v3Extra

	return $r
}

# Given a ftp to a root file, parse out version, etc. numbers
# from it, and turn it into an object we can use further on down the pipe-line.
filter parse-root-filename
{
	# The last bit of the ftp guy is the name of the file.
	$s = $_.Split("/")
	$fname = $s[$s.length-1]

	if ($fname.StartsWith("root_v")) {
		$mainInfo = $fname.SubString(6).Split('.')
		
		# Sometimes v3 contains extra info...
		$v3 = $mainInfo[2]
		$v3Extra = $v3.SubString(2)
		$v3 = $v3.SubString(0,2)
		
		# Next, figure out how many items are the file type (tar.gz, or msi...)		
		$lastindex = $mainInfo.length-1
		$ftype = $mainInfo[$lastindex]
		if ($mainInfo[$lastindex] -eq "gz")
		{
			$lastindex = $lastindex - 2
			$ftype = "tar.gz"
		}

		$downloadType = [string]::Join(".", $mainInfo[3..$lastindex])
		
		$r = New-Object System.Object
		$r | Add-Member -type NoteProperty -Name "VersionMajor" -Value $mainInfo[0]
		$r | Add-Member -type NoteProperty -Name "VersionMinor" -Value $mainInfo[1]
		$r | Add-Member -type NoteProperty -Name "VersionSubMinor" -Value $v3
		$r | Add-Member -type NoteProperty -Name "VersionExtraInfo" -Value $v3Extra
		$r | Add-Member -type NoteProperty -Name "DownloadType" -Value $downloadType
		$r | Add-Member -type NoteProperty -Name "FileType" -Value $ftype
		$r | Add-Member -type NoteProperty -Name "URL" -Value $_
	
		
		return $r
	}
}

#
# filter that returns an array of matches for a givin input string. Use % {$_} to flatten
# the array that comes back.
filter get-matches ([string] $regex = “”) 
{ 
   $returnMatches = new-object System.Collections.ArrayList 

   $resultingMatches = [Regex]::Matches($_, $regex, “IgnoreCase”) 
   foreach($match in $resultingMatches)  
   {  
      [void] $returnMatches.Add($match.Groups[1].Value.Trim()) 
   } 

   return $returnMatches    
} 

#
# Urls in web pages need some fixing up to make them usable out of the context of the webpage.
# Given where we found the web page, do the work.
#
filter fixup-relative-url ($domain, $base)
{
	if ($_.IndexOf("://") -gt 0)
	{
		return $_
	}
	if ($_[0] -eq "/")
	{
		return "$domain$_"
	}

	return "$base$_".Replace("/./", "/")
}

#
# Look at html and extract the a href, and from there, put the links
# together and return them as is.
#
filter parse-html-for-links ($hpath)
{
  $base = $hpath.Substring(0,$hpath.LastIndexOf(“/”) + 1) 
  $baseSlash = $base.IndexOf(“/”, $base.IndexOf(“://”) + 3) 
  $domain = $base.Substring(0, $baseSlash) 
   
  $regex = “<\s*a\s*[^>]*?href\s*=\s*[`"']*([^`"'>]+)[^>]*?>” 
  $allmatches = $_ | get-matches $regex | % {$_} | fixup-relative-url $domain $base
  return $allmatches
}

#
# Given a ftp listing, grab all the links and
# build soemthing nice out of them.
#
filter parse-ftp-dirlisting ($rootPath)
{
	$info = $_ -split " +"
	return "$rootPath/$($info[8])"
}

#
# Returns all versions of root that are on the server currently.
#
function Get-All-ROOT-Downloads ($htmlPath = "ftp://root.cern.ch/root")
{
	#
	# Use ftp in .NET to download the listing.
	#
	
	$ftp = [Net.WebRequest]::Create($htmlPath)
	$ftp.Method = [Net.WebRequestMethods+Ftp]::ListDirectoryDetails
	$response = $ftp.GetResponse()
	$stream = $response.GetResponseStream()
	$reader = New-Object IO.StreamReader $stream
	$links = $reader.ReadToEnd()
	$reader.Close()
	$response.Close()

	#
	# Pull it into seperate objects that should allow us to easily go after things.
	#
	
	$list = $links -split "\r\n" | parse-ftp-dirlisting $htmlPath
	return $list | ? {$_} | parse-root-filename
}

function Version-Not-In-Group ($rVersion, $groupVersions)
{
    $found = $groupVersions | ? { $_.Name -eq "$($rVersion.VersionMajor), $($rVersion.VersionMinor), $($rVersion.VersionSubMinor)" }

    return $found
}

#
# Return $v1 >= $v2
#
function Greater-Than-Equal-Version ($v1, $v2)
{
    if ([int] $v1.VersionMajor -gt [int] $v2.VersionMajor)
    {
        return $True
    }
    if ([int] $v1.VersionMajor -ne [int] $v2.VersionMajor)
    {
        return $False
    }

    if ([int] $v1.VersionMinor -gt [int] $v2.VersionMinor)
    {
        return $True
    }
    if ([int] $v1.VersionMinor -ne [int] $v2.VersionMinor)
    {
        return $False
    }

    if ([int] $v1.VersionSubMinor -gt [int] $v2.VersionSubMinor)
    {
        return $True
    }
    if ([int] $v1.VersionSubMinor -ne [int] $v2.VersionSubMinor)
    {
        return $False
    }

    return $True
}

function Get-Version-As-Int ($v)
{
    return $v.VersionMajor*10000 + $v.VersionMinor*100 + $v.VersionSubMinor
}

#
# Keep the best in the list (highest)
#
function Keep-Best-Version ($vList)
{
    $s = $vList | Sort-Object VersionSubMinor
    $last = 0
    foreach($i in $s)
    {
        $last = $i.VersionSubMinor
    }
    return $last
}

#
# Get the list of versions that are the same or
# larger than the version passed in.
#
# For even releases: do everything
# For odd releases: do only the latest in the series
# For a "-rc" release, only do the most recent if there
#  is no non-rc release.
#
function Get-Subsequent-Releases ($minROOTVersion)
{
    $minVersion = Parse-ROOT-Version($minROOTVersion)
    $allVersions = Get-All-ROOT-Downloads

    #
    # Only look at versions that are better or equal to what we care about.
    #

    $onlyGoodVersions = $allVersions | ? {Greater-Than-Equal-Version $_ $minVersion}

    #
    # Next, we need a list of all versions that don't have a -rc component.
    #

    $nonExtraVersions = $onlyGoodVersions | ? {-not $_.VersionExtraInfo}
    $nonExtraVersionsGroup = $nonExtraVersions | Group-Object Get-Version-As-Int

    $extraVersions = $onlyGoodVersions | ? {$_.VersionExtraInfo}

    $okExtraVersions = $extraVersions | ? {Version-Not-In-Group $_ $nonExtraVersionsGroup}

    #
    # Now, in the non-extension versions, find the odd ones, and grab the most recent
    # guys only for each download.
    #

    $nonOddVersions = $nonExtraVersions | ? {$_.VersionMinor % 2 -eq 0}
    $oddVersions = $nonExtraVersions | ? {($_.VersionMinor % 2) -eq 1}

    $oodVersionsGrouped = $oddVersions | Group-Object DownloadType,VersionMajor,VersionMinor

    $okOddVersions = @()
    foreach ($odd in $oodVersionsGrouped)
    {
        $last = Keep-Best-Version $odd.Group

        $arrs = $odd.Group | ? {$_.VersionSubMinor -eq $last}
        $okOddVersions = $okOddVersions + $arrs
    }

    return $nonOddVersions + $okOddVersions + $okExtraVersions
}

Export-ModuleMember -Function Get-All-ROOT-Downloads
Export-ModuleMember -Function Parse-ROOT-Version
Export-ModuleMember -Function Get-Subsequent-Releases
Export-ModuleMember -Function parse-root-filename

#getAllROOTVersions | Format-Table
#"ftp://root.cern.ch/root/root_v5.33.02.win32.vc10.debug.msi" | parse-root-filename | Format-Table
#"ftp://root.cern.ch/root/root_v5.32.00-rc2.source.tar.gz" | parse-root-filename | Format-Table
#"ftp://root.cern.ch/root/root_v5.28.00g.win32.vc90.tar.gz" | parse-root-filename | Format-Table
#"ftp://root.cern.ch/root/root_v5.32.00.macosx106-i386-gcc-4.2.tar.gz" | parse-root-filename | Format-Table