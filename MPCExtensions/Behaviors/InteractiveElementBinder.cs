// -----------------------------------------------------------------------
// <copyright file="InteractiveElementBinder.cs" company="IBV Informatik AG">
//    Copyright (c) IBV Informatik AG All rights reserved.
// </copyright>
// 
//  
// created: 2016-07-29 @ 11:43
//  edited: 2016-07-29 @ 11:43
// -----------------------------------------------------------------------

#region Using

using System;
using Windows.UI.Xaml;
using Microsoft.Xaml.Interactivity;
using MPCExtensions.Common;
using MPCExtensions.Controls;
using MPCExtensions.Infrastructure;

#endregion

namespace MPCExtensions.Behaviors
{
	/// <summary>
	/// Manages the UI position and size of a tile
	/// </summary>
	/// <seealso cref="Windows.UI.Xaml.DependencyObject" />
	/// <seealso cref="Microsoft.Xaml.Interactivity.IBehavior" />
	public class InteractiveElementBinder : DependencyObject, IBehavior
	{
		private FrameworkElement view;

		/// <summary>
		/// Attaches to the specified object.
		/// </summary>
		/// <param name="associatedObject">The <see cref="T:Windows.UI.Xaml.DependencyObject" /> to which the <seealso cref="T:Microsoft.Xaml.Interactivity.IBehavior" /> will be attached.</param>
		public void Attach(DependencyObject associatedObject)
		{
			this.view = (FrameworkElement)associatedObject;
			this.view.Loaded += this.OnViewLoaded;
		}

		/// <summary>
		/// Detaches this instance from its associated object.
		/// </summary>
		public void Detach()
		{
			this.view = null;
		}

		public DependencyObject AssociatedObject => this.view;

		private void OnViewLoaded(object sender, RoutedEventArgs e)
		{
			this.view.Loaded -= this.OnViewLoaded;
			ScatterViewItem host = VisualTreeHelperEx.FindParent<ScatterViewItem>(this.view);
			if (host == null)
				throw new ArgumentNullException("The view is not wrapped by a ScatterViewItem, is it hosted inside a ScatterView?");
			host.InteractiveElement = this.view.DataContext as IInteractiveElement;
		}
	}
}