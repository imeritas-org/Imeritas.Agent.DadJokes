using Imeritas.Agent.DadJokes.Models;

namespace Imeritas.Agent.DadJokes.Services;

/// <summary>
/// Static embedded collection of dad jokes.
/// </summary>
internal static class JokeData
{
    internal static readonly IReadOnlyList<Joke> AllJokes =
    [
        new("j01",
            "Why do programmers prefer dark mode?",
            "Because light attracts bugs.",
            ["tech"]),

        new("j02",
            "What do you call a fake noodle?",
            "An impasta.",
            ["food"]),

        new("j03",
            "Why don't scientists trust atoms?",
            "Because they make up everything.",
            ["science"]),

        new("j04",
            "What do you call a bear with no teeth?",
            "A gummy bear.",
            ["animals", "food"]),

        new("j05",
            "Why did the scarecrow win an award?",
            "Because he was outstanding in his field.",
            ["work", "general"]),

        new("j06",
            "What do you call a dog that does magic tricks?",
            "A Labracadabrador.",
            ["animals"]),

        new("j07",
            "Why did the coffee file a police report?",
            "It got mugged.",
            ["food", "general"]),

        new("j08",
            "How does a penguin build its house?",
            "Igloos it together.",
            ["animals", "science"]),

        new("j09",
            "Why do Java developers wear glasses?",
            "Because they don't C#.",
            ["tech"]),

        new("j10",
            "What did the ocean say to the beach?",
            "Nothing, it just waved.",
            ["science", "general"]),

        new("j11",
            "Why don't eggs tell jokes?",
            "They'd crack each other up.",
            ["food", "general"]),

        new("j12",
            "What do you call a musician who lost their instrument?",
            "A singer.",
            ["music"]),

        new("j13",
            "Why did the gym close down?",
            "It just didn't work out.",
            ["sports", "work"]),

        new("j14",
            "What do you call a sleeping dinosaur?",
            "A dino-snore.",
            ["animals", "science"]),

        new("j15",
            "Why did the developer go broke?",
            "Because he used up all his cache.",
            ["tech", "work"]),

        new("j16",
            "What do you call a fish without eyes?",
            "A fsh.",
            ["animals", "general"]),

        new("j17",
            "Why did the bicycle fall over?",
            "Because it was two-tired.",
            ["sports", "general"]),

        new("j18",
            "What do you call a piano that keeps falling down the stairs?",
            "A flat minor.",
            ["music", "general"]),

        new("j19",
            "Why do golfers bring an extra pair of pants?",
            "In case they get a hole in one.",
            ["sports"]),

        new("j20",
            "What's the best thing about Switzerland?",
            "I don't know, but the flag is a big plus.",
            ["general", "science"])
    ];
}
