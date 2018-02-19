// -----------------------------------------------------------------------
// <copyright file="TileGroupingInfoEventArgs.cs" company="IBV Informatik AG">
//    Copyright (c) IBV Informatik AG All rights reserved.
// </copyright>
// 
//  
// created: 2017-05-17 @ 18:32
//  edited: 2017-05-17 @ 18:32
// -----------------------------------------------------------------------

#region Using

using System;

#endregion

namespace MPCExtensions.Infrastructure
{
	public class TileGroupingInfoEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TileGroupingInfoEventArgs"/> class.
		/// </summary>
		/// <param name="deltaX">The delta x.</param>
		/// <param name="deltaY">The delta y.</param>
		/// <param name="deltaRotation">The delta rotation.</param>
		/// <param name="isActive">if set to <c>true</c> [is active].</param>
		public TileGroupingInfoEventArgs(double deltaX, double deltaY, double deltaRotation, bool isActive)
		{
			this.IsActive = isActive;
			this.DeltaRotation = deltaRotation;
			this.DeltaX = deltaX;
			this.DeltaY = deltaY;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TileGroupingInfoEventArgs"/> class.
		/// </summary>
		/// <param name="deltaX">The delta x.</param>
		/// <param name="deltaY">The delta y.</param>
		/// <param name="isQueryMode">if set to <c>true</c> [is query mode].</param>
		public TileGroupingInfoEventArgs(double deltaX, double deltaY, bool isQueryMode)
		{
			this.IsQueryMode = isQueryMode;
			this.DeltaX = deltaX;
			this.DeltaY = deltaY;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this tile is active.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
		/// </value>
		public bool IsActive { get;}

		public bool IsQueryMode { get; }

		/// <summary>
		/// Gets a value indicating whether group operation has been denied.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance is denied; otherwise, <c>false</c>.
		/// </value>
		public bool Success { get; set; }

		/// <summary>
		/// Gets or sets the delta x.
		/// </summary>
		/// <value>
		/// The delta x.
		/// </value>
		public double DeltaX { get;}

		/// <summary>
		/// Gets or sets the delta y.
		/// </summary>
		/// <value>
		/// The delta y.
		/// </value>
		public double DeltaY { get;}

		/// <summary>
		/// Gets or sets the delta rotation.
		/// </summary>
		/// <value>
		/// The delta rotation.
		/// </value>
		public double DeltaRotation { get;}
	}
}