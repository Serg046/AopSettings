using System.Reflection;
using Moq;
using Xunit;

namespace AopSettings.UnitTests
{
    public class SettingsInterceptorTests
    {
        private readonly Mock<ISettingsStore> _settingsStoreMock;
        private readonly Mock<Castle.DynamicProxy.IInvocation> _invocationMock;
        private readonly SettingsInterceptor _inteceptor;

        public SettingsInterceptorTests()
        {
            _settingsStoreMock = new Mock<ISettingsStore>();
            _invocationMock = new Mock<Castle.DynamicProxy.IInvocation>();
            _invocationMock.SetupGet(invocation => invocation.TargetType).Returns(GetType());
            _inteceptor = new SettingsInterceptor(_settingsStoreMock.Object);
        }

        [Fact]
        public void Intercept_GetterWithSettingAttribute_CallsReadOperation()
        {
            _invocationMock.Setup(invocation => invocation.Method)
                .Returns(GetType().GetProperty(nameof(SettingWithAttribute)).GetMethod);

            _inteceptor.Intercept(_invocationMock.Object);

            _settingsStoreMock
                .Verify(store => store.Read(It.Is<PropertyInfo>(pInfo => pInfo.Name == nameof(SettingWithAttribute))),
                Times.Once);
        }

        [Fact]
        public void Intercept_GetterWithoutSettingAttribute_DoNotCallReadOperation()
        {
            _invocationMock.Setup(invocation => invocation.Method)
                .Returns(GetType().GetProperty(nameof(SettingWithoutAttribute)).GetMethod);

            _inteceptor.Intercept(_invocationMock.Object);

            _settingsStoreMock
                .Verify(store => store.Read(It.IsAny<PropertyInfo>()),
                    Times.Never);
        }

        [Fact]
        public void Intercept_SetterWithSettingAttribute_CallsReadOperation()
        {
            var valueToSave = "test";
            _invocationMock.Setup(invocation => invocation.Method)
                .Returns(GetType().GetProperty(nameof(SettingWithAttribute)).SetMethod);
            _invocationMock.SetupGet(invocation => invocation.Arguments).Returns(new object[] {valueToSave});

            _inteceptor.Intercept(_invocationMock.Object);

            _settingsStoreMock
                .Verify(store => store.Save(It.Is<PropertyInfo>(pInfo => pInfo.Name == nameof(SettingWithAttribute)), valueToSave),
                    Times.Once);
        }

        [Fact]
        public void Intercept_SetterWithoutSettingAttribute_DoNotCallReadOperation()
        {
            _invocationMock.Setup(invocation => invocation.Method)
                .Returns(GetType().GetProperty(nameof(SettingWithoutAttribute)).SetMethod);

            _inteceptor.Intercept(_invocationMock.Object);

            _settingsStoreMock
                .Verify(store => store.Save(It.IsAny<PropertyInfo>(), It.IsAny<object>()),
                    Times.Never);
        }


        [Setting]
        public object SettingWithAttribute { get; set; }

        public object SettingWithoutAttribute { get; set; }

    }
}
