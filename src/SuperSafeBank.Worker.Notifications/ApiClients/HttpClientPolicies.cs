using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Polly;
using Polly.Extensions.Http;

namespace SuperSafeBank.Worker.Notifications.ApiClients
{
    public static class HttpClientPolicies
    {
        private static readonly HashSet<System.Net.HttpStatusCode> RetryableCodes = new HashSet<HttpStatusCode>()
        {
            System.Net.HttpStatusCode.NotFound,
            System.Net.HttpStatusCode.UnprocessableEntity,
            System.Net.HttpStatusCode.InternalServerError,
            System.Net.HttpStatusCode.ServiceUnavailable,
        };

        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retryCount = 3) 
            => HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => RetryableCodes.Contains(msg.StatusCode))
                .WaitAndRetryAsync(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
}