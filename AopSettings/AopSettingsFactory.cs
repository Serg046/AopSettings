using System;
using System.Diagnostics;
using Castle.DynamicProxy;

namespace AopSettings
{
    public class AopSettingsFactory
    {
        public static T Create<T>(ISettingsStore settingsStore, params object[] args) where T : class
        {
            var type = typeof(T);
            Debug.Assert(Validate(type), "All injected properties must be public virtual read/write allowed");
            return (T)new ProxyGenerator().CreateClassProxy(type, args, new SettingsInterceptor(settingsStore));
        }

        public static T Decorate<T>(ISettingsStore settingsStore, T target) where T : class
            => Decorate<T>(new SettingsInterceptor(settingsStore), target);

        private static T Decorate<T>(SettingsInterceptor settingsInterceptor, T target) where T : class
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }
            var inpcType = typeof(T);
            Debug.Assert(Validate(inpcType), "All injected properties must be public virtual read/write allowed");
            return (T)new ProxyGenerator().CreateClassProxyWithTarget(inpcType, target, settingsInterceptor);
        }

        internal static bool Validate(Type type)
        {
            foreach (var prop in type.GetProperties())
            {
                if (prop.IsDefined(typeof(SettingAttribute), true) && !(prop.GetGetMethod()?.IsVirtual == true && prop.GetSetMethod()?.IsVirtual == true))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
