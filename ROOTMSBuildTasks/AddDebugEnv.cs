using Microsoft.Build.Framework;
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

        /// Path to the user settings file.
        /// </summary>
        [Required]
        public string UserSettingsPath { get; set; }

        /// <summary>
        /// For which configuration and platofrm should we be setting this?
        /// </summary>
        [Required]
        public string ConfigPlatform { get; set; }

        /// <summary>
        /// How we will set the env variable
        /// </summary>
        public enum HowToSetEnvValue
        {
            PrefixAsPathValue, // Prepend the new setting, then a ";" then the old value
            PostfixAsPathValue, // The old value, then a ";", and then the new value.
            Set // Just set (this is the default value)
        };

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
        /// True if the file was updated. If no change was required, then the file isn't written out.
        /// </summary>
        public bool FileHadToBeUpdated { get; private set; }

        /// <summary>
        /// Load in the project file and set it, if it needs setting.
        /// </summary>
        /// <returns></returns>
        public override bool Execute()
        {
            FileHadToBeUpdated = false;

            // Load up the old file
            var f = LoadSettingsFile(UserSettingsPath);

            // Add the setting
            AddSetting(f);

            // Write it out
            if (FileHadToBeUpdated)
            {
                SaveSettingsFile(f);
            }

            return true;
        }

        /// <summary>
        /// Add the settings file according to our object's instructions.
        /// </summary>
        /// <param name="f"></param>
        private void AddSetting (XDocument f)
        {
            var settingsNode = GetSettingsNode(f);
            var listOfSettings = ExtractSettings(settingsNode);
            listOfSettings[EnvVarName] = BuildNewSettingValue(listOfSettings.ContainsKey(EnvVarName) ? listOfSettings[EnvVarName] : null);
            settingsNode.Value = SettingsToString(listOfSettings);
        }

        /// <summary>
        /// Given the old setting, construct a new setting depending on how we have been
        /// configured.
        /// </summary>
        /// <param name="oldSetting"></param>
        /// <returns></returns>
        private string BuildNewSettingValue(string oldSetting)
        {
            // The old setting is what we replace. If it is null, then it is whatever the env var was before.
            // This is only used if we are treating the env like a path variable.
            if (oldSetting == null)
            {
                oldSetting = string.Format("$({0})", EnvVarName);
            }

            // Now, figure out where to make the placement.
            switch (_envSetGuidance)
            {
                case HowToSetEnvValue.PrefixAsPathValue:
                    // Prepend the value
                    FileHadToBeUpdated = !PathContainsValue (EnvValue, oldSetting);
                    return string.Format("{0};{1}", EnvValue, oldSetting);

                case HowToSetEnvValue.PostfixAsPathValue:
                    // Append the value
                    FileHadToBeUpdated = !PathContainsValue (EnvValue, oldSetting);
                    return string.Format("{0};{1}", oldSetting, EnvValue);

                case HowToSetEnvValue.Set:
                    // Just a straight replacement
                    FileHadToBeUpdated = oldSetting != EnvValue;
                    return EnvValue;

                default:
                    throw new ArgumentException(string.Format("Do not know how to set with '{0}'.", _envSetGuidance.ToString()));
            }
        }

        /// <summary>
        /// Given a ";" seperated path, look to see if one is in the other.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="pathList"></param>
        /// <returns></returns>
        private bool PathContainsValue(string value, string pathList)
        {
            var plist = pathList.Split(';');
            return plist.Contains(value);
        }

        /// <summary>
        /// Convert the list of settings to a string for use by the IDE.
        /// </summary>
        /// <param name="listOfSettings"></param>
        /// <returns></returns>
        private string SettingsToString(IDictionary<string, string> listOfSettings)
        {
            StringBuilder bld = new StringBuilder();
            foreach (var item in listOfSettings)
            {
                bld.AppendFormat("{0}={1}\n", item.Key, item.Value);
            }
            bld.Append("$(LocalDebuggerEnvironment)");
            return bld.ToString();
        }

        /// <summary>
        /// Given a settings guy, parse through it and extract all the settings. One per line.
        /// </summary>
        /// <param name="settingsNode"></param>
        /// <returns></returns>
        private IDictionary<string, string> ExtractSettings(XElement settingsNode)
        {
            Dictionary<string, string> result = new Dictionary<string,string>();
            using (var rdr = new StringReader(settingsNode.Value))
            {
                var line = rdr.ReadLine();
                while (line != null)
                {
                    var eqIndex = line.IndexOf("=");
                    if (eqIndex > 0)
                    {
                        var vName = line.Substring(0, eqIndex).Trim();
                        var vValue = line.Substring(eqIndex + 1).Trim();
                        result[vName] = vValue;
                    }

                    line = rdr.ReadLine();
                }
            }
            return result;
        }

        /// <summary>
        /// Returns the condition test
        /// </summary>
        /// <returns></returns>
        private string ConditionString()
        {
            return string.Format("'$(Configuration)|$(Platform)'=='{0}'", ConfigPlatform);
        }

        /// <summary>
        /// Does this element have a good condition?
        /// </summary>
        /// <param name="atr"></param>
        /// <returns></returns>
        private bool HasGoodCondition(XElement atr)
        {
            var c = atr.Attribute("Condition");
            if (c == null)
                return false;
            return c.Value == ConditionString();
        }

        /// <summary>
        /// Get the local debug env node
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        private XElement GetSettingsNode(XDocument f)
        {
            var proj = f.Descendants(msBuildNamespace + "Project").First();
            var pg = proj.Descendants(msBuildNamespace + "PropertyGroup").Where(dc => HasGoodCondition(dc) && dc.Descendants(msBuildNamespace + "LocalDebuggerEnvironment").Count() > 0).FirstOrDefault();
            if (pg == null)
            {
                pg = proj.Descendants(msBuildNamespace + "PropertyGroup").Where(dc => HasGoodCondition(dc)).FirstOrDefault();
            }

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

        /// <summary>
        /// Create a PropertyGroup node.
        /// </summary>
        /// <param name="proj"></param>
        /// <returns></returns>
        private XElement CreatePropertyGroupNode(XElement proj)
        {
            var elm = new XElement(msBuildNamespace + "PropertyGroup");
            var cond = new XAttribute("Condition", ConditionString());
            elm.Add(cond);
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

        /// <summary>
        /// Cache for the namespace, as everything is in it...
        /// </summary>
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
        }
    }
}
