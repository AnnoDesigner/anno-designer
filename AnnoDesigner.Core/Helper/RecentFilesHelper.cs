using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.Core.Helper;

public class RecentFilesHelper : IRecentFilesHelper
{
    /// <summary>
    /// Occurs when the <see cref="RecentFiles"/> property has been updated.
    /// </summary>
    public event EventHandler<EventArgs> Updated;

    private readonly IRecentFilesSerializer _serializer;
    private readonly IFileSystem _fileSystem;
    private int _maximumItemCount;

    public RecentFilesHelper(IRecentFilesSerializer serializerToUse, IFileSystem fileSystemToUse, int maxItemCount = 10)
    {
        _serializer = serializerToUse ?? throw new ArgumentNullException(nameof(serializerToUse));
        _fileSystem = fileSystemToUse ?? throw new ArgumentNullException(nameof(fileSystemToUse));

        RecentFiles = [];
        _maximumItemCount = maxItemCount;
        Load();
    }

    public List<RecentFile> RecentFiles { get; private set; }

    /// <summary>
    /// Gets or sets the maximum item count.        
    /// </summary>
    /// <remarks>The default value is <c>10</c>.</remarks>
    /// <value>The maximum item count.</value>
    public int MaximumItemCount
    {
        get { return _maximumItemCount; }
        set
        {
            if (_maximumItemCount != value)
            {
                _maximumItemCount = value;
                EnsureMaxiumItemCount();
                Save();
                Updated?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private void Load()
    {
        var loadedFiles = _serializer.Deserialize();

        RemoveNonExistingFiles(loadedFiles);

        RecentFiles = loadedFiles;

        EnsureMaxiumItemCount();

        Updated?.Invoke(this, EventArgs.Empty);
    }

    private void Save()
    {
        _serializer.Serialize(RecentFiles);
    }

    public void AddFile(RecentFile fileToAdd)
    {
        if (fileToAdd is null)
        {
            return;
        }

        //if it already exists, remove it (will be added on top later)
        for (var i = 0; i < RecentFiles.Count; i++)
        {
            if (string.Equals(RecentFiles[i].Path, fileToAdd.Path, StringComparison.OrdinalIgnoreCase))
            {
                RecentFiles.RemoveAt(i);
                break;
            }
        }

        //add to top
        RecentFiles.Insert(0, fileToAdd);

        EnsureMaxiumItemCount();

        Save();

        Updated?.Invoke(this, EventArgs.Empty);
    }

    public void RemoveFile(RecentFile fileToRemove)
    {
        if (fileToRemove is null)
        {
            return;
        }

        for (var i = 0; i < RecentFiles.Count; i++)
        {
            if (string.Equals(RecentFiles[i].Path, fileToRemove.Path, StringComparison.OrdinalIgnoreCase))
            {
                RecentFiles.RemoveAt(i);
                break;
            }
        }

        Save();

        Updated?.Invoke(this, EventArgs.Empty);
    }

    private void EnsureMaxiumItemCount()
    {
        while (RecentFiles.Count > MaximumItemCount)
        {
            RecentFiles.RemoveAt(RecentFiles.Count - 1);
        }
    }

    private void RemoveNonExistingFiles(List<RecentFile> loadedFiles)
    {
        for (var i = 0; i < loadedFiles.Count; i++)
        {
            var curLoadedFile = loadedFiles[i];
            if (!_fileSystem.File.Exists(curLoadedFile.Path))
            {
                loadedFiles.RemoveAt(i);
                i--;
            }
        }
    }

    public void ClearRecentFiles()
    {
        RecentFiles.Clear();

        Save();
        Updated?.Invoke(this, EventArgs.Empty);
    }
}
