// -----------------------------------------------------------------------
// <copyright file="Library.cs" company="IBV Informatik AG">
//    Copyright (c) IBV Informatik AG All rights reserved.
// </copyright>
// 
//  
// created: 2016-08-01 @ 15:47
//  edited: 2016-08-01 @ 15:48
// -----------------------------------------------------------------------

#region Using

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

#endregion

namespace MPCExtensions.Controls
{
	/// <summary>
	/// Represents a container of othe items that support interaction
	/// </summary>
	/// <seealso cref="Windows.UI.Xaml.Controls.GridView" />
	public class Library : ScatterView
	{
		public Library()
		{
			this.DefaultStyleKey = typeof(Library);
			//this.IsLibrary = true;
			//this.DragOnHold = true;
		}

		/// <summary>
		/// Raises the <see cref="E:PointerPressed" /> event.
		/// </summary>
		/// <param name="e">The <see cref="PointerRoutedEventArgs"/> instance containing the event data.</param>
		protected override void OnPointerPressed(PointerRoutedEventArgs e)
		{
			//Note: We need to handle this event otherwise LibraryBar wont drag smoothly
			base.OnPointerPressed(e);
			e.Handled = true;
		}
	}
}