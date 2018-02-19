// -----------------------------------------------------------------------
// <copyright file="PinchToZoomRecognizer.cs" company="IBV Informatik AG">
//    Copyright (c) IBV Informatik AG All rights reserved.
// </copyright>
// 
//  
// created: 2016-12-02 @ 12:05
//  edited: 2016-12-02 @ 13:40
// -----------------------------------------------------------------------

#region Using

using System;
using System.Diagnostics;
using Windows.Devices.Input;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

#endregion

namespace MPCExtensions.Infrastructure
{
	public class PinchToZoomRecognizer
	{
		private const double AreaZoomConversionFactor = 1000;

		private readonly ScrollViewer host;
		private readonly string partName;
		private readonly Grid scatterView;

		private GestureRecognizer gestureRecognizer;
		private bool isZoomMode;

		/// <summary>
		/// Initializes a new instance of the <see cref="PinchToZoomRecognizer" /> class.
		/// </summary>
		/// <param name="host">The scrollviewer hosting the scatterview (can be null)</param>
		/// <param name="scatterView">The scatter view.</param>
		public PinchToZoomRecognizer(ScrollViewer host, Grid scatterView)
		{
			this.scatterView = scatterView;
			this.partName = scatterView.Name;
			this.host = host;
			this.scatterView.Unloaded += this.OnScatterViewUnloaded;
			if (this.host != null)
			{
				this.scatterView.Tapped += this.OnTriggerZoomMode;
			}
			else
			{
				this.InitGesture(true);
			}
		}

		/// <summary>
		/// Gets or sets the area zoom factor.
		/// </summary>
		/// <value>
		/// The area zoom factor.
		/// </value>
		public double AreaZoomPercent { get; private set; } = 100;

		/// <summary>
		/// Gets or sets a value indicating whether target area can zoom.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance can zoom; otherwise, <c>false</c>.
		/// </value>
		public bool CanZoom { get; set; }

		/// <summary>
		/// Gets a value indicating whether this instance is zoom mode.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is zoom mode; otherwise, <c>false</c>.
		/// </value>
		public bool IsZoomMode { get; private set; }

		public event EventHandler AreaZoomStateChanged;

		public event EventHandler AreaZoomUpdated;

		/// <summary>
		/// Exits the zoom mode.
		/// </summary>
		public void ExitZoomMode()
		{
			if (!this.isZoomMode) return;
			this.isZoomMode = false;
			this.host.VerticalScrollMode = ScrollMode.Auto;
			this.host.HorizontalScrollMode = ScrollMode.Auto;


			this.scatterView.PointerPressed -= this.OnPointerPressed;
			this.scatterView.PointerReleased -= this.OnPointerReleased;
			this.scatterView.PointerMoved -= this.OnPointerMoved;
			this.scatterView.PointerCaptureLost -= this.OnPointerCaptureLost;
			if (this.gestureRecognizer != null)
			{
				this.gestureRecognizer.ManipulationUpdated -= this.OnManipulationUpdated;
				this.gestureRecognizer.ManipulationCompleted -= this.OnManipulationCompleted;
				this.gestureRecognizer = null;
			}
		}

