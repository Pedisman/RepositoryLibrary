using RepositoryLibrary.Interfaces;
using RepositoryLibrary.OutputSources;

namespace RepositoryLibrary.Factories
{
    public class ReaderOutputSourceFactory : IOutputSourceFactory
    {
        public IOutputSource Create(object outputSource)
        {
            return new ReaderOutputSource(outputSource);
        }
    }
}
