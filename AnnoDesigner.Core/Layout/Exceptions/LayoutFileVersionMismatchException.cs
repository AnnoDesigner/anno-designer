using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoDesigner.Core.Layout.Exceptions
{
    public class LayoutFileVersionMismatchException : Exception
    {
        public LayoutFileVersionMismatchException()
        {
        }

        public LayoutFileVersionMismatchException(string message)
            : base(message)
        {
        }

        public LayoutFileVersionMismatchException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
