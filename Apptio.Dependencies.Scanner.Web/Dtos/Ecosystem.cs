using System.Text.Json.Serialization;

namespace Apptio.Dependencies.Scanner.Web.Dtos
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Ecosystem
    {
        Unknown,
        Npm
    }
}