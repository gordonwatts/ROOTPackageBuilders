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
        [DeploymentItem("upsersettings_empty.xml")]
        [UseReporter(typeof(DiffReporter))]
        public void SetWithAppend()
        {
            // Set PATH, prepend, and there was no setting done yet.
            var t = new AddDebugEnv();
            t.EnvVarName = "PATH";
            t.EnvValue = "c:\\root";
            t.EnvSetGuidance = "PostfixAsPathValue";
            t.UserSettingsPath = "upsersettings_empty.xml";

            Assert.IsTrue(t.Execute());

            Approvals.VerifyFile("upsersettings_empty.xml");
        }

        [TestMethod]
        [DeploymentItem("upsersettings_empty.xml")]
        [UseReporter(typeof(DiffReporter))]
        public void SetWithPrepend()
        {
            var t = new AddDebugEnv();
            t.EnvVarName = "PATH";
            t.EnvValue = "c:\\root";
            t.EnvSetGuidance = "PrefixAsPathValue";
            t.UserSettingsPath = "upsersettings_empty.xml";

            Assert.IsTrue(t.Execute());

            Approvals.VerifyFile("upsersettings_empty.xml");
        }

        [TestMethod]
        public void SetToSameValue()
        {
            Assert.Fail("File shoudl not be touched");
        }

        [TestMethod]
        public void AddPathWithValueAlreadyThere()
        {
            Assert.Fail("File should not be touched");
        }
    }
}
