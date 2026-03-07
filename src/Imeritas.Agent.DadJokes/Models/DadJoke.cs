namespace Imeritas.Agent.DadJokes.Models;

public class DadJoke
{
    public string Id { get; set; } = string.Empty;
    public string Setup { get; set; } = string.Empty;
    public string Punchline { get; set; } = string.Empty;
    public List<string> Categories { get; set; } = [];
}
