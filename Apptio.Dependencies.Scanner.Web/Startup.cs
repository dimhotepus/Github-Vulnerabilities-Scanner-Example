using System.Linq;
using System.Threading.Tasks;
using Apptio.Dependencies.Scanner.Web.Constants;
using Apptio.Dependencies.Scanner.Web.DelegatingHandlers;
using Apptio.Dependencies.Scanner.Web.Dtos;
using Apptio.Dependencies.Scanner.Web.Exceptions;
using Apptio.Dependencies.Scanner.Web.RequestDelegates;
using Apptio.Dependencies.Scanner.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly.Timeout;
using Version = Apptio.Dependencies.Scanner.Web.RequestDelegates.Version;

namespace Apptio.Dependencies.Scanner.Web
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IEnvVars, EnvVars>();

            services
                .AddHttpClient<IPackageVulnerabilityScanner, GithubPackageVulnerabilityScanner>()
                .AddHttpMessageHandler(provider =>
                {
                    // Github doesn't like requests without User-Agent header.
                    var version = provider.GetRequiredService<IEnvVars>().Version;
                    // Do not flicker company name for now, who knows how Gitlab tracks it.
                    return new UserAgentHeaderHandler("Apptio.Dependencies.Scanner.Web", version);
                })
                .AddHttpMessageHandler(provider =>
                {
                    var accessToken = provider.GetRequiredService<IEnvVars>().GithubAccessToken;
                    return new AuthorizationHandler("Bearer", accessToken);
                })
                .AddPolicyHandler((provider, _) => HttpResiliencePolicies.GetHttpPolicy(_configuration,
                    provider.GetRequiredService<ILogger<GithubPackageVulnerabilityScanner>>()));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (!env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async context =>
                    {
                        context.Response.ContentType = KnownContentTypes.ApplicationJson;

                        var exceptionHandlerPathFeature =
                            context.Features.Get<IExceptionHandlerPathFeature>();
                        var error = exceptionHandlerPathFeature?.Error;

                        switch (error)
                        {
                            case BadDependencyPackagesException ex1:
                                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                                await context.Response.WriteAsJsonAsync(new ScanErrorDto
                                {
                                    Code = KnownErrorCodes.Scan.FileContentNotPackageJson,
                                    Message = ex1.Message
                                });
                                break;
                            case UnknownEcosystemException ex2:
                                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                                await context.Response.WriteAsJsonAsync(new ScanErrorDto
                                {
                                    Code = KnownErrorCodes.Scan.UnknownEcosystem,
                                    Message = ex2.Message
                                });
                                break;
                            case TimeoutRejectedException:
                            case TaskCanceledException:
                                context.Response.StatusCode = StatusCodes.Status408RequestTimeout;
                                await context.Response.WriteAsJsonAsync(new ScanErrorDto
                                {
                                    Code = KnownErrorCodes.Scan.InternalScannerFailure,
                                    Message = "Unable to scan the package due to API timeout"
                                });
                                break;
                            default:
                                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                                await context.Response.WriteAsJsonAsync(new ScanErrorDto
                                {
                                    Code = KnownErrorCodes.Scan.GenericApiFailure,
                                    Message = "Status: " + context.Response.StatusCode
                                });
                                break;
                        }
                    });
                });
            }

            app.UseStatusCodePages(async context =>
            {
                context.HttpContext.Response.ContentType = KnownContentTypes.ApplicationJson;

                await context.HttpContext.Response.WriteAsJsonAsync(
                    new ScanErrorDto
                    {
                        Code = KnownErrorCodes.Scan.GenericApiFailure,
                        Message = "Status: " + context.HttpContext.Response.StatusCode
                    });
            });

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/",
                    context =>
                    {
                        context.Response.Redirect(
                            $"{context.Request.GetDisplayUrl().TrimEnd('/')}{KnownRoutes.ApiV1}/meta");
                        return Task.CompletedTask;
                    });
                endpoints.MapGet($"{KnownRoutes.ApiV1}/meta",
                    context => Meta.Handle(context, context.RequestServices.GetRequiredService<IEnvVars>()));
                endpoints.MapPost($"{KnownRoutes.ApiV1}/scan",
                    context =>
                        Scan.Handle(context,
                            context.RequestServices.GetServices<IPackageVulnerabilityScanner>().ToArray(),
                            context.RequestServices.GetRequiredService<ILogger<IPackageVulnerabilityScanner>>(),
                            context.RequestAborted));
                endpoints.MapGet($"{KnownRoutes.ApiV1}/version",
                    context => Version.Handle(context.Response,
                        context.RequestServices.GetRequiredService<IEnvVars>()));
            });
        }
    }
}