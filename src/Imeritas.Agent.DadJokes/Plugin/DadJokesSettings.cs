using Imeritas.Agent.Plugins.Configuration;

namespace Imeritas.Agent.DadJokes.Plugin;

/// <summary>
/// Configuration settings for the Dad Jokes plugin.
/// Properties decorated with <see cref="PluginSettingAttribute"/> are auto-generated
/// into admin UI fields by the framework.
/// </summary>
public class DadJokesSettings
{
    /// <summary>
    /// Maximum number of jokes the plugin will tell per session.
    /// Prevents overuse of the joke capability in a single conversation.
    /// </summary>
    [PluginSetting("Max Jokes Per Session",
        Key = "maxJokesPerSession",
        Description = "Maximum number of jokes to tell in a single session. Set to 0 for unlimited.",
        Order = 1)]
    public int MaxJokesPerSession { get; set; } = 10;
}
