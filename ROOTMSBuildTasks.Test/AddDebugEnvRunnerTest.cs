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
        [DeploymentItem("usersettings_alreadysetvar.xml")]
        [UseReporter(typeof(DiffReporter))]
        public void OverwriteSimpleVarFileWithVars()
        {
            var t = new AddDebugEnv();
            t.EnvVarName = "TEST";
            t.EnvValue = "FREAK";
            t.UserSettingsPath = "usersettings_alreadysetvar.xml";

            Assert.IsTrue(t.Execute());

            Approvals.VerifyFile("usersettings_alreadysetvar.xml");
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
