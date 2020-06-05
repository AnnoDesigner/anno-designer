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
        void RegisterHotkeys(HotkeyCommandManager manager);
        //TODO: resolve before PR - if we solve the InputBindingCollection issue, this is not needed anymore
        HotkeyCommandManager HotkeyCommandManager { get; set; }
    }
}
