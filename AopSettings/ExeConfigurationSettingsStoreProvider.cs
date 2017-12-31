using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace AopSettings
{
    public class ExeConfigurationSettingsStoreProvider : ISettingsStoreProvider
    {
        internal readonly AppSettingsSection ConfigurationSection;

        public ExeConfigurationSettingsStoreProvider(ConfigurationUserLevel userLevel, string sectionName)
        {
            var configuration = ConfigurationManager.OpenExeConfiguration(userLevel);
            ConfigurationSection = configuration.Sections[sectionName] as AppSettingsSection;
            if (ConfigurationSection == null)
            {
                ConfigurationSection = new AppSettingsSection();
                ConfigurationSection.SectionInformation.AllowExeDefinition =
                    ConfigurationAllowExeDefinition.MachineToLocalUser;
                configuration.Sections.Add(sectionName, ConfigurationSection);
                configuration.Save();
            }
        }

        public string GetSettingName(PropertyInfo property)
        {
            Debug.Assert(property?.DeclaringType != null);
            return $"{property.DeclaringType.FullName}.{property.Name}";
        }

        public bool Contains(string settingName)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(settingName));
            return ConfigurationSection.Settings.AllKeys.Contains(settingName);
        }

        public object Read(string settingName)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(settingName));
            Debug.Assert(Contains(settingName));
            return ConfigurationSection.Settings[settingName].Value;
        }

        public void Save(string settingName, object value)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(settingName));
            var stringValue = value.ToString();
            if (Contains(settingName))
            {
                ConfigurationSection.Settings[settingName].Value = stringValue;
            }
            else
            {
                ConfigurationSection.Settings.Add(settingName, stringValue);
            }
            ConfigurationSection.CurrentConfiguration.Save();
        }
    }
}
