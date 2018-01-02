using Gwen.Anim;
using Gwen.DragDrop;
using Gwen.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Gwen.Controls
{
    /// <summary>
    /// Base control class.
    /// </summary>
    public partial class ControlBase : IDisposable
    {
        /// <summary>
        /// Delegate used for all control event handlers.
        /// </summary>
        /// <param name="control">Event source.</param>
        /// <param name="args" >Additional arguments. May be empty (EventArgs.Empty).</param>
        public delegate void GwenEventHandler<in T>(ControlBase sender, T arguments) where T : System.EventArgs;

        private bool m_Disposed;

        private ControlBase m_Parent;

        /// <summary>
        /// This is the panel's actual parent - most likely the logical
        /// parent's InnerPanel (if it has one). You should rarely need this.
        /// </summary>
        private ControlBase m_ActualParent;

        /// <summary>
        /// If the innerpanel exists our children will automatically become children of that
        /// instead of us - allowing us to move them all around by moving that panel (useful for scrolling etc).
        /// </summary>
        protected ControlBase m_InnerPanel;

        private Skin.SkinBase m_Skin;

        private Rectangle m_Bounds;
        private Rectangle m_RenderBounds;
        private Rectangle m_InnerBounds;
        private Padding m_Padding;
        private Margin m_Margin;

        private string m_Name;

        private bool m_RestrictToParent;
        private bool m_Disabled;
        private bool m_Hidden;
        private bool m_MouseInputEnabled;
        private bool m_KeyboardInputEnabled;
        private bool m_DrawBackground;

        private Pos m_Dock;

        private Cursor m_Cursor;

        private bool m_Tabable;

        private bool m_NeedsLayout;
        private bool m_CacheTextureDirty;
        private bool m_CacheToTexture;

        private Package m_DragAndDrop_Package;

        private object m_UserData;

        private bool m_DrawDebugOutlines;

        /// <summary>
        /// Real list of children.
        /// </summary>
        private readonly List<ControlBase> m_Children;


        /// <summary>
        /// Accelerator map.
        /// </summary>
        private readonly Dictionary<string, GwenEventHandler<EventArgs>> m_Accelerators;

        public const int MaxCoord = 4096; // added here from various places in code

        /// <summary>
        /// Logical list of children. If InnerPanel is not null, returns InnerPanel's children.
        /// </summary>
        public virtual List<ControlBase> Children
        {
            get
            {
                if (m_InnerPanel != null)
                    return m_InnerPanel.Children;
                return m_Children;
            }
        }

        /// <summary>
        /// The logical parent. It's usually what you expect, the control you've parented it to.
        /// </summary>
        public ControlBase Parent
        {
            get { return m_Parent; }
            set
            {
                if (m_Parent == value)
                    return;

                if (m_Parent != null)
                {
                    m_Parent.RemoveChild(this, false);
                }

                m_Parent = value;
                m_ActualParent = null;

                if (m_Parent != null)
                {
                    m_Parent.AddChild(this);
                }
            }
        }

        // todo: ParentChanged event?

        /// <summary>
        /// Dock position.
        /// </summary>
        public Pos Dock
        {
            get { return m_Dock; }
            set
            {
                if (m_Dock == value)
                    return;

                m_Dock = value;

                Invalidate();
                InvalidateParent();
            }
        }

        /// <summary>
        /// Current skin.
        /// </summary>
        public Skin.SkinBase Skin
        {
            get
            {
                if (m_Skin != null)
                    return m_Skin;
                if (m_Parent != null)
                    return m_Parent.Skin;
                if (Gwen.Skin.SkinBase.DefaultSkin != null)
                    return Gwen.Skin.SkinBase.DefaultSkin;
                
                throw new InvalidOperationException("GetSkin: null");
            }
        }

        /// <summary>
        /// Indicates whether this control is a menu component.
        /// </summary>
        internal virtual bool IsMenuComponent
        {
            get
            {
                if (m_Parent == null)
                    return false;
                return m_Parent.IsMenuComponent;
            }
        }

        /// <summary>
        /// Determines whether the control should be clipped to its bounds while rendering.
        /// </summary>
        protected virtual bool ShouldClip { get { return true; } }

        /// <summary>
        /// Current padding - inner spacing.
        /// </summary>
        public virtual Padding Padding
        {
            get { return m_Padding; }
            set
            {
                if (m_Padding == value)
                    return;

                m_Padding = value;
                Invalidate();
                InvalidateParent();
            }
        }

        /// <summary>
        /// Current margin - outer spacing.
        /// </summary>
        public virtual Margin Margin
        {
            get { return m_Margin; }
            set
            {
                if (m_Margin == value)
                    return;

                m_Margin = value;
                Invalidate();
                InvalidateParent();
            }
        }

        private string _tooltip = null;

        public virtual string Tooltip
        {
            get
            {
                return _tooltip;
            }
            set
            {
                _tooltip = value;
            }
        }

        /// <summary>
        /// Indicates whether the control is on top of its parent's children.
        /// </summary>
        public virtual bool IsOnTop { get { return this == Parent.m_Children.First(); } } // todo: validate

        /// <summary>
        /// User data associated with the control.
        /// </summary>
        public object UserData { get { return m_UserData; } set { m_UserData = value; } }

        /// <summary>
        /// Indicates whether the control is hovered by mouse pointer.
        /// </summary>
        public virtual bool IsHovered { get { return InputHandler.HoveredControl == this; } }

        /// <summary>
        /// Indicates whether the control has focus.
        /// </summary>
        public bool HasFocus { get { return InputHandler.KeyboardFocus == this; } }

        /// <summary>
        /// Indicates whether the control is disabled.
        /// </summary>
        public bool IsDisabled { get { return m_Disabled; } set { m_Disabled = value; } }

        /// <summary>
        /// Indicates whether the control is hidden.
        /// </summary>
        public virtual bool IsHidden { get { return m_Hidden; } set { if (value == m_Hidden) return; m_Hidden = value; Invalidate(); } }

        /// <summary>
        /// Determines whether the control's position should be restricted to parent's bounds.
        /// </summary>
        public bool RestrictToParent { get { return m_RestrictToParent; } set { m_RestrictToParent = value; } }

        /// <summary>
        /// Determines whether the control receives mouse input events.
        /// </summary>
        public virtual bool MouseInputEnabled { get { return m_MouseInputEnabled; } set { m_MouseInputEnabled = value; } }

        /// <summary>
        /// Determines whether the control receives keyboard input events.
        /// </summary>
        public bool KeyboardInputEnabled { get { return m_KeyboardInputEnabled; } set { m_KeyboardInputEnabled = value; } }

        /// <summary>
        /// Gets or sets the mouse cursor when the cursor is hovering the control.
        /// </summary>
        public Cursor Cursor { get { return m_Cursor; } set { m_Cursor = value; } }

        /// <summary>
        /// Indicates whether the control is tabable (can be focused by pressing Tab).
        /// </summary>
        public bool IsTabable { get { return m_Tabable; } set { m_Tabable = value; } }

        /// <summary>
        /// Indicates whether control's background should be drawn during rendering.
        /// </summary>
        public bool ShouldDrawBackground { get { return m_DrawBackground; } set { m_DrawBackground = value; } }

        /// <summary>
        /// Indicates whether the renderer should cache drawing to a texture to improve performance (at the cost of memory).
        /// </summary>
        public bool ShouldCacheToTexture { get { return m_CacheToTexture; } set { m_CacheToTexture = value; /*Children.ForEach(x => x.ShouldCacheToTexture=value);*/ } }

        /// <summary>
        /// Gets or sets the control's internal name.
        /// </summary>
        public string Name { get { return m_Name; } set { m_Name = value; } }

        /// <summary>
        /// Control's size and position relative to the parent.
        /// </summary>
        public Rectangle Bounds { get { return m_Bounds; } }

        /// <summary>
        /// Bounds for the renderer.
        /// </summary>
        public Rectangle RenderBounds { get { return m_RenderBounds; } }

        /// <summary>
        /// Bounds adjusted by padding.
        /// </summary>
        public Rectangle InnerBounds { get { return m_InnerBounds; } }

        /// <summary>
        /// Size restriction.
        /// </summary>
        public Point MinimumSize { get { return m_MinimumSize; } set { m_MinimumSize = value; } }

        /// <summary>
        /// Size restriction.
        /// </summary>
        public Point MaximumSize { get { return m_MaximumSize; } set { m_MaximumSize = value; } }

        private Point m_MinimumSize = new Point(1, 1);
        private Point m_MaximumSize = new Point(MaxCoord, MaxCoord);

        /// <summary>
        /// Determines whether hover should be drawn during rendering.
        /// </summary>
        protected bool ShouldDrawHover { get { return InputHandler.MouseFocus == this || InputHandler.MouseFocus == null; } }

        protected virtual bool AccelOnlyFocus { get { return false; } }
        protected virtual bool NeedsInputChars { get { return false; } }

        /// <summary>
        /// Indicates whether the control and its parents are visible.
        /// </summary>
        public bool IsVisible
        {
            get
            {
                if (IsHidden)
                    return false;

                if (Parent != null)
                    return Parent.IsVisible;

                return true;
            }
        }

        /// <summary>
        /// Leftmost coordinate of the control.
        /// </summary>
        public int X { get { return m_Bounds.X; } set { SetPosition(value, Y); } }

        /// <summary>
        /// Topmost coordinate of the control.
        /// </summary>
        public int Y { get { return m_Bounds.Y; } set { SetPosition(X, value); } }

        // todo: Bottom/Right includes margin but X/Y not?

        public int Width { get { return m_Bounds.Width; } set { SetSize(value, Height); } }
        public int Height { get { return m_Bounds.Height; } set { SetSize(Width, value); } }
        public int Bottom { get { return m_Bounds.Bottom + m_Margin.Bottom; } }
        public int Right { get { return m_Bounds.Right + m_Margin.Right; } }

        /// <summary>
        /// Determines whether margin, padding and bounds outlines for the control will be drawn. Applied recursively to all children.
        /// </summary>
        public bool DrawDebugOutlines
        {
            get { return m_DrawDebugOutlines; }
            set
            {
                if (m_DrawDebugOutlines == value)
                    return;
                m_DrawDebugOutlines = value;
                foreach (ControlBase child in Children)
                {
                    child.DrawDebugOutlines = value;
                }
            }
        }

        public Color PaddingOutlineColor { get; set; }
        public Color MarginOutlineColor { get; set; }
        public Color BoundsOutlineColor { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlBase"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public ControlBase(ControlBase parent = null)
        {
            m_Children = new List<ControlBase>();
            m_Accelerators = new Dictionary<string, GwenEventHandler<EventArgs>>();

            Parent = parent;

            m_Hidden = false;
            m_Bounds = new Rectangle(0, 0, 10, 10);
            m_Padding = Padding.Zero;
            m_Margin = Margin.Zero;

            RestrictToParent = false;

            MouseInputEnabled = true;
            KeyboardInputEnabled = false;

            Invalidate();
            Cursor = Cursors.Default;
            //ToolTip = null;
            IsTabable = false;
            ShouldDrawBackground = true;
            m_Disabled = false;
            m_CacheTextureDirty = true;
            m_CacheToTexture = false;

            BoundsOutlineColor = Color.Red;
            MarginOutlineColor = Color.Green;
            PaddingOutlineColor = Color.Blue;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            //Debug.Print("Control.Base: Disposing {0} {1:X}", this, GetHashCode());
            if (m_Disposed)
            {
#if DEBUG
                throw new ObjectDisposedException(String.Format("Control.Base [{1:X}] disposed twice: {0}", this, GetHashCode()));
#else
                return;
#endif
            }

            if (InputHandler.HoveredControl == this)
                InputHandler.HoveredControl = null;
            if (InputHandler.KeyboardFocus == this)
                InputHandler.KeyboardFocus = null;
            if (InputHandler.MouseFocus == this)
                InputHandler.MouseFocus = null;

            DragAndDrop.ControlDeleted(this);
            Gwen.ToolTip.ControlDeleted(this);
            Animation.Cancel(this);

            foreach (ControlBase child in m_Children)
                child.Dispose();

            m_Children.Clear();

            m_Disposed = true;
            GC.SuppressFinalize(this);
        }

#if DEBUG

        ~ControlBase()
        {
            throw new InvalidOperationException(String.Format("IDisposable object finalized [{1:X}]: {0}", this, GetHashCode()));
            //Debug.Print(String.Format("IDisposable object finalized: {0}", GetType()));
        }

#endif

        /// <summary>
        /// Detaches the control from canvas and adds to the deletion queue (processed in Canvas.DoThink).
        /// </summary>
        public void DelayedDelete()
        {
            GetCanvas().AddDelayedDelete(this);
        }

        public override string ToString()
        {
            if (this is MenuItem)
                return "[MenuItem: " + (this as MenuItem).Text + "]";
            if (this is Label)
                return "[Label: " + (this as Label).Text + "]";
            if (this is ControlInternal.Text)
                return "[Text: " + (this as ControlInternal.Text).String + "]";
            return GetType().ToString();
        }

        /// <summary>
        /// Gets the canvas (root parent) of the control.
        /// </summary>
        /// <returns></returns>
        public virtual Canvas GetCanvas()
        {
            ControlBase canvas = m_Parent;
            if (canvas == null)
                return null;

            return canvas.GetCanvas();
        }

        /// <summary>
        /// Enables the control.
        /// </summary>
        public void Enable()
        {
            IsDisabled = false;
        }

        /// <summary>
        /// Disables the control.
        /// </summary>
        public virtual void Disable()
        {
            IsDisabled = true;
        }

        /// <summary>
        /// Hides the control.
        /// </summary>
        public virtual void Hide()
        {
            IsHidden = true;
        }

        /// <summary>
        /// Shows the control.
        /// </summary>
        public virtual void Show()
        {
            IsHidden = false;
        }

        /// <summary>
        /// Creates a tooltip for the control.
        /// </summary>
        /// <param name="text">Tooltip text.</param>
        public virtual void SetToolTipText(string text)
        {
            Tooltip = text;
        }

        /// <summary>
        /// Sends the control to the bottom of paren't visibility stack.
        /// </summary>
        public virtual void SendToBack()
        {
            if (m_ActualParent == null)
                return;
            if (m_ActualParent.m_Children.Count == 0)
                return;
            if (m_ActualParent.m_Children.First() == this)
                return;

            m_ActualParent.m_Children.Remove(this);
            m_ActualParent.m_Children.Insert(0, this);

            InvalidateParent();
        }

        /// <summary>
        /// Brings the control to the top of paren't visibility stack.
        /// </summary>
        public virtual void BringToFront()
        {
            if (m_ActualParent == null)
                return;
            if (m_ActualParent.m_Children.Last() == this)
                return;

            m_ActualParent.m_Children.Remove(this);
            m_ActualParent.m_Children.Add(this);
            InvalidateParent();
            Redraw();
        }

        public virtual void BringNextToControl(ControlBase child, bool behind)
        {
            if (null == m_ActualParent)
                return;

            m_ActualParent.m_Children.Remove(this);

            // todo: validate
            int idx = m_ActualParent.m_Children.IndexOf(child);
            if (idx == m_ActualParent.m_Children.Count - 1)
            {
                BringToFront();
                return;
            }

            if (behind)
            {
                ++idx;

                if (idx == m_ActualParent.m_Children.Count - 1)
                {
                    BringToFront();
                    return;
                }
            }

            m_ActualParent.m_Children.Insert(idx, this);
            InvalidateParent();
        }

        /// <summary>
        /// Finds a child by name.
        /// </summary>
        /// <param name="name">Child name.</param>
        /// <param name="recursive">Determines whether the search should be recursive.</param>
        /// <returns>Found control or null.</returns>
        public virtual ControlBase FindChildByName(string name, bool recursive = false)
        {
            ControlBase b = m_Children.Find(x => x.m_Name == name);
            if (b != null)
                return b;

            if (recursive)
            {
                foreach (ControlBase child in m_Children)
                {
                    b = child.FindChildByName(name, true);
                    if (b != null)
                        return b;
                }
            }
            return null;
        }

        /// <summary>
        /// Attaches specified control as a child of this one.
        /// </summary>
        /// <remarks>
        /// If InnerPanel is not null, it will become the parent.
        /// </remarks>
        /// <param name="child">Control to be added as a child.</param>
        public virtual void AddChild(ControlBase child)
        {
            if (m_InnerPanel != null)
            {
                m_InnerPanel.AddChild(child);
            }
            else 
            {
                if (!m_Children.Contains(child))
                    m_Children.Add(child);
                    child.m_Parent = this;
                child.m_ActualParent = this;
            }
            OnChildAdded(child);
        }

        /// <summary>
        /// Detaches specified control from this one.
        /// </summary>
        /// <param name="child">Child to be removed.</param>
        /// <param name="dispose">Determines whether the child should be disposed (added to delayed delete queue).</param>
        public virtual void RemoveChild(ControlBase child, bool dispose)
        {
            // If we removed our innerpanel
            // remove our pointer to it
            if (m_InnerPanel == child)
            {
                m_Children.Remove(m_InnerPanel);
                m_InnerPanel.DelayedDelete();
                m_InnerPanel = null;
                return;
            }

            if (m_InnerPanel != null && m_InnerPanel.Children.Contains(child))
            {
                m_InnerPanel.RemoveChild(child, dispose);
                return;
            }

            m_Children.Remove(child);
            OnChildRemoved(child);

            if (dispose)
                child.DelayedDelete();
        }

        /// <summary>
        /// Removes all children (and disposes them).
        /// </summary>
        public virtual void DeleteAllChildren()
        {
            // todo: probably shouldn't invalidate after each removal
            while (m_Children.Count > 0)
                RemoveChild(m_Children[0], true);
        }

        /// <summary>
        /// Handler invoked when a child is added.
        /// </summary>
        /// <param name="child">Child added.</param>
        protected virtual void OnChildAdded(ControlBase child)
        {
            Invalidate();
        }

        /// <summary>
        /// Handler invoked when a child is removed.
        /// </summary>
        /// <param name="child">Child removed.</param>
        protected virtual void OnChildRemoved(ControlBase child)
        {
            Invalidate();
        }

        /// <summary>
        /// Moves the control by a specific amount.
        /// </summary>
        /// <param name="x">X-axis movement.</param>
        /// <param name="y">Y-axis movement.</param>
        public virtual void MoveBy(int x, int y)
        {
            SetBounds(X + x, Y + y, Width, Height);
        }

        /// <summary>
        /// Moves the control to a specific point.
        /// </summary>
        /// <param name="x">Target x coordinate.</param>
        /// <param name="y">Target y coordinate.</param>
        public virtual void MoveTo(float x, float y)
        {
            MoveTo((int)x, (int)y);
        }

        /// <summary>
        /// Moves the control to a specific point, clamping on paren't bounds if RestrictToParent is set.
        /// </summary>
        /// <param name="x">Target x coordinate.</param>
        /// <param name="y">Target y coordinate.</param>
        public virtual void MoveTo(int x, int y)
        {
            if (RestrictToParent && (Parent != null))
            {
                ControlBase parent = Parent;
                if (x - Padding.Left < parent.Margin.Left)
                    x = parent.Margin.Left + Padding.Left;
                if (y - Padding.Top < parent.Margin.Top)
                    y = parent.Margin.Top + Padding.Top;
                if (x + Width + Padding.Right > parent.Width - parent.Margin.Right)
                    x = parent.Width - parent.Margin.Right - Width - Padding.Right;
                if (y + Height + Padding.Bottom > parent.Height - parent.Margin.Bottom)
                    y = parent.Height - parent.Margin.Bottom - Height - Padding.Bottom;
            }

            SetBounds(x, y, Width, Height);
        }

        /// <summary>
        /// Sets the control position.
        /// </summary>
        /// <param name="x">Target x coordinate.</param>
        /// <param name="y">Target y coordinate.</param>
        public virtual void SetPosition(float x, float y)
        {
            SetPosition((int)x, (int)y);
        }

        /// <summary>
        /// Sets the control position.
        /// </summary>
        /// <param name="x">Target x coordinate.</param>
        /// <param name="y">Target y coordinate.</param>
        public virtual void SetPosition(int x, int y)
        {
            SetBounds(x, y, Width, Height);
        }

        /// <summary>
        /// Sets the control size.
        /// </summary>
        /// <param name="width">New width.</param>
        /// <param name="height">New height.</param>
        /// <returns>True if bounds changed.</returns>
        public virtual bool SetSize(int width, int height)
        {
            return SetBounds(X, Y, width, height);
        }

        /// <summary>
        /// Sets the control bounds.
        /// </summary>
        /// <param name="bounds">New bounds.</param>
        /// <returns>True if bounds changed.</returns>
        public virtual bool SetBounds(Rectangle bounds)
        {
            return SetBounds(bounds.X, bounds.Y, bounds.Width, bounds.Height);
        }

        /// <summary>
        /// Sets the control bounds.
        /// </summary>
        /// <param name="x">X.</param>
        /// <param name="y">Y.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <returns>
        /// True if bounds changed.
        /// </returns>
        public virtual bool SetBounds(float x, float y, float width, float height)
        {
            return SetBounds((int)x, (int)y, (int)width, (int)height);
        }

        /// <summary>
        /// Sets the control bounds.
        /// </summary>
        /// <param name="x">X position.</param>
        /// <param name="y">Y position.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <returns>
        /// True if bounds changed.
        /// </returns>
        public virtual bool SetBounds(int x, int y, int width, int height)
        {
            if (m_Bounds.X == x &&
                m_Bounds.Y == y &&
                m_Bounds.Width == width &&
                m_Bounds.Height == height)
                return false;

            Rectangle oldBounds = Bounds;

            m_Bounds.X = x;
            m_Bounds.Y = y;

            m_Bounds.Width = width;
            m_Bounds.Height = height;

            OnBoundsChanged(oldBounds);

            if (BoundsChanged != null)
                BoundsChanged.Invoke(this, EventArgs.Empty);

            return true;
        }

        /// <summary>
        /// Positions the control inside its parent.
        /// </summary>
        /// <param name="pos">Target position.</param>
        /// <param name="xpadding">X padding.</param>
        /// <param name="ypadding">Y padding.</param>
        public virtual void Position(Pos pos, int xpadding = 0, int ypadding = 0) // todo: a bit ambiguous name
        {
            int w = Parent.Width;
            int h = Parent.Height;
            Padding padding = Parent.Padding;

            int x = X;
            int y = Y;
            if (0 != (pos & Pos.Left)) x = padding.Left + xpadding;
            if (0 != (pos & Pos.Right)) x = w - Width - padding.Right - xpadding;
            if (0 != (pos & Pos.CenterH))
                x = (int)(padding.Left + xpadding + (w - Width - padding.Left - padding.Right) * 0.5f);

            if (0 != (pos & Pos.Top)) y = padding.Top + ypadding;
            if (0 != (pos & Pos.Bottom)) y = h - Height - padding.Bottom - ypadding;
            if (0 != (pos & Pos.CenterV))
                y = (int)(padding.Top + ypadding + (h - Height - padding.Bottom - padding.Top) * 0.5f);

            SetPosition(x, y);
        }

        /// <summary>
        /// Handler invoked when control's bounds change.
        /// </summary>
        /// <param name="oldBounds">Old bounds.</param>
        protected virtual void OnBoundsChanged(Rectangle oldBounds)
        {
            //Anything that needs to update on size changes
            //Iterate my children and tell them I've changed
            //
            if (Parent != null)
                Parent.OnChildBoundsChanged(oldBounds, this);

            if (m_Bounds.Width != oldBounds.Width || m_Bounds.Height != oldBounds.Height)
            {
                Invalidate();
            }

            Redraw();
            UpdateRenderBounds();
        }

        /// <summary>
        /// Handler invoked when control's scale changes.
        /// </summary>
        protected virtual void OnScaleChanged()
        {
            foreach (ControlBase child in m_Children)
            {
                child.OnScaleChanged();
            }
        }

        /// <summary>
        /// Handler invoked when control children's bounds change.
        /// </summary>
        protected virtual void OnChildBoundsChanged(Rectangle oldChildBounds, ControlBase child)
        {
        }


        /// <summary>
        /// Gets a child by its coordinates.
        /// </summary>
        /// <param name="x">Child X.</param>
        /// <param name="y">Child Y.</param>
        /// <returns>Control or null if not found.</returns>
        public virtual ControlBase GetControlAt(int x, int y)
        {
            if (IsHidden)
                return null;

            if (x < 0 || y < 0 || x >= Width || y >= Height)
                return null;

            // todo: convert to linq FindLast
            var rev = ((IList<ControlBase>)m_Children).Reverse(); // IList.Reverse creates new list, List.Reverse works in place.. go figure
            foreach (ControlBase child in rev)
            {
                ControlBase found = child.GetControlAt(x - child.X, y - child.Y);
                if (found != null)
                    return found;
            }

            if (!MouseInputEnabled)
                return null;

            return this;
        }

        /// <summary>
        /// Lays out the control's interior according to alignment, padding, dock etc.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected virtual void Layout(Skin.SkinBase skin)
        {
            if (skin.Renderer.CTT != null && ShouldCacheToTexture)
                skin.Renderer.CTT.CreateControlCacheTexture(this);
        }

        /// <summary>
        /// Recursively lays out the control's interior according to alignment, margin, padding, dock etc.
        /// </summary>
        public void Layout()
        {
            RecurseLayout(Skin);
        }

        /// <summary>
        /// Recursively lays out the control's interior according to alignment, margin, padding, dock etc.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected virtual void RecurseLayout(Skin.SkinBase skin)
        {
            if (m_Skin != null)
                skin = m_Skin;
            if (IsHidden)
                return;

            if (m_NeedsLayout)
            {
                m_NeedsLayout = false;
                Layout(skin);
            }

            Rectangle bounds = RenderBounds;

            // Adjust bounds for padding
            bounds.X += m_Padding.Left;
            bounds.Width -= m_Padding.Left + m_Padding.Right;
            bounds.Y += m_Padding.Top;
            bounds.Height -= m_Padding.Top + m_Padding.Bottom;

            foreach (ControlBase child in m_Children)
            {
                if (child.IsHidden)
                    continue;

                Pos dock = child.Dock;

                if (0 != (dock & Pos.Fill))
                    continue;

                if (dock.HasFlag((Pos.Top)))
                {
                    Margin margin = child.Margin;

                    child.SetBounds(bounds.X + margin.Left, bounds.Y + margin.Top,
                                    bounds.Width - margin.Left - margin.Right, child.Height);

                    int height = margin.Top + margin.Bottom + child.Height;
                    bounds.Y += height;
                    bounds.Height -= height;
                }

                if (dock.HasFlag((Pos.Left)))
                {
                    Margin margin = child.Margin;

                    child.SetBounds(bounds.X + margin.Left, 
                                    bounds.Y + margin.Top, 
                                    child.Width,
                                      bounds.Height - margin.Top - margin.Bottom);

                    int width = margin.Left + margin.Right + child.Width;
                    bounds.X += width;
                    bounds.Width -= width;
                }

                if (dock.HasFlag((Pos.Right)))
                {
                    // TODO: THIS MARGIN CODE MIGHT NOT BE FULLY FUNCTIONAL
                    Margin margin = child.Margin;

                    child.SetBounds((bounds.X + bounds.Width) - child.Width - margin.Right, 
                                    bounds.Y + margin.Top,
                                      child.Width, 
                                    bounds.Height - margin.Top - margin.Bottom);

                    int width = margin.Left + margin.Right + child.Width;
                    bounds.Width -= width;
                }

                if (dock.HasFlag((Pos.Bottom)))
                {
                    // TODO: THIS MARGIN CODE MIGHT NOT BE FULLY FUNCTIONAL
                    Margin margin = child.Margin;

                    child.SetBounds(bounds.X + margin.Left,
                                      (bounds.Y + bounds.Height) - child.Height - margin.Bottom,
                                      bounds.Width - margin.Left - margin.Right, child.Height);
                    bounds.Height -= child.Height + margin.Bottom + margin.Top;
                }

                child.RecurseLayout(skin);
            }

            m_InnerBounds = bounds;

            //
            // Fill uses the left over space, so do that now.
            //
            foreach (ControlBase child in m_Children)
            {
                Pos dock = child.Dock;

                if (!(0 != (dock & Pos.Fill)))
                    continue;

                Margin margin = child.Margin;

                child.SetBounds(bounds.X + margin.Left, bounds.Y + margin.Top,
                                  bounds.Width - margin.Left - margin.Right, bounds.Height - margin.Top - margin.Bottom);
                child.RecurseLayout(skin);
            }

            PostLayout(skin);

            if (IsTabable)
            {
                if (GetCanvas().FirstTab == null)
                    GetCanvas().FirstTab = this;
                if (GetCanvas().NextTab == null)
                    GetCanvas().NextTab = this;
            }

            if (InputHandler.KeyboardFocus == this)
            {
                GetCanvas().NextTab = null;
            }
        }

        /// <summary>
        /// Checks if the given control is a child of this instance.
        /// </summary>
        /// <param name="child">Control to examine.</param>
        /// <returns>True if the control is out child.</returns>
        public bool IsChild(ControlBase child)
        {
            return m_Children.Contains(child);
        }

        /// <summary>
        /// Converts local coordinates to canvas coordinates.
        /// </summary>
        /// <param name="pnt">Local coordinates.</param>
        /// <returns>Canvas coordinates.</returns>
        public virtual Point LocalPosToCanvas(Point pnt)
        {
            if (m_Parent != null)
            {
                int x = pnt.X + X;
                int y = pnt.Y + Y;

                // If our parent has an innerpanel and we're a child of it
                // add its offset onto us.
                //
                if (m_Parent.m_InnerPanel != null && m_Parent.m_InnerPanel.IsChild(this))
                {
                    x += m_Parent.m_InnerPanel.X;
                    y += m_Parent.m_InnerPanel.Y;
                }

                return m_Parent.LocalPosToCanvas(new Point(x, y));
            }

            return pnt;
        }

        /// <summary>
        /// Converts canvas coordinates to local coordinates.
        /// </summary>
        /// <param name="pnt">Canvas coordinates.</param>
        /// <returns>Local coordinates.</returns>
        public virtual Point CanvasPosToLocal(Point pnt)
        {
            if (m_Parent != null)
            {
                int x = pnt.X - X;
                int y = pnt.Y - Y;

                // If our parent has an innerpanel and we're a child of it
                // add its offset onto us.
                //
                if (m_Parent.m_InnerPanel != null && m_Parent.m_InnerPanel.IsChild(this))
                {
                    x -= m_Parent.m_InnerPanel.X;
                    y -= m_Parent.m_InnerPanel.Y;
                }

                return m_Parent.CanvasPosToLocal(new Point(x, y));
            }

            return pnt;
        }

        /// <summary>
        /// Closes all menus recursively.
        /// </summary>
        public virtual void CloseMenus()
        {
            //Debug.Print("Base.CloseMenus: {0}", this);

            // todo: not very efficient with the copying and recursive closing, maybe store currently open menus somewhere (canvas)?
            var childrenCopy = m_Children.FindAll(x => true);
            foreach (ControlBase child in childrenCopy)
            {
                child.CloseMenus();
            }
        }

        /// <summary>
        /// Copies Bounds to RenderBounds.
        /// </summary>
        protected virtual void UpdateRenderBounds()
        {
            m_RenderBounds.X = 0;
            m_RenderBounds.Y = 0;

            m_RenderBounds.Width = m_Bounds.Width;
            m_RenderBounds.Height = m_Bounds.Height;
        }

        /// <summary>
        /// Sets mouse cursor to current cursor.
        /// </summary>
        public virtual void UpdateCursor()
        {
            Platform.Neutral.SetCursor(m_Cursor);
        }
        /// <summary>
        /// Resizes the control to fit its children.
        /// </summary>
        /// <param name="width">Determines whether to change control's width.</param>
        /// <param name="height">Determines whether to change control's height.</param>
        /// <returns>True if bounds changed.</returns>
        public virtual bool SizeToChildren(bool width = true, bool height = true)
        {
            Point size = GetChildrenSize();
            size.X += Padding.Right;
            size.Y += Padding.Bottom;
            return SetSize(width ? size.X : Width, height ? size.Y : Height);
        }

        /// <summary>
        /// Returns the total width and height of all children.
        /// </summary>
        /// <remarks>Default implementation returns maximum size of children since the layout is unknown.
        /// Implement this in derived compound controls to properly return their size.</remarks>
        /// <returns></returns>
        public virtual Point GetChildrenSize()
        {
            Point size = Point.Empty;

            foreach (ControlBase child in m_Children)
            {
                if (child.IsHidden)
                    continue;

                size.X = Math.Max(size.X, child.Right);
                size.Y = Math.Max(size.Y, child.Bottom);
            }

            return size;
        }

        /// <summary>
        /// Function invoked after layout.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected virtual void PostLayout(Skin.SkinBase skin)
        {
        }

        /// <summary>
        /// Invalidates control's parent.
        /// </summary>
        public void InvalidateParent()
        {
            if (m_Parent != null)
            {
                m_Parent.Invalidate();
            }
        }

        public virtual void Anim_WidthIn(float length, float delay = 0.0f, float ease = 1.0f)
        {
            Animation.Add(this, new Anim.Size.Width(0, Width, length, false, delay, ease));
            Width = 0;
        }

        public virtual void Anim_HeightIn(float length, float delay, float ease)
        {
            Animation.Add(this, new Anim.Size.Height(0, Height, length, false, delay, ease));
            Height = 0;
        }

        public virtual void Anim_WidthOut(float length, bool hide, float delay, float ease)
        {
            Animation.Add(this, new Anim.Size.Width(Width, 0, length, hide, delay, ease));
        }

        public virtual void Anim_HeightOut(float length, bool hide, float delay, float ease)
        {
            Animation.Add(this, new Anim.Size.Height(Height, 0, length, hide, delay, ease));
        }

        public void FitChildrenToSize()
        {
            foreach (ControlBase child in Children)
            {
                //push them back into view if they are outside it
                child.X = Math.Min(Bounds.Width, child.X + child.Width) - child.Width;
                child.Y = Math.Min(Bounds.Height, child.Y + child.Height) - child.Height;

                //Non-negative has priority, so do it second.
                child.X = Math.Max(0, child.X);
                child.Y = Math.Max(0, child.Y);
            }
        }
    }
}