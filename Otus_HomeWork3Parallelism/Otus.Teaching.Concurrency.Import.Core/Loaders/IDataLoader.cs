using System.Threading.Tasks;

namespace Otus.Teaching.Concurrency.Import.Core.Loaders
{
    public interface IDataLoader
    {
        Task LoadDataAsync(string path);
        string GetData();
        int GetSpaceCount();
    }
}