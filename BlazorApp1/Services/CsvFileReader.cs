using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BlazorApp1.Services
{
    public class CsvFileReader : IFileReader
    {
        public async Task<List<string[]>> ReadFile(Stream stream)
        {
            var rows = new List<string[]>();
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    var values = line.Split(',');
                    rows.Add(values);
                }
            }
            return rows;
        }
    }
}
