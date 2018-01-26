# AopSettings

Castle.DynamicProxy based library that provides a way to create application settings using AOP style.

## Declaration:
```csharp
public class Model
{
    [Setting(Default = 600)]
    public virtual int Width { get; set; }
}

//You can use existing ExeConfigurationSettingsStoreProvider instead
class SettingsStoreProvider : ISettingsStoreProvider
{
    private readonly Dictionary<string, object> _settings = new Dictionary<string, object>();

    public string GetSettingName(PropertyInfo property)
    {
        Debug.Assert(property?.DeclaringType != null);
        return $"{property.DeclaringType.FullName}.{property.Name}";
    }

    public bool Contains(string settingName)
    {
        Debug.Assert(!string.IsNullOrWhiteSpace(settingName));
        return _settings.ContainsKey(settingName);
    }

    public object Read(string settingName)
    {
        Debug.Assert(!string.IsNullOrWhiteSpace(settingName));
        Debug.Assert(Contains(settingName));
        var value = _settings[settingName];
        Console.WriteLine($"The setting '{settingName}' has been read, value is {value}");
        return value;
    }

    public void Save(string settingName, object value)
    {
        Debug.Assert(!string.IsNullOrWhiteSpace(settingName));
        _settings[settingName] = value;
        Console.WriteLine($"The setting '{settingName}' has been written, value is {value}");
    }
}
```
## Usage:
- AopInpcFactory
```csharp
static void Main(string[] args)
{
    var provider = new SettingsStoreProvider();
    //var provider = new ExeConfigurationSettingsStoreProvider(ConfigurationUserLevel.None, "test_section");
    var settingStore = new SettingsStore(provider);
    var model = AopSettingsFactory.Create<Model>(settingStore);
    //var model = AopSettingsFactory.Decorate(store, new Model());
    Console.WriteLine($"Default value is {model.Width}, no calls to provider");
    model.Width = 800;
    Console.WriteLine($"Current value is {model.Width}, retrieved from provider");
}
```
- DI-container
```csharp
static void Main(string[] args)
{
    var provider = new SettingsStoreProvider();
    //var provider = new ExeConfigurationSettingsStoreProvider(ConfigurationUserLevel.None, "test_section");
    var settingsStore = new SettingsStore(provider);
    var containerBuilder = new ContainerBuilder();
    containerBuilder.Register(ctx => new SettingsInterceptor(settingsStore));
    containerBuilder.RegisterType<Model>()
        .EnableClassInterceptors()
        .InterceptedBy(typeof(SettingsInterceptor));
    var container = containerBuilder.Build();

    var model = container.Resolve<Model>();
    Console.WriteLine($"Default value is {model.Width}, no calls to provider");
    model.Width = 800;
    Console.WriteLine($"Current value is {model.Width}, retrieved from provider");
}
```
- Castle.DynamicProxy ProxyGenerator
```csharp
static void Main(string[] args)
{
    var provider = new SettingsStoreProvider();
    //var provider = new ExeConfigurationSettingsStoreProvider(ConfigurationUserLevel.None, "test_section");
    var settingsStore = new SettingsStore(provider);
    var model = new ProxyGenerator().CreateClassProxy<Model>(new SettingsInterceptor(settingsStore));
    Console.WriteLine($"Default value is {model.Width}, no calls to provider");
    model.Width = 800;
    Console.WriteLine($"Current value is {model.Width}, retrieved from provider");
}
```
## Output:
>Default value is 600, no calls to provider  
>The setting 'DemoApp.Model.Width' has been written, value is 800  
>The setting 'DemoApp.Model.Width' has been read, value is 800  
>Current value is 800, retrieved from provider
## Other features:
- SettingsStore provides possibility to get/set a setting outside of setting's instance:
```csharp
static void Main(string[] args)
{
    var provider = new SettingsStoreProvider();
    var store = new SettingsStore(provider);
    store.Save((Model m) => m.Width, 777);
    var currentWidth = store.Read((Model m) => m.Width);
    Console.WriteLine($"Current value is {currentWidth}, retrieved from provider");
}
```
>The setting 'DemoApp.Model.Width' has been written, value is 777  
>The setting 'DemoApp.Model.Width' has been read, value is 777  
>Current value is 777, retrieved from provider
- SettingsStore provides possibility to bind a setting to some instance
```csharp
static void Main(string[] args)
{
    var settingsStore = new SettingsStore(new SettingsStoreProvider());
    var model = AopSettingsFactory.Create<Model>(settingsStore);
    int externalObject = -1; 
    Console.WriteLine(externalObject);
    settingsStore.Bind((Model m) => m.Width, settingValue => externalObject = (int)settingValue);
    Console.WriteLine(externalObject);
    model.Width = 800;
    Console.WriteLine(externalObject);
}
```
>-1  
>600  
>The setting 'DemoApp.Model.Width' has been written, value is 800  
>800
