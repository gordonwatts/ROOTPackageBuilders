ROOTPackageBuilders
===================

Scripts and code to build the ROOT nuget packages (and ROOT.NET packages).

The ROOT nuget pacakge () allows "easier" building against the C++ ROOT libraries in a C++ project.
It has some nice features - for example, failing with a nice error message if you build an incompatible configuration (e.g. debug).

Using the ROOT NuGet Package
------------

Usage is a little more complex than a normal nuget package because ROOT is an installation, not a set of libraries:

1. Install the ROOT library from nuget as you would any other nuget package in your C++ application.
2. Build
   - If you have the same version of ROOT already installed on your computer, everything will just work.
   - If you don't, the first time you build ROOT will be downloaded from the net. Your computer's environment
     will not be altered. ROOT is a large download. This step may take some time!!

Getting Intellisense working (if it isn't by default):

- Select the project you have just built (you must have built at least once), and select "Rescan Project" from the Project menu.
It takes the IDE a minute or so, but it should start working.

Running:

- The build will modify the .user file for your project to set the environment variables that ROOT needs to run properly.
- To get the IDE to recognize them you must close and re-open the solution (or restart the IDE).

Unfortunately, I've found no way around this feature. :(

Developer Instructions
------------

New:
1. Make sure to build the ROOTMSBuildTasks in release mode.
2. Run nuget on the nuspec file found in ROOT.nuget directory
3. Publish the file.

Scenarios
---------

I find it tricky, these MSBUILD files. This is a list of situations that this package should handle correctly when you hit build. You should only have to
hit build once for your code to build (as long as there is internet, etc.). This is a by-hand test matrix for a release.

1. ROOT is not at all installed on the machine anywhere.
2. ROOT has been downloaded and unpacked into the "common area"
3. The incorrect version of ROOT has been installed on your machine.
4. The correct version of ROOT has been installed on your machine, and ROOTSYS is correctly defined.
5. The correct version of ROOT has been installed on your machine, and ROOTSYS is not defined.

In each case, intellisense should pick up the proper version of ROOT after the first build, and after a rescan of the solution (see Project menu).
Easiest way to check is right-click on a ROOT include file, open it, and then mouse over the open file tab to check the path.

In cases 1, 2, and 3 there should be a .bat file that contains the proper definition of the ROOTSYS variable and modification to PATH.
If 10 projects are built at once, they should not fail because of multiple writes to that file.

Waiting for the next step:
--------------
Ignore everything here for now.

Old:
Things to do one time:

Set-ExecutionPolicy RemoteSigned
Install CoApp (http://coapp.org/pages/releases.html) Make sure to follow instructions found in the tutorial: http://coapp.org/tutorials/installation.html - or it won't work until you reboot! :-)

Then from a ps prompt:
PackageROOTForNuget.ps1 ftp://root.cern.ch/root/root_v5.34.18.win32.vc11.tar.gz 1 $env:TEMP/root

