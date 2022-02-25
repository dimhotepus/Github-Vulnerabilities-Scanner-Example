using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Apptio.Dependencies.Scanner.Web.Constants;
using Apptio.Dependencies.Scanner.Web.Dtos;
using Apptio.Dependencies.Scanner.Web.Models;
using Apptio.Dependencies.Scanner.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace Apptio.Dependencies.Scanner.Web.RequestDelegates
{
    public static class Scan
    {
        // Just add new Ecosystem handlers here.
        private static readonly IReadOnlyDictionary<Ecosystem, Func<string, IDependencyPackages>>
            PackageDependenciesReaderMap =
                new Dictionary<Ecosystem, Func<string, IDependencyPackages>>
                {
                    { Ecosystem.Npm, content => new PackageJsonDependencyPackages(content) }
                };

        public static async Task Handle(HttpContext context,
            IPackageVulnerabilityScanner[] scanners,
            ILogger<IPackageVulnerabilityScanner> logger,
            CancellationToken cancellationToken)
        {
            var (dependencyPackages, ecosystem, ok) = await ValidateAndGetDependencies(context, cancellationToken);
            if (!ok) return;

            logger.LogTrace("Going to scan packages for {ecosystem} from {packages}", ecosystem,
                dependencyPackages.AsString());

            var scanResults = await Task.WhenAll(
                scanners
                    .Where(scanner => scanner.SupportedEcosystems.Contains(ecosystem))
                    .Select(scanner => scanner.Scan(dependencyPackages, cancellationToken)));
            if (scanResults.Length == 0)
            {
                logger.LogWarning("No scanners for {ecosystem} are present", ecosystem);
                await WriteBadRequestJson(context.Response, KnownErrorCodes.Scan.UnknownEcosystem,
                    $"'{nameof(ScanRequestDto.Ecosystem)}' {ecosystem} is not supported. Expect any of [{string.Join(", ", scanners.SelectMany(s => s.SupportedEcosystems).Distinct())}].");
                return;
            }

            var vulnerablePackages = scanResults
                .SelectMany(sr => sr.VulnerablePackages)
                .ToArray();
            var failedScanPackages = scanResults
                .SelectMany(sr => sr.FailedScanPackages)
                .ToArray();

            logger.LogTrace("Scanned packages for {ecosystem} from {packages}: {vulnerable}, {failedScan}",
                ecosystem, dependencyPackages.AsString(), vulnerablePackages, failedScanPackages);

            context.Response.StatusCode = StatusCodes.Status200OK;
            await context.Response
                .WriteAsJsonAsync(
                    new ScanResponseDto
                        { VulnerablePackages = vulnerablePackages, FailedScanPackages = failedScanPackages },
                    cancellationToken);
        }

        private static async Task WriteBadRequestJson(HttpResponse response, string errorCode, string errorMessage)
        {
            response.StatusCode = StatusCodes.Status400BadRequest;
            await response.WriteAsJsonAsync(new ScanErrorDto { Code = errorCode, Message = errorMessage });
        }

        private static async Task<(IDependencyPackages Packages, Ecosystem Ecosystem, bool Ok)>
            ValidateAndGetDependencies(HttpContext context, CancellationToken cancellationToken)
        {
            if (!context.Request.HasJsonContentType())
            {
                await WriteBadRequestJson(context.Response, KnownErrorCodes.Scan.NotJson,
                    $"'{HeaderNames.ContentType}' header should be application/json.");
                return (Packages: null, Ecosystem: Ecosystem.Unknown, Ok: false);
            }

            ScanRequestDto request;
            try
            {
                request = await context.Request.ReadFromJsonAsync<ScanRequestDto>(cancellationToken);
            }
            catch (JsonException)
            {
                await WriteBadRequestJson(context.Response, KnownErrorCodes.Scan.BadJsonBody,
                    $"{{'{nameof(request.Ecosystem)}': 'npm', '{nameof(request.FileContent)}': '<base64> file content'}} like JSON expected.");
                return (Packages: null, Ecosystem: Ecosystem.Unknown, Ok: false);
            }

            if (request == null)
            {
                await WriteBadRequestJson(context.Response, KnownErrorCodes.Scan.BadJsonBody,
                    $"{{\"{nameof(request.Ecosystem)}\": \"npm\", \"{nameof(request.FileContent)}\": \"base64\"}} like JSON expected.");
                return (Packages: null, Ecosystem: Ecosystem.Unknown, Ok: false);
            }

            if (!PackageDependenciesReaderMap.TryGetValue(request.Ecosystem, out var packageDependenciesReader))
            {
                await WriteBadRequestJson(context.Response, KnownErrorCodes.Scan.UnknownEcosystem,
                    $"'{nameof(request.Ecosystem)}' {request.Ecosystem} is not supported. Expect any of [{string.Join(", ", PackageDependenciesReaderMap.Keys)}].");
                return (Packages: null, request.Ecosystem, Ok: false);
            }

            if (string.IsNullOrEmpty(request.FileContent))
            {
                await WriteBadRequestJson(context.Response, KnownErrorCodes.Scan.MissedFileContent,
                    $"'{nameof(request.FileContent)}' should be present.");
                return (Packages: null, request.Ecosystem, Ok: false);
            }

            string fileContent;
            try
            {
                var decodedBytes = Convert.FromBase64String(request.FileContent);
                fileContent = Encoding.UTF8.GetString(decodedBytes);
            }
            catch (FormatException)
            {
                await WriteBadRequestJson(context.Response, KnownErrorCodes.Scan.FileContentNotBase64,
                    $"'{nameof(request.FileContent)}' should be Base64 string.");
                return (Packages: null, request.Ecosystem, Ok: false);
            }
            catch (ArgumentException ex) when (ex.GetType() == typeof(ArgumentException) ||
                                               ex is DecoderFallbackException)
            {
                await WriteBadRequestJson(context.Response, KnownErrorCodes.Scan.FileContentNotUtf8,
                    $"'{nameof(request.FileContent)}' should be Base64 encoded UTF-8 string.");
                return (Packages: null, request.Ecosystem, Ok: false);
            }

            var dependencyPackages = packageDependenciesReader(fileContent);
            return (Packages: dependencyPackages, request.Ecosystem, Ok: true);
        }
    }
}