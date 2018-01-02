using Gwen.ControlInternal;
using System;
using System.Drawing;

namespace Gwen.Controls
{
    /// <summary>
    /// Static text label.
    /// </summary>
    public class Label : ControlBase
	{
		#region Fields

		protected readonly Text m_Text;
		private Color m_TextColor;
		private Color m_textColorOverride;
		private Pos m_Align;
		private bool m_AutoSizeToContents;
		private Padding m_TextPadding;

		#endregion Fields

        #region Properties

        public override bool MouseInputEnabled
        {
            get
            {
                return base.MouseInputEnabled || ClickEventAssigned;
            }
            set
            {
                base.MouseInputEnabled = value;
            }
        }
        /// <summary>
        /// Text alignment.
        /// </summary>
        public Pos Alignment
        {
            get { return m_Align; }
            set
            {
                m_Align = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Determines if the control should autosize to its text.
        /// </summary>
        public bool AutoSizeToContents
        {
            get { return m_AutoSizeToContents; }
            set
            {
                m_AutoSizeToContents = value;
                Invalidate();
                InvalidateParent();
            }
        }

        /// <summary>
        /// Font.
        /// </summary>
        public Font Font
        {
            get { return m_Text.Font; }
            set
            {
                m_Text.Font = value;
                if (m_AutoSizeToContents)
                    SizeToContents();
                Invalidate();
            }
        }

        /// <summary>
        /// Text.
        /// </summary>
        public virtual string Text { get { return m_Text.String; } set { SetText(value); } }

        /// <summary>
        /// Text color.
        /// </summary>
        public Color TextColor
        {
            get { return m_TextColor; }
            set
            {
                if (m_TextColor != value)
                {
                    m_TextColor = value;
                    Redraw();
                }
            }
        }

        /// <summary>
        /// Height of the text (in pixels).
        /// </summary>
        public int TextHeight { get { return m_Text.Height; } }

        /// <summary>
        /// Text length (in characters).
        /// </summary>
        public int TextLength { get { return m_Text.Length; } }

        /// <summary>
        /// Text color override - used by tooltips.
        /// </summary>
        public Color TextColorOverride
        {
            get
            {
                return m_textColorOverride;
            }

            set
            {
                m_textColorOverride = value;
                if (IsTextOverrideVisible)
                {
                    Redraw();
                }
            }
        }

        string m_TextOverride;

        /// <summary>
        /// Text override - used to display different string.
        /// </summary>
        public string TextOverride
        {
            get
            {
                return m_TextOverride;
            }

            set
            {
                m_TextOverride = value;
                if (IsTextOverrideVisible)
                {
                    Redraw();
                }
            }
        }

        /// <summary>
        /// Text padding.
        /// </summary>
        public Padding TextPadding
        {
            get { return m_TextPadding; }
            set
            {
                m_TextPadding = value;
                Invalidate();
                InvalidateParent();
            }
        }

        public int TextRight { get { return m_Text.Right; } }

        /// <summary>
        /// Width of the text (in pixels).
        /// </summary>
        public int TextWidth { get { return m_Text.Width; } }

        public int TextX { get { return m_Text.X; } }
        public int TextY { get { return m_Text.Y; } }
        public bool IsTextOverrideVisible
        {
            get
            {
                return TextColorOverride.A != 0;
            }
        }
        protected virtual Color CurrentColor
        {
            get
			{
				if (IsDisabled)
				{
					return Skin.Colors.Button.Disabled;
				}
				else if (IsHovered && ClickEventAssigned)
				{
					return Skin.Colors.Button.Hover;
				}
				else
				{
                    return Skin.Colors.Button.Normal;
				}
            }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Label"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public Label(ControlBase parent) : base(parent)
        {
            m_Text = new Text(this);
            SetSize(100, m_Text.Height);
            Alignment = Pos.Left | Pos.Top;

            m_AutoSizeToContents = true;
            base.MouseInputEnabled = false;
            TextColorOverride = Color.FromArgb(0, 255, 255, 255);// A==0, override disabled
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Gets the coordinates of specified character.
        /// </summary>
        /// <param name="index">Character index.</param>
        /// <returns>Character coordinates (local).</returns>
        public virtual Point GetCharacterPosition(int index)
        {
            Point p = m_Text.GetCharacterPosition(index);
            return new Point(p.X + m_Text.X, p.Y + m_Text.Y);
        }

        /// <summary>
        /// Sets the label text.
        /// </summary>
        /// <param name="str">Text to set.</param>
        /// <param name="doEvents">Determines whether to invoke "text changed" event.</param>
        public virtual void SetText(string str, bool doEvents = true)
        {
            if (Text == str)
                return;

            m_Text.String = str;
            if (m_AutoSizeToContents)
                SizeToContents();
            Invalidate();
            InvalidateParent();

            if (doEvents)
                OnTextChanged();
        }

        public virtual void SizeToContents()
        {
            m_Text.SetPosition(m_TextPadding.Left + Padding.Left, m_TextPadding.Top + Padding.Top);
            m_Text.SizeToContents();

            SetSize(m_Text.Width + Padding.Left + Padding.Right + m_TextPadding.Left + m_TextPadding.Right,
                m_Text.Height + Padding.Top + Padding.Bottom + m_TextPadding.Top + m_TextPadding.Bottom);
            InvalidateParent();
        }

        #endregion Methods


        /// <summary>
        /// Returns index of the character closest to specified point (in canvas coordinates).
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        protected virtual Point GetClosestCharacter(int x, int y)
        {
            return new Point(m_Text.GetClosestCharacter(m_Text.CanvasPosToLocal(new Point(x, y))), 0);
        }

        /// <summary>
        /// Lays out the control's interior according to alignment, padding, dock etc.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Layout(Skin.SkinBase skin)
        {
            base.Layout(skin);

            Pos align = m_Align;

            if (m_AutoSizeToContents)
                SizeToContents();

            int x = m_TextPadding.Left + Padding.Left;
            int y = m_TextPadding.Top + Padding.Top;

            if (0 != (align & Pos.Right))
                x = Width - m_Text.Width - m_TextPadding.Right - Padding.Right;
            if (0 != (align & Pos.CenterH))
                x = (int)((m_TextPadding.Left + Padding.Left) + ((Width - m_Text.Width - m_TextPadding.Left - Padding.Left - m_TextPadding.Right - Padding.Right) * 0.5f));

            if (0 != (align & Pos.CenterV))
                y = (int)((m_TextPadding.Top + Padding.Top) + ((Height - m_Text.Height) * 0.5f) - m_TextPadding.Bottom - Padding.Bottom);
            if (0 != (align & Pos.Bottom))
                y = Height - m_Text.Height - m_TextPadding.Bottom - Padding.Bottom;

            m_Text.SetPosition(x, y);
        }

        /// <summary>
        /// Handler for text changed event.
        /// </summary>
        protected virtual void OnTextChanged() { }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin)
        {
            base.Render(skin);
            m_Text.TextColor = CurrentColor;
        }

        /// <summary>
        /// Sets the position of the internal text control.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        protected void SetTextPosition(int x, int y)
        {
            m_Text.SetPosition(x, y);
        }
    }
}