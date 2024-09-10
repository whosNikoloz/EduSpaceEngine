using EduSpaceEngine.Services;

namespace EduSpaceEngine.GraphQL
{
    public class Mutation
    {
        public string SayHello(string name) => $"Hello, {name}!";
    }
}
