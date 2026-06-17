using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SchoolERP.Net.Models.Common;

namespace SchoolERP.Net.Services.Clients
{
    /// <summary>
    /// This is a base tool that handles all the technical details of sending and receiving data through the internet for the application.
    /// </summary>
    public abstract class BaseApiClient
    {
        protected readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IHttpContextAccessor _httpContextAccessor;

        protected BaseApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpContextAccessor = new HttpContextAccessor();
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        /// <summary>
        /// Sends a request to 'get' or 'read' information from the server.
        /// </summary>
        protected async Task<ApiResponse<T>> GetAsync<T>(string url)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                AttachBearerToken(request);
                var response = await _httpClient.SendAsync(request);
                return await HandleResponse<T>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<T>.ErrorResponse($"Network Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a request to 'create' or 'submit' new information to the server.
        /// </summary>
        protected async Task<ApiResponse<T>> PostAsync<T>(string url, object data)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = JsonContent.Create(data)
                };
                AttachBearerToken(request);
                var response = await _httpClient.SendAsync(request);
                return await HandleResponse<T>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<T>.ErrorResponse($"Network Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a request to 'update' or 'change' existing information on the server.
        /// </summary>
        protected async Task<ApiResponse<T>> PutAsync<T>(string url, object data)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Put, url)
                {
                    Content = JsonContent.Create(data)
                };
                AttachBearerToken(request);
                var response = await _httpClient.SendAsync(request);
                return await HandleResponse<T>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<T>.ErrorResponse($"Network Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a request to 'remove' or 'delete' information from the server.
        /// </summary>
        protected async Task<ApiResponse<T>> DeleteAsync<T>(string url)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Delete, url);
                AttachBearerToken(request);
                var response = await _httpClient.SendAsync(request);
                return await HandleResponse<T>(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<T>.ErrorResponse($"Network Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Automatically adds the user's secret 'security token' to the request so the server knows who is asking.
        /// </summary>
        private void AttachBearerToken(HttpRequestMessage request)
        {
            var token = _httpContextAccessor.HttpContext?.Request?.Cookies["token"];
            if (string.IsNullOrWhiteSpace(token)) return;
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>
        /// A tool that carefully reads the data sent back by the server and checks if everything went smoothly or if there was an error.
        /// </summary>
        private async Task<ApiResponse<T>> HandleResponse<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    using (var jsonDoc = JsonDocument.Parse(content))
                    {
                        if (jsonDoc.RootElement.ValueKind == JsonValueKind.Object && 
                            (jsonDoc.RootElement.TryGetProperty("success", out _) || 
                             jsonDoc.RootElement.TryGetProperty("Success", out _)))
                        {
                            return JsonSerializer.Deserialize<ApiResponse<T>>(content, _jsonOptions);
                        }
                    }
                    
                    // Fallback block: wraps raw arrays / primitives into standard GUI tracking structures mapping
                    var data = JsonSerializer.Deserialize<T>(content, _jsonOptions);
                    return ApiResponse<T>.SuccessResponse(data);
                }
                catch
                {
                    try
                    {
                        var data = JsonSerializer.Deserialize<T>(content, _jsonOptions);
                        return ApiResponse<T>.SuccessResponse(data);
                    }
                    catch
                    {
                        return ApiResponse<T>.ErrorResponse($"API Error: {response.ReasonPhrase}", new List<string> { content });
                    }
                }
            }

            try
            {
                var errorResponse = JsonSerializer.Deserialize<ApiResponse<T>>(content, _jsonOptions);
                if (errorResponse != null) return errorResponse;
            }
            catch { }

            return ApiResponse<T>.ErrorResponse($"API Error: {response.ReasonPhrase}", new List<string> { content });
        }
    }
}
