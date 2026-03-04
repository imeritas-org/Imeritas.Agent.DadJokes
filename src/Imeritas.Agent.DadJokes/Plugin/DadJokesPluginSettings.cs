using Imeritas.Agent.Plugins.Configuration;

namespace Imeritas.Agent.DadJokes.Plugin;

/// <summary>
/// Tenant-level settings for the DadJokes plugin.
/// </summary>
public class DadJokesPluginSettings
{
    [PluginSetting("Max Jokes Per Session",
        Description = "Maximum number of jokes the plugin will tell in a single session",
        Order = 1)]
    public int MaxJokesPerSession { get; set; } = 10;
}
