using System.Threading.Tasks;
using Apptio.Dependencies.Scanner.Web.Constants;
using Apptio.Dependencies.Scanner.Web.Services;
using Microsoft.AspNetCore.Http;

namespace Apptio.Dependencies.Scanner.Web.RequestDelegates
{
    public static class Meta
    {
        public static async Task Handle(HttpContext context, IEnvVars envVars)
        {
            context.Response.ContentType = KnownContentTypes.ApplicationJson;
            var apiV1Path = context.Request.Scheme + "://" + context.Request.Host + KnownRoutes.ApiV1;
            await context.Response.WriteAsync(
$@"{{
                ""version"": ""{envVars.Version}"",
                ""endpoints"": [
                    {{
                        ""method"": ""GET"",
                        ""path"": ""{apiV1Path}/meta"",
                        ""params"": [],
                        ""description"": ""Gets API metadata."",
                        ""response"": {{
                            ""200"": {{
                                ""contentType"": ""JSON"",
                                ""params"": [
                                    {{
                                        ""location"": ""body"",
                                        ""name"": null,
                                        ""type"": ""string"",
                                        ""required"": true,
                                        ""description"": ""API metadata.""
                                    }}
                                ]
                            }}
                        }}
                    }},
                    {{
                        ""method"": ""GET"",
                        ""path"": ""{apiV1Path}/version"",
                        ""params"": [],
                        ""description"": ""Gets API version."",
                        ""response"": {{
                            ""200"": {{
                                ""contentType"": ""JSON"",
                                ""params"": [
                                    {{
                                        ""location"": ""body"",
                                        ""name"": null,
                                        ""type"": ""string"",
                                        ""required"": true,
                                        ""description"": ""API version.""
                                    }}
                                ]
                            }}
                        }}
                    }},
                    {{
                        ""method"": ""POST"",
                        ""path"": ""{apiV1Path}/scan"",
                        ""params"": [
                            {{
                                ""location"": ""body"",
                                ""name"": ""ecosystem"",
                                ""type"": ""string"",
                                ""required"": true,
                                ""description"": ""Name of the ecosystem the project file is using - e.g. npm, nuget, pip.""
                            }},
                            {{
                                ""location"": ""body"",
                                ""name"": ""fileContent"",
                                ""type"": ""string"",
                                ""required"": true,
                                ""description"": ""The project definition file content encoded in base64.""
                            }}
                        ],
                        ""description"": ""Scans dependencies in package description file for vulnerabilities."",
                        ""response"": {{
                            ""200"": {{
                                ""contentType"": ""JSON"",
                                ""params"": [
                                    {{
                                        ""location"": ""body"",
                                        ""name"": ""vulnerablePackages"",
                                        ""type"": ""array"",
                                        ""required"": true,
                                        ""description"": ""Vulnerable packages."",
                                        ""params"": [
                                            {{
                                                ""name"": ""name"",
                                                ""type"": ""string"",
                                                ""required"": true,
                                                ""description"": ""Package name.""
                                            }},
                                            {{
                                                ""name"": ""version"",
                                                ""type"": ""string"",
                                                ""required"": true,
                                                ""description"": ""Package version.""
                                            }},
                                            {{
                                                ""name"": ""severity"",
                                                ""type"": ""string"",
                                                ""required"": true,
                                                ""description"": ""Vulnerability severity.""
                                            }},
                                            {{
                                                ""name"": ""advisoryLink"",
                                                ""type"": ""string"",
                                                ""required"": false,
                                                ""description"": ""Link to advisory for vulnerability.""
                                            }},
                                            {{
                                                ""name"": ""firstPatchedVersion"",
                                                ""type"": ""string"",
                                                ""required"": false,
                                                ""description"": ""Package version when vulnerability is patched first.""
                                            }}
                                        ]
                                    }},
                                    {{
                                        ""location"": ""body"",
                                        ""name"": ""failedScanPackages"",
                                        ""type"": ""array"",
                                        ""required"": true,
                                        ""description"": ""Failed to scan packages."",
                                        ""params"": [
                                            {{
                                                ""name"": ""name"",
                                                ""type"": ""string"",
                                                ""required"": true,
                                                ""description"": ""Package name.""
                                            }},
                                            {{
                                                ""name"": ""version"",
                                                ""type"": ""string"",
                                                ""required"": true,
                                                ""description"": ""Package version.""
                                            }},
                                            {{
                                                ""name"": ""error"",
                                                ""type"": ""object"",
                                                ""required"": true,
                                                ""description"": ""Scan error."",
                                                ""params"": [
                                                    {{
                                                        ""location"": ""body"",
                                                        ""name"": ""code"",
                                                        ""type"": ""string"",
                                                        ""required"": true,
                                                        ""description"": ""Error code.""
                                                    }},
                                                    {{
                                                        ""location"": ""body"",
                                                        ""name"": ""message"",
                                                        ""type"": ""string"",
                                                        ""required"": true,
                                                        ""description"": ""Scan error message.""
                                                    }}
                                                ]
                                            }}
                                        ]
                                    }}
                                ]
                            }},
                            ""400"": {{
                                ""contentType"": ""JSON"",
                                ""params"": [
                                    {{
                                        ""location"": ""body"",
                                        ""name"": ""code"",
                                        ""type"": ""string"",
                                        ""required"": true,
                                        ""description"": ""Error code.""
                                    }},
                                    {{
                                        ""location"": ""body"",
                                        ""name"": ""message"",
                                        ""type"": ""string"",
                                        ""required"": true,
                                        ""description"": ""Error message. Related to a bad request.""
                                    }}
                                ]
                            }},
                            ""408"": {{
                                ""contentType"": ""JSON"",
                                ""params"": [
                                    {{
                                        ""location"": ""body"",
                                        ""name"": ""code"",
                                        ""type"": ""string"",
                                        ""required"": true,
                                        ""description"": ""Error code.""
                                    }},
                                    {{
                                        ""location"": ""body"",
                                        ""name"": ""message"",
                                        ""type"": ""string"",
                                        ""required"": true,
                                        ""description"": ""Request timeout message.""
                                    }}
                                ]
                            }},
                            ""500"": {{
                                ""contentType"": ""JSON"",
                                ""params"": [
                                    {{
                                        ""location"": ""body"",
                                        ""name"": ""code"",
                                        ""type"": ""string"",
                                        ""required"": true,
                                        ""description"": ""Error code.""
                                    }},
                                    {{
                                        ""location"": ""body"",
                                        ""name"": ""message"",
                                        ""type"": ""string"",
                                        ""required"": true,
                                        ""description"": ""Error message. Related to an internal server error.""
                                    }}
                                ]
                            }},
                            ""*"": {{
                                ""contentType"": ""JSON"",
                                ""params"": [
                                    {{
                                        ""location"": ""body"",
                                        ""name"": ""code"",
                                        ""type"": ""string"",
                                        ""required"": true,
                                        ""description"": ""Error code.""
                                    }},
                                    {{
                                        ""location"": ""body"",
                                        ""name"": ""message"",
                                        ""type"": ""string"",
                                        ""required"": true,
                                        ""description"": ""Error message. Contains status code.""
                                    }}
                                ]
                            }}
                        }}
                    }}
                ]
}}");
        }
    }
}