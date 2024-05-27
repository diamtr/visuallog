using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VisualLog.Capture.Search
{
  public class PathBuilder
  {
    public List<string> Files { get; private set; }
    public List<string> Directories { get; private set; }
    public string Output { get; private set; }
    public string WorkingDirectory { get; private set; }

    private List<string> files;
    private List<string> directories;
    private string output;

    public PathBuilder()
    {
      this.files = new List<string>();
      this.Files = new List<string>();
      this.directories = new List<string>();
      this.Directories = new List<string>();
    }

    public void WithFiles(IEnumerable<string> paths)
    {
      this.files.AddRange(paths);
    }

    public void WithDirectories(IEnumerable<string> paths)
    {
      this.directories.AddRange(paths);
    }

    public void WithOutput(string path)
    {
      this.output = path;
      this.BuildOutput();
    }

    public void WithWorkingDirectory(string path)
    {
      if (!Path.IsPathRooted(path))
        throw new ArgumentException("Working directory path must be rooted");
      this.WorkingDirectory = path;
    }

    public void Build()
    {
      this.BuildOutput();
      this.BuildDirectories();
      this.BuildFiles();
    }

    public void BuildOutput()
    {
      if (string.IsNullOrWhiteSpace(this.output))
      {
        this.Output = string.Empty;
        return;
      }

      if (Path.IsPathRooted(this.output))
      {
        this.Output = this.output;
        return;
      }

      if (!string.IsNullOrWhiteSpace(this.WorkingDirectory))
        this.Output = Path.Combine(this.WorkingDirectory, this.output);

      this.Output = this.output;
    }

    public void BuildDirectories()
    {
      foreach (var directory in this.directories)
      {
        if (string.IsNullOrWhiteSpace(directory))
          continue;

        if (Path.IsPathRooted(directory))
        {
          this.Directories.Add(directory);
          continue;
        }

        if (!string.IsNullOrWhiteSpace(this.WorkingDirectory))
        {
          this.Directories.Add(Path.Combine(this.WorkingDirectory, directory));
          continue;
        }

        this.Directories.Add(directory);
      }
    }

    public void BuildFiles()
    {
      if (this.Directories.Any() && this.Directories.Any(x => !Path.IsPathRooted(x)))
        this.BuildDirectories();

      var directoryInfos = this.Directories.Select(x => new DirectoryInfo(x)).Where(x => x.Exists);

      foreach (var file in this.files)
      {
        if (string.IsNullOrWhiteSpace(file))
          continue;

        if (Path.IsPathRooted(file))
        {
          this.Files.Add(file);
          continue;
        }

        var fileName = Path.GetFileName(file);
        var fileFound = false;
        foreach (var directoryInfo in directoryInfos)
        {
          var fileInfos = directoryInfo.EnumerateFiles(fileName);
          if (fileInfos.Any())
          {
            this.Files.AddRange(fileInfos.Select(x => x.FullName));
            fileFound = true;
          }
        }

        if (!fileFound)
          this.Files.Add(file);
      }

      if (!this.Files.Any())
        foreach (var directoryInfo in directoryInfos)
          this.Files.AddRange(directoryInfo.EnumerateFiles().Select(x => x.FullName));

      if (!this.Files.Any() && !string.IsNullOrWhiteSpace(this.WorkingDirectory))
        this.Files.AddRange(new DirectoryInfo(this.WorkingDirectory).EnumerateFiles().Select(x => x.FullName));
    }
  }
}
