namespace Apptio.Dependencies.Scanner.Web.Services
{
    public interface IEnvVars
    {
        string Version { get; }

        string GithubAccessToken { get; }
    }
}