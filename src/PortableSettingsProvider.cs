/*
BalloonRSS - Simple RSS news aggregator using balloon tooltips
    Copyright (C) 2026  derived from BalloonRSS by Roman Morawek

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Xml;


namespace BalloonRss
{
    // settings provider which stores user settings in a "settings.xml" file inside a
    // "data" folder next to the executable instead of the per-user profile;
    // it is activated for the portable build via the PORTABLE compiler constant,
    // which attaches it to the settings class in Properties/Settings.Portable.cs
    class PortableSettingsProvider : SettingsProvider
    {
        // name of the data folder and the settings file within the application directory
        private const String dataFolderName = "data";
        private const String settingsFilename = "settings.xml";

        // the loaded settings store, created lazily on first access
        private XmlDocument settingsDocument;
        private String storeFilename;


        // the folder where the portable data is stored
        public static String DataDirectory
        {
            get
            {
                return Path.Combine(AppContext.BaseDirectory, dataFolderName);
            }
        }


        private String StoreFilename
        {
            get
            {
                if (storeFilename == null)
                    storeFilename = Path.Combine(DataDirectory, settingsFilename);
                return storeFilename;
            }
        }


        private XmlDocument SettingsDocument
        {
            get
            {
                if (settingsDocument == null)
                {
                    settingsDocument = new XmlDocument();
                    try
                    {
                        settingsDocument.Load(StoreFilename);
                    }
                    catch (Exception)
                    {
                        // no store file available yet, or it is corrupted: start with an empty store,
                        // so that all settings fall back to their default values
                        settingsDocument.LoadXml("<settings />");
                    }
                }
                return settingsDocument;
            }
        }


        public override void Initialize(String name, NameValueCollection config)
        {
            base.Initialize(this.GetType().Name, config);
        }


        public override String ApplicationName
        {
            get
            {
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            }
            set
            {
                // not needed; the store location does not depend on the application name
            }
        }


        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context,
            SettingsPropertyCollection properties)
        {
            SettingsPropertyValueCollection values = new SettingsPropertyValueCollection();
            XmlDocument document = SettingsDocument;

            foreach (SettingsProperty property in properties)
            {
                SettingsPropertyValue value = new SettingsPropertyValue(property);

                if (IsUserScoped(property))
                {
                    // read the stored user setting
                    XmlNode node = document.DocumentElement.SelectSingleNode("setting[@name='" + property.Name + "']");
                    if (node != null)
                    {
                        value.SerializedValue = node.InnerText;
                        value.IsDirty = false;
                    }
                    // if the setting was not stored yet, it falls back to its default value
                }
                else
                {
                    // application scoped settings are read from the application configuration file
                    String appConfigValue = GetApplicationSetting(property.Name);
                    if (appConfigValue != null)
                    {
                        value.SerializedValue = appConfigValue;
                        value.IsDirty = false;
                    }
                }

                values.Add(value);
            }

            return values;
        }


        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection values)
        {
            XmlDocument document = SettingsDocument;

            foreach (SettingsPropertyValue value in values)
            {
                // only user scoped settings are stored; application scoped ones are read-only
                if (!IsUserScoped(value.Property))
                    continue;

                // find or create the node of this setting
                XmlNode node = document.DocumentElement.SelectSingleNode("setting[@name='" + value.Property.Name + "']");
                if (node == null)
                {
                    node = document.CreateElement("setting");
                    XmlAttribute nameAttribute = document.CreateAttribute("name");
                    nameAttribute.Value = value.Property.Name;
                    node.Attributes.Append(nameAttribute);
                    document.DocumentElement.AppendChild(node);
                }

                Object serialized = value.SerializedValue;
                if (serialized == null)
                    continue;  // do not store null values; the default value applies on next start
                node.InnerText = serialized.ToString();
            }

            Directory.CreateDirectory(DataDirectory);
            document.Save(StoreFilename);
        }


        private static bool IsUserScoped(SettingsProperty property)
        {
            return property.Attributes.ContainsKey(typeof(UserScopedSettingAttribute).FullName);
        }


        // reads an application scoped setting from the application configuration file
        // (returns null in case of any problem, so the default value applies)
        private static String GetApplicationSetting(String settingName)
        {
            try
            {
                String configFilename = Path.Combine(AppContext.BaseDirectory, "BalloonRss.dll.config");
                XmlDocument configDocument = new XmlDocument();
                configDocument.Load(configFilename);
                XmlNode node = configDocument.SelectSingleNode(
                    "/configuration/applicationSettings/BalloonRss.Properties.Settings/setting[@name='" + settingName + "']/value");
                return node?.InnerText;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
