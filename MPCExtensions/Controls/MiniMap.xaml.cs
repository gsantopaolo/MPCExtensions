// -----------------------------------------------------------------------
// <copyright file="MiniMap.xaml.cs" company="IBV Informatik AG">
//    Copyright (c) IBV Informatik AG All rights reserved.
// </copyright>
// 
//  
// created: 2016-09-05 @ 17:57
//  edited: 2016-12-01 @ 14:14
// -----------------------------------------------------------------------

#region Using

using System;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

#endregion

namespace MPCExtensions.Controls
{
    public partial class MiniMap : UserControl
    {
        /// <summary>
        /// ScrollingHost Dependency Property
        /// </summary>
        public static readonly DependencyProperty ScrollingHostProperty =
            DependencyProperty.Register("ScrollingHost", typeof(ScrollViewer), typeof(MiniMap),
                new PropertyMetadata(null,
                    MiniMap.OnScrollingHostChanged));

        /// <summary>
        /// ScatterArea Dependency Property
        /// </summary>
        public static readonly DependencyProperty ScatterAreaProperty =
            DependencyProperty.Register("ScatterArea", typeof(ScatterView), typeof(MiniMap),
                new PropertyMetadata(null,
                    MiniMap.OnScatterAreaChanged));

        /// <summary>
        /// ThumbBorderBrush Dependency Property
        /// </summary>
        public static readonly DependencyProperty ThumbBorderBrushProperty =
            DependencyProperty.Register("ThumbBorderBrush", typeof(Brush), typeof(MiniMap),
                new PropertyMetadata(new SolidColorBrush(Colors.GreenYellow),
                    MiniMap.OnThumbBorderBrushChanged));

        /// <summary>
        /// ThumbBorderSize Dependency Property
        /// </summary>
        public static readonly DependencyProperty ThumbBorderSizeProperty =
            DependencyProperty.Register("ThumbBorderSize", typeof(double), typeof(MiniMap),
                new PropertyMetadata(1D,
                    MiniMap.OnThumbBorderSizeChanged));

