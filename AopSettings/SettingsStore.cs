using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using Castle.Core.Internal;

namespace AopSettings
{
    public class SettingsStore : ISettingsStore
    {
        private readonly ISettingsStoreProvider _settingsStoreProvider;
        private readonly Dictionary<PropertyInfo, Action<object>> _subscriptions = new Dictionary<PropertyInfo, Action<object>>();

        public SettingsStore(ISettingsStoreProvider settingsStoreProvider)
        {
            _settingsStoreProvider = settingsStoreProvider;
        }

        private PropertyInfo GetProperty(LambdaExpression expression)
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression?.Member is PropertyInfo property)
            {
                return property;
            }
            throw new InvalidOperationException("Expression must access a property");
        }

        public TResult Read<TResult>(Expression<Func<TResult>> settingExpression) => (TResult)Read(GetProperty(settingExpression));

        public TResult Read<TEntity, TResult>(Expression<Func<TEntity, TResult>> settingExpression) => (TResult)Read(GetProperty(settingExpression));

        public object Read(PropertyInfo propertyInfo)
        {
            var settingAttribute = propertyInfo.GetAttribute<SettingAttribute>();
            if (settingAttribute == null)
            {
                throw new InvalidOperationException("The property must be marked by SettingAttribute");
            }

            var settingName = _settingsStoreProvider.GetSettingName(propertyInfo);
            var settingType = propertyInfo.PropertyType;

            object result = null;
            if (_settingsStoreProvider.Contains(settingName))
            {
                result = _settingsStoreProvider.Read(settingName);
                if (result != null)
                {
                    var valueType = result.GetType();
                    if (!settingType.IsAssignableFrom(valueType))
                    {
                        var typeConverter = TypeDescriptor.GetConverter(settingType);
                        if (typeConverter.CanConvertFrom(valueType))
                        {
                            result = typeConverter.ConvertFrom(result);
                        }
                        else
                        {
                            throw new InvalidCastException("TypeConverter must be properly defined for a property type");
                        }
                    }
                }
            }
            else if (settingAttribute.Default != null)
            {
                if (settingType.IsInstanceOfType(settingAttribute.Default))
                {
                    result = settingAttribute.Default;
                }
                else
                {
                    throw new InvalidCastException($"Invalid setting value. Expected - {settingType}. Actual - {settingAttribute.Default.GetType()}");
                }
            }
            else if (settingType.IsValueType)
            {
                result = Activator.CreateInstance(settingType);
            }
            return result;
        }

        public void Save<TResult>(Expression<Func<TResult>> settingExpression, TResult value) => Save(GetProperty(settingExpression), value);

        public void Save<TEntity, TResult>(Expression<Func<TEntity, TResult>> settingExpression, TResult value) => Save(GetProperty(settingExpression), value);

        public void Save(PropertyInfo propertyInfo, object value)
        {
            if (!propertyInfo.IsDefined(typeof(SettingAttribute), false))
            {
                throw new InvalidOperationException("The property must be marked by SettingAttribute");
            }

            if (_subscriptions.TryGetValue(propertyInfo, out var subscriberCallback))
            {
                subscriberCallback(value);
            }
            var settingName = _settingsStoreProvider.GetSettingName(propertyInfo);
            _settingsStoreProvider.Save(settingName, value);
        }

        public void Bind<TResult>(Expression<Func<TResult>> settingExpression, Action<object> subscriberCallback)
        {
            Bind(GetProperty(settingExpression), subscriberCallback);
        }

        public void Bind<TEntity, TResult>(Expression<Func<TEntity, TResult>> settingExpression, Action<object> subscriberCallback)
        {
            Bind(GetProperty(settingExpression), subscriberCallback);
        }

        public void Bind(PropertyInfo propertyInfo, Action<object> subscriberCallback)
        {
            subscriberCallback(Read(propertyInfo));
            _subscriptions.Add(propertyInfo, subscriberCallback);
        }
    }
}
