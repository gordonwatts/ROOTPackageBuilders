ROOTPackageBuilders
===================

Scripts and code to build the ROOT nuget packages (and ROOT.NET packages).

Using the ROOT NuGet Package
------------

The ROOT nuget pacakge () allows "easier" building against the C++ ROOT libraries in a C++ project.
It has some nice features - for example, failing if you build incorrectly. Usage is a little more complex
than install and run, however:

1. Create a new C++ appliction (a console application, for example).
2. Install the ROOT library from nuget as you would any other nuget package.
3. Build.
   - If you have the same version of ROOT already installed on your computer, everything will just work.
   - If you don't, the first time you build ROOT will be downloaded from the net. Your computer's environment
     will not be altered. You will have to build a second time to pick up the newly downloaded version.
4. Run
   - If you have ROOT already installed, it will just work.
   - If ROOT had to be downloaded, it won't be in your path when you hit build. You can either use the .bat file
     written next to your solution to start visual studio, or use the Debugging pane of the project options to
	 update the PATH variable with "PATH=$(ROOTSYS)\bin;$(PATH)".

Developer Instructions
------------

New:
1. Make sure to build the ROOTMSBuildTasks in release mode.
2. Run nuget on the nuspec file found in ROOT.nuget directory
3. Publish the file.

Old:
Things to do one time:

Set-ExecutionPolicy RemoteSigned
Install CoApp (http://coapp.org/pages/releases.html) Make sure to follow instructions found in the tutorial: http://coapp.org/tutorials/installation.html - or it won't work until you reboot! :-)

Then from a ps prompt:
PackageROOTForNuget.ps1 ftp://root.cern.ch/root/root_v5.34.18.win32.vc11.tar.gz 1 $env:TEMP/root

