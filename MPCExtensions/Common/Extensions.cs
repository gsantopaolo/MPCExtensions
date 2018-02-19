// -----------------------------------------------------------------------
// <copyright file="Extensions.cs" company="IBV Informatik AG">
//    Copyright (c) IBV Informatik AG All rights reserved.
// </copyright>
// 
//  
// created: 2016-07-15 @ 19:40
//  edited: 2016-07-15 @ 19:40
// -----------------------------------------------------------------------

using Windows.UI;
using Windows.UI.Xaml;

namespace MPCExtensions.Common
{
	public static class Extensions
	{
		public static T GetAttachedPropertyValue<T>(this DependencyObject item, DependencyProperty property, T defaultValue)
		{
			var d = item.ReadLocalValue(property);
			string typeName = d.GetType().Name;
			if (typeName == "__ComObject") return defaultValue;
			return (T)item.GetValue(property);
		}
    }
}