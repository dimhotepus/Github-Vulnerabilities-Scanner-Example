Dependency packages scanner

# How to build

docker build --file Apptio.Dependencies.Scanner.Web/Dockerfile --tag cdsw-1.0.0.0 .

# How to run

docker run -it -p 8080:8080 -e ASPNETCORE_URLS="http://+:8080" -e SCANNER-APP-VERSION="1.0.0.0" -e GITHUB-ACCESS-TOKEN="<Your Github personal access token>" docker.io/library/cdsw-1.0.0.0

Open http://localhost:8080 in browser and follow instructions.

# Assumptions

MVP
No CI
No metrics
No production-level logging
No tests
No authentication / authorization / audit
No Github responses verification, rate limits handling
Use OpenAPI / Swagger for API meta

# Goals

1. In the future, we will want to support finding vulnerabilities for more ecosystems (Python
and pip, .NET and Nuget, etc.)

You can add new ecosystems to existing Github scanner.  Derive from `IDependencyPackages` to
implement dependencies extraction service.  Register that service in
`Scan.PackageDependenciesReaderMap`.  Add new ecosystem to 
`GithubPackageVulnerabilityScanner.SupportedEcosystems`.

2. We also might want to switch from Github’s vulnerabilities API to a different provider.

You can implement `IPackageVulnerabilityScanner` and register it in `Startup.ConfigureServices`.
Vulnerabilties are scanned by all suitable scanners.  Results are joined.
