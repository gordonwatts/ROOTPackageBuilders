﻿using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace ROOTMSBuildTasks
{
    /// <summary>
    /// Add something to the environment variables for running in debug mode for a C++ app.
    /// </summary>
    public class AddDebugEnv : Task
    {
        /// <summary>
        /// The environment variable that we should set
        /// </summary>
        [Required]
        public string EnvVarName { get; set; }

        /// <summary>
        /// The value you wish it set to
        /// </summary>
        [Required]
        public string EnvValue { get; set; }
        
        /// <summary>
        /// How we will set the env variable
        /// </summary>
        public enum HowToSetEnvValue {
            PrefixValue, // Prepend, with the $() after the new value
            PostfixValue, // Post-pend, with the $() before the new value
            Set // Just set
        };

        /// <summary>
        /// Path to the user settings file.
        /// </summary>
        [Required]
        public string UserSettingsPath { get; set; }

        /// <summary>
        /// Local variable reflecting what is set (or not).
        /// </summary>
        private HowToSetEnvValue _envSetGuidance = HowToSetEnvValue.Set;

        /// <summary>
        /// Get/Set the guidance on how to set the enum.
        /// </summary>
        public string EnvSetGuidance
        {
            get { return _envSetGuidance.ToString(); }
            set { _envSetGuidance = (HowToSetEnvValue) Enum.Parse(typeof(HowToSetEnvValue), value); }
        }

        /// <summary>
        /// Load in the project file and set it, if it needs setting.
        /// </summary>
        /// <returns></returns>
        public override bool Execute()
        {
            // Load up the old file
            var f = LoadSettingsFile(UserSettingsPath);

            // Add the setting
            AddSetting(f);

            // Write it out
            SaveSettingsFile(f);

            return true;
        }

        /// <summary>
        /// Add the settings file according to our object's instructions.
        /// </summary>
        /// <param name="f"></param>
        private void AddSetting (XDocument f)
        {
            var settingsNote = GetSettingsNode(f);
            settingsNote.Value = string.Format("{0}={1}\n$(LocalDebuggerEnvironment)", EnvVarName, EnvValue);
        }

        /// <summary>
        /// Get the local debug env node
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        private XElement GetSettingsNode(XDocument f)
        {
            var proj = f.Descendants(msBuildNamespace + "Project").First();
            var pg = proj.Descendants(msBuildNamespace + "PropertyGroup").Where(dc => dc.Descendants(msBuildNamespace + "LocalDebuggerEnvironment").Count() > 0).FirstOrDefault();

            if (pg == null)
                pg = CreatePropertyGroupNode(proj);

            var localEnv = pg.Descendants(msBuildNamespace + "LocalDebuggerEnvironment").FirstOrDefault();
            if (localEnv == null)
            {
                localEnv = CreateLocalDebugEnv(pg);
            }
            return localEnv;
        }

        /// <summary>
        /// Create the debug env part of this guy
        /// </summary>
        /// <param name="f"></param>
        /// <param name="pg"></param>
        /// <returns></returns>
        private XElement CreateLocalDebugEnv(XElement pg)
        {
            var pgNode = new XElement(msBuildNamespace + "LocalDebuggerEnvironment");
            pg.Add(pgNode);
            return pgNode;
        }

        private XElement CreatePropertyGroupNode(XElement proj)
        {
            var elm = new XElement(msBuildNamespace + "PropertyGroup");
            proj.Add(elm);
            return elm;
        }

        /// <summary>
        /// Load in the old file.
        /// </summary>
        /// <param name="UserSettingsPath"></param>
        /// <returns></returns>
        private XDocument LoadSettingsFile(string UserSettingsPath)
        {
            var finfo = new FileInfo(UserSettingsPath);
            if (!finfo.Exists)
            {
                return CreateEmptySettingsXML();
            }

            // Load it from the file.

            var x = XDocument.Load(UserSettingsPath);

            return x;
        }

        /// <summary>
        /// Write out the xml document
        /// </summary>
        /// <param name="f"></param>
        private void SaveSettingsFile(XDocument f)
        {
            f.Save(UserSettingsPath);
        }

        private XNamespace msBuildNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";

        /// <summary>
        /// Create an empty settings files.
        /// </summary>
        /// <returns></returns>
        private XDocument CreateEmptySettingsXML()
        {
            var elm = new XElement(msBuildNamespace + "Project", new XAttribute("ToolsVersion", "12.0"));

            var xml = new XDocument(elm);
            xml.Declaration = new XDeclaration("1.0", "UTF-8", null);

            return xml;
#if false
            var xml = new XmlDocument();

            var xmlDeclaration = xml.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = xml.DocumentElement;
            xml.InsertBefore(xmlDeclaration, root);
            
            var proj = xml.CreateElement("Project", "http://schemas.microsoft.com/developer/msbuild/2003");
            proj.SetAttribute("Tools", "12.0");
            xml.AppendChild(proj);

            return new XDocument(xml);
#endif
        }
    }
}
