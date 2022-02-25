namespace Apptio.Dependencies.Scanner.Web.Dtos
{
    public class ScanResponseDto
    {
        public VulnerablePackageDto[] VulnerablePackages { get; set; } 
        public FailedScanPackageDto[] FailedScanPackages { get; set; } 
    }
}