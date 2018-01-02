using System;
using Gwen.Controls;

namespace Gwen.ControlInternal
{
    /// <summary>
    /// Label for PropertyRow.
    /// </summary>
    public class PropertyRowLabel : Label
    {
        private readonly PropertyRow m_PropertyRow;
        protected override System.Drawing.Color CurrentColor
        {
            get
			{
				if (IsDisabled)
				{
                    return Skin.Colors.Button.Disabled;
				}

				else if (m_PropertyRow != null && m_PropertyRow.IsEditing)
				{
					return Skin.Colors.Properties.Label_Selected;
				}

				else if (m_PropertyRow != null && m_PropertyRow.IsHovered)
				{
					return Skin.Colors.Properties.Label_Hover;
				}
				else
				{
					return Skin.Colors.Properties.Label_Normal;
				}
            }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyRowLabel"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public PropertyRowLabel(PropertyRow parent) : base(parent)
        {
			AutoSizeToContents = false;
            Alignment = Pos.Left | Pos.CenterV;
            m_PropertyRow = parent;
        }
    }
}
