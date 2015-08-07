using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace ROOTMSBuildTasks
{
    /// <summary>
    /// MSBuild Task that will download and unpack a requested version of root.
    /// </summary>
    /// <remarks>
    /// 1) 7-Zip must be installed. This task will fail if it is not installed.
    /// 2) Project clean will not remove a ROOT installation.
    /// 3) If the output directory already exists, this task will do nothing.
    /// </remarks>
    public class DownloadAndUnpackROOT : Task
    {
        /// <summary>
        /// The version of ROOT to be installed. Without any leading v - "5.34.20" for example.
        /// </summary>
        [Required]
        public string Version { get; set; }

        /// <summary>
        /// Location where the gzip file should be downloaded and where it should be unpacked to.
        /// </summary>
        /// <remarks>
        /// When done this directory will contains a gzip file, a tar file, and a directory called "root-[version]" which will contain
        /// the unpacked root installation.
        /// </remarks>
        [Required]
        public string InstallationPath { get; set; }

        /// <summary>
        /// The version of ROOT to download - built for vc10, vc11, etc.
        /// </summary>
        [Required]
        public string VCVersion { get; set; }

        /// <summary>
        /// Contains the path name that the ROOTSYS environment variable should be set to.
        /// </summary>
        [Output]
        public string ROOTSYS { get; private set; }

        /// <summary>
        /// Fetch ROOT, if needed.
        /// </summary>
        /// <returns></returns>
        public override bool Execute()
        {
            // Setup the expected output directory
            ROOTSYS = Path.Combine(new string[]{InstallationPath, string.Format("root-{0}-{1}", Version, VCVersion)});

            // If it exists, then we have nothing to do!
            if (Directory.Exists(ROOTSYS))
            {
                return true;
            }

            // Build the URL, and then download the file.
            string url = string.Format("ftp://root.cern.ch/root/root_v{0}.win32.{1}.tar.gz", Version, VCVersion);
            string filePath = Path.Combine(new string[] { InstallationPath, string.Format("root_v{0}.win32.{1}.tar.gz", Version, VCVersion) });

            if (!File.Exists(filePath))
            {
                // If we are here we are going to have to do the complete download. Log a message first.
                Log.LogMessage(MessageImportance.Low, "Downloading from URL {0} to location {1}", url, filePath);
                Log.LogMessage(MessageImportance.High, string.Format("Downloading ROOT v{0}", Version));

                // Create the directory
                if (!Directory.Exists(InstallationPath))
                {
                    Directory.CreateDirectory(InstallationPath);
                }

                // Do the download
                WebClient webClient = new WebClient();
                try
                {
                    webClient.DownloadFile(url, filePath);
                }
                catch (WebException e)
                {
                    throw new InvalidOperationException(string.Format("Failed to download from {0}.", url), e);
                }
            }

            // Next, unpack the downloaded file.
            Log.LogMessage(MessageImportance.High, string.Format("Unpacking ROOT v{0}", Version));

            // Make sure 7 zip is there.
            string sevenZipPath = @"C:\Program Files\7-Zip\7z.exe";
            if (!File.Exists(sevenZipPath))
            {
                Log.LogError("The 7zip utility to uncompress the ROOT archive must be installed");
                return false;
            }

            // Unpack the gz
            string uncompressCmdArgs = string.Format("x -y {0}", filePath);
            if (!ExecuteCommandLine(sevenZipPath, uncompressCmdArgs))
            {
                return false;
            }

            // Unpack the resulting tar file
            var tarFile = filePath.Replace(".tar.gz", ".tar");
            uncompressCmdArgs = string.Format("x -y {0}", tarFile);
            if (!ExecuteCommandLine(sevenZipPath, uncompressCmdArgs))
            {
                return false;
            }

            // It will be in a directory called "root". We need to rename that.
            var rootInstallLocation = Path.Combine(InstallationPath, "root");
            Directory.Move(rootInstallLocation, ROOTSYS);

            // Remove the tar file, which can be quite large. We'll leave the gz around so that if something
            // happens we don't have to re-download things.
            File.Delete(tarFile);

            return true;
        }

        /// <summary>
        /// Run a command.
        /// </summary>
        /// <param name="exePath"></param>
        /// <param name="args"></param>
        private bool ExecuteCommandLine(string exePath, string args)
        {
            Log.LogCommandLine(string.Format("{0} {1}", exePath, args));
            var proc = new ProcessStartInfo(exePath, args);
            proc.WorkingDirectory = InstallationPath;
            proc.UseShellExecute = false;
            proc.CreateNoWindow = true;
            proc.RedirectStandardOutput = true;

            // Start it up, and then relay all the output we can.
            var pwait = Process.Start(proc);

            Log.LogMessagesFromStream(pwait.StandardOutput, MessageImportance.Normal);

            // Finish up and return.
            pwait.WaitForExit();
            if (pwait.ExitCode != 0)
            {
                Log.LogError(string.Format("7Zip failed with exit code {0}.", pwait.ExitCode));
                return false;
            }
            return true;
        }
    }
}
