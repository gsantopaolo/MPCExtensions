// -----------------------------------------------------------------------
// <copyright file="InkToTextBox.cs" company="IBV Informatik AG">
//    Copyright (c) IBV Informatik AG All rights reserved.
// </copyright>
// 
//  
// created: 2016-07-18 @ 14:55
//  edited: 2017-01-11 @ 11:05
// -----------------------------------------------------------------------

#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

#endregion

namespace MPCExtensions.Controls
{
	[TemplatePart(Name = PART_INKER_NAME, Type = typeof(InkCanvas))]
	[TemplatePart(Name = PART_CONTENT_NAME, Type = typeof(ContentPresenter))]
	[ContentProperty(Name = "Content")]
	public class InkToTextBox : Control
	{
		private const string PART_INKER_NAME = "PART_INKER";
		private const string PART_CONTENT_NAME = "PART_CONTENT";
		private const string TEXT_PROPERTY_NAME = "Text";

		public static readonly DependencyProperty TargetTextControlProperty =
			DependencyProperty.Register(nameof(InkToTextBox.TargetTextControl), typeof(Control), typeof(InkToTextBox), new PropertyMetadata(null));

		public static readonly DependencyProperty PenColorProperty =
			DependencyProperty.Register(nameof(InkToTextBox.PenColor), typeof(Color), typeof(InkToTextBox), new PropertyMetadata(Colors.Black));

		public static readonly DependencyProperty PenTipProperty =
			DependencyProperty.Register(nameof(InkToTextBox.PenTip), typeof(PenTipShape), typeof(InkToTextBox), new PropertyMetadata(PenTipShape.Circle));

		public static readonly DependencyProperty PenSizeProperty =
			DependencyProperty.Register(nameof(InkToTextBox.PenSize), typeof(Size), typeof(InkToTextBox), new PropertyMetadata(new Size(3, 3)));

		public static readonly DependencyProperty ContentProperty =
			DependencyProperty.Register(nameof(InkToTextBox.Content), typeof(object), typeof(InkToTextBox),
				new PropertyMetadata(null));

		private readonly DispatcherTimer timer;

		private ContentPresenter contentPresenter;
		private InkCanvas inker;
		private UIElement root;

		/// <summary>
		/// Initializes a new instance of the <see cref="InkToTextBox"/> class.
		/// </summary>
		public InkToTextBox()
		{
			this.DefaultStyleKey = typeof(InkToTextBox);
			this.timer = new DispatcherTimer();
			this.timer.Tick += this.OnTimer;
			this.timer.Interval = TimeSpan.FromSeconds(3);
		}

		/// <summary>
		/// Gets or sets the content.
		/// </summary>
		/// <value>
		/// The content.
		/// </value>
		public object Content
		{
			get { return (object)this.GetValue(ContentProperty); }
			set { this.SetValue(ContentProperty, value); }
		}

		/// <summary>
		/// Gets or sets the color of the pen.
		/// </summary>
		/// <value>
		/// The color of the pen.
		/// </value>
		public Color PenColor
		{
			get { return (Color)this.GetValue(PenColorProperty); }
			set { this.SetValue(PenColorProperty, value); }
		}

		/// <summary>
		/// Gets or sets the size of the pen.
		/// </summary>
		/// <value>
		/// The size of the pen.
		/// </value>
		public Size PenSize
		{
			get { return (Size)this.GetValue(PenSizeProperty); }
			set { this.SetValue(TargetTextControlProperty, value); }
		}

		/// <summary>
		/// Gets or sets the pen tip.
		/// </summary>
		/// <value>
		/// The pen tip.
		/// </value>
		public PenTipShape PenTip
		{
			get { return (PenTipShape)this.GetValue(PenTipProperty); }
			set { this.SetValue(PenTipProperty, value); }
		}

		/// <summary>
		/// Gets or sets the target text control.
		/// </summary>
		/// <value>
		/// The target text control.
		/// </value>
		/// <exception cref="ArgumentException">Control provided does not expose a class of type string with name Text</exception>
		public Control TargetTextControl
		{
			get { return (Control)this.GetValue(TargetTextControlProperty); }
			set
			{
				if (value != null)
				{
					var type = value.GetType();
					// Get the PropertyInfo object by passing the property name.
					PropertyInfo pInfo = type.GetProperty(TEXT_PROPERTY_NAME);

					if (pInfo == null)
						throw new ArgumentException("Control provided does not expose a class of type string with name Text");
				}
				this.SetValue(TargetTextControlProperty, value);
			}
		}

		/// <summary>
		/// Invoked whenever application code or internal processes (such as a rebuilding layout pass) call ApplyTemplate. In simplest terms, this means the method is called just before a UI element displays in your app. Override this method to influence the default post-template logic of a class.
		/// </summary>
		protected override void OnApplyTemplate()
		{
			try
			{
				this.inker = this.GetTemplateChild(PART_INKER_NAME) as InkCanvas;
				this.contentPresenter = this.GetTemplateChild(PART_CONTENT_NAME) as ContentPresenter;
				if (this.contentPresenter != null) this.contentPresenter.Content = this.Content;
				if (this.inker != null)
				{
					this.InitializeInker();
				}
			}
			catch
			{
			}
		}


		/// <summary>
		/// Initializes the inker.
		/// </summary>
		private void InitializeInker()
		{
			try
			{
				this.inker.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Pen;// | Windows.UI.Core.CoreInputDeviceTypes.Mouse;

				var drawingAttributes = new InkDrawingAttributes();

				drawingAttributes.DrawAsHighlighter = false;
				drawingAttributes.IgnorePressure = false;

				drawingAttributes.Color = this.PenColor;
				drawingAttributes.PenTip = this.PenTip;
				drawingAttributes.Size = this.PenSize;

				this.inker.InkPresenter.UpdateDefaultDrawingAttributes(drawingAttributes);
				this.inker.InkPresenter.StrokesCollected += this.InkPresenter_StrokesCollected;
			}
			catch
			{
			}
		}

		private void InkPresenter_StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
		{
			this.timer.Start();
		}

		/// <summary>
		/// Recognizes the inker text.
		/// </summary>
		/// <returns></returns>
		private async Task RecognizeInkerText()
		{
			try
			{
				InkRecognizerContainer inkRecognizer = new InkRecognizerContainer();
				IReadOnlyList<InkRecognitionResult> recognitionResults = await inkRecognizer.RecognizeAsync(this.inker.InkPresenter.StrokeContainer, InkRecognitionTarget.All);

				string value = string.Empty;
				
				foreach (var result in recognitionResults)
				{

					Point p = new Point(result.BoundingRect.X, result.BoundingRect.Y);
					Size s = new Size(result.BoundingRect.Width, result.BoundingRect.Height);
					Rect r = new Rect(p, s);

					GeneralTransform gt = this.TransformToVisual(Window.Current.Content);
					var r2 = gt.TransformBounds(r);
					var elements = VisualTreeHelper.FindElementsInHostCoordinates(r2, (UIElement)this.Content, true);
					TextBox box = elements.FirstOrDefault(el => el is TextBox && (el as TextBox).IsEnabled) as TextBox;
					if (box != null)
					{
					    box.Text += result.GetTextCandidates().FirstOrDefault().Trim() + " ";
					}
				}

				this.inker.InkPresenter.StrokeContainer.Clear();
			}
			catch
			{
			}
		}

		/// <summary>
		/// Called when timer.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The e.</param>
		private async void OnTimer(object sender, object e)
		{
			this.timer.Stop();
			await this.RecognizeInkerText();
		}
	}
}