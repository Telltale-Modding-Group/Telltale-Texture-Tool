using System;
using Avalonia.Media;

public class FileSystemItem : IEquatable<FileSystemItem>
{
    public IImage? Icon { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public string Type { get; set; } = "File";
    public DateTime ModifiedDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsDirectory { get; set; }
    public long Size { get; set; }

    public bool Equals(FileSystemItem? other)
    {
        return FullPath == other.FullPath;
    }
}
