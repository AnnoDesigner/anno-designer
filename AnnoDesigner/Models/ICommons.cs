using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoDesigner.Models
{
    public interface ICommons
    {
        event EventHandler SelectedLanguageChanged;

        string SelectedLanguage { get; set; }

        bool CanWriteInFolder(string folderPathToCheck = null);
        void RestartApplication(bool asAdmin, string parameters, string path = null);
    }
}
