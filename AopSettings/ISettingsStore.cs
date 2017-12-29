using System.Reflection;

namespace AopSettings
{
    public interface ISettingsStore
    {
        object Read(PropertyInfo propertyInfo);
        void Save(PropertyInfo propertyInfo, object value);
    }
}
