using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AnnoDesigner.Models
{
    public interface IHotkeySource<T> where T : InputBinding
    {
        void RegisterBindings(IHotkeyCommandManager<T> manager);
        IHotkeyCommandManager<T> HotkeyCommandManager { get; set; }
    }
}
