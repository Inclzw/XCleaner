namespace XCleaner.Models;

public partial class CleanItem : ObservableObject
{
    [ObservableProperty] private bool _checked;
    public string Name { get; set; }
    public string Path { get; set; }
    public long Size { get; set; }
    public DateTime LastModified { get; set; }
    public DateTime LatestFileModifiedTime { get; set; }
}