using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using Moq;
using Xunit;

namespace AopSettings.UnitTests
{
    public class SettingsStoreTests
    {
        private const string SETTING_NAME = "setting";

        private readonly Mock<ISettingsStoreProvider> _settingsStoreProvider;
        private readonly SettingsStore _settingsStore;

        public SettingsStoreTests()
        {
            _settingsStoreProvider = new Mock<ISettingsStoreProvider>();
            _settingsStoreProvider.Setup(p => p.GetSettingName(It.IsAny<PropertyInfo>())).Returns(SETTING_NAME);
            _settingsStoreProvider.Setup(p => p.Contains(SETTING_NAME)).Returns(true);
            _settingsStore = new SettingsStore(_settingsStoreProvider.Object);
        }

        [Fact]
        public void Read_SettingWithoutAttribute_Fails()
        {
            var propertyInfo = GetPropertyInfo(() => SettingWithoutAttribute);

            Assert.Throws<InvalidOperationException>(() => _settingsStore.Read(propertyInfo));
        }

        [Fact]
        public void Read_ParentSettingAndParentValue_Success()
        {
            var expectedValue = new Parent();
            _settingsStoreProvider.Setup(p => p.Read(SETTING_NAME)).Returns(expectedValue);
            var propertyInfo = GetPropertyInfo(() => ParentSetting);

            var actualValue = _settingsStore.Read(propertyInfo);

            Assert.Same(expectedValue, actualValue);
            _settingsStoreProvider.VerifyAll();
        }

        [Fact]
        public void Read_ParentSettingAndChildValue_Success()
        {
            var expectedValue = new Child();
            _settingsStoreProvider.Setup(p => p.Read(SETTING_NAME)).Returns(expectedValue);
            var propertyInfo = GetPropertyInfo(() => ParentSetting);

            var actualValue = _settingsStore.Read(propertyInfo);

            Assert.Same(expectedValue, actualValue);
            _settingsStoreProvider.VerifyAll();
        }

        [Fact]
        public void Read_ChildSettingAndParentValue_Fails()
        {
            var expectedValue = new Parent();
            _settingsStoreProvider.Setup(p => p.Read(SETTING_NAME)).Returns(expectedValue);
            var propertyInfo = GetPropertyInfo(() => ChildSetting);

            Assert.Throws<InvalidCastException>(() => _settingsStore.Read(propertyInfo));
            _settingsStoreProvider.VerifyAll();
        }

        [Fact]
        public void Read_ChildSettingWithConverterAndSupportedValue_Success()
        {
            var value = "s";
            _settingsStoreProvider.Setup(p => p.Read(SETTING_NAME)).Returns(value);
            var propertyInfo = GetPropertyInfo(() => ConvertableChildSetting);

            var actualValue = _settingsStore.Read(propertyInfo);

            Assert.Same(ChildWithTypeConverter.Converter.Instance, actualValue);
            _settingsStoreProvider.VerifyAll();
        }

        [Fact]
        public void Read_ChildSettingWithConverterAndUnsupportedValue_Fails()
        {
            var value = 5;
            _settingsStoreProvider.Setup(p => p.Read(SETTING_NAME)).Returns(value);
            var propertyInfo = GetPropertyInfo(() => ConvertableChildSetting);

            Assert.Throws<InvalidCastException>(() => _settingsStore.Read(propertyInfo));
            _settingsStoreProvider.VerifyAll();
        }

        [Fact]
        public void Read_IntSettingWithIntDefault_Success()
        {
            _settingsStoreProvider.Setup(p => p.Contains(SETTING_NAME)).Returns(false);
            var propertyInfo = GetPropertyInfo(() => IntSettingWithIntDefault);

            var actualValue = _settingsStore.Read(propertyInfo);

            Assert.Equal(5, actualValue);
            _settingsStoreProvider.VerifyAll();
        }

        [Fact]
        public void Read_ObjSettingWithIntDefault_Success()
        {
            _settingsStoreProvider.Setup(p => p.Contains(SETTING_NAME)).Returns(false);
            var propertyInfo = GetPropertyInfo(() => ObjSettingWithIntDefault);

            var actualValue = _settingsStore.Read(propertyInfo);

            Assert.Equal(5, actualValue);
            _settingsStoreProvider.VerifyAll();
        }

        [Fact]
        public void Read_StringSettingWithIntDefault_Fails()
        {
            _settingsStoreProvider.Setup(p => p.Contains(SETTING_NAME)).Returns(false);
            var propertyInfo = GetPropertyInfo(() => StringSettingWithIntDefault);

            Assert.Throws<InvalidCastException>(() => _settingsStore.Read(propertyInfo));
            _settingsStoreProvider.VerifyAll();
        }

