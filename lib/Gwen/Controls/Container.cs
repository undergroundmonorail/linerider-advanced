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
        protected ControlBase m_Panel;
        public override ControlCollection Children
        {
            get
            {
                return m_Panel == null ? PrivateChildren : m_Panel.Children;
            }
        }

        /// <summary>
        /// The children of the container that aren't within the panel.
        /// </summary>
        protected ControlCollection PrivateChildren
        {
            get
            {
                return base.Children;
            }
        }
        protected virtual Margin PanelMargin
        {
            get
            {
                return Margin.Zero;
            }
        }
        internal Rectangle PanelBounds => m_Panel.Bounds;
        public Container(ControlBase parent) : base(parent)
        {
            m_Panel = new Panel(null);
            m_Panel.Dock = Pos.Fill;
			PrivateChildren.Add(m_Panel);
            this.BoundsOutlineColor = Color.Cyan;
        }
        public override void BringChildToFront(ControlBase control)
		{
			var privateidx = PrivateChildren.IndexOf(control);
			if (privateidx != -1)
			{
                PrivateChildren.BringToFront(privateidx);
			}
			else
			{
				base.BringChildToFront(control);
			}
        }
        public override void SendChildToBack(ControlBase control)
        {
            var privateidx = PrivateChildren.IndexOf(control);
            if (privateidx != -1)
            {
                PrivateChildren.SendToBack(privateidx);
            }
            else
            {
                base.SendChildToBack(control);
            }
        }
        public override void DeleteAllChildren()
        {
            m_Panel.DeleteAllChildren();
        }
        protected class Panel : ControlBase
        {
            public override Margin Margin
            {
                get
                {
                    var parent = Parent as Container;

                    return parent != null ? parent.PanelMargin : base.Margin;
                }
                set
                {
                    throw new Exception("Attempt to set panel margin");
                }
            }
            public Panel(ControlBase parent) : base(parent)
            {
            }
            public override void SendToBack()
            {
                var container = Parent as Container;
                container.PrivateChildren.SendToBack(this);
            }
            public override void BringToFront()
			{
                var container = (Container)Parent;
				container.PrivateChildren.BringToFront(this);
            }
            protected override void ProcessLayout()
            {
                base.ProcessLayout();
            }
        }
    }
}
