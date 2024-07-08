using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiSdk;
using ApiSdk.Models;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;

namespace TraXile.Enhanced
{
    internal class TraXileEnhancedApiClient
    {
        public TraXileEnhancedApiClient(string apiUrl, string apiKey)
        {
            ApiKeyAuthenticationProvider authProvider;
            HttpClientRequestAdapter adapter;
            ApiClient apiClient;

            authProvider = new ApiKeyAuthenticationProvider(apiKey, "X-API-KEY", ApiKeyAuthenticationProvider.KeyLocation.Header);
            adapter = new HttpClientRequestAdapter(authProvider) { BaseUrl = apiUrl };
            apiClient = new ApiClient(adapter);
            
        }
    }
}
