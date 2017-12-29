using System.Reflection;

namespace AopSettings
{
    public interface ISettingsStoreProvider
    {
        string GetSettingName(PropertyInfo property);
        bool Contains(string settingName);
        object Read(string settingName);
        void Save(string settingName, object value);
    }
}
