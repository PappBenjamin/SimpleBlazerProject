using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace BlazorApp1.Services
{
    /// <summary>
    /// Stores and manages all JSON data in memory as a Singleton.
    /// All CRUD operations (Create, Read, Update, Delete) go through this service.
    /// </summary>
    public class DataManagementService
    {
        // The main data storage
        private List<Dictionary<string, object>> _currentData;

        public DataManagementService()
        {
            _currentData = new List<Dictionary<string, object>>();
        }

        // Load data from uploaded file
        public void InitializeData(List<Dictionary<string, object>> data, string fileName)
        {
            _currentData = new List<Dictionary<string, object>>(data);
        }

        // Get all data
        public List<Dictionary<string, object>> GetAllData()
        {
            return new List<Dictionary<string, object>>(_currentData);
        }

        // Get one item by index
        public Dictionary<string, object> GetItemByIndex(int index)
        {
            if (index < 0 || index >= _currentData.Count)
                throw new IndexOutOfRangeException($"Index {index} is out of range");

            return new Dictionary<string, object>(_currentData[index]);
        }

        // Get total count
        public int GetItemCount()
        {
            return _currentData.Count;
        }

        // Add new item
        public int AddItem(Dictionary<string, object> item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _currentData.Add(new Dictionary<string, object>(item));
            return _currentData.Count - 1;
        }

        // Update existing item
        public void UpdateItem(int index, Dictionary<string, object> updatedItem)
        {
            if (index < 0 || index >= _currentData.Count)
                throw new IndexOutOfRangeException($"Index {index} is out of range");

            if (updatedItem == null)
                throw new ArgumentNullException(nameof(updatedItem));

            _currentData[index] = new Dictionary<string, object>(updatedItem);
        }

        // Delete item by index
        public void DeleteItem(int index)
        {
            if (index < 0 || index >= _currentData.Count)
                throw new IndexOutOfRangeException($"Index {index} is out of range");

            _currentData.RemoveAt(index);
        }

        // Filter by column value
        public List<Dictionary<string, object>> FilterByColumn(string columnName, string value)
        {
            return _currentData
                .Where(item => item.ContainsKey(columnName) && 
                               item[columnName]?.ToString()?.Contains(value, StringComparison.OrdinalIgnoreCase) == true)
                .ToList();
        }

        // Search across all columns
        public List<Dictionary<string, object>> SearchMultipleColumns(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<Dictionary<string, object>>(_currentData);

            return _currentData
                .Where(item => item.Values.Any(v => 
                    v?.ToString()?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true))
                .ToList();
        }

        // Sort by column
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

            sorted.AddRange(_currentData.Where(item => !item.ContainsKey(columnName)));
            return sorted;
        }

        // Get unique values in a column
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

        // Get all column names
        public List<string> GetColumnNames()
        {
            if (!_currentData.Any())
                return new List<string>();

            return _currentData[0].Keys.ToList();
        }

        // Export to JSON format
        public string ExportToJson()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(_currentData, options);
        }

        // Export to CSV format
        public string ExportToCsv()
        {
            if (!_currentData.Any())
                return string.Empty;

            var columnNames = GetColumnNames();
            var csv = new System.Text.StringBuilder();

            csv.AppendLine(string.Join(",", columnNames.Select(c => EscapeCsvValue(c))));

            foreach (var item in _currentData)
            {
                var row = columnNames.Select(col => 
                    EscapeCsvValue(item.ContainsKey(col) ? item[col]?.ToString() ?? "" : ""));
                csv.AppendLine(string.Join(",", row));
            }

            return csv.ToString();
        }

        // Get paged data
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

        // Clear all data
        public void ClearData()
        {
            _currentData.Clear();
        }

        // Escape CSV special characters
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

        // Custom comparer for sorting mixed data types
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
