﻿using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnnoDesigner.Core.Models;

namespace AnnoDesigner.Core.Helper
{
    public class RecentFilesHelper : IRecentFilesHelper
    {
        /// <summary>
        /// Occurs when the <see cref="RecentFiles"/> property has been updated.
        /// </summary>
        public event EventHandler<EventArgs> Updated;

        private readonly string _settingsPath;
        private readonly IRecentFilesSerializer _serializer;
        private readonly IFileSystem _fileSystem;

        public RecentFilesHelper(string pahToSettingsFile, IRecentFilesSerializer serializerToUse, IFileSystem fileSystemToUse)
        {
            _settingsPath = pahToSettingsFile;
            _serializer = serializerToUse ?? throw new ArgumentNullException(nameof(serializerToUse));
            _fileSystem = fileSystemToUse ?? throw new ArgumentNullException(nameof(fileSystemToUse));

            RecentFiles = new List<RecentFile>();
            MaximumItemCount = 10;

            Load();
        }

        public List<RecentFile> RecentFiles { get; private set; }

        /// <summary>
        /// Gets or sets the maximum item count.
        /// <para />
        /// The default value is <c>10</c>.
        /// </summary>
        /// <value>The maximum item count.</value>
        public int MaximumItemCount { get; set; }

        private void Load()
        {
            if (!_fileSystem.File.Exists(_settingsPath))
            {
                return;
            }

            var loadedFiles = _serializer.Deserialize(_settingsPath);

            //remove non existing files
            for (var i = 0; i < loadedFiles.Count; i++)
            {
                var curLoadedFile = loadedFiles[i];
                if (!_fileSystem.File.Exists(curLoadedFile.Path))
                {
                    loadedFiles.RemoveAt(i);
                }
            }

            RecentFiles = loadedFiles;
        }

        private void Save()
        {
            _serializer.Serialize(_settingsPath, RecentFiles);
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

            //only keep max items
            if (RecentFiles.Count > MaximumItemCount)
            {
                for (var i = MaximumItemCount; i < RecentFiles.Count; i++)
                {
                    RecentFiles.RemoveAt(i);
                }
            }

            Save();
        }

        public void RemoveFile(RecentFile fileToRemove)
        {
            throw new NotImplementedException();
        }
    }
}
