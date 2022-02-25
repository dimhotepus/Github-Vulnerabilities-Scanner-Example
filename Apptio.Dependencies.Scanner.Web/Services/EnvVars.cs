using System;
using Apptio.Dependencies.Scanner.Web.Constants;
using Apptio.Dependencies.Scanner.Web.Exceptions;

namespace Apptio.Dependencies.Scanner.Web.Services
{
    public class EnvVars : IEnvVars
    {
        public string Version
        {
            get
            {
                var version = Environment.GetEnvironmentVariable(KnownEnvVars.ScannerAppVersion);
                if (string.IsNullOrEmpty(version))
                {
                    throw new BadConfigurationException(KnownEnvVars.ScannerAppVersion);
                }

                return version;
            }
        }

        public string GithubAccessToken
        {
            get
            {
                var accessToken = Environment.GetEnvironmentVariable(KnownEnvVars.GithubAccessToken);
                if (string.IsNullOrEmpty(accessToken))
                {
                    throw new BadConfigurationException(KnownEnvVars.GithubAccessToken);
                }

                return accessToken;
            }
        }
    }
}