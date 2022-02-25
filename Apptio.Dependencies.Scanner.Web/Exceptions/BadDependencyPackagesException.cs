using System;
using Apptio.Dependencies.Scanner.Web.Dtos;

namespace Apptio.Dependencies.Scanner.Web.Exceptions
{
    public class BadDependencyPackagesException : Exception
    {
        public BadDependencyPackagesException(Ecosystem ecosystem, Exception ex)
            : base($"Bad {ecosystem} dependency packages file format", ex)
        {
        }

        public BadDependencyPackagesException(Ecosystem ecosystem, string details)
            : base(
                $"Bad {ecosystem} dependency packages file format: {details ?? throw new ArgumentNullException(nameof(details))}")
        {
        }
    }
}