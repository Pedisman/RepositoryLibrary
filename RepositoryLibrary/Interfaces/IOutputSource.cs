namespace RepositoryLibrary.Interfaces
{
    public interface IOutputSource
    {
        object this[string key]
        {
            get;
        }
    }
}
