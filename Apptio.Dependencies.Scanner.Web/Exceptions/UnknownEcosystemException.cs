using System;
using Apptio.Dependencies.Scanner.Web.Dtos;

namespace Apptio.Dependencies.Scanner.Web.Exceptions
{
    public class UnknownEcosystemException : Exception
    {
        public UnknownEcosystemException(Ecosystem ecosystem)
            : base($"{ecosystem} ecosystem is not supported")
        {
        }
    }
}