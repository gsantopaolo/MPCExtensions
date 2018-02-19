// -----------------------------------------------------------------------
// <copyright file="RectHelper.cs" company="IBV Informatik AG">
//    Copyright (c) IBV Informatik AG All rights reserved.
// </copyright>
// 
//  
// created: 2017-06-08 @ 15:49
//  edited: 2017-06-08 @ 18:04
// -----------------------------------------------------------------------

#region Using

using System.Numerics;
using Windows.Foundation;

#endregion

namespace CollaBoard.Infrastructure.Helpers
{
    /// <summary>
    /// Helper for missing Rect methods
    /// </summary>
    public static class RectHelper
    {
        /// <summary>
        /// Gets rectangle bottom left point
        /// </summary>
        /// <param name="rect">The rect.</param>
        /// <returns></returns>
        public static Point BottomLeft(this Rect rect) => new Point(rect.Left, rect.Bottom);

        /// <summary>
        ///Gets rectangle top right point
        /// </summary>
        /// <param name="rect">The rect.</param>
        /// <returns></returns>
        public static Point BottomRight(this Rect rect) => new Point(rect.Right, rect.Bottom);


        /// <summary>
        /// Inflates the specified rectangle.
        /// </summary>
        /// <param name="rect">The rect.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>Inflated rectangle</returns>
        public static Rect Inflate(this Rect rect, double width, double height) => new Rect(rect.Left - width, rect.Y - height, rect.Width + (2 * width), rect.Height + (2 * height));

        /// <summary>
        /// Return true when rectangle intersect others
        /// </summary>
        /// <param name="rect1">The rect1.</param>
        /// <param name="rect2">The rect2.</param>
        /// <returns></returns>
        public static bool IntersectsWith(this Rect rect1, Rect rect2)
        {
            //see https://github.com/mono/mono/blob/master/mcs/class/System.Drawing/System.Drawing/Rectangle.cs
            return !((rect1.Left > rect2.Right) || (rect1.Right < rect2.Left) ||
                     (rect1.Top > rect2.Bottom) || (rect1.Bottom < rect2.Top));
        }

        /// <summary>
        /// Subtracts a point from another.
        /// </summary>
        /// <param name="p1">The p1 point.</param>
        /// <param name="p2">The p2 poin.</param>
        /// <returns>Pint subtracts reult</returns>
        public static Vector2 Subtract(Point p1, Point p2) => new Vector2((float) (p1.X - p2.X), (float) (p1.Y - p2.Y));

        /// <summary>
        /// Gets rectangle top left point
        /// </summary>
        /// <param name="rect">The rect.</param>
        /// <returns></returns>
        public static Point TopLeft(this Rect rect) => new Point(rect.Left, rect.Top);

        /// <summary>
        /// Gets rectangle top right point
        /// </summary>
        /// <param name="rect">The rect.</param>
        /// <returns></returns>
        public static Point TopRight(this Rect rect) => new Point(rect.Right, rect.Top);
    }
}