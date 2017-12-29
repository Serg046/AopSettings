using System;
using System.Reflection;
using Moq;
using Xunit;

namespace AopSettings.UnitTests
{
    public class AopSettingsFactoryTests
    {
        private readonly Mock<ISettingsStore> _settingsStoreMock;

        public AopSettingsFactoryTests()
        {
            _settingsStoreMock = new Mock<ISettingsStore>();
        }

        [Fact]
        public void Create_PropWithoutSettingAttribute_SettingsCallIsNotInjected()
        {
            var injectedVm = AopSettingsFactory.Create<Model>(_settingsStoreMock.Object);
            _settingsStoreMock.Setup(store => store.Save(It.IsAny<PropertyInfo>(), It.IsAny<object>()))
                .Callback(() => throw new InvalidOperationException());

            injectedVm.Prop = 5;
        }

        [Fact]
        public void Create_PropWithSettingAttribute_SettingsCallInjected()
        {
            var injectedVm = AopSettingsFactory.Create<Model>(_settingsStoreMock.Object);
            _settingsStoreMock.Setup(store => store.Save(It.IsAny<PropertyInfo>(), It.IsAny<object>()))
                .Callback(() => throw new InvalidOperationException());

            Assert.Throws<InvalidOperationException>(() => injectedVm.InjectProp = 5);
        }

        [Fact]
        public void Create_NullAsEmptyCtorArgument_SettingsCallInjected()
        {
            var injectedVm = AopSettingsFactory.Create<Model>(_settingsStoreMock.Object, null);
            _settingsStoreMock.Setup(store => store.Save(It.IsAny<PropertyInfo>(), It.IsAny<object>()))
                .Callback(() => throw new InvalidOperationException());

            Assert.Throws<InvalidOperationException>(() => injectedVm.InjectProp = 5);
        }

        [Fact]
        public void Create_CtorWithArguments_SettingsCallInjected()
        {
            var injectedVm = AopSettingsFactory.Create<Model>(_settingsStoreMock.Object, 7);
            _settingsStoreMock.Setup(store => store.Save(It.IsAny<PropertyInfo>(), It.IsAny<object>()))
                .Callback(() => throw new InvalidOperationException());

            Assert.Equal(7, injectedVm.Prop);
            Assert.Throws<InvalidOperationException>(() => injectedVm.InjectProp = 5);
        }

        [Fact]
        public void Validate_IncorrectVewModel_Fails()
        {
            Assert.True(AopSettingsFactory.Validate(typeof(Model)));
            Assert.False(AopSettingsFactory.Validate(typeof(NonVirtualPropModel)));
            Assert.False(AopSettingsFactory.Validate(typeof(NonPublicPropGetterModel)));
            Assert.False(AopSettingsFactory.Validate(typeof(NonPublicPropSetterModel)));
        }

        [Fact]
        public void Decorate_PropWithoutSettingAttribute_SettingsCallIsNotInjected()
        {
            var injectedVm = AopSettingsFactory.Decorate(_settingsStoreMock.Object, new Model());
            _settingsStoreMock.Setup(store => store.Save(It.IsAny<PropertyInfo>(), It.IsAny<object>()))
                .Callback(() => throw new InvalidOperationException());

            injectedVm.Prop = 5;
        }

        [Fact]
        public void Decorate_PropWithInpcAttribute_InpcCallInjected()
        {
            var injectedVm = AopSettingsFactory.Decorate(_settingsStoreMock.Object, new Model());
            _settingsStoreMock.Setup(store => store.Save(It.IsAny<PropertyInfo>(), It.IsAny<object>()))
                .Callback(() => throw new InvalidOperationException());

            Assert.Throws<InvalidOperationException>(() => injectedVm.InjectProp = 5);
        }

        [Fact]
        public void Decorate_NullAsTarget_Fails()
        {
            Model viewModel = null;
            Assert.Throws<ArgumentNullException>(() => AopSettingsFactory.Decorate(_settingsStoreMock.Object, viewModel));
        }


        public class Model
        {
            public Model()
            {
            }

            public Model(int prop)
            {
                Prop = prop;
            }

            public virtual int Prop { get; set; }

            [Setting]
            public virtual int InjectProp { get; set; }
        }

        public class NonVirtualPropModel
        {
            [Setting]
            public int Prop { get; set; }
        }

        public class NonPublicPropGetterModel
        {
            [Setting]
            public virtual int Prop { internal get; set; }
        }

        public class NonPublicPropSetterModel
        {
            [Setting]
            public virtual int Prop { get; internal set; }
        }
    }
}
