// -----------------------------------------------------------------------
// <copyright file="AreaZoomChangedEventArgs.cs" company="IBV Informatik AG">
//    Copyright (c) IBV Informatik AG All rights reserved.
// </copyright>
// 
//  
// created: 2016-12-02 @ 11:18
//  edited: 2016-12-02 @ 11:19
// -----------------------------------------------------------------------

#region Using

using System;

#endregion

namespace MPCExtensions.Controls
{
	public class ZoomFactorChangedEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ZoomFactorChangedEventArgs" /> class.
		/// </summary>
		/// <param name="scale">The scale.</param>
		/// <param name="controlTileScale">The control tile scale.</param>
		public ZoomFactorChangedEventArgs(double scale, double controlTileScale)
		{
			this.Scale = scale;
			this.ControlTileScale = controlTileScale;
		}

		/// <summary>
		/// Gets or sets the offset y.
		/// </summary>
		/// <value>
		/// The offset y.
		/// </value>
		public double Scale { get; private set; }

		public double ControlTileScale { get; private set; }
	}
}