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
        [DeploymentItem("usersettings_oneothervar.xml")]
        [UseReporter(typeof(DiffReporter))]
        public void AddSimpleVarToFileWithVars()
        {
            var t = new AddDebugEnv();
            t.EnvVarName = "TEST";
            t.EnvValue = "VALUE";
            t.UserSettingsPath = "usersettings_oneothervar.xml";

            Assert.IsTrue(t.Execute());

            Approvals.VerifyFile("usersettings_oneothervar.xml");
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
