using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Apptio.Dependencies.Scanner.Web.Dtos;
using Apptio.Dependencies.Scanner.Web.Exceptions;

namespace Apptio.Dependencies.Scanner.Web.Models
{
    public class PackageJsonDependencyPackages : IDependencyPackages
    {
        private readonly string _packageJsonContent;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly Lazy<IReadOnlyDictionary<string, string>> _packages;

        public PackageJsonDependencyPackages(string packageJsonContent)
        {
            _packageJsonContent = packageJsonContent ?? throw new ArgumentNullException(nameof(packageJsonContent));
            _jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            _packages = new Lazy<IReadOnlyDictionary<string, string>>(GetValues);
        }

        public Ecosystem Ecosystem => Ecosystem.Npm;

        public IReadOnlyDictionary<string, string> Values() => _packages.Value;

        public string AsString()
        {
            return Values()
                .Aggregate(new StringBuilder(), (acc, kv) => acc.AppendFormat("{0}: {1}, ", kv.Key, kv.Value))
                .ToString();
        }

        private IReadOnlyDictionary<string, string> GetValues()
        {
            try
            {
                var packageJson =
                    JsonSerializer.Deserialize<PackageJsonModel>(_packageJsonContent, _jsonSerializerOptions);
                return packageJson?.Dependencies?
                           .Select(d =>
                           {
                               var (key, value) = d;
                               if (string.IsNullOrEmpty(key))
                               {
                                   throw new BadDependencyPackagesException(Ecosystem, "Package name is null or empty");
                               }

                               if (string.IsNullOrEmpty(value))
                               {
                                   throw new BadDependencyPackagesException(Ecosystem,
                                       $"Package version for package {key} is null or empty");
                               }

                               return (Name: key, Version: value);
                           })
                           .ToDictionary(d => d.Name, d => d.Version) ??
                       throw new BadDependencyPackagesException(Ecosystem, "Package has no Dependencies section");
            }
            catch (JsonException ex)
            {
                throw new BadDependencyPackagesException(Ecosystem, ex);
            }
            catch (NotSupportedException ex)
            {
                throw new BadDependencyPackagesException(Ecosystem, ex);
            }
        }

        public class PackageJsonModel
        {
            // Anything which has dependencies is suitable for us.
            public Dictionary<string, string> Dependencies { get; set; }
        }
    }
}