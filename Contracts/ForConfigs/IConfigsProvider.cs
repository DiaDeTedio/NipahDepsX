using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DepsX.Contracts.ForConfigs;

public interface IConfigsProvider
{
    T GetConfig<T>(string section);
}
