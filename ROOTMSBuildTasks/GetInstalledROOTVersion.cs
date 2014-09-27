using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ROOTMSBuildTasks
{
    /// <summary>
    /// MSBuild task that will return "none" or the version number "5.34.01" for whatever current
    /// is the current version of ROOT.
    /// 
    /// Look for the ROOT version number in the following locations:
    ///   1) ROOTSYS env var
    ///   2) c:\root
    ///   
    /// To find the version number given one of the above directories is found:
    /// 
    ///   1) etc/gitinfo.txt, the first line, with the leading "v" stripped off.
    ///   
    /// If either of these things fails, then the string "notfound" is returned. 
    /// </summary>
    public class GetInstalledROOTVersion : Task
    {
        /// <summary>
        /// Returns the current version of ROOT installed on the machine.
        /// </summary>
        /// <remarks>
        /// Set to "nofound" if no version of ROOT was found.
        /// </remarks>
        [Output]
        public string Version { get; private set; }

        /// <summary>
        /// The ROOTSYS that points to the installed ROOT we found. Only contains
        /// something sensible if Version is set to soemthing other than "notfound".
        /// </summary>
        [Output]
        public string ROOTSYS { get; private set; }

        /// <summary>
        /// Look for the ROOT install and the version number.
        /// </summary>
        /// <returns>Always return success</returns>
        public override bool Execute()
        {
            Version = "notfound";
            ROOTSYS = "";

            // Try the ROOTSYS number first.
            var rootsys = Environment.GetEnvironmentVariable("ROOTSYS");
            if (!directory_ok(rootsys))
            {
                rootsys = @"c:\root";
                if (!directory_ok(rootsys))
                    return true;
            }

            // Extract the version number.
            var v = get_git_version(rootsys);
            if (v != null)
            {
                Version = v;
                ROOTSYS = rootsys;
                return true;
            }
            return true;
        }

        /// <summary>
        /// Given rootsys, look for the version number in a git file.
        /// </summary>
        /// <param name="rootsys"></param>
        /// <returns></returns>
        private string get_git_version(string rootsys)
        {
            var gitv_file = Path.Combine(new string[] { rootsys, @"etc\gitinfo.txt" });
            if (!File.Exists(gitv_file))
                return null;

            var vl = File.ReadAllLines(gitv_file);
            if (vl.Length < 1)
                return null;
            if (!vl[0].StartsWith("v"))
                return null;
            return vl[0].Substring(1).Replace("-",".");
        }

        /// <summary>
        /// Is this a directory we might use to look for ROOT?
        /// </summary>
        /// <param name="rootsys"></param>
        /// <returns></returns>
        private bool directory_ok(string rootsys)
        {
            return !string.IsNullOrWhiteSpace(rootsys) && Directory.Exists(rootsys);
        }
    }
}
