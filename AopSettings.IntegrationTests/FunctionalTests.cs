using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace AopSettings.IntegrationTests
{
    public class FunctionalTests : IDisposable
    {
        private readonly SettingsStoreProvider _settingsStoreProvider;
        private readonly SettingsStore _settingsStore;
        private readonly Model _model;

        public FunctionalTests()
        {
            _settingsStoreProvider = new SettingsStoreProvider();
            _settingsStore = new SettingsStore(_settingsStoreProvider);
            _model = AopSettingsFactory.Create<Model>(_settingsStore);
        }

        public void Dispose()
        {
            _settingsStoreProvider.Clear();
        }

        [Fact]
        public void DefaultValue()
        {
            Assert.Equal(Model.DEFAULT_SETTING_VALUE, _model.Setting);
        }

        [Fact]
        public void SaveValueBySetter()
        {
            var settingValue = "new value";

            _model.Setting = settingValue;

            Assert.Equal(settingValue, _settingsStore.Read((Model model) => model.Setting));
        }

        [Fact]
        public void BindSetting()
        {
            var bindedValue = string.Empty;
            _settingsStore.Bind((Model model) => model.Setting, value => bindedValue = (string)value);
            var settingValue = "new value";

            Assert.Equal(Model.DEFAULT_SETTING_VALUE, bindedValue);
            _model.Setting = settingValue;
            Assert.Equal(settingValue, bindedValue);
        }

        public class Model
        {
            public const string DEFAULT_SETTING_VALUE = "default";

            [Setting(Default = DEFAULT_SETTING_VALUE)]
            public virtual string Setting { get; set; }
        }

        private class SettingsStoreProvider : ISettingsStoreProvider
        {
            private readonly Dictionary<string, object> _settings = new Dictionary<string, object>();

            public string GetSettingName(PropertyInfo property)
                => $"{property.DeclaringType.FullName}.{property.Name}";

            public bool Contains(string settingName)
                => _settings.ContainsKey(settingName);

            public object Read(string settingName)
                => _settings[settingName];

            public void Save(string settingName, object value)
                => _settings[settingName] = value;

            public void Clear() => _settings.Clear();
        }
    }
}
