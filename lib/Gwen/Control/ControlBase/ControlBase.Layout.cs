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
    public partial class ControlBase
    {

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
        /// Function invoked after layout.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected virtual void PostLayout(Skin.SkinBase skin)
        {
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
        /// Handler invoked when control children's bounds change.
        /// </summary>
        protected virtual void OnChildBoundsChanged(Rectangle oldChildBounds, ControlBase child)
        {
        }

        /// <summary>
        /// Sends the control to the bottom of paren't visibility stack.
        /// </summary>
        public virtual void SendToBack()
        {
            if (m_Parent == null)
                return;
            var idx = m_Parent.Children.IndexOf(this);
            if (idx != -1)
			{
				m_Parent.Children.SendToBack(idx);
            }
            else
			{
                if (m_Parent.m_InnerPanel != null)
				{
					idx = m_Parent.m_Children.IndexOf(this);
				}
				if (idx == -1)
					throw new Exception("Unable to send control to back of parent -- index missing");
				m_Parent.m_Children.SendToBack(idx);
            }
            InvalidateParent();
        }

        /// <summary>
        /// Brings the control to the top of parent visibility stack.
        /// </summary>
        public virtual void BringToFront()
        {
            if (m_Parent == null)
				return;
			var idx = m_Parent.Children.IndexOf(this);
			if (idx != -1)
			{
				m_Parent.Children.BringToFront(idx);
			}
			else
			{
				if (m_Parent.m_InnerPanel != null)
				{
					idx = m_Parent.m_Children.IndexOf(this);
				}
				if (idx == -1)
					throw new Exception("Unable to send control to front of parent -- index missing");
				m_Parent.m_Children.BringToFront(idx);
			}
            InvalidateParent();
            Redraw();
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
        /// Checks if the given control is a child of this instance.
        /// </summary>
        /// <param name="child">Control to examine.</param>
        /// <returns>True if the control is out child.</returns>
        public bool IsChild(ControlBase child)
        {
            return m_Children.Contains(child);
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
    }
}