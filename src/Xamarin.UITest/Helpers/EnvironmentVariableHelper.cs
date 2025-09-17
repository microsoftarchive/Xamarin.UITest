using System;
using System.Collections.Generic;
using System.Linq;

namespace Xamarin.UITest.Helpers
{
    /// <summary>
    /// Helper class for handling Environment Variables to aid testability.
    /// </summary>
    public class EnvironmentVariableHelper : IEnvironmentVariableHelper
    {
        /// <summary>
        /// Gets an environment variable from the system.
        /// </summary>
        public string GetEnvironmentVariable(string key)
        {
            return Environment.GetEnvironmentVariable(key);
        }

        /// <summary>
        /// Gets all environment variables from the system.
        /// </summary>
        public Dictionary<string, string> GetEnvironmentVariables()
        {
            var variables = Environment.GetEnvironmentVariables();
            var output = variables.Keys.Cast<object>().ToDictionary(k => k.ToString(), v => variables[v].ToString());

            return output;
        }
    }
}
