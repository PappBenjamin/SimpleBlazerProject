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
                // Get all unique keys from all rows to handle rows with missing fields
                var allKeys = new HashSet<string>();
                foreach (var item in data)
                {
                    foreach (var key in item.Keys)
                    {
                        allKeys.Add(key);
                    }
                }

                var headers = new List<string>(allKeys);
                rows.Add(headers.ToArray());

                // Build rows with all columns, using empty string for missing values
                foreach (var dict in data)
                {
                    var row = new List<string>();
                    foreach (var key in headers)
                    {
                        // If key exists in this row, use its value; otherwise use empty string
                        if (dict.ContainsKey(key))
                        {
                            row.Add(dict[key]?.ToString() ?? "");
                        }
                        else
                        {
                            row.Add("");
                        }
                    }
                    rows.Add(row.ToArray());
                }
            }

            return rows;
        }
    }
}
