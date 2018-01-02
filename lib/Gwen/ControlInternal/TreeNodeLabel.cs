using System;
using Gwen.Controls;

namespace Gwen.ControlInternal
{
    /// <summary>
    /// Tree node label.
    /// </summary>
    public class TreeNodeLabel : Button
    {
        protected override System.Drawing.Color CurrentColor
        {
            get
			{
				if (IsDisabled)
				{
                    return Skin.Colors.Button.Disabled;
				}

				if (IsDepressed || ToggleState)
				{
                    return Skin.Colors.Tree.Selected;
				}

				if (IsHovered)
				{
                    return Skin.Colors.Tree.Hover;
				}

                return Skin.Colors.Tree.Normal;
            }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="TreeNodeLabel"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public TreeNodeLabel(Controls.ControlBase parent)
            : base(parent)
        {
            Alignment = Pos.Left | Pos.CenterV;
            ShouldDrawBackground = false;
            Height = 16;
            TextPadding = new Padding(3, 0, 3, 0);
        }
    }
}
