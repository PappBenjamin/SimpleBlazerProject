using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlazorApp1.Services
{
    public class JsonFileReader : IFileReader
    {
        public async Task<List<string[]>> ReadFile(Stream stream)
        {
            var rows = new List<string[]>();
            var json = await new StreamReader(stream).ReadToEndAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var data = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json, options);

            if (data != null && data.Count > 0)
            {
                var headers = new List<string>();
                foreach (var key in data[0].Keys)
                {
                    headers.Add(key);
                }
                rows.Add(headers.ToArray());

                foreach (var dict in data)
                {
                    var row = new List<string>();
                    foreach (var key in headers)
                    {
                        row.Add(dict[key].ToString());
                    }
                    rows.Add(row.ToArray());
                }
            }

            return rows;
        }
    }
}