		/// <summary>
		/// Called when area zoom state changes.
		/// </summary>
		protected virtual void OnAreaZoomStateChanged()
		{
			this.AreaZoomStateChanged?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// Called when area zoom updates.
		/// </summary>
		protected virtual void OnAreaZoomUpdated()
		{
			this.AreaZoomUpdated?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// Initializes the gesture.
		/// </summary>
		/// <param name="isZoomMode">if set to <c>true</c> indicated that we are in zoom mode.</param>
		private void InitGesture(bool isZoomMode)
		{
			if (isZoomMode)
			{
				this.gestureRecognizer = new GestureRecognizer
				{
					GestureSettings = GestureSettings.ManipulationScale | GestureSettings.ManipulationScaleInertia
				};

				this.scatterView.PointerPressed += this.OnPointerPressed;
				this.scatterView.PointerReleased += this.OnPointerReleased;
				this.scatterView.PointerMoved += this.OnPointerMoved;
				this.scatterView.PointerCaptureLost += this.OnPointerCaptureLost;

				this.gestureRecognizer.ManipulationUpdated += this.OnManipulationUpdated;
				this.gestureRecognizer.ManipulationCompleted += this.OnManipulationCompleted;
			}
			else
			{
				this.scatterView.PointerPressed -= this.OnPointerPressed;
				this.scatterView.PointerReleased -= this.OnPointerReleased;
				this.scatterView.PointerMoved -= this.OnPointerMoved;
				this.scatterView.PointerCaptureLost -= this.OnPointerCaptureLost;

				this.gestureRecognizer.ManipulationUpdated -= this.OnManipulationUpdated;
				this.gestureRecognizer.ManipulationCompleted -= this.OnManipulationCompleted;
				this.gestureRecognizer = null;
			}

			this.IsZoomMode = isZoomMode;
			if (this.host != null) this.OnAreaZoomStateChanged();
		}

		/// <summary>
		/// Called when manipulation completes.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="ManipulationCompletedEventArgs"/> instance containing the event data.</param>
		private void OnManipulationCompleted(GestureRecognizer sender, ManipulationCompletedEventArgs e)
		{
			if (e.Cumulative.Expansion != 0)
			{
				this.AreaZoomPercent = e.Cumulative.Expansion;
				this.OnAreaZoomUpdated();
			}
		}

		/// <summary>
		/// Called when manipulation gets updates.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="ManipulationUpdatedEventArgs"/> instance containing the event data.</param>
		private void OnManipulationUpdated(object sender, ManipulationUpdatedEventArgs e)
		{
			if (e.Cumulative.Expansion != 0)
			{
				this.AreaZoomPercent = e.Cumulative.Expansion;
				this.OnAreaZoomUpdated();
			}
		}

		/// <summary>
		/// Called when pointer capture get lost.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="args">The <see cref="Windows.UI.Xaml.Input.PointerRoutedEventArgs" /> instance containing the event data.</param>
		private void OnPointerCaptureLost(object sender, PointerRoutedEventArgs args)
		{
			try
			{
				this.gestureRecognizer.CompleteGesture();
			}
			finally
			{
				args.Handled = true;
			}
		}

		/// <summary>
		/// Called when pointer moves.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="args">The <see cref="Windows.UI.Xaml.Input.PointerRoutedEventArgs" /> instance containing the event data.</param>
		private void OnPointerMoved(object sender, PointerRoutedEventArgs args)
		{
			try
			{
				this.gestureRecognizer.ProcessMoveEvents(args.GetIntermediatePoints(this.scatterView));
			}
			catch (Exception)
			{
				//see https://connect.microsoft.com/VisualStudio/feedback/details/895979/exceptions-thrown-by-gesturerecognizer
			}

			args.Handled = true;
		}

		/// <summary>
		/// Called when pointer is pressed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="args">The <see cref="Windows.UI.Xaml.Input.PointerRoutedEventArgs"/> instance containing the event data.</param>
		private void OnPointerPressed(object sender, PointerRoutedEventArgs args)
		{
			if (!args.Pointer.IsInContact) return;
			this.gestureRecognizer.ProcessDownEvent(args.GetCurrentPoint(this.scatterView));
			this.scatterView.CapturePointer(args.Pointer);
			args.Handled = true;
		}

		/// <summary>
		/// Called when [pointer released].
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="args">The <see cref="PointerRoutedEventArgs"/> instance containing the event data.</param>
		private void OnPointerReleased(object sender, PointerRoutedEventArgs args)
		{
			try
			{
				this.gestureRecognizer.ProcessUpEvent(args.GetCurrentPoint(this.scatterView));
				this.scatterView.ReleasePointerCapture(args.Pointer);
			}
			finally
			{
				args.Handled = true;
				this.scatterView.ManipulationMode = ManipulationModes.System;
			}
		}

		/// <summary>
		/// Called when scatter view unloads.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		private void OnScatterViewUnloaded(object sender, RoutedEventArgs e)
		{
			this.scatterView.Unloaded -= this.OnScatterViewUnloaded;
			if (this.host != null) this.scatterView.Tapped -= this.OnTriggerZoomMode;
			this.InitGesture(false);
		}

		/// <summary>
		/// Called when user enter/leaves zoom mode.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="TappedRoutedEventArgs"/> instance containing the event data.</param>
		private void OnTriggerZoomMode(object sender, TappedRoutedEventArgs e)
		{
			if (!this.CanZoom) return;

			Grid originElement = e.OriginalSource as Grid;
			bool isValid = originElement != null && originElement.Name == this.partName;
			if (e.PointerDeviceType == PointerDeviceType.Touch && isValid)
			{
				this.isZoomMode = !this.isZoomMode;

				this.host.VerticalScrollMode = this.isZoomMode ? ScrollMode.Disabled : ScrollMode.Auto;
				this.host.HorizontalScrollMode = this.isZoomMode ? ScrollMode.Disabled : ScrollMode.Auto;
				this.InitGesture(this.isZoomMode);
			}
		}
	}
}