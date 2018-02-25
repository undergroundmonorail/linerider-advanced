//
//  PencilTool.cs
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
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Color = System.Drawing.Color;
using linerider.Rendering;
using linerider.Lines;

namespace linerider.Tools
{
    public class PencilTool : Tool
    {
        public override bool RequestsMousePrecision
        {
            get
            {
                return DrawingScenery;
            }
        }
        public override bool NeedsRender
        {
            get
            {
                return DrawingScenery || Active;
            }
        }
        public bool Snapped = false;
        private Vector2d _start;
        private Vector2d _end;
        const float MINIMUM_LINE = 0.5f;
        private bool _addflip = false;
        private Vector2d _mouseshadow;
        private bool DrawingScenery
        {
            get
            {
                return game.Canvas.ColorControls.Selected == LineType.Scenery;
            }
        }
        public override MouseCursor Cursor
        {
            get { return game.Cursors["pencil"]; }
        }
        public PencilTool() : base() { }
        public override void OnMouseDown(Vector2d pos)
        {
            Active = true;

            if (game.EnableSnap)
            {
                var gamepos = ScreenToGameCoords(pos);
                using (var trk = game.Track.CreateTrackReader())
                {
                    var snap = TrySnapPoint(trk, gamepos, out bool snapped);
                    if (snapped)
                    {
                        _start = snap;
                        Snapped = true;
                    }
                    else
                    {
                        _start = gamepos;
                        Snapped = false;
                    }
                }
            }
            else
            {
                _start = ScreenToGameCoords(pos);
                Snapped = false;
            }
            _addflip = UI.InputUtils.Check(UI.Hotkey.LineToolFlipLine);
            _end = _start;
            game.Invalidate();
            base.OnMouseDown(pos);
        }
        public override void OnChangingTool()
        {
            Stop();
            _mouseshadow = Vector2d.Zero;
        }
        private void AddLine()
        {
            using (var trk = game.Track.CreateTrackWriter())
            {
                game.Track.UndoManager.BeginAction();
                var added = CreateLine(trk, _start, _end, false, Snapped, false);
                if (added is StandardLine)
                {
                    game.Track.NotifyTrackChanged();
                }
                game.Track.UndoManager.EndAction();
            }
            game.Invalidate();
        }
        public override void OnMouseMoved(Vector2d pos)
        {
            if (Active)
            {
                _end = ScreenToGameCoords(pos);
                var diff = _end - _start;
                if (DrawingScenery || diff.Length >= MINIMUM_LINE)
                {
                    AddLine();
                    _start = _end;
                    Snapped = true;//we are now connected to the newest line
                }
                game.Invalidate();
            }

            _mouseshadow = ScreenToGameCoords(pos);
            base.OnMouseMoved(pos);
        }
        public override void OnMouseUp(Vector2d pos)
        {
            game.Invalidate();
            if (Active)
            {
                Active = false;
                var diff = _end - _start;
                if (!DrawingScenery && diff.Length < MINIMUM_LINE)
                    return;
                AddLine();
            }
            base.OnMouseUp(pos);
        }
        public override void Render()
        {
            base.Render();
            if (DrawingScenery && _mouseshadow != Vector2d.Zero)
            {
                GameRenderer.RenderRoundedLine(_mouseshadow, _mouseshadow, Color.FromArgb(100, 0x00, 0xCC, 0x00), 2f * game.Canvas.ColorControls.GreenMultiplier, false, false);
            }
        }
        public override void Stop()
        {
            Active = false;
            _mouseshadow = Vector2d.Zero;
        }
    }
}