using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Display;

namespace _3045陈文坛exp
{
    interface ILabelsStyle
    {
        esriBalloonCalloutStyle style { get; set; }
        int FontSize { get; set; }
        int fColor { get; set; }
        int tColor { get; set; }
    }
}
