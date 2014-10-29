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
	 
--> Rescan Solution (under Project menu) - to update references and includes (automatically happens after 60 minutes).

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

Scenarios
---------

I find it tricky, these MSBUILD files. This is a list of situations that this package should handle correctly when you hit build. You should only have to
hit build once for your code to build (as long as there is internet, etc.).

1. ROOT is not at all installed on the machine anywhere.
2. ROOT has been downloaded and unpacked into the "common area"
3. The incorrect version of ROOT has been installed on your machine.
4. The correct version of ROOT hasg been installed on your machine.

In each case, intellisense should pick up the proper version of ROOT after the first build, and after a rescan of the solution (see Project menu).
Easiest way to check is right-click on a ROOT include file, open it, and then mouse over the open file tab to check the path.

In cases 1, 2, and 3 there should be a .bat file that contains the proper definition of the ROOTSYS variable and modification to PATH.
If 10 projects are built at once, they should not fail because of multiple writes to that file.

