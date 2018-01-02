using System;
using Gwen.Controls;

namespace Gwen.ControlInternal
{
    /// <summary>
    /// Header of CollapsibleCategory.
    /// </summary>
    public class CategoryHeaderButton : Button
    {
        protected override System.Drawing.Color CurrentColor
        {
            get
			{
				if (IsDepressed || ToggleState)
                    return Skin.Colors.Category.Header_Closed;
				else
					return Skin.Colors.Category.Header;
            }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryHeaderButton"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public CategoryHeaderButton(Controls.ControlBase parent)
            : base(parent)
        {
            ShouldDrawBackground = false;
            IsToggle = true;
            Alignment = Pos.Center;
            TextPadding = new Padding(3, 0, 3, 0);
        }
    }
}
