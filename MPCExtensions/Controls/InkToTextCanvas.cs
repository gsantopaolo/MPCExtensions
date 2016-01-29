using MPCExtensions.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace MPCExtensions.Controls
{
    [TemplatePart(Name = PART_ROOT_NAME, Type = typeof(Grid))]
    public class InkToTextCanvas : Control
    {
        private const string PART_ROOT_NAME = "PART_ROOT";
        private const string PART_INKER_NAME = "PART_INKER";
        private const string TEXT_PROPERTY_NAME = "Text";
        private DispatcherTimer timer;
        //private UIElement rootElement;
        //private bool InkerActive = false;TargetInkCanvas
        private Grid container;
        private InkCanvas inker;
        private UIElement root;
        private UIElement scatterView;
        public InkToTextCanvas()
        {
            this.DefaultStyleKey = typeof(InkToTextCanvas);
            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromSeconds(3);
            
        }

        public Control TargetTextControl
        {
            get { return (Control)GetValue(TargetTextControlProperty); }
            set
            {
                var type = value.GetType();
                // Get the PropertyInfo object by passing the property name.
                PropertyInfo pInfo = type.GetProperty(TEXT_PROPERTY_NAME);
                
                if (pInfo == null)
                    throw new ArgumentException("Control provided does not expose a class of type string with name Text");

                SetValue(TargetTextControlProperty, value);
            }
        }
        public static readonly DependencyProperty TargetTextControlProperty =
           DependencyProperty.Register(nameof(TargetTextControl), typeof(Control), typeof(ScatterView), new PropertyMetadata(null));


        protected override void OnApplyTemplate()
        {
            //if (Windows.ApplicationModel.DesignMode.DesignModeEnabled == false)
            //{

            try
            {
                container = GetTemplateChild(PART_ROOT_NAME) as Grid;
                inker = GetTemplateChild(PART_INKER_NAME) as InkCanvas;
                if (container != null && inker != null)
                {
                    container.Visibility = Visibility.Visible;
                    InitializeInker();

                    root = VisualTreeHelperEx.FindRoot(container, false);
                    scatterView = VisualTreeHelperEx.FindRoot(container, true);

                    scatterView.PointerCanceled += RootElement_PointerCanceled;
                    scatterView.PointerReleased += RootElement_PointerCanceled;
                    scatterView.PointerEntered += ScatterView_PointerEntered;
                    scatterView.PointerExited += ScatterView_PointerExited;
                  
                }
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.ToString());
            }
        }

        private void ScatterView_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("PointerExited");
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Pen)
            {
                root.ReleasePointerCapture(e.Pointer);

            }
            PointerProcessor(e.Pointer);
        }

        private void ScatterView_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("PointerEntered");
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Pen)
            {
                root.CapturePointer(e.Pointer);

            }
            PointerProcessor(e.Pointer);
        }

        private void RootElement_PointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("PointerCanceled");
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Pen)
            {
                root.ReleasePointerCapture(e.Pointer);
            }
            PointerProcessor(e.Pointer);
        }


        private void InitializeInker()
        {
            try
            {
                inker.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Pen;

                var drawingAttributes = new InkDrawingAttributes
                {
                    DrawAsHighlighter = false,
                    Color = Colors.DarkBlue,
                    PenTip = PenTipShape.Circle,
                    IgnorePressure = false,
                    Size = new Size(3, 3)
                };
                inker.InkPresenter.UpdateDefaultDrawingAttributes(drawingAttributes);
                inker.InkPresenter.StrokesCollected += InkPresenter_StrokesCollected;
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.ToString());
            }

        }

        private void PointerProcessor(Pointer pointer)
        {
            System.Diagnostics.Debug.WriteLine("PointerProcessor");
            try
            {
                if (pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Pen && container.Visibility == Visibility.Collapsed)
                {
                    // enable Inker
                    container.Visibility = Visibility.Visible;
                    System.Diagnostics.Debug.WriteLine("PointerProcessor_enable");
                }
                else if (pointer.PointerDeviceType != Windows.Devices.Input.PointerDeviceType.Pen && container.Visibility == Visibility.Visible)
                {
                    System.Diagnostics.Debug.WriteLine("PointerProcessor_diable");
                    // disable Inker
                    timer.Stop();
                    container.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.ToString());
            }
        }

        private void InkPresenter_StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("InkPresenter_StrokesCollected");
            timer.Start();
        }

        private async void Timer_Tick(object sender, object e)
        {
            System.Diagnostics.Debug.WriteLine("Timer_Tick");
            timer.Stop();
            await RecognizeInkerText();
        }

        private async Task RecognizeInkerText()
        {
            System.Diagnostics.Debug.WriteLine("RecognizeInkerText");
            try
            {
                var inkRecognizer = new InkRecognizerContainer();
                var recognitionResults = await inkRecognizer.RecognizeAsync(inker.InkPresenter.StrokeContainer, InkRecognitionTarget.All);

                List<TextBox> boxes = new List<TextBox>();

                string value = string.Empty; 

                foreach (var result in recognitionResults)
                {
                    if (TargetTextControl == null)
                    {
                        Point p = new Point(result.BoundingRect.X, result.BoundingRect.Y);
                        Size s = new Size(result.BoundingRect.Width, result.BoundingRect.Height);
                        Rect r = new Rect(p, s);
                        var elements = VisualTreeHelper.FindElementsInHostCoordinates(r, scatterView);

                        TextBox box = elements.Where(el => el is TextBox && (el as TextBox).IsEnabled).FirstOrDefault() as TextBox;
                        if (box != null)
                        {
                            if (!boxes.Contains(box))
                            {
                                boxes.Add(box);
                                box.Text = "";
                            }
                            if (string.IsNullOrEmpty(box.Text) == false)
                                box.Text += " ";
                            box.Text += result.GetTextCandidates().FirstOrDefault().Trim();
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(value) == false)
                            value += " ";
                        value += result.GetTextCandidates().FirstOrDefault().Trim();
                    }
                        
                }

                if (TargetTextControl != null)
                {
                    var type = TargetTextControl.GetType();
                    PropertyInfo pInfo = type.GetProperty(TEXT_PROPERTY_NAME);
                    pInfo.SetValue(TargetTextControl, value);
                }

                inker.InkPresenter.StrokeContainer.Clear();
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.ToString());
            }
        }
    }
}
