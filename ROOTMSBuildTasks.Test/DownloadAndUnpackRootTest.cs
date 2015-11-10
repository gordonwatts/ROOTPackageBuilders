using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace ROOTMSBuildTasks.Test
{
    /// <summary>
    /// Warning: these tests can take a very long time, as they download a lot from the internet!!
    /// </summary>
    [TestClass]
    public class DownloadAndUnpackRootTest
    {
        private class dummyBuildEngine : Microsoft.Build.Framework.IBuildEngine4
        {
            public object GetRegisteredTaskObject(object key, Microsoft.Build.Framework.RegisteredTaskObjectLifetime lifetime)
            {
                throw new NotImplementedException();
            }

            public void RegisterTaskObject(object key, object obj, Microsoft.Build.Framework.RegisteredTaskObjectLifetime lifetime, bool allowEarlyCollection)
            {
                throw new NotImplementedException();
            }

            public object UnregisterTaskObject(object key, Microsoft.Build.Framework.RegisteredTaskObjectLifetime lifetime)
            {
                throw new NotImplementedException();
            }

            public Microsoft.Build.Framework.BuildEngineResult BuildProjectFilesInParallel(string[] projectFileNames, string[] targetNames, System.Collections.IDictionary[] globalProperties, System.Collections.Generic.IList<string>[] removeGlobalProperties, string[] toolsVersion, bool returnTargetOutputs)
            {
                throw new NotImplementedException();
            }

            public void Reacquire()
            {
                throw new NotImplementedException();
            }

            public void Yield()
            {
                throw new NotImplementedException();
            }

            public bool BuildProjectFile(string projectFileName, string[] targetNames, System.Collections.IDictionary globalProperties, System.Collections.IDictionary targetOutputs, string toolsVersion)
            {
                throw new NotImplementedException();
            }

            public bool BuildProjectFilesInParallel(string[] projectFileNames, string[] targetNames, System.Collections.IDictionary[] globalProperties, System.Collections.IDictionary[] targetOutputsPerProject, string[] toolsVersion, bool useResultsCache, bool unloadProjectsOnCompletion)
            {
                throw new NotImplementedException();
            }

            public bool IsRunningMultipleNodes
            {
                get { throw new NotImplementedException(); }
            }

            public bool BuildProjectFile(string projectFileName, string[] targetNames, System.Collections.IDictionary globalProperties, System.Collections.IDictionary targetOutputs)
            {
                throw new NotImplementedException();
            }

            public int ColumnNumberOfTaskNode
            {
                get { throw new NotImplementedException(); }
            }

            public bool ContinueOnError
            {
                get { throw new NotImplementedException(); }
            }

            public int LineNumberOfTaskNode
            {
                get { throw new NotImplementedException(); }
            }

            public void LogCustomEvent(Microsoft.Build.Framework.CustomBuildEventArgs e)
            {
                Console.WriteLine(e.Message);
            }

            public void LogErrorEvent(Microsoft.Build.Framework.BuildErrorEventArgs e)
            {
                Console.WriteLine(e.Message);
            }

            public void LogMessageEvent(Microsoft.Build.Framework.BuildMessageEventArgs e)
            {
                Console.WriteLine(e.Message);
            }

            public void LogWarningEvent(Microsoft.Build.Framework.BuildWarningEventArgs e)
            {
                Console.WriteLine(e.Message);
            }

            public string ProjectFileOfTaskNode
            {
                get { throw new NotImplementedException(); }
            }
        }


        /// <summary>
        /// Download an old guy
        /// </summary>
        [TestMethod]
        public void MakeSureFreshDownloadWorksTarGZ()
        {
            var tempDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "MakeSureFreshDownloadWorksTarGZ"));
            if (tempDir.Exists)
            {
                tempDir.Delete(true);
            }

            // In normal running the directory isn't created first, so make sure we can deal with the fact
            // that it isn't there!!
            //tempDir.Create();

            var d = new DownloadAndUnpackROOT();
            d.BuildEngine = new dummyBuildEngine();

            d.InstallationPath = tempDir.FullName;
            //d.Log = new Microsoft.Build.Utilities.TaskLoggingHelper(new DummyBuildEngine, "MakeSureFreshDownloadWorksTarGZ");
            d.VCVersion = "vc11";
            d.Version = "5.34.20";

            Assert.IsTrue(d.Execute());
            Assert.AreEqual(string.Format("{0}\\root-5.34.20-vc11", tempDir.FullName), d.ROOTSYS);
        }

        [TestMethod]
        public void MakeSureFreshDownloadWorksZIP()
        {
            var tempDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "MakeSureFreshDownloadWorksZIP"));
            if (tempDir.Exists)
            {
                tempDir.Delete(true);
            }
            //tempDir.Create();

            var d = new DownloadAndUnpackROOT();
            d.BuildEngine = new dummyBuildEngine();

            d.InstallationPath = tempDir.FullName;
            //d.Log = new Microsoft.Build.Utilities.TaskLoggingHelper(new DummyBuildEngine, "MakeSureFreshDownloadWorksTarGZ");
            d.VCVersion = "vc11";
            d.Version = "5.34.32";

            Assert.IsTrue(d.Execute());
            Assert.AreEqual(string.Format("{0}\\root-5.34.32-vc11", tempDir.FullName), d.ROOTSYS);
        }
    }
}
