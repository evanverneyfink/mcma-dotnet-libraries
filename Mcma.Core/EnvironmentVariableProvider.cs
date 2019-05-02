
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mcma.Core
{
    public class EnvironmentVariableProvider : IContextVariableProvider
    {
        public IReadOnlyDictionary<string, string> ContextVariables { get; } =
            Environment.GetEnvironmentVariables().Keys.OfType<string>().ToDictionary(k => Environment.GetEnvironmentVariable(k));

        public static EnvironmentVariableProvider Instance { get; } = new EnvironmentVariableProvider();
    }
}