using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorApp1.Services
{
    public class ImageHandlingService
    {
        private readonly HttpClient _httpClient;
        private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg"
        };

        public ImageHandlingService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Checks if a URL is a valid image URL
        /// </summary>
        public bool IsImageUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            try
            {
                var uri = new Uri(url);
                var extension = Path.GetExtension(uri.AbsolutePath).ToLower();
                return ImageExtensions.Contains(extension);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates if an image URL is accessible
        /// </summary>
        public async Task<bool> ValidateImageUrlAsync(string url)
        {
            if (!IsImageUrl(url))
                return false;

            try
            {
                // Try GET with Range header to avoid full download
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Range", "bytes=0-0");
                
                using (var response = await _httpClient.SendAsync(request))
                {
                    return response.IsSuccessStatusCode && 
                           response.Content.Headers.ContentType?.MediaType?.StartsWith("image/") == true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Converts image URL to Base64 string
        /// </summary>
        public async Task<string> ConvertUrlToBase64Async(string url)
        {
            if (!IsImageUrl(url))
                throw new InvalidOperationException("URL is not a valid image URL");

            try
            {
                using (var response = await _httpClient.GetAsync(url))
                {
                    if (!response.IsSuccessStatusCode)
                        throw new InvalidOperationException($"Failed to download image. Status code: {response.StatusCode}");

                    var imageBytes = await response.Content.ReadAsByteArrayAsync();
                    var base64String = Convert.ToBase64String(imageBytes);
                    
                    // Determine MIME type
                    var uri = new Uri(url);
                    var extension = Path.GetExtension(uri.AbsolutePath).ToLower();
                    var mimeType = GetMimeType(extension);
                    
                    return $"data:{mimeType};base64,{base64String}";
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to convert image to base64: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Downloads an image from URL
        /// </summary>
        public async Task<byte[]> DownloadImageAsync(string url)
        {
            if (!IsImageUrl(url))
                throw new InvalidOperationException("URL is not a valid image URL");

            try
            {
                using (var response = await _httpClient.GetAsync(url))
                {
                    if (!response.IsSuccessStatusCode)
                        throw new InvalidOperationException($"Failed to download image. Status code: {response.StatusCode}");

                    return await response.Content.ReadAsByteArrayAsync();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to download image: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets MIME type from file extension
        /// </summary>
        private string GetMimeType(string extension)
        {
            return extension.ToLower() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                ".svg" => "image/svg+xml",
                _ => "image/jpeg"
            };
        }

        /// <summary>
        /// Detects if a string value is likely an image URL
        /// </summary>
        public bool LooksLikeImageUrl(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            value = value.ToLower().Trim();
            
            // Check if it starts with http/https and contains image extension
            if ((value.StartsWith("http://") || value.StartsWith("https://")) && 
                ImageExtensions.Any(ext => value.Contains(ext)))
            {
                return true;
            }

            // Check if it's just an image extension
            return ImageExtensions.Any(ext => value.EndsWith(ext));
        }

        /// <summary>
        /// Detects if a column name is likely an image column
        /// </summary>
        public bool IsImageColumn(string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
                return false;

            var lowerName = columnName.ToLower().Trim();
            
            // Common image column names
            var imageColumnNames = new[] 
            { 
                "image", "img", "photo", "picture", 
                "imageurl", "image_url", "img_url", 
                "photourl", "photo_url", "pictureurl", "picture_url",
                "thumbnail", "avatar", "icon", "logo",
                "image_link", "imagelink"
            };

            return imageColumnNames.Contains(lowerName);
        }
    }
}
