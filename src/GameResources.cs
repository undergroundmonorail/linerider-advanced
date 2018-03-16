﻿//
//  GameResources.cs
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
using System.Drawing;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.Linq;
namespace linerider
{
    internal class GameResources
    {
        private static Assembly Assembly = null;
        private static Dictionary<string, object> _lookuptable = null;
        public static void Init()
        {
            if (Assembly == null)
            {
                Assembly = typeof(GameResources).Assembly;
            }
            if (_lookuptable == null)
            {
                _lookuptable = new Dictionary<string, object>();
            }
        }
        private static Bitmap GetBitmap(string name)
        {
            object lookup;
            if (_lookuptable.TryGetValue(name, out lookup))
            {
                return (Bitmap)lookup;
            }
            using (var stream = Assembly.GetManifestResourceStream("linerider.Resources." + name))
            {
                var ret = new Bitmap(stream);
                _lookuptable[name] = ret;
                return ret;
            }
        }
        private static byte[] GetBytes(string name)
        {
            object lookup;
            if (_lookuptable.TryGetValue(name, out lookup))
            {
                return ((byte[])lookup).ToArray();//prevent writing to resource
            }
            using (var stream = Assembly.GetManifestResourceStream("linerider.Resources." + name))
            {
                byte[] ret = new byte[stream.Length];
                stream.Read(ret, 0, ret.Length);
                _lookuptable[name] = ret;
                return ret;
            }
        }
        private static string GetString(string name)
        {
            object lookup;
            if (_lookuptable.TryGetValue(name, out lookup))
            {
                return (string)lookup;//strings are immutable so there's no chance of writing to resource
            }
            using (var stream = Assembly.GetManifestResourceStream("linerider.Resources." + name))
            {
                using (var reader = new StreamReader(stream))
                {
                    var ret = reader.ReadToEnd();
                    _lookuptable[name] = ret;
                    return ret;
                }
            }
        }
        internal static byte[] beep
        {
            get
            {
                return GetBytes("beep.wav");
            }
        }
        internal static System.Drawing.Bitmap DefaultSkin
        {
            get
            {
                return GetBitmap("DefaultSkin.png");
            }
        }
        internal static System.Drawing.Bitmap loading
        {
            get
            {
                return GetBitmap("loading.png");
            }
        }

        internal static byte[] icon
        {
            get
            {
                return GetBytes("icon.ico");
            }
        }
        internal static string DefaultColors
        {
            get
            {
                return GetString("DefaultColors.xml");
            }
        }
        internal static string gamefont_15_fnt
        {
            get
            {
                return GetString("gamefont_15.fnt");
            }
        }
        internal static System.Drawing.Bitmap gamefont_15_png
        {
            get
            {
                return GetBitmap("gamefont_15_0.png");
            }
        }
        #region rider
        internal static Bitmap sled_img
        {
            get
            {
                return GetBitmap("rider.sled.png");
            }
        }
        internal static Bitmap brokensled_img
        {
            get
            {
                return GetBitmap("rider.brokensled.png");
            }
        }
        internal static Bitmap arm_img
        {
            get
            {
                return GetBitmap("rider.arm.png");
            }
        }
        internal static Bitmap leg_img
        {
            get
            {
                return GetBitmap("rider.leg.png");
            }
        }
        internal static Bitmap body_img
        {
            get
            {
                return GetBitmap("rider.body.png");
            }
        }
        internal static Bitmap bodydead_img
        {
            get
            {
                return GetBitmap("rider.bodydead.png");
            }
        }
        #endregion
        #region cursors
        internal static System.Drawing.Bitmap cursor_move
        {
            get
            {
                return GetBitmap("cursors.move.png");
            }
        }
        internal static System.Drawing.Bitmap cursor_dragging
        {
            get
            {
                return GetBitmap("cursors.closed_move.png");
            }
        }

