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
using System.Collections;

namespace Gwen.Controls
{
    public partial class ControlBase
    {
        public class ControlCollection : IList<ControlBase>
        {
            private ControlBase _owner;
            private List<ControlBase> _controls;
            public ControlBase this[int index]
            {
                get
                {
                    return _controls[index];
                }
                set
                {
                    throw new Exception("Cannot use setter for ControlBase.Children, use AddChild instead.");
                }
            }

            public int Count => _controls.Count;

            public bool IsReadOnly => false;

            public ControlCollection(ControlBase parent)
            {
                _owner = parent;
                _controls = new List<ControlBase>();
            }

            internal void SendToBack(int index)
            {
                if (Count != 0 && index != 0)
                {
                    var control = _controls[index];
                    _controls.RemoveAt(index);
                    _controls.Insert(0, control);
                }
            }
            internal void BringToFront(int index)
            {
                if (Count != 0 && index != Count - 1)
                {
                    var control = _controls[index];
                    _controls.RemoveAt(index);
                    _controls.Add(control);
                }
            }

            public void Add(ControlBase item)
            {
                Insert(Count, item);
            }

            public void Clear()
            {
                while (Count > 0)
                    RemoveAt(Count - 1);
            }

            public bool Contains(ControlBase item)
            {
                return _controls.Contains(item);
            }

            public void CopyTo(ControlBase[] array, int arrayIndex)
            {
                _controls.CopyTo(array, arrayIndex);
            }

            public IEnumerator<ControlBase> GetEnumerator()
            {
                return _controls.GetEnumerator();
            }

            public int IndexOf(ControlBase item)
            {
                return _controls.IndexOf(item);
            }

            public void Insert(int index, ControlBase item)
            {
                if (!Contains(item))
                {
                    if (item.m_Parent != null && item.m_Parent != _owner)
                    {
                        //remove previous parent
                        item.m_Parent.RemoveChild(item,false);
                    }
                    _controls.Insert(index, item);
                    item.m_Parent = _owner;
                }
            }

            public bool Remove(ControlBase item)
            {
                var idx = _controls.IndexOf(item);
                if (idx == -1)
                    return false;
                RemoveAt(idx);
                return true;
            }

            public void RemoveAt(int index)
            {
                _controls[index].m_Parent = null;
                _controls.RemoveAt(index);
            }
            public ControlBase[] ToArray()
            {
                var copy = new ControlBase[Count];
                CopyTo(copy, 0);
                return copy;
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IList)_controls).GetEnumerator();
            }
        }
    }
}