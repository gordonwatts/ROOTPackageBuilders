ROOTPackageBuilders
===================

Scripts to build packages (nuget, etc.) for native ROOT libraries.

Instructions
------------

Things to do one time:
Set-ExecutionPolicy RemoteSigned
Install CoApp (http://coapp.org/pages/releases.html) Make sure to follow instructions found in the tutorial: http://coapp.org/tutorials/installation.html - or it won't work until you reboot! :-)

Then from a ps prompt:
PackageROOTForNuget.ps1 ftp://root.cern.ch/root/root_v5.34.18.win32.vc11.tar.gz 1 $env:TEMP/root

