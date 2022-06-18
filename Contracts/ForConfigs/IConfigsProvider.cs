﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiProtect.App.DepsX.Contracts.ForConfigs;

public interface IConfigsProvider
{
    T GetConfig<T>(string section);
}
