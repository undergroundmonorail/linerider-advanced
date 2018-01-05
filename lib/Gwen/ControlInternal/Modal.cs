using System;
using Gwen.Controls;

namespace Gwen.ControlInternal
{
    /// <summary>
    /// Modal control for windows.
    /// </summary>
    public class Modal : Controls.ControlBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Modal"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public Modal(Controls.ControlBase parent)
            : base(parent)
        {
            KeyboardInputEnabled = true;
            MouseInputEnabled = true;
            ShouldDrawBackground = true;
            SetBounds(0, 0, GetCanvas().Width, GetCanvas().Height);
        }
        protected override void PrepareLayout()
		{
			SetBounds(0, 0, GetCanvas().Width, GetCanvas().Height);
            base.PrepareLayout();
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin)
        {
            skin.DrawModalControl(this);
        }
    }
}
