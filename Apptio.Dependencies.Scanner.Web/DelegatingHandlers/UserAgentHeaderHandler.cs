using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;

namespace Apptio.Dependencies.Scanner.Web.DelegatingHandlers
{
    public class UserAgentHeaderHandler : DelegatingHandler
    {
        private readonly ProductInfoHeaderValue _headerValue;

        public UserAgentHeaderHandler(string productName, string productVersion)
        {
            _headerValue = new ProductInfoHeaderValue(productName, productVersion);
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (UserAgentIsMissing(request))
            {
                AddUserAgent(request);
            }

            return await base.SendAsync(request, cancellationToken);
        }

        private void AddUserAgent(HttpRequestMessage request)
        {
            request.Headers.Add(HeaderNames.UserAgent, _headerValue.ToString());
        }

        protected virtual bool UserAgentIsMissing(HttpRequestMessage request) =>
            !request.Headers.TryGetValues(HeaderNames.UserAgent, out var userAgent) || !userAgent.Any();
    }
}