        /// <summary>
        /// ThumbBackground Dependency Property
        /// </summary>
        public static readonly DependencyProperty ThumbBackgroundProperty =
            DependencyProperty.Register("ThumbBackground", typeof(Brush), typeof(MiniMap),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(100, 0xFF, 0, 0)),
                    MiniMap.OnThumbBackgroundChanged));

        /// <summary>
        /// ItemsSource Dependency Property
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(object), typeof(MiniMap),
                new PropertyMetadata((object)null,
                    MiniMap.OnItemsSourceChanged));

        /// <summary>
        /// InactiveOpacity Dependency Property
        /// </summary>
        public static readonly DependencyProperty InactiveOpacityProperty =
            DependencyProperty.Register("InactiveOpacity", typeof(double), typeof(MiniMap),
                new PropertyMetadata(0.05));

        /// <summary>
        /// ActiveOpacity Dependency Property
        /// </summary>
        public static readonly DependencyProperty ActiveOpacityProperty =
            DependencyProperty.Register("ActiveOpacity", typeof(double), typeof(MiniMap),
                new PropertyMetadata(0.3));

        /// <summary>
        /// IsVisible Dependency Property
        /// </summary>
        public static readonly DependencyProperty IsVisibleProperty =
            DependencyProperty.Register("IsVisible", typeof(bool), typeof(MiniMap),
                new PropertyMetadata(true,
                    MiniMap.OnIsVisibleChanged));

        /// <summary>
        /// AllowItemsRotation Dependency Property
        /// </summary>
        public static readonly DependencyProperty AllowItemsRotationProperty =
            DependencyProperty.Register("AllowItemsRotation", typeof(bool), typeof(MiniMap),
                new PropertyMetadata((bool)false,
                    MiniMap.OnAllowItemsRotationChanged));

        /// <summary>
        /// SelectedItemIndex Dependency Property
        /// </summary>
        public static readonly DependencyProperty SelectedItemIndexProperty =
            DependencyProperty.Register("SelectedItemIndex", typeof(int), typeof(MiniMap),
                new PropertyMetadata(-1, MiniMap.OnSelectedItemIndexChanged));

        /// <summary>
        /// IsPinchToZoomMode Dependency Property
        /// </summary>
        public static readonly DependencyProperty IsPinchToZoomModeProperty =
            DependencyProperty.Register("IsPinchToZoomMode", typeof(bool), typeof(MiniMap),
                new PropertyMetadata(false,
                    new PropertyChangedCallback(MiniMap.OnIsPinchToZoomModeChanged)));

        #region ItemTemplateSelector

        /// <summary>
        /// ItemTemplateSelector Dependency Property
        /// </summary>
        public static readonly DependencyProperty ItemTemplateSelectorProperty =
             DependencyProperty.Register("ItemTemplateSelector", typeof(DataTemplateSelector), typeof(MiniMap),
                  new PropertyMetadata(null,
                        new PropertyChangedCallback(OnItemTemplateSelectorChanged)));

        /// <summary>
        /// Gets or sets the ItemTemplateSelector property. This dependency property 
        /// indicates ....
        /// </summary>
        public DataTemplateSelector ItemTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(ItemTemplateSelectorProperty); }
            set { SetValue(ItemTemplateSelectorProperty, value); }
        }

        /// <summary>
        /// Handles changes to the ItemTemplateSelector property.
        /// </summary>
        /// <param name="d">Source dependendency o bject</param>
        /// <param name="e">Event argument</param>
        private static void OnItemTemplateSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MiniMap target = (MiniMap)d;
            DataTemplateSelector oldValue = (DataTemplateSelector)e.OldValue;
            DataTemplateSelector newValue = target.ItemTemplateSelector;
            target.OnItemTemplateSelectorChanged(oldValue, newValue);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the ItemTemplateSelector property.
        /// </summary>
        /// <param name="oldValue">Parameter old value</param>
        /// <param name="newValue">Paramenter new value </param>]        
        protected virtual void OnItemTemplateSelectorChanged(DataTemplateSelector oldValue, DataTemplateSelector newValue)
        {
            if (this.ScatterHost != null) this.ScatterHost.ItemTemplateSelector = newValue;
        }

        #endregion

        #region IsFreezed

        /// <summary>
        /// IsFreezed Dependency Property
        /// </summary>
        public static readonly DependencyProperty IsFreezedProperty =
            DependencyProperty.Register("IsFreezed", typeof(bool), typeof(MiniMap),
                new PropertyMetadata((bool)false));

        /// <summary>
        /// Gets or sets the IsFreezed property. This dependency property 
        /// indicates ....
        /// </summary>
        public bool IsFreezed
        {
            get { return (bool)GetValue(IsFreezedProperty); }
            set { SetValue(IsFreezedProperty, value); }
        }

        #endregion


        private readonly GestureRecognizer gestureRecognizer;
        private double areaHeight;
        private double areaWidth;
        private bool ignoreOutsizeEvent;
        private double ratioX;
        private double ratioY;
        private CompositeTransform transform;

        /// <summary>
        /// Initializes a new instance of the <see cref="MiniMap"/> class.
        /// </summary>
        public MiniMap()
        {
            this.InitializeComponent();

            this.Loaded += this.OnLoaded;
            this.Unloaded += this.OnUnloaded;

            this.gestureRecognizer = new GestureRecognizer
            {
                GestureSettings = GestureSettings.ManipulationTranslateX |
                                        GestureSettings.ManipulationTranslateY
            };

            this.Clip = new RectangleGeometry();
            this.SizeChanged += (s, e) =>
            {
                this.Clip.Rect = new Rect(0, 0, e.NewSize.Width, e.NewSize.Height);
            };
        }

        /// <summary>
        /// Gets or sets the ActiveOpacity property. This dependency property 
        /// indicates ....
        /// </summary>
        public double ActiveOpacity
        {
            get { return (double)this.GetValue(ActiveOpacityProperty); }
            set { this.SetValue(ActiveOpacityProperty, value); }
        }

        /// <summary>
        /// Gets or sets the AllowItemsRotation property. This dependency property 
        /// indicates ....
        /// </summary>
        public bool AllowItemsRotation
        {
            get { return (bool)this.GetValue(AllowItemsRotationProperty); }
            set { this.SetValue(AllowItemsRotationProperty, value); }
        }

        public double AreaZoomStepHeigth { get; set; }

        public double AreaZoomStepWidth { get; set; }

        /// <summary>
        /// Gets or sets the InactiveOpacity property. This dependency property 
        /// indicates ....
        /// </summary>
        public double InactiveOpacity
        {
            get { return (double)this.GetValue(InactiveOpacityProperty); }
            set { this.SetValue(InactiveOpacityProperty, value); }
        }

        /// <summary>
        /// Gets or sets the IsPinchToZoomMode property. This dependency property 
        /// indicates ....
        /// </summary>
        public bool IsPinchToZoomMode
        {
            get { return (bool)this.GetValue(IsPinchToZoomModeProperty); }
            set { this.SetValue(IsPinchToZoomModeProperty, value); }
        }

        /// <summary>
        /// Gets or sets the IsVisible property. This dependency property 
        /// indicates ....
        /// </summary>
        public bool IsVisible
        {
            get { return (bool)this.GetValue(IsVisibleProperty); }
            set { this.SetValue(IsVisibleProperty, value); }
        }


        /// <summary>
        /// Gets or sets the ItemsSource property. This dependency property 
        /// indicates ....
        /// </summary>
        public object ItemsSource
        {
            get { return (object)this.GetValue(ItemsSourceProperty); }
            set { this.SetValue(ItemsSourceProperty, value); }
        }

        /// <summary>
        /// Gets or sets the ScatterArea property. This dependency property 
        /// indicates ....
        /// </summary>
        public ScatterView ScatterArea
        {
            get { return (ScatterView)this.GetValue(ScatterAreaProperty); }
            set { this.SetValue(ScatterAreaProperty, value); }
        }

        /// <summary>
        /// Gets or sets the ScrollingHost property. This dependency property 
        /// indicates ....
        /// </summary>
        public ScrollViewer ScrollingHost
        {
            get { return (ScrollViewer)this.GetValue(ScrollingHostProperty); }
            set { this.SetValue(ScrollingHostProperty, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedItemIndex property. This dependency property 
        /// indicates ....
        /// </summary>
        public int SelectedItemIndex
        {
            get { return (int)this.GetValue(SelectedItemIndexProperty); }
            set { this.SetValue(SelectedItemIndexProperty, value); }
        }

        /// <summary>
        /// Gets or sets the ThumbBackground property. This dependency property 
        /// indicates ....
        /// </summary>
        public Brush ThumbBackground
        {
            get { return (Brush)this.GetValue(ThumbBackgroundProperty); }
            set { this.SetValue(ThumbBackgroundProperty, value); }
        }

        /// <summary>
        /// Gets or sets the ThumbBorderBrush property. This dependency property 
        /// indicates ....
        /// </summary>
        public Brush ThumbBorderBrush
        {
            get { return (Brush)this.GetValue(ThumbBorderBrushProperty); }
            set { this.SetValue(ThumbBorderBrushProperty, value); }
        }

        /// <summary>
        /// Gets or sets the ThumbBorderSize property. This dependency property 
        /// indicates ....
        /// </summary>
        public double ThumbBorderSize
        {
            get { return (double)this.GetValue(ThumbBorderSizeProperty); }
            set { this.SetValue(ThumbBorderSizeProperty, value); }
        }

        /// <summary>
        /// Resets the scatterview to initial position.
        /// </summary>
        public void Reset()
        {
            this.MoveThumb(0, 0);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the AllowItemsRotation property.
        /// </summary>
        /// <param name="oldValue">Parameter old value</param>
        /// <param name="newValue">Paramenter new value </param>]        
        protected virtual void OnAllowItemsRotationChanged(bool oldValue, bool newValue)
        {
            this.ScatterHost.AllowItemsRotation = newValue;
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the IsPinchToZoomMode property.
        /// </summary>
        /// <param name="oldValue">Parameter old value</param>
        /// <param name="newValue">Paramenter new value </param>]        
        protected virtual void OnIsPinchToZoomModeChanged(bool oldValue, bool newValue)
        {
            VisualStateManager.GoToState(this, newValue ? "ZoomOn" : "ZoomOff", true);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the IsVisible property.
        /// </summary>
        /// <param name="oldValue">Parameter old value</param>
        /// <param name="newValue">Paramenter new value </param>]        
        protected virtual void OnIsVisibleChanged(bool oldValue, bool newValue)
        {
            this.Margin = newValue
                ? new Thickness(this.Margin.Left + 50000, 0, 0, 0)
                : new Thickness(this.Margin.Left - 50000, 0, 0, 0);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the ItemsSource property.
        /// </summary>
        /// <param name="oldValue">Parameter old value</param>
        /// <param name="newValue">Paramenter new value </param>]        
        protected virtual void OnItemsSourceChanged(object oldValue, object newValue)
        {
            if (this.ScatterHost != null)
            {
                this.ScatterHost.ItemsSource = newValue;
            }
        }

        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            try
            {
                base.OnPointerPressed(e);
                this.CapturePointer(e.Pointer);
            }
            catch 
            {
                this.ReleasePointerCapture(e.Pointer);
            }
            finally
            {
                this.Opacity = this.ActiveOpacity;
            }
        }

        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            base.OnPointerReleased(e);
            this.ReleasePointerCapture(e.Pointer);
            this.Opacity = this.InactiveOpacity;
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the ScatterArea property.
        /// </summary>
        /// <param name="oldValue">Parameter old value</param>
        /// <param name="newValue">Paramenter new value </param>]        
        protected virtual void OnScatterAreaChanged(ScatterView oldValue, ScatterView newValue)
        {
            if (newValue != null)
            {
                this.areaWidth = newValue.ActualWidth;
                this.areaHeight = newValue.ActualHeight;
                this.InitRatios();
                this.InitScatterViewHost(newValue);
                //newValue.ZoomFactorChanged += this.OnZoomFactorChanged;
                newValue.SizeChanged += this.UpdateScatterViewInfo;

            }
            else
            {
                this.areaWidth = -1;
                this.areaHeight = -1;
                this.ratioX = 1;
                this.ratioY = 1;
            }
        }

        private void UpdateScatterViewInfo(object sender, SizeChangedEventArgs e)
        {
            ScatterView source = (ScatterView) sender;
            this.areaWidth = source.ActualWidth;
            this.areaHeight = source.ActualHeight;
            this.InitRatios();
            this.InitScatterViewHost(source);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the ScrollingHost property.
        /// </summary>
        /// <param name="oldValue">Parameter old value</param>
        /// <param name="newValue">Paramenter new value </param>]        
        protected virtual async void OnScrollingHostChanged(ScrollViewer oldValue, ScrollViewer newValue)
        {
            if (newValue != null)
            {
                newValue.ViewChanged += this.OnViewScrolledOutside;
                newValue.SizeChanged += this.OnScrollingHostSizeChanged;

                //var x = this.ScrollingHost.ViewportWidth;
                //this.Width = x*0.2;
                //this.Height = this.Width/this.ratioXY;
                //this.InitRatios();
                this.InitElements(newValue);
            }
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the SelectedItemIndex property.
        /// </summary>
        /// <param name="oldValue">Parameter old value</param>
        /// <param name="newValue">Paramenter new value </param>]        
        protected virtual void OnSelectedItemIndexChanged(int oldValue, int newValue)
        {
            //if (this.ScatterArea != null) this.ScatterHost.SelectedItemIndex = newValue;
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the ThumbBackground property.
        /// </summary>
        /// <param name="oldValue">Parameter old value</param>
        /// <param name="newValue">Paramenter new value </param>]        
        protected virtual void OnThumbBackgroundChanged(Brush oldValue, Brush newValue)
        {
            this.Thumb.Fill = newValue;
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the ThumbBorderBrush property.
        /// </summary>
        /// <param name="oldValue">Parameter old value</param>
        /// <param name="newValue">Paramenter new value </param>]        
        protected virtual void OnThumbBorderBrushChanged(Brush oldValue, Brush newValue)
        {
            this.Thumb.Stroke = newValue;
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the ThumbBorderSize property.
        /// </summary>
        /// <param name="oldValue">Parameter old value</param>
        /// <param name="newValue">Paramenter new value </param>]        
        protected virtual void OnThumbBorderSizeChanged(double oldValue, double newValue)
        {
            this.Thumb.StrokeThickness = newValue;
        }

        /// <summary>
        /// Initializes the elements.
        /// </summary>
        /// <param name="source">The source.</param>
        private void InitElements(ScrollViewer source)
        {
            this.Thumb.Width = source.ViewportWidth / this.ratioX;
            this.Thumb.Height = source.ViewportHeight / this.ratioY;
        }

        /// <summary>
        /// Initializes the internal ratios.
        /// </summary>
        private void InitRatios()
        {
            this.ratioX = this.areaWidth / this.ActualWidth;
            this.ratioY = this.areaHeight / this.ActualHeight;
        }

        private void InitScatterViewHost(ScatterView mainScatterView)
        {
            this.ScatterHost.Width = mainScatterView.ActualWidth;
            this.ScatterHost.Height = mainScatterView.ActualHeight;
            this.ScatterHost.Background = mainScatterView.Background;
            //@@@this.ScatterHost.ItemTemplateSelector = this.ItemTemplateSelector;
        }

        /// <summary>
        /// Initializes the transforms.
        /// </summary>
        private void InitTransforms()
        {
            this.transform = new CompositeTransform();
            this.RenderTransformOrigin = new Point(0.5, 0.5);
            this.Thumb.RenderTransform = this.transform;
        }

        /// <summary>
        /// Scrolls the scatter are to new position
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="top">The top.</param>
        private void Move(double left, double top)
        {
            this.ignoreOutsizeEvent = true;
            this.ScrollingHost.ChangeView(left * this.ratioX, top * this.ratioY, null, false);

        }

        /// <summary>
        /// Moves the thumbto a specific position
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        private void MoveThumb(double x, double y)
        {
            this.transform.TranslateX = x;
            this.transform.TranslateY = y;
        }

        /// <summary>
        /// Handles changes to the AllowItemsRotation property.
        /// </summary>
        /// <param name="d">Source dependendency o bject</param>
        /// <param name="e">Event argument</param>
        private static void OnAllowItemsRotationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MiniMap target = (MiniMap)d;
            bool oldValue = (bool)e.OldValue;
            bool newValue = target.AllowItemsRotation;
            target.OnAllowItemsRotationChanged(oldValue, newValue);
        }

        /// <summary>
        /// Handles changes to the IsPinchToZoomMode property.
        /// </summary>
        /// <param name="d">Source dependendency o bject</param>
        /// <param name="e">Event argument</param>
        private static void OnIsPinchToZoomModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MiniMap target = (MiniMap)d;
            bool oldValue = (bool)e.OldValue;
            bool newValue = target.IsPinchToZoomMode;
            target.OnIsPinchToZoomModeChanged(oldValue, newValue);
        }

        /// <summary>
        /// Handles changes to the IsVisible property.
        /// </summary>
        /// <param name="d">Source dependendency o bject</param>
        /// <param name="e">Event argument</param>
        private static void OnIsVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MiniMap target = (MiniMap)d;
            bool oldValue = (bool)e.OldValue;
            bool newValue = target.IsVisible;
            target.OnIsVisibleChanged(oldValue, newValue);
        }

        /// <summary>
        /// Handles changes to the ItemsSource property.
        /// </summary>
        /// <param name="d">Source dependendency o bject</param>
        /// <param name="e">Event argument</param>
        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MiniMap target = (MiniMap)d;
            object oldValue = (object)e.OldValue;
            object newValue = target.ItemsSource;
            target.OnItemsSourceChanged(oldValue, newValue);
        }

        /// <summary>
        /// Called when control loads.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= this.OnLoaded;

            this.Thumb.PointerCanceled += this.OnPointerCanceled;
            this.Thumb.PointerPressed += this.OnPointerPressed;
            this.Thumb.PointerReleased += this.OnPointerReleased;
            this.Thumb.PointerMoved += this.OnPointerMoved;

            this.gestureRecognizer.ManipulationUpdated += this.OnManipulationUpdated;

            this.InitTransforms();
            this.ScatterHost.AllowItemsRotation = this.AllowItemsRotation;
        }

        /// <summary>
        /// Called when manipulation is updated.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ManipulationUpdatedEventArgs"/> instance containing the event data.</param>
        private void OnManipulationUpdated(GestureRecognizer sender, ManipulationUpdatedEventArgs e)
        {
            if (this.Thumb.ActualWidth >= this.ActualWidth || this.Thumb.ActualHeight >= this.ActualHeight) return;

            GeneralTransform hostTransform = this.Thumb.TransformToVisual(this);
            Rect itemRect = hostTransform.TransformBounds(new Rect(0, 0, this.Thumb.ActualWidth, this.Thumb.ActualHeight));
            double deltaX = 0;
            double deltaY = 0;

            //Constraints X
            if (e.Delta.Translation.X < 0)
            {
                deltaX = itemRect.Left < 0 ? 0 : e.Delta.Translation.X;
                if (deltaX < 0 && Math.Abs(deltaX) > itemRect.Left) deltaX = -itemRect.Left;
            }
            else
            {
                deltaX = itemRect.Right > this.ActualWidth ? 0D : e.Delta.Translation.X;
                double diff = this.ActualWidth - itemRect.Right;
                if (deltaX > diff) deltaX = diff;
            }

            //Constraints Y
            if (e.Delta.Translation.Y < 0)
            {
                deltaY = itemRect.Top < 0 ? 0 : e.Delta.Translation.Y;
                if (deltaY < 0 && Math.Abs(deltaY) > itemRect.Top) deltaY = -itemRect.Top;
            }
            else
            {
                deltaY = itemRect.Bottom > this.ActualHeight ? 0D : e.Delta.Translation.Y;
                double diff = this.ActualHeight - itemRect.Bottom;
                if (deltaY > diff) deltaY = diff;
            }

            this.transform.TranslateX += deltaX;
            this.transform.TranslateY += deltaY;

            this.Move(itemRect.Left, itemRect.Top);
        }

        void OnPointerCanceled(object sender, PointerRoutedEventArgs args)
        {
            this.gestureRecognizer.CompleteGesture();
            args.Handled = true;
        }

        void OnPointerMoved(object sender, PointerRoutedEventArgs args)
        {
            this.gestureRecognizer.ProcessMoveEvents(args.GetIntermediatePoints(this));
            args.Handled = true;
        }

        void OnPointerPressed(object sender, PointerRoutedEventArgs args)
        {
            this.gestureRecognizer.ProcessDownEvent(args.GetCurrentPoint(this));
            this.Thumb.CapturePointer(args.Pointer);
            this.ignoreOutsizeEvent = true;
            this.Opacity = this.ActiveOpacity;
            args.Handled = true;
        }

        void OnPointerReleased(object sender, PointerRoutedEventArgs args)
        {
            this.gestureRecognizer.ProcessUpEvent(args.GetCurrentPoint(this));
            this.ignoreOutsizeEvent = false;
            this.Opacity = this.InactiveOpacity;
            args.Handled = true;
        }

        /// <summary>
        /// Handles changes to the ScatterArea property.
        /// </summary>
        /// <param name="d">Source dependendency o bject</param>
        /// <param name="e">Event argument</param>
        private static void OnScatterAreaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MiniMap target = (MiniMap)d;
            ScatterView oldValue = (ScatterView)e.OldValue;
            ScatterView newValue = target.ScatterArea;
            target.OnScatterAreaChanged(oldValue, newValue);
        }

        /// <summary>
        /// Handles changes to the ScrollingHost property.
        /// </summary>
        /// <param name="d">Source dependendency o bject</param>
        /// <param name="e">Event argument</param>
        private static void OnScrollingHostChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MiniMap target = (MiniMap)d;
            ScrollViewer oldValue = (ScrollViewer)e.OldValue;
            ScrollViewer newValue = target.ScrollingHost;
            target.OnScrollingHostChanged(oldValue, newValue);
        }

        /// <summary>
        /// Called when scrolling host size changes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SizeChangedEventArgs"/> instance containing the event data.</param>
        private void OnScrollingHostSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.InitElements(this.ScrollingHost);
            this.InitRatios();
        }

        /// <summary>
        /// Handles changes to the SelectedItemIndex property.
        /// </summary>
        /// <param name="d">Source dependendency o bject</param>
        /// <param name="e">Event argument</param>
        private static void OnSelectedItemIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MiniMap target = (MiniMap)d;
            int oldValue = (int)e.OldValue;
            int newValue = target.SelectedItemIndex;
            target.OnSelectedItemIndexChanged(oldValue, newValue);
        }

        /// <summary>
        /// Handles changes to the ThumbBackground property.
        /// </summary>
        /// <param name="d">Source dependendency o bject</param>
        /// <param name="e">Event argument</param>
        private static void OnThumbBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MiniMap target = (MiniMap)d;
            Brush oldValue = (Brush)e.OldValue;
            Brush newValue = target.ThumbBackground;
            target.OnThumbBackgroundChanged(oldValue, newValue);
        }

        /// <summary>
        /// Handles changes to the ThumbBorderBrush property.
        /// </summary>
        /// <param name="d">Source dependendency o bject</param>
        /// <param name="e">Event argument</param>
        private static void OnThumbBorderBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MiniMap target = (MiniMap)d;
            Brush oldValue = (Brush)e.OldValue;
            Brush newValue = target.ThumbBorderBrush;
            target.OnThumbBorderBrushChanged(oldValue, newValue);
        }

        /// <summary>
        /// Handles changes to the ThumbBorderSize property.
        /// </summary>
        /// <param name="d">Source dependendency o bject</param>
        /// <param name="e">Event argument</param>
        private static void OnThumbBorderSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MiniMap target = (MiniMap)d;
            double oldValue = (double)e.OldValue;
            double newValue = target.ThumbBorderSize;
            target.OnThumbBorderSizeChanged(oldValue, newValue);
        }

        /// <summary>
        /// Called when unloads
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.Thumb.PointerCanceled -= this.OnPointerCanceled;
            this.Thumb.PointerPressed -= this.OnPointerPressed;
            this.Thumb.PointerReleased -= this.OnPointerReleased;
            this.Thumb.PointerMoved -= this.OnPointerMoved;
            if (this.ScrollingHost != null) this.ScrollingHost.ViewChanged -= this.OnViewScrolledOutside;

            this.gestureRecognizer.ManipulationUpdated -= this.OnManipulationUpdated;
            //if (this.ScatterArea != null) this.ScatterArea.ZoomFactorChanged -= this.OnZoomFactorChanged;
        }

        /// <summary>
        /// Called when view is scrolled on main scroller.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ScrollViewerViewChangingEventArgs"/> instance containing the event data.</param>
        private void OnViewScrolledOutside(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if(this.IsFreezed) return;

            if (!this.ignoreOutsizeEvent)
            {
                double percentX = this.ScrollingHost.HorizontalOffset / this.ScrollingHost.ScrollableWidth;
                double percentY = this.ScrollingHost.VerticalOffset / this.ScrollingHost.ScrollableHeight;
                var x = (this.ActualWidth - this.Thumb.ActualWidth) * percentX;
                var y = (this.ActualHeight - this.Thumb.ActualHeight) * percentY;
                this.MoveThumb(x, y);
            }
        }

        private void OnZoomFactorChanged(object sender, ZoomFactorChangedEventArgs e)
        {
            if (this.ScatterHost != null && !this.IsFreezed)
            {
                this.Update(e.Scale);
            }
        }

        public async void Update(double scale)
        {
            this.areaWidth = this.ScatterArea.ActualWidth * scale;
            this.areaHeight = this.ScatterArea.ActualHeight * scale;
            this.InitRatios();
            this.InitElements(this.ScrollingHost);
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.OnViewScrolledOutside(this.ScrollingHost, null));
        }

        public async void Update(int width, int height)
        {
            this.areaWidth = width;
            this.areaHeight = height;
            this.InitRatios();
            this.InitElements(this.ScrollingHost);
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.OnViewScrolledOutside(this.ScrollingHost, null));
        }
    }
}