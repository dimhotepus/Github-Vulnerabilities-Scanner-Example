using JetBrains.Annotations;

namespace Apptio.Dependencies.Scanner.Web.Dtos
{
    public interface IPackageDto
    {
        [NotNull] string Name { get; set; }
        [NotNull] string Version { get; set; }
    }
    
    public class VulnerablePackageDto : IPackageDto
    {
        public string Name { get; set; }
        public string Version { get; set; }
        [NotNull] public string Severity { get; set; }
        [CanBeNull] public string AdvisoryLink { get; set; }
        [CanBeNull] public string FirstPatchedVersion { get; set; }

        public override string ToString() => $"[{Severity}] {Name}: {Version} => {FirstPatchedVersion} | {AdvisoryLink}";
    }

    public class FailedScanPackageDto : IPackageDto
    {
        public string Name { get; set; }
        public string Version { get; set; }
        [NotNull] public ScanErrorDto Error { get; set; }
        
        public override string ToString() => $"{Name}: {Version} => {Error.Code} {Error.Message}";
    }
}