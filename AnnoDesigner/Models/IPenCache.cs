using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoDesigner.Models
{
    public interface IPenCache
    {
        Pen GetPen(Brush brush, double thickness);
    }
}
