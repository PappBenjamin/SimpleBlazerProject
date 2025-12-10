using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BlazorApp1.Services
{
    public class FileReaderService
    {
        private readonly Dictionary<string, IFileReader> _fileReaders;

        public FileReaderService()
        {
            _fileReaders = new Dictionary<string, IFileReader>(StringComparer.OrdinalIgnoreCase)
            {
                { ".csv", new CsvFileReader() },
                { ".json", new JsonFileReader() },
                { ".txt", new TxtFileReader() },
            };
        }

        public Task<List<string[]>> ReadFile(Stream stream, string fileName)
        {
            var extension = Path.GetExtension(fileName);
            if (_fileReaders.TryGetValue(extension, out var reader))
            {
                return reader.ReadFile(stream);
            }

            throw new NotSupportedException($"File type {extension} is not supported.");
        }
    }
}
