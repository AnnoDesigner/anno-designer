using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AnnoDesigner.Models
{
    public interface IHotkeySource
    {
        void RegisterBindings(HotkeyCommandManager manager);
        HotkeyCommandManager HotkeyCommandManager { get; set; }
    }
}
