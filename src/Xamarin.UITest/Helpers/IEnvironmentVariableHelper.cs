using System;
using System.Collections.Generic;

namespace Xamarin.UITest.Helpers
{
    /// <summary>
    /// Helper class for handling Environment Variables to aid testability.
    /// </summary>
    public interface IEnvironmentVariableHelper
    {
        /// <summary>
        /// Gets an environment variable from the system.
        /// </summary>
        string GetEnvironmentVariable(string key);

        /// <summary>
        /// Gets all environment variables from the system.
        /// </summary>
        Dictionary<string, string> GetEnvironmentVariables();
    }
}
