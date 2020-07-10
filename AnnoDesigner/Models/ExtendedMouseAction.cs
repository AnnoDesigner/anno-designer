using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AnnoDesigner.Models
{
    public enum ExtendedMouseAction
    {
        /// <summary>
        /// None
        /// </summary>
        None = MouseAction.None,
        /// <summary>
        /// LeftClick
        /// </summary>
        LeftClick = MouseAction.LeftClick,
        /// <summary>
        /// RightClick
        /// </summary>
        RightClick = MouseAction.RightClick,
        /// <summary>
        /// MiddleClick
        /// </summary>
        MiddleClick = MouseAction.MiddleClick,
        /// <summary>
        /// WheelClick
        /// </summary>
        WheelClick = MouseAction.WheelClick,
        /// <summary>
        /// LeftDoubleClick
        /// </summary>
        LeftDoubleClick = MouseAction.LeftDoubleClick,
        /// <summary>
        /// RightDoubleClick
        /// </summary>
        RightDoubleClick = MouseAction.RightDoubleClick,
        /// <summary>
        /// MiddleDoubleClick
        /// </summary>
        MiddleDoubleClick = MouseAction.MiddleDoubleClick,
        /// <summary>
        /// XButton1Click
        /// </summary>
        XButton1Click,
        /// <summary>
        /// Xbutton2Click
        /// </summary>
        XButton2Click,
        /// <summary>
        /// XButton1DoubleClick
        /// </summary>
        XButton1DoubleClick,
        /// <summary>
        /// XButton2DoubleClick
        /// </summary>
        XButton2DoubleClick

    }
}
