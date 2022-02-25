using System;

namespace Apptio.Dependencies.Scanner.Web.Exceptions
{
    public class BadConfigurationException : Exception
    {
        public BadConfigurationException(string environmentVarName)
            : base(
                $"Bad {environmentVarName ?? throw new ArgumentNullException(nameof(environmentVarName))} env value - either null or empty")
        {
        }
    }
}