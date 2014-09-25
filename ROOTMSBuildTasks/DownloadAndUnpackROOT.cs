using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
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

            // If we are here we are going to have to do the complete download. Log a message first.
            Log.LogMessage(MessageImportance.High, string.Format("Downloading ROOT v{0}", Version));

            // Build the URL, and then download the file.
            string url = string.Format("ftp://root.cern.ch/root/root_v{0}.win32.{1}.tar.gz", Version, VCVersion);
            string filePath = Path.Combine(new string[] { InstallationPath, string.Format("root_v{0}.win32.{1}.tar.gz", Version, VCVersion) });

            if (!File.Exists(filePath))
            {
                WebClient webClient = new WebClient();
                webClient.DownloadFile(url, filePath);
            }

            Log.LogMessage(MessageImportance.High, string.Format("Unpacking ROOT v{0}", Version));

            return true;
        }
    }
}
