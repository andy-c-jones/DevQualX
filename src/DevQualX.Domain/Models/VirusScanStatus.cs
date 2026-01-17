namespace DevQualX.Domain.Models;

/// <summary>
/// Status of virus scan for a blob.
/// </summary>
public enum VirusScanStatus
{
    NotScanned,
    Scanning,
    Clean,
    Infected,
    Error
}
