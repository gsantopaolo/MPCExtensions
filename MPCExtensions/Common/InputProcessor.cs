using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace MPCExtensions.Common
{
    public static class helper
    {
        public static T FindVisualParent<T>(this FrameworkElement obj) where T : FrameworkElement
        {
            if (obj != null)
            {
                for (DependencyObject obj2 = VisualTreeHelper.GetParent(obj); obj2 != null; obj2 = VisualTreeHelper.GetParent(obj2))
                {
                    T local = obj2 as T;
                    if (local != null)
                    {
                        return local;
                    }
                }
            }
            return default(T);
        }

        
    }

    /// <summary>
    /// Thin wrapper around the <see cref="Windows.UI.Input.GestureRecognizer"/>, routes pointer events received by
    /// the manipulation target to the gesture recognizer.
    /// </summary>
    /// <remarks>
    /// Transformations during manipulations cannot be expressed in the coordinate space of the manipulation target.
    /// Instead they need be expressed with respect to a reference coordinate space, usually an ancestor (in the UI tree)
    /// of the element being manipulated.
    /// </remarks>
    internal abstract class InputProcessor
    {
        protected Windows.UI.Input.GestureRecognizer _gestureRecognizer;

        // Element being manipulated
        protected Windows.UI.Xaml.FrameworkElement _target;
        public Windows.UI.Xaml.FrameworkElement Target
        {
            get { return _target; }
        }

        // Reference element that contains the coordinate space used for expressing transformations 
        // during manipulation, usually the parent element of Target in the UI tree
        protected Windows.UI.Xaml.Controls.Canvas _reference;
        public Windows.UI.Xaml.FrameworkElement Reference
        {
            get { return _reference; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="element">
        /// Manipulation target.
        /// </param>
        /// <param name="reference">
        /// Element that contains the coordinate space used for expressing transformations
        /// during manipulations, usually the parent element of Target in the UI tree.
        /// </param>
        /// <remarks>
        /// Transformations during manipulations cannot be expressed in the coordinate space of the manipulation target.
        /// Thus <paramref name="element"/> and <paramref name="reference"/> must be different. Usually <paramref name="reference"/>
        /// will be an ancestor of <paramref name="element"/> in the UI tree.
        /// </remarks>
        internal InputProcessor(Windows.UI.Xaml.FrameworkElement element, Windows.UI.Xaml.Controls.Canvas reference)
        {
            _target = element;
            _reference = reference;

            // Setup pointer event handlers for the element.
            // They are used to feed the gesture recognizer.    
            //_target.PointerCanceled += OnPointerCanceled;
            //_target.PointerMoved += OnPointerMoved;
            //_target.PointerPressed += OnPointerPressed;
            //_target.PointerReleased += OnPointerReleased;
            //_target.PointerWheelChanged += OnPointerWheelChanged;

            _target.PointerCanceled += new WeakEvent<UIElement, object, Windows.UI.Xaml.Input.PointerRoutedEventArgs>(_target)
            {
                EventAction = (instance, source, eventArgs) => OnPointerCanceled(source, eventArgs),
                DetachAction = (instance, weakEventListener) => _target.PointerCanceled -= weakEventListener.Handler
            }.Handler; 

            _target.PointerMoved += new WeakEvent<UIElement, object, Windows.UI.Xaml.Input.PointerRoutedEventArgs>(_target)
            {
                EventAction = (instance, source, eventArgs) => OnPointerMoved(source, eventArgs),
                DetachAction = (instance, weakEventListener) => _target.PointerMoved -= weakEventListener.Handler
            }.Handler;

            _target.PointerPressed += new WeakEvent<UIElement, object, Windows.UI.Xaml.Input.PointerRoutedEventArgs>(_target)
            {
                EventAction = (instance, source, eventArgs) => OnPointerPressed(source, eventArgs),
                DetachAction = (instance, weakEventListener) => _target.PointerPressed -= weakEventListener.Handler
            }.Handler;

            _target.PointerReleased += new WeakEvent<UIElement, object, Windows.UI.Xaml.Input.PointerRoutedEventArgs>(_target)
            {
                EventAction = (instance, source, eventArgs) => OnPointerReleased(source, eventArgs),
                DetachAction = (instance, weakEventListener) => _target.PointerReleased -= weakEventListener.Handler
            }.Handler; 

            _target.PointerWheelChanged += new WeakEvent<UIElement, object, Windows.UI.Xaml.Input.PointerRoutedEventArgs>(_target)
            {
                EventAction = (instance, source, eventArgs) => OnPointerWheelChanged(source, eventArgs),
                DetachAction = (instance, weakEventListener) => _target.PointerWheelChanged -= weakEventListener.Handler
            }.Handler;

            // Create the gesture recognizer
            _gestureRecognizer = new Windows.UI.Input.GestureRecognizer();
            _gestureRecognizer.GestureSettings = Windows.UI.Input.GestureSettings.None;
        }

        private void _target_ManipulationCompleted(object sender, Windows.UI.Xaml.Input.ManipulationCompletedRoutedEventArgs e)
        {
            _target.Opacity = 1;
        }

        #region Pointer event handlers
        public static int i = 100;
        private void OnPointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs args)
        {
            if (args.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Pen)
                return;

            _target.Opacity = 0.5;
           
                        
            var indexes = new List<int>();
            Panel panel = _target.FindVisualParent<Panel>();
            if (panel != null)
            {
                foreach (UIElement element in panel.Children)
                {
                    if (element != _target as UIElement)
                    {
                        indexes.Add(Canvas.GetZIndex(element));
                    }
                }

                Int16 currentIndex = 0;
                if (indexes.Count > 0 && indexes.Max() < Int16.MaxValue)
                {
                    currentIndex = (Int16)indexes.Max();
                    ContentPresenter presenter = _target as ContentPresenter;
                    if (presenter != null)
                    {
                        presenter.SetValue(Canvas.ZIndexProperty, indexes.Max() + 1);
                    }
                }
                else if (indexes.Count > 0 && indexes.Max() >= Int16.MaxValue)
                {
                    // Need to rearrange all ZIndexs!
                    var result = panel.Children.OrderBy(x => Canvas.GetZIndex(x));
                    Int16 count = 0;
                    foreach (UIElement element in result)
                    {
                        if (element != _target as UIElement)
                            element.SetValue(Canvas.ZIndexProperty, count);
                        count++;
                    }
                    //at the end we set the ZIndex of our element as the highest
                    ContentPresenter presenter = _target as ContentPresenter;
                    if (presenter != null)
                    {
                        presenter.SetValue(Canvas.ZIndexProperty, count);
                    }
                }
            }

            // Obtain current point in the coordinate system of the reference element
            Windows.UI.Input.PointerPoint currentPoint = args.GetCurrentPoint(_reference);

            // Route the event to the gesture recognizer
            _gestureRecognizer.ProcessDownEvent(currentPoint);

            // Capture the pointer associated to this event
            _target.CapturePointer(args.Pointer);

            // Mark event handled, to prevent execution of default event handlers
            args.Handled = true;
        }

        private void OnPointerMoved(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs args)
        {
            if (args.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Pen)
                return;
            // Route the events to the gesture recognizer.
            // All intermediate points are passed to the gesture recognizer in
            // the coordinate system of the reference element.
            _gestureRecognizer.ProcessMoveEvents(args.GetIntermediatePoints(_reference));

            // Mark event handled, to prevent execution of default event handlers
            args.Handled = true;
            
        }

        private void OnPointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs args)
        {
            if (args.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Pen)
                return;
            // Obtain current point in the coordinate system of the reference element
            Windows.UI.Input.PointerPoint currentPoint = args.GetCurrentPoint(_reference);

            // Route the event to the gesture recognizer
            _gestureRecognizer.ProcessUpEvent(currentPoint);

            // Release pointer capture on the pointer associated to this event
            _target.ReleasePointerCapture(args.Pointer);

            _target.Opacity = 1;

            // Mark event handled, to prevent execution of default event handlers
            args.Handled = true;

            
        }

        private void OnPointerWheelChanged(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs args)
        {
            // Obtain current point in the coordinate system of the reference element
            Windows.UI.Input.PointerPoint currentPoint = args.GetCurrentPoint(_reference);

            // Find out whether shift/ctrl buttons are pressed
            bool shift = (args.KeyModifiers & Windows.System.VirtualKeyModifiers.Shift) == Windows.System.VirtualKeyModifiers.Shift;
            bool ctrl = (args.KeyModifiers & Windows.System.VirtualKeyModifiers.Control) == Windows.System.VirtualKeyModifiers.Control;

            // Route the event to the gesture recognizer
            _gestureRecognizer.ProcessMouseWheelEvent(currentPoint, shift, ctrl);

            // Mark event handled, to prevent execution of default event handlers
            args.Handled = true;
        }

        private void OnPointerCanceled(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs args)
        {
            if (args.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Pen)
                return;

            _gestureRecognizer.CompleteGesture();

            // Release pointer capture on the pointer associated to this event
            _target.ReleasePointerCapture(args.Pointer);

            // Mark event handled, to prevent execution of default event handlers
            args.Handled = true;

            
        }

        #endregion Pointer event handlers
    }



    
}