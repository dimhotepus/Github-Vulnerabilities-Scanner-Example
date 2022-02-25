using JetBrains.Annotations;

namespace Apptio.Dependencies.Scanner.Web.Dtos
{
    public class ScanErrorDto
    {
        [NotNull] public string Code { get; set; }
        [NotNull] public string Message { get; set; }
    }
}