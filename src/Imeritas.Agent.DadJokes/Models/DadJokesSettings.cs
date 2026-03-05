using Imeritas.Agent.Plugins.Configuration;

namespace Imeritas.Agent.DadJokes.Models;

/// <summary>
/// Configuration settings for the Dad Jokes plugin.
/// Properties decorated with <see cref="PluginSettingAttribute"/> are auto-discovered
/// by the framework to generate admin UI forms, validation, and typed access.
/// </summary>
public class DadJokesSettings
{
    /// <summary>
    /// Maximum number of jokes the plugin will deliver in a single chat session.
    /// Prevents joke fatigue by capping output per session.
    /// </summary>
    [PluginSetting("Max Jokes Per Session",
        Key = "maxJokesPerSession",
        Description = "Maximum number of jokes the plugin will deliver in a single chat session.",
        Order = 1)]
    public int MaxJokesPerSession { get; set; } = 10;
}
