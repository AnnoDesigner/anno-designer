using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoDesigner.Core.Helper
{
    public static class FileHelper
    {
        /// <summary>
        /// Sets the attributes of a file to 'normal'. If the file was set to ReadOnly the attribute will be removed.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <exception cref="Exception">The attributes of the file couldnot be set to 'normal'.</exception>        
        public static void ResetFileAttributes(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath), "The path to the file was not specified.");
            }

            if (!File.Exists(filePath))
            {
                return;
            }
#if DEBUG
            var fileAttributes = File.GetAttributes(filePath);

            //check whether a file is read only
            var isReadOnly = fileAttributes.HasFlag(FileAttributes.ReadOnly);

            //check whether a file is hidden
            var isHidden = fileAttributes.HasFlag(FileAttributes.Hidden);

            //check whether a file has archive attribute
            var isArchive = fileAttributes.HasFlag(FileAttributes.Archive);

            //check whether a file is system file
            var isSystem = fileAttributes.HasFlag(FileAttributes.System);
#endif            
            try
            {
                File.SetAttributes(filePath, FileAttributes.Normal);
            }
            catch (Exception ex)
            {
                var errorMessage = $"The attributes of the file \"{filePath}\" could not be set to 'normal'.";
                throw new IOException(errorMessage, ex);
            }
        }
    }
}
