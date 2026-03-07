namespace Imeritas.Agent.DadJokes.Services;

using Imeritas.Agent.DadJokes.Models;

public static class JokeData
{
    public static IReadOnlyList<DadJoke> All { get; } = new List<DadJoke>
    {
        new() { Id = "joke-01", Setup = "Why do programmers prefer dark mode?", Punchline = "Because light attracts bugs.", Categories = ["tech", "general"] },
        new() { Id = "joke-02", Setup = "What do you call a fake noodle?", Punchline = "An impasta.", Categories = ["food", "general"] },
        new() { Id = "joke-03", Setup = "What do you call a fish without eyes?", Punchline = "A fsh.", Categories = ["animals", "general"] },
        new() { Id = "joke-04", Setup = "Why did the scarecrow get a promotion?", Punchline = "He was outstanding in his field.", Categories = ["work", "general"] },
        new() { Id = "joke-05", Setup = "Why can't you trust atoms?", Punchline = "They make up everything.", Categories = ["science", "general"] },
        new() { Id = "joke-06", Setup = "Why did the musician get arrested?", Punchline = "Because she got in treble.", Categories = ["music", "general"] },
        new() { Id = "joke-07", Setup = "Why did the golfer bring two pairs of pants?", Punchline = "In case he got a hole in one.", Categories = ["sports", "general"] },
        new() { Id = "joke-08", Setup = "What's a computer's favorite snack?", Punchline = "Microchips.", Categories = ["tech"] },
        new() { Id = "joke-09", Setup = "Why did the cookie go to the doctor?", Punchline = "Because it was feeling crummy.", Categories = ["food"] },
        new() { Id = "joke-10", Setup = "What do you call a sleeping dinosaur?", Punchline = "A dino-snore.", Categories = ["animals"] },
        new() { Id = "joke-11", Setup = "Why did the employee get fired from the calendar factory?", Punchline = "He took a couple of days off.", Categories = ["work"] },
        new() { Id = "joke-12", Setup = "Why did the biologist break up with the physicist?", Punchline = "There was no chemistry.", Categories = ["science"] },
        new() { Id = "joke-13", Setup = "What do you call a musical insect?", Punchline = "A humbug.", Categories = ["music"] },
        new() { Id = "joke-14", Setup = "Why was the stadium so hot?", Punchline = "All the fans left.", Categories = ["sports"] },
        new() { Id = "joke-15", Setup = "Why do Java developers wear glasses?", Punchline = "Because they can't C#.", Categories = ["tech", "work"] },
        new() { Id = "joke-16", Setup = "What did the scientist say when he found two isotopes of helium?", Punchline = "HeHe.", Categories = ["food", "science"] },
        new() { Id = "joke-17", Setup = "Why do cows have hooves instead of feet?", Punchline = "Because they lactose.", Categories = ["animals", "science"] },
        new() { Id = "joke-18", Setup = "What do you call a fish that wears a crown?", Punchline = "A king fisher.", Categories = ["food", "animals"] },
        new() { Id = "joke-19", Setup = "Why did the computer go to the doctor?", Punchline = "Because it had a virus.", Categories = ["tech", "science"] },
        new() { Id = "joke-20", Setup = "Why did the bicycle fall over at work?", Punchline = "It was two-tired.", Categories = ["sports", "work"] },
    }.AsReadOnly();
}
