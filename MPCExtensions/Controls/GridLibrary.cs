// -----------------------------------------------------------------------
// <copyright file="GridLibrary.cs" company="IBV Informatik AG">
//    Copyright (c) IBV Informatik AG All rights reserved.
// </copyright>
// 
//  
// created: 2016-10-12 @ 14:15
//  edited: 2016-10-12 @ 14:15
// -----------------------------------------------------------------------

using Windows.UI.Xaml.Controls;

namespace MPCExtensions.Controls
{
	public class GridLibrary : GridView
	{
		public GridLibrary()
		{
			this.CanReorderItems = true;
		}
	}
}