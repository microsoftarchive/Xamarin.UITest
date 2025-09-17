using System;

namespace Xamarin.UITest.Configuration
{
    /// <summary>
    /// IDE integration mode. Decides what settings to use in case of both explicit configuration and active IDE integration.
    /// </summary>
    public enum IdeIntegrationMode
    {
        /// <summary>
        /// Prefers any explicit configuration choices made by the user as part of configuration.
        /// </summary>
        PreferExplicitConfiguration,

        /// <summary>
        /// Prefers IDE integration settings if it is active.
        /// </summary>
        PreferIdeSettingsIfPresent,
    }
}