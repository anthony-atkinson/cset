using System.Text.Json;

namespace CSETWebBlazor.Services
{
    /// <summary>
    /// API client service interface for communicating with the CSET API
    /// Handles HTTP requests, authentication headers, and response processing
    /// </summary>
    public interface IApiClientService
    {
        /// <summary>
        /// Makes a GET request to the API
        /// </summary>
        /// <typeparam name="T">Response type</typeparam>
        /// <param name="endpoint">API endpoint</param>
        /// <param name="requiresAuth">Whether authentication is required</param>
        /// <returns>Deserialized response</returns>
        Task<T?> GetAsync<T>(string endpoint, bool requiresAuth = true);

        /// <summary>
        /// Makes a POST request to the API
        /// </summary>
        /// <typeparam name="T">Response type</typeparam>
        /// <param name="endpoint">API endpoint</param>
        /// <param name="data">Request data</param>
        /// <param name="requiresAuth">Whether authentication is required</param>
        /// <returns>Deserialized response</returns>
        Task<T?> PostAsync<T>(string endpoint, object? data = null, bool requiresAuth = true);

        /// <summary>
        /// Makes a PUT request to the API
        /// </summary>
        /// <typeparam name="T">Response type</typeparam>
        /// <param name="endpoint">API endpoint</param>
        /// <param name="data">Request data</param>
        /// <param name="requiresAuth">Whether authentication is required</param>
        /// <returns>Deserialized response</returns>
        Task<T?> PutAsync<T>(string endpoint, object? data = null, bool requiresAuth = true);

        /// <summary>
        /// Makes a DELETE request to the API
        /// </summary>
        /// <typeparam name="T">Response type</typeparam>
        /// <param name="endpoint">API endpoint</param>
        /// <param name="requiresAuth">Whether authentication is required</param>
        /// <returns>Deserialized response</returns>
        Task<T?> DeleteAsync<T>(string endpoint, bool requiresAuth = true);

        /// <summary>
        /// Makes a GET request and returns the raw response
        /// </summary>
        /// <param name="endpoint">API endpoint</param>
        /// <param name="requiresAuth">Whether authentication is required</param>
        /// <returns>HTTP response message</returns>
        Task<HttpResponseMessage> GetRawAsync(string endpoint, bool requiresAuth = true);

        /// <summary>
        /// Makes a POST request and returns the raw response
        /// </summary>
        /// <param name="endpoint">API endpoint</param>
        /// <param name="data">Request data</param>
        /// <param name="requiresAuth">Whether authentication is required</param>
        /// <returns>HTTP response message</returns>
        Task<HttpResponseMessage> PostRawAsync(string endpoint, object? data = null, bool requiresAuth = true);

        /// <summary>
        /// Uploads a file to the API
        /// </summary>
        /// <typeparam name="T">Response type</typeparam>
        /// <param name="endpoint">API endpoint</param>
        /// <param name="fileStream">File stream</param>
        /// <param name="fileName">File name</param>
        /// <param name="contentType">Content type</param>
        /// <param name="requiresAuth">Whether authentication is required</param>
        /// <returns>Deserialized response</returns>
        Task<T?> UploadFileAsync<T>(string endpoint, Stream fileStream, string fileName, string contentType, bool requiresAuth = true);

        /// <summary>
        /// Downloads a file from the API
        /// </summary>
        /// <param name="endpoint">API endpoint</param>
        /// <param name="requiresAuth">Whether authentication is required</param>
        /// <returns>File content as byte array</returns>
        Task<byte[]> DownloadFileAsync(string endpoint, bool requiresAuth = true);

        /// <summary>
        /// Sets custom headers for subsequent requests
        /// </summary>
        /// <param name="headers">Dictionary of header name-value pairs</param>
        void SetCustomHeaders(Dictionary<string, string> headers);

        /// <summary>
        /// Clears custom headers
        /// </summary>
        void ClearCustomHeaders();

        /// <summary>
        /// Gets the base URL for the API
        /// </summary>
        /// <returns>Base URL</returns>
        string GetBaseUrl();

        /// <summary>
        /// Checks if the API is accessible
        /// </summary>
        /// <returns>True if accessible, false otherwise</returns>
        Task<bool> IsApiAccessibleAsync();
    }
} 