using System;
using System.Threading.Tasks;
using Apptio.Dependencies.Scanner.Web.Constants;
using Apptio.Dependencies.Scanner.Web.Services;
using Microsoft.AspNetCore.Http;

namespace Apptio.Dependencies.Scanner.Web.RequestDelegates
{
    public static class Version
    {
        public static async Task Handle(HttpResponse response, IEnvVars envVars)
        {
            response.ContentType = KnownContentTypes.ApplicationJson;
            await response.WriteAsync($"\"{envVars.Version}\"");
        }
    }
}