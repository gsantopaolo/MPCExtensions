// -----------------------------------------------------------------------
// <copyright file="UiUtility.cs" company="IBV Informatik AG">
//    Copyright (c) IBV Informatik AG All rights reserved.
// </copyright>
// 
//  
// created: 2017-07-25 @ 14:48
//  edited: 2017-08-08 @ 14:00
// -----------------------------------------------------------------------

#region Using

using System;
using Windows.UI;
using MPCExtensions.Infrastructure;
using MPCExtensions.Infrastructure.Diagram;

#endregion

namespace MPCExtensions.Helpers
{
    internal static class UiUtility
    {
        /// <summary>
        /// Converts an hex to a color.
        /// </summary>
        /// <param name="hex">The hexadecimal.</param>
        /// <returns></returns>
        public static Color GetColor(string hex)
        {
            if (string.IsNullOrEmpty(hex)) return Colors.Black;
            try
            {
                hex = hex.Replace("#", string.Empty);
                byte r = Convert.ToByte(hex.Substring(0, 2), 16);
                byte g = Convert.ToByte(hex.Substring(2, 2), 16);
                byte b = Convert.ToByte(hex.Substring(4, 2), 16);
                return Color.FromArgb(0xFF, r, g, b);
            }
            catch
            {
                return Colors.Blue;
            }
        }

        /// <summary>
        /// Maps a HitTestInfo to a ConnectorOrientation
        /// </summary>
        /// <param name="hitTestInfo">The hit test information.</param>
        /// <returns>Mapped value</returns>
        public static ConnectorOrientation ToConnectorOrientation(this HitTestInfo hitTestInfo)
        {
            switch (hitTestInfo)
            {
                case HitTestInfo.AnchorLeft:
                    return ConnectorOrientation.Left;
                case HitTestInfo.AnchorTop:
                    return ConnectorOrientation.Top;
                case HitTestInfo.AnchorRight:
                    return ConnectorOrientation.Right;
                case HitTestInfo.AnchorBottom:
                    return ConnectorOrientation.Bottom;
                default:
                    return ConnectorOrientation.None;
            }
        }
    }
}