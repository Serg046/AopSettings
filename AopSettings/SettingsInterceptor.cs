using System.Linq;
using Castle.DynamicProxy;

namespace AopSettings
{
    public class SettingsInterceptor : IInterceptor
    {
        private readonly ISettingsStore _settingsStore;

        public SettingsInterceptor(ISettingsStoreProvider settingsStoreProvider)
            : this(new SettingsStore(settingsStoreProvider))
        {
        }

        public SettingsInterceptor(ISettingsStore settingsStore)
        {
            _settingsStore = settingsStore;
        }

        public void Intercept(IInvocation invocation)
        {
            var isIntercepted = false;
            if (invocation.Method.Name.StartsWith("get_"))
            {
                var propertyName = invocation.Method.Name.Substring(4);
                var propertyInfo = invocation.TargetType.GetProperty(propertyName);

                if (propertyInfo?.IsDefined(typeof(SettingAttribute), false) == true)
                {
                    invocation.ReturnValue = _settingsStore.Read(propertyInfo);
                    isIntercepted = true;
                }
            }
            else if (invocation.Method.Name.StartsWith("set_"))
            {
                var propertyName = invocation.Method.Name.Substring(4);
                var propertyInfo = invocation.TargetType.GetProperty(propertyName);

                if (propertyInfo?.IsDefined(typeof(SettingAttribute), false) == true)
                {
                    _settingsStore.Save(propertyInfo, invocation.Arguments.Single());
                    isIntercepted = true;
                }
            }
            if (!isIntercepted)
            {
                invocation.Proceed();
            }
        }
    }
}
