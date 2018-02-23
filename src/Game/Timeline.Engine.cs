//
//  PlaybackBufferManager.cs
//
//  Author:
//       Noah Ablaseau <nablaseau@hotmail.com>
//
//  Copyright (c) 2017 
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Text;
using OpenTK;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using linerider.Tools;
using linerider.Rendering;
using linerider.Game;
using linerider.Lines;
using linerider.UI;
using linerider.Utils;
using System.Diagnostics;

namespace linerider.Game
{
    /// <summary>
    /// playback scrubber/buffer manager
    /// properties:
    /// has full access to track.RiderStates at all times
    /// calls thread safe access to createtrackreader to simulate
    /// </summary>
    public partial class Timeline
    {
        private readonly object _changesync = new object();
        private HashSet<GridPoint> _changedcells = new HashSet<GridPoint>();
        private int _first_invalid_frame = 1;
        public void SaveCells(Vector2d start, Vector2d end)
        {
            var positions = SimulationGrid.GetGridPositions(start, end, _track.Grid.GridVersion);
            lock(_changesync)
            {
                foreach (var cellpos in positions)
                {
                    _changedcells.Add(cellpos.Point);
                }
            }
        }
        public void NotifyChanged()
        {
            lock (_changesync)
            {
                int start = FindUpdateStart();
                _changedcells.Clear();
                if (start != -1)
                {
                    _first_invalid_frame = Math.Min(start, _first_invalid_frame);
                }
            }
        }
        private int FindUpdateStart()
        {
            if (_changedcells.Count == 0)
                return -1;
            RectLRTB changebounds = new RectLRTB(_changedcells.First());
            foreach (var cell in _changedcells)
            {
                changebounds.left = Math.Min(cell.X, changebounds.left);
                changebounds.top = Math.Min(cell.Y, changebounds.top);
                changebounds.right = Math.Max(cell.X, changebounds.right);
                changebounds.bottom = Math.Max(cell.Y, changebounds.bottom);
            }
            return CalculateFirstInteraction(changebounds);
        }
        private int CalculateFirstInteraction(RectLRTB changebounds)
        {
            int framecount = _frames.Count;
            for (int frame = 1; frame < framecount; frame++)
            {
                if (!changebounds.Intersects(_frames[frame].PhysicsBounds))
                    continue;
                foreach (var change in _changedcells)
                {
                    if (_frames[frame].PhysicsBounds.ContainsPoint(change))
                    {
                        if (CheckInteraction(frame))
                            return frame;
                        // we dont have to check this rider more than once!
                        break;
                    }
                }
            }
            return -1;
        }
        private bool CheckInteraction(int frame)
        {
            // even though its this frame that may need changing, we have to regenerate it using
            // the previous frame.
            var newsimulated = _frames[frame - 1].Simulate(
                _track.Grid,
                _track.Bones,
                null,
                null,
                6,
                false);
            if (!newsimulated.Body.CompareTo(_frames[frame].Body))
            {
                return true;
            }

            return false;
        }
    }
}
