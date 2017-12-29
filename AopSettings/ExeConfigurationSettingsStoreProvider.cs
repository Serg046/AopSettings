using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace AopSettings
{
    public class ExeConfigurationSettingsStoreProvider : ISettingsStoreProvider
    {
        private readonly Configuration _configuration;
        private readonly KeyValueConfigurationCollection _settings;

        public ExeConfigurationSettingsStoreProvider(ConfigurationUserLevel userLevel, string sectionName)
        {
            _configuration = ConfigurationManager.OpenExeConfiguration(userLevel);
            var configurationSection = _configuration.Sections[sectionName] as AppSettingsSection;
            if (configurationSection == null)
            {
                configurationSection = new AppSettingsSection();
                configurationSection.SectionInformation.AllowExeDefinition =
                    ConfigurationAllowExeDefinition.MachineToLocalUser;
                _configuration.Sections.Add(sectionName, configurationSection);
                _configuration.Save();
            }
            _settings = configurationSection.Settings;
        }

        public string GetSettingName(PropertyInfo property)
        {
            Debug.Assert(property?.DeclaringType != null);
            return $"{property.DeclaringType.FullName}.{property.Name}";
        }

        public bool Contains(string settingName)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(settingName));
            return _settings.AllKeys.Contains(settingName);
        }

        public object Read(string settingName)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(settingName));
            return _settings[settingName]?.Value;
        }

        public void Save(string settingName, object value)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(settingName));
            var stringValue = value.ToString();
            if (Contains(settingName))
            {
                _settings[settingName].Value = stringValue;
            }
            else
            {
                _settings.Add(settingName, stringValue);
            }
            _configuration.Save();
        }
    }
}
