// -----------------------------------------------------------------------
// <copyright file="UpdateUiInfo.cs" company="IBV Informatik AG">
//    Copyright (c) IBV Informatik AG All rights reserved.
// </copyright>
// 
//  
// created: 2016-10-17 @ 10:03
//  edited: 2016-10-17 @ 10:05
// -----------------------------------------------------------------------

namespace MPCExtensions.Infrastructure
{
	/// <summary>
	/// Parameters of UpdateUi method
	/// </summary>
	public class UpdateUiInfo
	{
		public UpdateUiOperation Operation { get; set; } = UpdateUiOperation.Update;
		public double? Rotation { get; set; }
		public double? Height { get; set; }
		public bool IsRotationEnabled { get; set; }
		public bool IsRemotelyLocked { get; set; }
		public double? Width { get; set; }
		public double? X { get; set; }
		public double? Y { get; set; }
		public bool Animated { get; set; }

		public bool IsGroupFrameVisible { get; set; }
	}
}