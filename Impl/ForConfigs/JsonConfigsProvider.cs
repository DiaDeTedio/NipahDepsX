using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WiProtect.App.DepsX.Contracts.ForConfigs;

namespace DepsX.Impl.ForConfigs;

public class JsonConfigsProvider : IConfigsProvider
{
    readonly JObject configs;

    public T GetConfig<T>(string section)
    {
        JToken sectionObj;
        if (section.Contains('/'))
        {
            var sections = section.Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            sectionObj = configs;
            foreach (var sec in sections)
                sectionObj = configs[sec] ?? throw new Exception($"Can't find section {sec}");
        }
        else
            sectionObj = configs[section] ?? throw new Exception($"Can't find section {section}");

        return sectionObj.ToObject<T>() ?? throw new Exception($"Problem casting section {section} into {typeof(T).Name}");
    }

    public JsonConfigsProvider(string json)
    {
        configs = JObject.Parse(json);
    }
}
