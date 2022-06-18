using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DepsX.Contracts.ForConfigs;
using DepsX.Impl.ForConfigs;

namespace DepsX;

public static class Configs
{
    static List<IConfigsProvider> providers = new (32);

    static string FormatForEnvironment(string input)
    {
        if (input.Contains("{env}"))
        {
#if DEBUG
            input = input.Replace("{env}", "Development");
#elif RELEASE
            input = input.Replace("{env}", "Production");
#endif
        }
        return input;
    }

    public static void AddProvider(string json) => providers.Add(new JsonConfigsProvider(json));
    public static void AddProviderFile(string jsonFile)
    {
        jsonFile = FormatForEnvironment(jsonFile);

        string json = File.ReadAllText(jsonFile);
        AddProvider(json);
    }
    public static void AddProviderFromAssemblyResource(string name, Assembly assembly, bool formatNameForAssembly = true)
    {
        name = FormatForEnvironment(name);

        if(formatNameForAssembly)
            name = assembly.GetName().Name + '.' + name;

        using var stream = assembly.GetManifestResourceStream(name) ?? throw new Exception("Can't find {name} in assembly resources");

        using var reader = new StreamReader(stream);
        string json = reader.ReadToEnd();
        AddProvider(json);
    }

    public static void AddProvider(IConfigsProvider provider) => providers.Add(provider);

    public static T GetConfigs<T>(string section)
    {
        foreach (var provider in providers)
            if (provider.GetConfig<T>(section) is T configs and not null)
                return configs;
        throw new Exception($"Can't find configs for {section}");
    }

    public static void InjectConfigs<T>(string section) where T : class
    {
        Deps.InjectSingleton<T, T>(() => GetConfigs<T>(section));
    }
}
