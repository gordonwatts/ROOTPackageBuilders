ROOTPackageBuilders
===================

Scripts and code to build the ROOT nuget packages (and ROOT.NET packages).

The ROOT nuget pacakge (ROOT and ROOT-Local) allows "easier" building against the C++ ROOT libraries in a C++ project.
It has some nice features - for example, failing with a nice error message if you build an incompatible configuration (e.g. debug).

Using The Packages in your C++ Builds
--------------

1. Create a C++ project (e.g. a win32 console app)
2. Right click on the program, and select "Manage NuGet packages"
3. Enter either "ROOT" or "ROOT-Local" in the search box when the "Online" nuget.org repo is selected.
   -   ROOT: Use this to build against a specific version of ROOT. If the version isn't on your machine, it will be downloaded during the build. Robot (e.g. Jenkins) friendly.
   -   ROOT-Local: Use this to build against whatever you have installed on your machine.
4. Install the former if you want a specific version of root, and want it downloaded. Or the latter if you want to build against whatever is on your machine.
5. Build.
6. To get intellisense working, either restart Visual Studio, or select "Rescan Project" from the "Project" menu after first build.

ROOT: https://www.nuget.org/packages/ROOT/

ROOT-Local: https://www.nuget.org/packages/ROOT-Local/

Using the Packages: Fine Print
------------

No surprise here, it is possible to get yourself into trouble using ROOT. Please read through these caveats
to help you around some common problems:

- ROOT v5 is build only for VS2010, VS2012, and VS2013. These nuget packages only support VS2012 and VS2013 compilers!! If
  you try to use something else you'll get errors. ROOT v6 is not available on windows at the time of this writing.
- ROOT on windows is distributed linked against the optimized DLL version of the standard libraries. This is viral: all of your
  projects must be built the same way. If the compiler options look wrong, you will get a build failure.
- To debug your code, I recommend deleting the Debug configuration, creating a copy of the Release configuration and then
  turning off optimization, and turn on writing out a debug database.
- If you use ACLiC in your ROOT program, make sure that the VC environment variables, etc., are defined in a shell and then start
  devenv. By default, devenv doesn't clutter your program's environment with all those extra defines, and thus ROOT can't find
  the compiler!
- ROOT-Local can't automatically figure out what you have installed (VS2012 or VS2013 version of ROOT). Make sure they match,
  or you will get some weird and unexpected crashes. See issue #18 on github!
- The ROOT nuget package will download ROOT the first time it builds if it isn't installed locally. So you'll need an internet
  connection. And the first time it might take a while!! By default it  stashes it in c:\users\<user>\AppData\Local\Temp\root.
  Delete at anytime, and on the next build it will re-download. Or install it in c:\root, and it will automatically pick it up
  there if it wasn't downloaded.
- After selecting "Rescan Project", give the IDE a minute or so to scan and find all the ROOT files.
- Using the ROOT nuget package, when you run, you have to make sure ROOT is in your PATH. The package will do this for you,
  but the IDE only scans this file when you first open the solution. So you might have to close and re-open the solution the
  first time you do this. The symptom: your program can't load because it is missing some DLL. There isn't (as far as I know) a
  way around this.
- Running with VS2015 installed should work, as long as you first install the VS2013 libraries (get them from the Windows SDK,
  should be free), and then make sure your projects are setup to build using those compilers. Sadly, this means you can't
  take advantage of all the compiler improvements.

Developer Instructions
------------

1. Make sure to build the ROOTMSBuildTasks in release mode.
2. Start powershell
3. import-module ROOTNuGetPackageBUildser.psm1
4. New-ROOTNugetPackage 5.34.32 534.32
5. nuget push ROOT.534.32.11.nupkg

Note the nuget version number is a compressed version. This is because semantic versioning only allows for three version numbers,
and we are using the last one for the nuget packaging.

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
