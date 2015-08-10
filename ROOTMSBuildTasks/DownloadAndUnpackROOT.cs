using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
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
        /// Contains everything we need to perform the download
        /// </summary>
        private abstract class DownloadOptions
        {
            /// <summary>
            /// The URL that we will fetch the file from.
            /// </summary>
            public string FileNameOnServer;

            /// <summary>
            /// For handy logging.
            /// </summary>
            public TaskLoggingHelper Log;

            /// <summary>
            /// Download and unpack the file, or make sure it has been done already.
            /// </summary>
            /// <param name="ROOTSYS"></param>
            /// <returns></returns>
            /// <remarks>
            /// The assumption is made that ROOT can't be found, so we will download a new tar/gz file if we need to.
            /// </remarks>
            internal bool DownloadAndUnpack(string ROOTSYS, string installationPath)
            {
                // Get the zip/tar-gz file.
                var localFile = DownloadFileToLocation(installationPath);
                if (localFile == null)
                {
                    return false;
                }

                // Next, unpack the downloaded file.
                Log.LogMessage(MessageImportance.High, "Unpacking ROOT");

                // Make sure 7 zip is there.
                string sevenZipPath = @"C:\Program Files\7-Zip\7z.exe";
                if (!File.Exists(sevenZipPath))
                {
                    Log.LogError("The 7zip utility to uncompress the ROOT archive must be installed");
                    return false;
                }

                Unpack(sevenZipPath, localFile, installationPath);

                // It will be in a directory called "root". We need to rename that.
                var rootInstallLocation = Path.Combine(installationPath, "root");
                Directory.Move(rootInstallLocation, ROOTSYS);

                return true;
            }

            /// <summary>
            /// Unpack the given file. Use wd as a place to actually execute!
            /// </summary>
            /// <param name="sevenZipPath"></param>
            /// <param name="filepath"></param>
            /// <param name="wd"></param>
            /// <returns></returns>
            protected abstract bool Unpack(string sevenZipPath, string filepath, string wd);

            /// <summary>
            /// Make sure the compressed file is there.
            /// </summary>
            /// <param name="installationPath"></param>
            /// <returns></returns>
            private string DownloadFileToLocation(string installationPath)
            {
                // Download the file if it hasn't already been.
                var filePath = Path.Combine(new string[] { installationPath, FileNameOnServer });
                if (!File.Exists(filePath))
                {
                    var url = string.Format("ftp://root.cern.ch/root/{0}", FileNameOnServer);

                    Log.LogMessage(MessageImportance.Low, "Downloading from URL {0} to location {1}", url, filePath);
                    Log.LogMessage(MessageImportance.High, string.Format("Downloading ROOT ({0})", FileNameOnServer));

                    if (!Directory.Exists(installationPath))
                    {
                        Directory.CreateDirectory(installationPath);
                    }

                    // Do the download
                    WebClient webClient = new WebClient();
                    try
                    {
                        webClient.DownloadFile(url, filePath);
                    }
                    catch (WebException)
                    {
                        return null;
                    }
                }
                return filePath;
            }
        }

        /// <summary>
        /// Fetch ROOT, if needed.
        /// </summary>
        /// <returns></returns>
        public override bool Execute()
        {
            // Setup the expected output directory
            ROOTSYS = Path.Combine(new string[] { InstallationPath, string.Format("root-{0}-{1}", Version, VCVersion) });

            // If it exists, then we have nothing to do!
            if (Directory.Exists(ROOTSYS))
            {
                return true;
            }

            // Try the .zip file first, and if that fails, try the .tar.gz file second.
            var downloads = new List<DownloadOptions>();
            downloads.Add(DownloadFromZip());
            downloads.Add(DownloadFromTarGz());

            foreach (var d in downloads)
            {
                d.Log = Log;
            }

            // Do the download. Hopefully someone will get it right.
            var gotit = downloads
                .Select(d => d.DownloadAndUnpack(ROOTSYS, InstallationPath))
                .Where(r => r == true)
                .FirstOrDefault();

            // If we fail, then we should let everyone know.
            if (!gotit)
            {
                var err = new StringBuilder();
                err.AppendLine("Unable to download ROOT. Tried the following: ");
                foreach (var d in downloads)
                {
                    err.AppendLine("   ftp://root.cern.ch/root/" + d.FileNameOnServer);
                }
                Log.LogError(err.ToString());
                return false;
            }

            return true;

        }

        /// <summary>
        /// Download from a tar/gz location.
        /// </summary>
        class DownloadOptionsTarGz : DownloadOptions
        {
            /// <summary>
            /// Unpack a tar/gz file properly
            /// </summary>
            /// <param name="sevenZipPath"></param>
            /// <param name="filepath"></param>
            /// <returns></returns>
            protected override bool Unpack(string sevenZipPath, string filePath, string wd)
            {
                // Unpack the gz
                string uncompressCmdArgs = string.Format("x -y {0}", filePath);
                if (!ExecuteCommandLine(Log, wd, sevenZipPath, uncompressCmdArgs))
                {
                    return false;
                }

                // Unpack the resulting tar file
                var tarFile = filePath.Replace(".tar.gz", ".tar");
                uncompressCmdArgs = string.Format("x -y {0}", tarFile);
                if (!ExecuteCommandLine(Log, wd, sevenZipPath, uncompressCmdArgs))
                {
                    return false;
                }

                // Remove the tar file, which can be quite large. We'll leave the gz around so that if something
                // happens we don't have to re-download things.
                File.Delete(tarFile);

                return true;
            }
        }


        /// <summary>
        /// Create a download options that will fetch and unpack a tar/gz file.
        /// </summary>
        /// <returns></returns>
        private DownloadOptions DownloadFromTarGz()
        {
            return new DownloadOptionsTarGz()
            {
                FileNameOnServer = string.Format("root_v{0}.win32.{1}.tar.gz", Version, VCVersion)
            };
        }

        /// <summary>
        /// Deal with a ZIP file style ROOT download
        /// </summary>
        class DownloadOptionsZIP : DownloadOptions
        {
            protected override bool Unpack(string sevenZipPath, string filepath, string wd)
            {
                // Unpack the zip
                string uncompressCmdArgs = string.Format("x -y {0}", filepath);
                if (!ExecuteCommandLine(Log, wd, sevenZipPath, uncompressCmdArgs))
                {
                    return false;
                }
                return true;
            }
        }


        /// <summary>
        /// Create a download options that will fetch and download a ZIP file.
        /// </summary>
        /// <returns></returns>
        private DownloadOptions DownloadFromZip()
        {
            return new DownloadOptionsZIP()
            {
                FileNameOnServer = string.Format("root_v{0}.win32.{1}.zip", Version, VCVersion)
            };
        }

        /// <summary>
        /// Run a command.
        /// </summary>
        /// <param name="exePath"></param>
        /// <param name="args"></param>
        private static bool ExecuteCommandLine(TaskLoggingHelper Log, string wd, string exePath, string args)
        {
            Log.LogCommandLine(string.Format("{0} {1}", exePath, args));
            var proc = new ProcessStartInfo(exePath, args);
            proc.WorkingDirectory = wd;
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
