namespace Imeritas.Agent.DadJokes.Services;

using Imeritas.Agent.DadJokes.Models;

public interface IJokeService
{
    List<DadJoke> GetByCategory(string category);
    DadJoke GetRandom();
    List<string> GetAllCategories();
}
