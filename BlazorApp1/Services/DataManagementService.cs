using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace BlazorApp1.Services
{
    public class DataManagementService
    {
        private List<Dictionary<string, object>> _currentData;

        public DataManagementService()
        {
            _currentData = new List<Dictionary<string, object>>();
        }

        /// <summary>
        /// Initializes the service with JSON data
        /// </summary>
        public void InitializeData(List<Dictionary<string, object>> data, string fileName)
        {
            _currentData = new List<Dictionary<string, object>>(data);
        }

        /// <summary>
        /// Gets all current data
        /// </summary>
        public List<Dictionary<string, object>> GetAllData()
        {
            return new List<Dictionary<string, object>>(_currentData);
        }

        /// <summary>
        /// Gets a specific item by index
        /// </summary>
        public Dictionary<string, object> GetItemByIndex(int index)
        {
            if (index < 0 || index >= _currentData.Count)
                throw new IndexOutOfRangeException($"Index {index} is out of range");

            return new Dictionary<string, object>(_currentData[index]);
        }

        /// <summary>
        /// Gets the total count of items
        /// </summary>
        public int GetItemCount()
        {
            return _currentData.Count;
        }

        /// <summary>
        /// Adds a new item to the data
        /// </summary>
        public int AddItem(Dictionary<string, object> item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _currentData.Add(new Dictionary<string, object>(item));
            return _currentData.Count - 1;
        }

        /// <summary>
        /// Updates an existing item by index
        /// </summary>
        public void UpdateItem(int index, Dictionary<string, object> updatedItem)
        {
            if (index < 0 || index >= _currentData.Count)
                throw new IndexOutOfRangeException($"Index {index} is out of range");

            if (updatedItem == null)
                throw new ArgumentNullException(nameof(updatedItem));

            _currentData[index] = new Dictionary<string, object>(updatedItem);
        }

        /// <summary>
        /// Deletes an item by index
        /// </summary>
        public void DeleteItem(int index)
        {
            if (index < 0 || index >= _currentData.Count)
                throw new IndexOutOfRangeException($"Index {index} is out of range");

            _currentData.RemoveAt(index);
        }

        /// <summary>
        /// Filters data by a specific column and value
        /// </summary>
        public List<Dictionary<string, object>> FilterByColumn(string columnName, string value)
        {
            return _currentData
                .Where(item => item.ContainsKey(columnName) && 
                               item[columnName]?.ToString()?.Contains(value, StringComparison.OrdinalIgnoreCase) == true)
                .ToList();
        }

        /// <summary>
        /// Searches across multiple columns
        /// </summary>
        public List<Dictionary<string, object>> SearchMultipleColumns(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<Dictionary<string, object>>(_currentData);

            return _currentData
                .Where(item => item.Values.Any(v => 
                    v?.ToString()?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true))
                .ToList();
        }

        /// <summary>
        /// Sorts data by a specific column
        /// </summary>
        public List<Dictionary<string, object>> SortByColumn(string columnName, bool ascending = true)
        {
            if (string.IsNullOrWhiteSpace(columnName) || !_currentData.Any())
                return new List<Dictionary<string, object>>(_currentData);

            var sorted = _currentData
                .Where(item => item.ContainsKey(columnName))
                .ToList();

            if (ascending)
            {
                sorted = sorted.OrderBy(item => item[columnName], new CustomComparer()).ToList();
            }
            else
            {
                sorted = sorted.OrderByDescending(item => item[columnName], new CustomComparer()).ToList();
            }

            // Add items that don't have the column at the end
            sorted.AddRange(_currentData.Where(item => !item.ContainsKey(columnName)));

            return sorted;
        }

        /// <summary>
        /// Gets distinct values for a column
        /// </summary>
        public List<string> GetDistinctValues(string columnName)
        {
            return _currentData
                .Where(item => item.ContainsKey(columnName))
                .Select(item => item[columnName]?.ToString() ?? "")
                .Distinct()
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .OrderBy(v => v)
                .ToList();
        }

        /// <summary>
        /// Gets all column names from the data
        /// </summary>
        public List<string> GetColumnNames()
        {
            if (!_currentData.Any())
                return new List<string>();

            return _currentData[0].Keys.ToList();
        }

        /// <summary>
        /// Exports current data to JSON format
        /// </summary>
        public string ExportToJson()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(_currentData, options);
        }

        /// <summary>
        /// Exports current data to CSV format
        /// </summary>
        public string ExportToCsv()
        {
            if (!_currentData.Any())
                return string.Empty;

            var columnNames = GetColumnNames();
            var csv = new System.Text.StringBuilder();

            // Write header
            csv.AppendLine(string.Join(",", columnNames.Select(c => EscapeCsvValue(c))));

            // Write data rows
            foreach (var item in _currentData)
            {
                var row = columnNames.Select(col => 
                    EscapeCsvValue(item.ContainsKey(col) ? item[col]?.ToString() ?? "" : ""));
                csv.AppendLine(string.Join(",", row));
            }

            return csv.ToString();
        }

        /// <summary>
        /// Gets pagination of data
        /// </summary>
        public (List<Dictionary<string, object>> Items, int TotalPages) GetPagedData(int pageNumber, int pageSize)
        {
            if (pageNumber < 1 || pageSize < 1)
                throw new ArgumentException("Page number and page size must be greater than 0");

            var totalPages = (int)Math.Ceiling(_currentData.Count / (double)pageSize);
            var items = _currentData
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (items, totalPages);
        }

        /// <summary>
        /// Clears all data
        /// </summary>
        public void ClearData()
        {
            _currentData.Clear();
        }

        private string EscapeCsvValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "\"\"";

            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }

            return value;
        }

        // Custom comparer for sorting
        private class CustomComparer : IComparer<object?>
        {
            public int Compare(object? x, object? y)
            {
                if (x == null && y == null) return 0;
                if (x == null) return -1;
                if (y == null) return 1;

                if (x is IComparable xComparable && y is IComparable yComparable)
                {
                    try
                    {
                        return xComparable.CompareTo(yComparable);
                    }
                    catch
                    {
                        return StringComparer.Ordinal.Compare(x.ToString(), y.ToString());
                    }
                }

                return StringComparer.Ordinal.Compare(x.ToString(), y.ToString());
            }
        }
    }
}
