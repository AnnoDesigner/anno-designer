using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AnnoDesigner.Models
{
    //TODO: resolve before PR - if we solve the InputBindingCollection issue, this entire interface is needed anymore,
    //as the hotkey binding will be automatic
    public interface IHotkeySource
    {
        void RegisterHotkeys(HotkeyCommandManager manager);
        HotkeyCommandManager HotkeyCommandManager { get; set; }
    }
}
