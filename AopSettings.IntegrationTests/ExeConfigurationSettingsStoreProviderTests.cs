using System;
using System.Configuration;
using Xunit;

namespace AopSettings.IntegrationTests
{
    public class ExeConfigurationSettingsStoreProviderTests : IDisposable
    {
        private static readonly ExeConfigurationSettingsStoreProvider _settingsStoreProvider;
        private static readonly AppSettingsSection _configurationSection;

        static ExeConfigurationSettingsStoreProviderTests()
        {
            _settingsStoreProvider = new ExeConfigurationSettingsStoreProvider(ConfigurationUserLevel.None, "test_section");
            _configurationSection = _settingsStoreProvider.ConfigurationSection;
        }

        public void Dispose()
        {
            _configurationSection.Settings.Clear();
            _configurationSection.CurrentConfiguration.Save();
        }

        [Fact]
        public void GetSettingName_PropertyInfo_MemberFullName()
        {
            var type = typeof(DateTime);
            var name = nameof(DateTime.Now);

            var settingName = _settingsStoreProvider.GetSettingName(type.GetProperty(name));

            Assert.Equal($"{type.FullName}.{name}", settingName);
        }

        [Fact]
        public void Contains_SettingName_Success()
        {
            var settingName = "someSetting";
            _configurationSection.Settings.Add(settingName, "some value");
            _configurationSection.CurrentConfiguration.Save();

            Assert.True(_settingsStoreProvider.Contains(settingName));
            Assert.False(_settingsStoreProvider.Contains("other setting"));
        }

        [Fact]
        public void Read_ExistingSetting_ValidValue()
        {
            var settingName = "someSetting";
            var settingValue = "some value";
            _configurationSection.Settings.Add(settingName, settingValue);
            _configurationSection.CurrentConfiguration.Save();

            Assert.Equal(settingValue, _settingsStoreProvider.Read(settingName));
        }

        [Fact]
        public void Save_AddSomeSetting_Saved()
        {
            var settingName = "someSetting";
            var settingValue = "some value";

            _settingsStoreProvider.Save(settingName, settingValue);

            var actualSettingValue = _configurationSection.Settings[settingName].Value;
            Assert.Equal(settingValue, actualSettingValue);
        }

        [Fact]
        public void Save_UpdateSomeSetting_Saved()
        {
            var settingName = "someSetting";
            var settingValue = "some value";
            _configurationSection.Settings.Add(settingName, settingValue + "salt");
            _configurationSection.CurrentConfiguration.Save();

            _settingsStoreProvider.Save(settingName, settingValue);

            var actualSettingValue = _configurationSection.Settings[settingName].Value;
            Assert.Equal(settingValue, actualSettingValue);
        }
    }
}
