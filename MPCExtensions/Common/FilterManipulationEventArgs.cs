// -----------------------------------------------------------------------
// <copyright file="Class1.cs" company="IBV Informatik AG">
//    Copyright (c) IBV Informatik AG All rights reserved.
// </copyright>
// 
//  
// created: 2016-07-22 @ 15:56
//  edited: 2016-07-22 @ 15:56
// -----------------------------------------------------------------------

#region Using

using Windows.Foundation;
using Windows.UI.Input;

#endregion

namespace MPCExtensions.Common
{
	internal class FilterManipulationEventArgs
	{
		internal FilterManipulationEventArgs(ManipulationUpdatedEventArgs args)
		{
			this.Delta = args.Delta;
			this.Pivot = args.Position;
		}

		public ManipulationDelta Delta { get; set; }

		public Point Pivot { get; set; }
	}
}