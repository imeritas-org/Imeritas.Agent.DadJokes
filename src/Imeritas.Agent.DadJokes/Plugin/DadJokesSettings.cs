using Imeritas.Agent.Plugins.Configuration;

namespace Imeritas.Agent.DadJokes.Plugin;

/// <summary>
/// Configuration settings for the DadJokes plugin.
/// Properties decorated with <see cref="PluginSettingAttribute"/> are auto-generated
/// into admin UI fields by the framework.
/// </summary>
public class DadJokesSettings
{
    /// <summary>
    /// Maximum number of jokes the plugin will deliver per session.
    /// Prevents excessive joke-telling in a single conversation.
    /// </summary>
    [PluginSetting("Max Jokes Per Session",
        Key = "maxJokesPerSession",
        Description = "Maximum number of jokes to deliver in a single session. Default: 10.",
        Order = 1)]
    public int MaxJokesPerSession { get; set; } = 10;
}
