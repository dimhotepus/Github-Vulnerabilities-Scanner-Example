using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace Apptio.Dependencies.Scanner.Web
{
    public static class HttpResiliencePolicies
    {
        public static IAsyncPolicy<HttpResponseMessage> GetHttpPolicy(IConfiguration configuration, ILogger logger)
        {
            var policies = new List<IAsyncPolicy<HttpResponseMessage>>();

            if (configuration.GetValue("RESILIENCE:Http:Timeout:Enabled", true))
            {
                policies.Add(GetTimeoutPolicy(configuration, logger));
            }

            if (configuration.GetValue("RESILIENCE:Http:CircuitBreaker:Enabled", true))
            {
                policies.Add(GetCircuitBreakerPolicy(configuration, logger));
            }

            if (configuration.GetValue("RESILIENCE:Http:Retry:Enabled", true))
            {
                policies.Add(GetRetryPolicy(configuration));
            }

            return Policy.WrapAsync(policies.ToArray());
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(IConfiguration configuration)
        {
            var retriesCount = configuration.GetValue("RESILIENCE:Http:Retry:Retries", 3);

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(retriesCount, retryAttempt => TimeSpan.FromSeconds(retryAttempt));
        }

        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(IConfiguration configuration,
            ILogger logger)
        {
            var exceptionsAllowedBeforeBreaking =
                configuration.GetValue("RESILIENCE:Http:CircuitBreaker:ExceptionsAllowedBeforeBreaking", 3);
            var durationOfBreak = configuration.GetValue("RESILIENCE:Http:CircuitBreaker:DurationOfBreak",
                TimeSpan.FromSeconds(10));

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TimeoutRejectedException>()
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking,
                    durationOfBreak,
                    (result, duration) => { logger.LogWarning(result.Exception, "Circuit breaker opened"); },
                    () => { logger.LogDebug("Circuit breaker reset"); });
        }

        private static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(IConfiguration configuration, ILogger logger)
        {
            var timeout = configuration.GetValue("RESILIENCE:Http:Timeout:Timeout", TimeSpan.FromSeconds(3));

            return Policy.TimeoutAsync<HttpResponseMessage>(timeout, (context, timeoutApplied, task, exception) =>
            {
                logger.LogWarning(exception, "Request timed out");

                return Task.CompletedTask;
            });
        }
    }
}