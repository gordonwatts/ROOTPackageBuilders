using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ROOTMSBuildTasks;
using ApprovalTests;
using ApprovalTests.Reporters;
using System.IO;

namespace ROOTMSBuildTasks.Test
{
    [TestClass]
    public class AddDebugEnvRunnerTest
    {
        [TestMethod]
        public void Ctor()
        {
            var t = new AddDebugEnv();
        }

        [TestMethod]
        [DeploymentItem("upsersettings_empty.xml")]
        public void AddSimpleVariableToEmpty()
        {
            var t = new AddDebugEnv();
            t.EnvVarName = "TEST";
            t.EnvValue = "VALUE";
            t.ConfigPlatform = "Debug|Win32";
            t.UserSettingsPath = "usersettings.xml";

            Assert.IsTrue(t.Execute());

            Approvals.VerifyFile("usersettings.xml");
        }

        [TestMethod]
        [DeploymentItem("usersettings_empty.xml")]
        public void AddSimpleVarToEmptyFile()
        {
            var t = new AddDebugEnv();
            t.EnvVarName = "TEST";
            t.EnvValue = "VALUE";
            t.ConfigPlatform = "Debug|Win32";
            t.UserSettingsPath = "usersettings_empty.xml";

            Assert.IsTrue(t.Execute());

            Approvals.VerifyFile("usersettings_empty.xml");
        }

        [TestMethod]
        [DeploymentItem("usersettings_oneothervar.xml")]
        public void AddSimpleVarToFileWithVars()
        {
            var t = new AddDebugEnv();
            t.EnvVarName = "TEST";
            t.EnvValue = "VALUE";
            t.ConfigPlatform = "Debug|Win32";
            t.UserSettingsPath = "usersettings_oneothervar.xml";

            Assert.IsTrue(t.Execute());

            Approvals.VerifyFile("usersettings_oneothervar.xml");
        }

        [TestMethod]
        [DeploymentItem("usersettings_alreadysetvar.xml")]
        public void OverwriteSimpleVarFileWithVars()
        {
            var t = new AddDebugEnv();
            t.EnvVarName = "TEST";
            t.EnvValue = "FREAK";
            t.ConfigPlatform = "Debug|Win32";
            t.UserSettingsPath = "usersettings_alreadysetvar.xml";

            Assert.IsTrue(t.Execute());

            Approvals.VerifyFile("usersettings_alreadysetvar.xml");
        }

        [TestMethod]
        [DeploymentItem("upsersettings_empty.xml")]
        public void SetWithAppend()
        {
            // Set PATH, prepend, and there was no setting done yet.
            var t = new AddDebugEnv();
            t.EnvVarName = "PATH";
            t.EnvValue = "c:\\root";
            t.ConfigPlatform = "Debug|Win32";
            t.EnvSetGuidance = "PostfixAsPathValue";
            t.UserSettingsPath = "upsersettings_empty.xml";

            Assert.IsTrue(t.Execute());

            Approvals.VerifyFile("upsersettings_empty.xml");
        }

        [TestMethod]
        [DeploymentItem("upsersettings_empty.xml")]
        public void SetWithPrepend()
        {
            var t = new AddDebugEnv();
            t.EnvVarName = "PATH";
            t.EnvValue = "c:\\root";
            t.ConfigPlatform = "Debug|Win32";
            t.EnvSetGuidance = "PrefixAsPathValue";
            t.UserSettingsPath = "upsersettings_empty.xml";

            Assert.IsTrue(t.Execute());

            Approvals.VerifyFile("upsersettings_empty.xml");
        }

        [TestMethod]
        [DeploymentItem("usersettings_path.xml")]
        public void SetExistingPathWithAppend()
        {
            // Set PATH, prepend, and there was no setting done yet.
            var t = new AddDebugEnv();
            t.EnvVarName = "PATH";
            t.EnvValue = "c:\\root";
            t.ConfigPlatform = "Debug|Win32";
            t.EnvSetGuidance = "PostfixAsPathValue";
            t.UserSettingsPath = "usersettings_path.xml";

            Assert.IsTrue(t.Execute());

            Approvals.VerifyFile("usersettings_path.xml");
        }

        [TestMethod]
        [DeploymentItem("usersettings_path.xml")]
        //[UseReporter(typeof(DiffReporter))]
        public void SetExistingPathWithPrepend()
        {
            var t = new AddDebugEnv();
            t.EnvVarName = "PATH";
            t.EnvValue = "c:\\root";
            t.ConfigPlatform = "Debug|Win32";
            t.EnvSetGuidance = "PrefixAsPathValue";
            t.UserSettingsPath = "usersettings_path.xml";

            Assert.IsTrue(t.Execute());

            Approvals.VerifyFile("usersettings_path.xml");
        }

        [TestMethod]
        [DeploymentItem("usersettings_path.xml")]
        public void SetToSameValue()
        {
            var f = new FileInfo("usersettings_path.xml");
            var cModTime = f.LastWriteTime;
            var t = new AddDebugEnv();
            t.ConfigPlatform = "Debug|Win32";
            t.EnvVarName = "ROOT";
            t.EnvValue = "DUDE";
            t.UserSettingsPath = "usersettings_path.xml";

            Assert.IsTrue(t.Execute());

            f.Refresh();
            Assert.AreEqual(cModTime, f.LastWriteTime);
        }

        [TestMethod]
        [DeploymentItem("usersettings_alreadypath.xml")]
        public void AddPathWithValueAlreadyThere()
        {
            var f = new FileInfo("usersettings_alreadypath.xml");
            var cModTime = f.LastWriteTime;

            var t = new AddDebugEnv();
            t.EnvVarName = "PATH";
            t.EnvValue = "c:\\root";
            t.ConfigPlatform = "Debug|Win32";
            t.EnvSetGuidance = "PrefixAsPathValue";
            t.UserSettingsPath = "usersettings_alreadypath.xml";

            Assert.IsTrue(t.Execute());

            f.Refresh();
            Assert.AreEqual(cModTime, f.LastWriteTime);
        }

        [TestMethod]
        [DeploymentItem("usersettings_debrel.xml")]
        public void AddVarWhenExistingVarHasDebugAndRelPropGroups()
        {
            var t = new AddDebugEnv();
            t.EnvVarName = "ROOTSYS";
            t.EnvValue = "c:\\root";
            t.ConfigPlatform = "Debug|Win32";
            t.UserSettingsPath = "usersettings_debrel.xml";

            Assert.IsTrue(t.Execute());
            Approvals.VerifyFile("usersettings_debrel.xml");
        }
    }
}
