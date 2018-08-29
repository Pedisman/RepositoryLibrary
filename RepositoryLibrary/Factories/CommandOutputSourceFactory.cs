using System;
using RepositoryLibrary.Interfaces;
using RepositoryLibrary.OutputSources;

namespace RepositoryLibrary.Factories
{
    public class CommandOutputSourceFactory : IOutputSourceFactory
    {
        public IOutputSource Create(object outputSource)
        {
            return new CommandOutputSource(outputSource);
        }
    }
}
