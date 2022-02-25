using System.Collections.Generic;
using Apptio.Dependencies.Scanner.Web.Dtos;

namespace Apptio.Dependencies.Scanner.Web.Models
{
    public interface IDependencyPackages
    {
        Ecosystem Ecosystem { get; }
        IReadOnlyDictionary<string, string> Values();
        string AsString();
    }
}