        [Fact]
        public void Read_EmptyIntSettingAndNoDefaultValue_ReturnsZero()
        {
            _settingsStoreProvider.Setup(p => p.Contains(SETTING_NAME)).Returns(false);
            var propertyInfo = GetPropertyInfo(() => IntSetting);

            var actualValue = _settingsStore.Read(propertyInfo);

            Assert.Equal(0, actualValue);
            _settingsStoreProvider.VerifyAll();
        }

        [Fact]
        public void Read_EmptyParentSettingAndNoDefaultValue_ReturnsNull()
        {
            _settingsStoreProvider.Setup(p => p.Contains(SETTING_NAME)).Returns(false);
            var propertyInfo = GetPropertyInfo(() => ParentSetting);

            var actualValue = _settingsStore.Read(propertyInfo);

            Assert.Null(actualValue);
            _settingsStoreProvider.VerifyAll();
        }

        [Fact]
        public void Read_LambdaAccessToInternalProperty_CallsReadMethod()
        {
            _settingsStore.Read(() => ParentSetting);

            _settingsStoreProvider.Verify(provider => provider.Read(SETTING_NAME), Times.Once);
        }

        [Fact]
        public void Read_LambdaAccessToExternalProperty_CallsReadMethod()
        {
            _settingsStore.Read((Parent setting) => setting.SomeSetting);

            _settingsStoreProvider.Verify(provider => provider.Read(SETTING_NAME), Times.Once);
        }

        [Fact]
        public void Save_SettingWithoutAttribute_Fails()
        {
            var propertyInfo = GetPropertyInfo(() => SettingWithoutAttribute);

            Assert.Throws<InvalidOperationException>(() => _settingsStore.Save(propertyInfo, "test"));
        }

        [Fact]
        public void Save_SomeValue_CallsSaveMethod()
        {
            const int valueToSave = 456;
            var propertyInfo = GetPropertyInfo(() => IntSetting);

            _settingsStore.Save(propertyInfo, valueToSave);

            _settingsStoreProvider.Verify(provider => provider.Save(SETTING_NAME, valueToSave));
        }

        [Fact]
        public void Save_SomeValue_SubscribedCallbackIsCalled()
        {
            var counter = 0;
            const int valueToSave = 456;
            var propertyInfo = GetPropertyInfo(() => IntSetting);


            _settingsStore.Bind(() => IntSetting, value => counter++);
            _settingsStore.Save(propertyInfo, valueToSave);

            _settingsStoreProvider.Verify(provider => provider.Save(SETTING_NAME, valueToSave));
            Assert.Equal(2, counter);
        }

        [Fact]
        public void Bind_Callback_CallbackAndReadMethodAreCalled()
        {
            var counter = 0;
            var propertyInfo = GetPropertyInfo(() => IntSetting);

            _settingsStore.Bind(propertyInfo, value => counter++);

            _settingsStoreProvider.Verify(provider => provider.Read(SETTING_NAME), Times.Once);
            Assert.Equal(1, counter);
        }

        [Fact]
        public void Bind_LambdaAccessToInternalProperty_CallsReadMethod()
        {
            _settingsStore.Bind(() => ParentSetting, o => { });

            _settingsStoreProvider.Verify(provider => provider.Read(SETTING_NAME), Times.Once);
        }

        [Fact]
        public void Bind_LambdaAccessToExternalProperty_CallsReadMethod()
        {
            _settingsStore.Bind((Parent setting) => setting.SomeSetting, o => { });

            _settingsStoreProvider.Verify(provider => provider.Read(SETTING_NAME), Times.Once);
        }

        private PropertyInfo GetPropertyInfo<T>(Expression<Func<T>> expression) => ((MemberExpression)expression.Body).Member as PropertyInfo;

        public object SettingWithoutAttribute { get; set; }

        [Setting]
        public int IntSetting { get; set; }

        [Setting]
        public Parent ParentSetting { get; set; }

        [Setting]
        public Child ChildSetting { get; set; }

        [Setting]
        public ChildWithTypeConverter ConvertableChildSetting { get; set; }

        [Setting(Default = 5)]
        public int IntSettingWithIntDefault { get; set; }

        [Setting(Default = 5)]
        public object ObjSettingWithIntDefault { get; set; }

        [Setting(Default = 5)]
        public string StringSettingWithIntDefault { get; set; }

        public class Parent
        {
            [Setting]
            public object SomeSetting { get; set; }
        }

        public class Child : Parent
        {
        }

        [TypeConverter(typeof(Converter))]
        public class ChildWithTypeConverter : Parent
        {
            public class Converter : TypeConverter
            {
                public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
                {
                    return sourceType == typeof(string);
                }

                public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
                {
                    return Instance;
                }

                public static ChildWithTypeConverter Instance { get; } = new ChildWithTypeConverter();
            }
        }
    }
}
