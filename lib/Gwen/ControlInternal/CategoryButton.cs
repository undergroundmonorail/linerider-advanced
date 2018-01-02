using System;
using Gwen.Controls;

namespace Gwen.ControlInternal
{
    /// <summary>
    /// Item in CollapsibleCategory.
    /// </summary>
    public class CategoryButton : Button
    {
        internal bool m_Alt; // for alternate coloring

		protected override System.Drawing.Color CurrentColor
		{
			get
			{
				if (m_Alt)
				{
					if (IsDepressed || ToggleState)
					{
						return Skin.Colors.Category.LineAlt.Text_Selected;
					}
					if (IsHovered)
					{
						return Skin.Colors.Category.LineAlt.Text_Hover;
					}
					return Skin.Colors.Category.LineAlt.Text;
				}

				if (IsDepressed || ToggleState)
				{
					return Skin.Colors.Category.Line.Text_Selected;
				}
				if (IsHovered)
				{
					return Skin.Colors.Category.Line.Text_Hover;
				}
				return Skin.Colors.Category.Line.Text;
			}
		}
        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryButton"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public CategoryButton(Controls.ControlBase parent) : base(parent)
        {
            Alignment = Pos.Left | Pos.CenterV;
            m_Alt = false;
            IsToggle = true;
            TextPadding = new Padding(3, 0, 3, 0);
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin)
        {
            if (m_Alt)
            {
                if (IsDepressed || ToggleState)
                    Skin.Renderer.DrawColor = skin.Colors.Category.LineAlt.Button_Selected;
                else if (IsHovered)
                    Skin.Renderer.DrawColor = skin.Colors.Category.LineAlt.Button_Hover;
                else
                    Skin.Renderer.DrawColor = skin.Colors.Category.LineAlt.Button;
            }
            else
            {
                if (IsDepressed || ToggleState)
                    Skin.Renderer.DrawColor = skin.Colors.Category.Line.Button_Selected;
                else if (IsHovered)
                    Skin.Renderer.DrawColor = skin.Colors.Category.Line.Button_Hover;
                else
                    Skin.Renderer.DrawColor = skin.Colors.Category.Line.Button;
            }

            skin.Renderer.DrawFilledRect(RenderBounds);
        }
    }
}
