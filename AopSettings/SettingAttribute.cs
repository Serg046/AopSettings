using System;

namespace AopSettings
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SettingAttribute : Attribute
    {
        public object Default { get; set; }
    }
}
