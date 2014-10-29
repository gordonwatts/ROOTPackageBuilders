using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ROOTMSBuildTasks;
using ApprovalTests;
using ApprovalTests.Reporters;

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
            t.UserSettingsPath = "usersettings.xml";

            Assert.IsTrue(t.Execute());

            Approvals.VerifyFile("usersettings.xml");
        }

        [TestMethod]
        [DeploymentItem("usersettings_empty.xml")]
        [UseReporter(typeof(DiffReporter))]
        public void AddSimpleVarToEmptyFile()
        {
            var t = new AddDebugEnv();
            t.EnvVarName = "TEST";
            t.EnvValue = "VALUE";
            t.UserSettingsPath = "usersettings_empty.xml";

            Assert.IsTrue(t.Execute());

            Approvals.VerifyFile("usersettings_empty.xml");
        }

        [TestMethod]
        public void AddSimpleVarToFileWithOtherSettings()
        {
            // File exists, and has other settings, add to it
            Assert.Fail("not yet");
        }

        [TestMethod]
        public void AddSimpleVarToFileWithVars()
        {
            // FIle has settingsfor other env vars in it
            Assert.Fail("not hyet");
        }

        [TestMethod]
        public void OverwriteSimpleVarFileWithVars()
        {
            // File has the setting for this var, overwrite
            Assert.Fail("not hyet");
        }

        [TestMethod]
        public void SetWithAppend()
        {
            // Set with an append
            Assert.Fail("not hyet");
        }

        [TestMethod]
        public void SetWithPrepend()
        {
            // Set with an append
            Assert.Fail("not hyet");
        }
    }
}
