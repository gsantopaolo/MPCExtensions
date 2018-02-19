// -----------------------------------------------------------------------
// <copyright file="AreaZoomEventArgs.cs" company="IBV Informatik AG">
//    Copyright (c) IBV Informatik AG All rights reserved.
// </copyright>
// 
//  
// created: 2016-12-01 @ 19:16
//  edited: 2016-12-01 @ 19:16
// -----------------------------------------------------------------------

#region Using

using System;

#endregion

namespace MPCExtensions.Controls
{
	public class AreaZoomEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AreaZoomEventArgs"/> class.
		/// </summary>
		/// <param name="x">The x.</param>
		/// <param name="y">The y.</param>
		public AreaZoomEventArgs(double x, double y)
		{
			this.OffsetX = x;
			this.OffsetY = y;
		}

		/// <summary>
		/// Gets or sets the offset x.
		/// </summary>
		/// <value>
		/// The offset x.
		/// </value>
		public double OffsetX { get; private set; }
		/// <summary>
		/// Gets or sets the offset y.
		/// </summary>
		/// <value>
		/// The offset y.
		/// </value>
		public double OffsetY { get; private set; }
	}
}