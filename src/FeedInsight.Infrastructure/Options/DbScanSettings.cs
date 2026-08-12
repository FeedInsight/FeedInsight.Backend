namespace FeedInsight.Infrastructure.Options;

public class DbScanSettings
{
    public const string SectionName = "DbScan";

    public double Epsilon { get; set; }
    public int MinimumPointsPerCluster { get; set; }
}
