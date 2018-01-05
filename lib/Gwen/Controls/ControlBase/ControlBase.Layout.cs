using Gwen.Anim;
using Gwen.DragDrop;
using Gwen.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Gwen.Controls.Layout;
namespace Gwen.Controls
{
    public partial class ControlBase
    {
        private bool m_AutoSizeToContents;
        internal bool NeedsLayout => m_NeedsLayout;
        /// <summary>
        /// Bounds adjusted by padding.
        /// </summary>
        internal virtual Rectangle InnerBounds
        {
            get
            {
                var padding = this.Padding;
                var bounds = this.Bounds;
                bounds.X += padding.Left;
                bounds.Width -= padding.Left + padding.Right;
                bounds.Y += padding.Top;
                bounds.Height -= padding.Top + padding.Bottom;
                return bounds;
            }
        }
        /// <summary>
        /// Bounds adjusted by margin.
        /// </summary>
        internal virtual Rectangle OuterBounds
        {
            get
            {
                var margin = this.Margin;
                var bounds = this.Bounds;
                bounds.X += margin.Left;
                bounds.Width -= margin.Left + margin.Right;
                bounds.Y += margin.Top;
                bounds.Height -= margin.Top + margin.Bottom;
                return bounds;
            }
        }
        /// <summary>
        /// Determines if the control should autosize to its text.
        /// </summary>
        public virtual bool AutoSizeToContents
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
        /// Function invoked before layout, after AutoSizeToContents if applicable
        /// Is called regardless of needslayout.
        /// </summary>
        protected virtual void PrepareLayout()
        {
        }
        /// <summary>
        /// Function invoked after layout.
        /// </summary>
        protected virtual void PostLayout()
        {
        }
        /// <summary>
        /// Function that does the layout process. Applying child docks, etc
        /// Does *not* resize ourself
        /// </summary>
        protected virtual void ProcessLayout()
        {
            var control = this;
            Rectangle bounds = new Rectangle(control.Padding.Left,
                                            control.Padding.Top,
                                            control.Width - (control.Padding.Right + control.Padding.Left),
                                            control.Height - (control.Padding.Bottom + control.Padding.Top));
            foreach (var child in control.m_Children)
            {
                if (child.IsHidden)
                    continue;
                //ignore fill for now, as it uses total free space.
                if (child.Dock != Pos.Fill)
                {
                    var dock = CalculateBounds(child, ref bounds);
                    child.SetBounds(dock);
                }
            }
            foreach (var child in control.m_Children)
            {
                if (child.IsHidden)
                    continue;
                // fill uses leftover space
                if (child.Dock == Pos.Fill)
                {
                    child.SetBounds(bounds.X + child.Margin.Left,
                                    bounds.Y + child.Margin.Top,
                                    bounds.Width - child.Margin.Left - child.Margin.Right,
                                    bounds.Height - child.Margin.Top - child.Margin.Bottom);
                }
            }
        }
        /// <summary>
        /// Recursively lays out the control's interior according to alignment, margin, padding, dock etc.
        /// If AutoSizeToContents is enabled, sizes the control before layout.
        /// </summary>
        public void Layout(bool force = true, bool recursioncheck = false)
        {
            if (IsHidden)
                return;
            var shouldlayout = m_NeedsLayout || force;
            if (shouldlayout)
            {
                if (AutoSizeToContents)
                {
                    var sz = GetSizeToFitContents();
                    SetBounds(X, Y, sz.Width, sz.Height);
                }
                PrepareLayout();
                ProcessLayout();
                m_NeedsLayout = false;
                foreach (var child in m_Children)
                {
                    child.Layout(force);
                }
                PostLayout();
            }
            else
            {
                PrepareLayout();
                foreach (var child in m_Children)
                {
                    child.Layout(false);
                }
                if (m_NeedsLayout)
                {
                    if (recursioncheck)
                        throw new Exception("Recursion check failed.");
                    Layout(true, true);
                }
            }
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
        /// Positions the control inside its parent relative to its edges
        /// </summary>
        /// <param name="pos">Target position.</param>
        /// <param name="xpadding">X padding.</param>
        /// <param name="ypadding">Y padding.</param>
        public void AlignToEdge(Pos pos, int xpadding = 0, int ypadding = 0)
        {
            AlignToEdge(pos, new Padding(xpadding, ypadding, xpadding, ypadding));
        }
        /// <summary>
        /// Positions the control inside its parent relative to its edges
        /// </summary>
        /// <param name="pos">Target position.</param>
        /// <param name="xpadding">X padding.</param>
        /// <param name="ypadding">Y padding.</param>
        public virtual void AlignToEdge(Pos pos, Padding padding)
        {
            int newx = X;
            int newy = Y;

            if (pos.HasFlag(Pos.Left))
            {
                newx = Parent.Padding.Left + padding.Left;
            }
            else if (pos.HasFlag(Pos.Right))
            {
                newx = (Parent.Width - Width) - (Parent.Padding.Right + padding.Right);
            }
            else if (pos.HasFlag(Pos.CenterH))
            {
                var left = Parent.Padding.Left + padding.Left;
                var right = (Parent.Width - Width) - (Parent.Padding.Right + padding.Right);
                newx = left + (right / 2);
            }

            if (pos.HasFlag(Pos.Top))
            {
                newy = Parent.Padding.Top + padding.Top;
            }
            else if (pos.HasFlag(Pos.Bottom))
            {
                newy = (Parent.Height - Height) - (Parent.Padding.Bottom + padding.Bottom);
            }
            else if (pos.HasFlag(Pos.CenterV))
            {
                var top = Parent.Padding.Top + padding.Top;
                var bot = (Parent.Height - Height) - (Parent.Padding.Bottom + padding.Bottom);
                newy = top + (bot / 2);
            }

            SetPosition(newx, newy);
        }
        /// <summary>
        /// Gets the minimum size of the control based on its children
        /// </summary>
        public virtual Size GetSizeToFitContents()
        {
            var control = this;
            //we need minimum size
            Size size = Size.Empty;
            Size maxundocked = Size.Empty;
            int verticaldock = 0;
            int horzdock = 0;
            // Adjust bounds for padding
            foreach (var child in control.m_Children)
            {
                if (child.IsHidden)
                    continue;
                //ignore fill for now, as it uses total free space.
                if (child.Dock != Pos.Fill)
                {
                    var childsize = child.Bounds.Size;
                    if (child.AutoSizeToContents)
                    {
                        childsize = child.GetSizeToFitContents();
                    }
                    childsize.Width += child.Margin.Left + child.Margin.Right;
                    childsize.Height += child.Margin.Top + child.Margin.Bottom;
                    if (child.Dock == Pos.None)
                    {
                        maxundocked.Width = Math.Max(maxundocked.Width, child.X + childsize.Width);
                        maxundocked.Height = Math.Max(maxundocked.Height, child.Y + childsize.Height);
                        continue;
                    }

                    if (child.Dock == Pos.Top || child.Dock == Pos.Bottom)
                    {
                        verticaldock += childsize.Height;
                        size.Height += childsize.Height;
                        var avail = size.Width - horzdock;
                        if (childsize.Width > avail)
                        {
                            size.Width += childsize.Width - avail;
                        }
                    }
                    else if (child.Dock == Pos.Right || child.Dock == Pos.Left)
                    {
                        horzdock += childsize.Width;
                        size.Width += childsize.Width;
                        var avail = size.Height - verticaldock;
                        if (childsize.Height > size.Height - verticaldock)
                        {
                            size.Height += childsize.Height - avail;
                        }
                    }
                }
                else
                {
                    var childsize = child.Bounds.Size;
                    if (child.AutoSizeToContents)
                    {
                        childsize = child.GetSizeToFitContents();
                    }
                    childsize.Width += child.Margin.Left + child.Margin.Right;
                    childsize.Height += child.Margin.Top + child.Margin.Bottom;
                    // fill is lowest priority
                    maxundocked.Width = Math.Max(maxundocked.Width, childsize.Width);
                    maxundocked.Height = Math.Max(maxundocked.Height, childsize.Height);
                }
            }
            // if theres a control placed somewhere greater than our dock needs,
            // size to that.
            size.Width = Math.Max(maxundocked.Width, size.Width);
            size.Height = Math.Max(maxundocked.Height, size.Height);

            size.Width += control.Padding.Left + control.Padding.Right;
            size.Height += control.Padding.Top + control.Padding.Bottom;
            return size;
        }
        /// <summary>
        /// Calculates a child's new bounds according to its Dock property and applies AutoSizeToContents
        /// </summary>
        /// <param name="control">The child control.</param>
        /// <param name="area">The area available to dock to.</param>
        /// <returns>New bounds of the control.</returns>
        public static Rectangle CalculateBounds(ControlBase control, ref Rectangle area)
        {
            if (control.IsHidden)
                return control.Bounds;
            Rectangle ret = control.Bounds;
            if (control.AutoSizeToContents)
            {
                ret.Size = control.GetSizeToFitContents();
            }
            if (control.Dock == Pos.None)
            {
                return ret;
            }
            Margin cm = control.Margin;
            if (control.Dock == Pos.Left)
            {
                ret = new Rectangle(area.X + cm.Left,
                                area.Y + cm.Top,
                                ret.Width,
                                area.Height - cm.Top - cm.Bottom);

                int width = cm.Left + cm.Right + ret.Width;
                area.X += width;
                area.Width -= width;
            }
            else if (control.Dock == Pos.Right)
            {
                ret = new Rectangle((area.X + area.Width) - ret.Width - cm.Right,
                                area.Y + cm.Top,
                                ret.Width,
                                area.Height - cm.Top - cm.Bottom);

                int width = cm.Left + cm.Right + ret.Width;
                area.Width -= width;
            }
            else if (control.Dock == Pos.Top)
            {
                ret = new Rectangle(area.X + cm.Left,
                                area.Y + cm.Top,
                                area.Width - cm.Left - cm.Right,
                                ret.Height);

                int height = cm.Top + cm.Bottom + ret.Height;
                area.Y += height;
                area.Height -= height;
            }
            else if (control.Dock == Pos.Bottom)
            {
                ret = new Rectangle(area.X + cm.Left,
                                (area.Y + area.Height) - ret.Height - cm.Bottom,
                                area.Width - cm.Left - cm.Right,
                                ret.Height);
                area.Height -= ret.Height + cm.Bottom + cm.Top;
            }
            else
            {
                throw new Exception("Unhandled Dock Pos" + control.Dock);
            }
            return ret;
        }
        /// <summary>
        /// Resizes the control to fit its children.
        /// </summary>
        /// <param name="width">Determines whether to change control's width.</param>
        /// <param name="height">Determines whether to change control's height.</param>
        /// <returns>True if bounds changed.</returns>
        public virtual bool SizeToChildren(bool width = true, bool height = true)
        {
            Size size = GetSizeToFitContents();
            return SetSize(width ? size.Width : Width, height ? size.Height : Height);
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

        /// <summary>
        /// Sends the control to the bottom of the parent's visibility stack.
        /// </summary>
        public virtual void SendToBack()
        {
            if (m_Parent == null)
                return;
            m_Parent.SendChildToBack(this);
        }

        /// <summary>
        /// Brings the control to the top of the parent's visibility stack.
        /// </summary>
        public virtual void BringToFront()
        {
            if (m_Parent == null)
                return;
            m_Parent.BringChildToFront(this);
        }
        /// <summary>
        /// Sends the child to the bottom of the visibility stack.
        /// </summary>
        public virtual void SendChildToBack(ControlBase control)
        {
            var idx = Children.IndexOf(control);
            if (idx != -1)
            {
                Children.SendToBack(idx);
                InvalidateParent();
                Redraw();
            }
            else
            {
                throw new Exception("Unable to send control to back of parent -- index missing");
            }
        }
        /// <summary>
        /// Sends the child to the top of the visibility stack.
        /// </summary>
        public virtual void BringChildToFront(ControlBase control)
        {
            var idx = Children.IndexOf(control);
            if (idx != -1)
            {
                Children.BringToFront(idx);
                InvalidateParent();
                Redraw();
            }
            else
            {
                throw new Exception("Unable to send control to front of parent -- index missing");
            }
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
            if (!Children.Contains(child))
                Children.Add(child);
            child.m_Parent = this;
            OnChildAdded(child);
        }

        /// <summary>
        /// Detaches specified control from this one.
        /// </summary>
        /// <param name="child">Child to be removed.</param>
        /// <param name="dispose">Determines whether the child should be disposed (added to delayed delete queue).</param>
        public virtual void RemoveChild(ControlBase child, bool dispose)
        {

            Children.Remove(child);
            OnChildRemoved(child);

            if (dispose)
                child.DelayedDelete();
        }

        /// <summary>
        /// Checks if the given control is a child of this instance.
        /// </summary>
        /// <param name="child">Control to examine.</param>
        /// <returns>True if the control is out child.</returns>
        public bool IsChild(ControlBase child)
        {
            return Children.Contains(child);
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
        }
        /// <summary>
        /// Handler invoked when control children's bounds change.
        /// </summary>
        protected virtual void OnChildBoundsChanged(Rectangle oldChildBounds, ControlBase child)
        {
            if (AutoSizeToContents)
                Invalidate();
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
    }
}