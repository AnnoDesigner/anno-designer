using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AnnoDesigner.model
{
    public interface IBrushCache
    {
        SolidColorBrush GetSolidBrush(Color color);
    }
}