        internal static System.Drawing.Bitmap cursor_line
        {
            get
            {
                return GetBitmap("cursors.line.png");
            }
        }
        internal static System.Drawing.Bitmap cursor_eraser
        {
            get
            {
                return GetBitmap("cursors.eraser.png");
            }
        }
        internal static System.Drawing.Bitmap cursor_select
        {
            get
            {
                return GetBitmap("cursors.select.png");
            }
        }
        internal static System.Drawing.Bitmap cursor_pencil
        {
            get
            {
                return GetBitmap("cursors.pencil.png");
            }
        }
        internal static System.Drawing.Bitmap cursor_size_nesw
        {
            get
            {
                return GetBitmap("cursors.size_bdiag.png");
            }
        }
        internal static System.Drawing.Bitmap cursor_size_nwse
        {
            get
            {
                return GetBitmap("cursors.size_fdiag.png");
            }
        }
        internal static System.Drawing.Bitmap cursor_size_horz
        {
            get
            {
                return GetBitmap("cursors.size_hor.png");
            }
        }
        internal static System.Drawing.Bitmap cursor_size_vert
        {
            get
            {
                return GetBitmap("cursors.size_ver.png");
            }
        }
        internal static System.Drawing.Bitmap cursor_size_all
        {
            get
            {
                return GetBitmap("cursors.size_all.png");
            }
        }
        internal static System.Drawing.Bitmap cursor_zoom_in
        {
            get
            {
                return GetBitmap("cursors.zoom.png");
            }
        }
        internal static System.Drawing.Bitmap cursor_default
        {
            get
            {
                return GetBitmap("cursors.default.png");
            }
        }
        #endregion
        #region shaders
        internal static string simline_frag
        {
            get
            {
                return GetString("shaders.simline.frag");
            }
        }
        internal static string simline_vert
        {
            get
            {
                return GetString("shaders.simline.vert");
            }
        }
        internal static string rider_frag
        {
            get
            {
                return GetString("shaders.rider.frag");
            }
        }
        internal static string rider_vert
        {
            get
            {
                return GetString("shaders.rider.vert");
            }
        }
        #endregion
        #region Icons
        #region white
        internal static System.Drawing.Bitmap pencil_icon_white
        {
            get
            {
                return GetBitmap("icons.white.penciltool.png");
            }
        }
        internal static System.Drawing.Bitmap line_icon_white
        {
            get
            {
                return GetBitmap("icons.white.linetool.png");
            }
        }
        internal static System.Drawing.Bitmap eraser_icon_white
        {
            get
            {
                return GetBitmap("icons.white.erasertool.png");
            }
        }
        internal static System.Drawing.Bitmap movetool_icon_white
        {
            get
            {
                return GetBitmap("icons.white.movetool.png");
            }
        }
        internal static System.Drawing.Bitmap pantool_icon_white
        {
            get
            {
                return GetBitmap("icons.white.pantool.png");
            }
        }
        internal static System.Drawing.Bitmap menu_icon_white
        {
            get
            {
                return GetBitmap("icons.white.menu.png");
            }
        }
        internal static System.Drawing.Bitmap fast_forward_white
        {
            get
            {
                return GetBitmap("icons.white.fast-forward.png");
            }
        }
        internal static System.Drawing.Bitmap flag_icon_white
        {
            get
            {
                return GetBitmap("icons.white.flag.png");
            }
        }
        internal static System.Drawing.Bitmap pause_white
        {
            get
            {
                return GetBitmap("icons.white.pause.png");
            }
        }
        internal static System.Drawing.Bitmap play_icon_white
        {
            get
            {
                return GetBitmap("icons.white.play.png");
            }
        }
        internal static System.Drawing.Bitmap rewind_white
        {
            get
            {
                return GetBitmap("icons.white.rewind.png");
            }
        }
        internal static System.Drawing.Bitmap stop_icon_white
        {
            get
            {
                return GetBitmap("icons.white.stop.png");
            }
        }
        #endregion
        internal static System.Drawing.Bitmap pencil_icon
        {
            get
            {
                return GetBitmap("icons.penciltool.png");
            }
        }
        internal static System.Drawing.Bitmap line_icon
        {
            get
            {
                return GetBitmap("icons.linetool.png");
            }
        }
        internal static System.Drawing.Bitmap eraser_icon
        {
            get
            {
                return GetBitmap("icons.erasertool.png");
            }
        }
        internal static System.Drawing.Bitmap movetool_icon
        {
            get
            {
                return GetBitmap("icons.movetool.png");
            }
        }
        internal static System.Drawing.Bitmap pantool_icon
        {
            get
            {
                return GetBitmap("icons.pantool.png");
            }
        }
        internal static System.Drawing.Bitmap menu_icon
        {
            get
            {
                return GetBitmap("icons.menu.png");
            }
        }
        internal static System.Drawing.Bitmap flag_icon
        {
            get
            {
                return GetBitmap("icons.flag.png");
            }
        }
        internal static System.Drawing.Bitmap flag_invalid_icon
        {
            get
            {
                return GetBitmap("icons.flag_invalid.png");
            }
        }
        internal static System.Drawing.Bitmap fast_forward
        {
            get
            {
                return GetBitmap("icons.fast-forward.png");
            }
        }
        internal static System.Drawing.Bitmap rewind
        {
            get
            {
                return GetBitmap("icons.rewind.png");
            }
        }
        internal static System.Drawing.Bitmap play_icon
        {
            get
            {
                return GetBitmap("icons.play.png");
            }
        }
        internal static System.Drawing.Bitmap stop_icon
        {
            get
            {
                return GetBitmap("icons.stop.png");
            }
        }
        internal static System.Drawing.Bitmap pause
        {
            get
            {
                return GetBitmap("icons.pause.png");
            }
        }
        #endregion
    }
}
