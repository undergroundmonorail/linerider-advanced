using Gwen.Anim;
using Gwen.DragDrop;
using Gwen.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Gwen.Controls
{
    public class Container : ControlBase
    {
        public override ControlBase.ControlCollection Children
        {
            get
            {
                return base.Children;
            }
        }
        public Container()
        {
        }
        public override Point LocalPosToCanvas(Point pnt)
        {
            return base.LocalPosToCanvas(pnt);
        }
        public override Point CanvasPosToLocal(Point pnt)
        {
            return base.CanvasPosToLocal(pnt);
        }
    }
}
