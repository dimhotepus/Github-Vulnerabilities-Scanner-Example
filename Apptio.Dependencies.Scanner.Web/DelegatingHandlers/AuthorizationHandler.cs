using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Apptio.Dependencies.Scanner.Web.DelegatingHandlers
{
    public class AuthorizationHandler : DelegatingHandler
    {
        private readonly string _scheme;
        private readonly string _secureValue;

        public AuthorizationHandler(string scheme, string secureValue)
        {
            _scheme = scheme ?? throw new ArgumentNullException(nameof(scheme));
            _secureValue = secureValue ?? throw new ArgumentNullException(nameof(secureValue));
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (AuthenticationIsMissing(request))
            {
                Authenticate(request);
            }

            return await base.SendAsync(request, cancellationToken);
        }

        private void Authenticate(HttpRequestMessage request)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(_scheme, _secureValue);
        }

        protected virtual bool AuthenticationIsMissing(HttpRequestMessage request) =>
            request.Headers.Authorization == null;
    }
}