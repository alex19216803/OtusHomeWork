using System;
using System.Threading;
using System.IO;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;
using System.Threading.Tasks;

namespace Otus.Teaching.Concurrency.Import.Core.Loaders
{
    public class DataLoader
        : IDataLoader
    {
        private string _data;

        public async Task LoadDataAsync(string path)
        {
            _data = await File.ReadAllTextAsync(path);
        }

        public string GetData()
        {
            return _data;
        }

        public int GetSpaceCount()
        {
            return _data?.Count(c => c == ' ') ?? 0;
        }
    }
}