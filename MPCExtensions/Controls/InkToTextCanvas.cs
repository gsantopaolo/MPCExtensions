using MPCExtensions.Common;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private DispatcherTimer timer;
        //private UIElement rootElement;
        private bool InkerActive = false;
        private Grid rootElement;
        private InkCanvas inker;
        private UIElement root;
        public InkToTextCanvas()
        {
            this.DefaultStyleKey = typeof(InkToTextCanvas);
            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromSeconds(3);
            
        }

        protected override void OnApplyTemplate()
        {

            

            //if (Windows.ApplicationModel.DesignMode.DesignModeEnabled == false)
            //{
            

            rootElement = GetTemplateChild(PART_ROOT_NAME) as Grid;
            inker = GetTemplateChild(PART_INKER_NAME) as InkCanvas;
            if (rootElement != null && inker != null)
            {
                rootElement.Visibility = Visibility.Collapsed;
                InitializeInker();

                root = VisualTreeHelperEx.FindRoot(rootElement);
                
                root.PointerPressed += RootElement_PointerEvent;
                root.PointerMoved += RootElement_PointerEvent;
                
            }
            //}
        }

        private void RootElement_PointerEvent(object sender, PointerRoutedEventArgs e)
        {
            DecideInputMethod(e.Pointer);
        }


        private void InitializeInker()
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

        

        private void DecideInputMethod(Pointer pointer)
        {
            if (pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Pen && !InkerActive)
            {
                // enable Inker
                InkerActive = true;
                rootElement.Visibility = Visibility.Visible;
            }
            else if (pointer.PointerDeviceType != Windows.Devices.Input.PointerDeviceType.Pen && InkerActive)
            {
                // disable Inker
                InkerActive = false;
                timer.Stop();
                RecognizeInkerText();
                rootElement.Visibility = Visibility.Collapsed;
            }
        }

        private void InkPresenter_StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            timer.Start();
        }

        private async void Timer_Tick(object sender, object e)
        {
            timer.Stop();
            await RecognizeInkerText();
        }

        private async Task RecognizeInkerText()
        {
            var inkRecognizer = new InkRecognizerContainer();
            var recognitionResults = await inkRecognizer.RecognizeAsync(inker.InkPresenter.StrokeContainer, InkRecognitionTarget.All);

            List<TextBox> boxes = new List<TextBox>();

            foreach (var result in recognitionResults)
            {
                List<UIElement> elements = new List<UIElement>(
                    VisualTreeHelper.FindElementsInHostCoordinates(
                        new Rect(new Point(result.BoundingRect.X, result.BoundingRect.Y),
                        new Size(result.BoundingRect.Width, result.BoundingRect.Height)),root
                    ));

                TextBox box = elements.Where(el => el is TextBox && (el as TextBox).IsEnabled).FirstOrDefault() as TextBox;

                if (box != null)
                {
                    if (!boxes.Contains(box))
                    {
                        boxes.Add(box);
                        box.Text = "";
                    }
                    box.Text += result.GetTextCandidates().FirstOrDefault().Trim();
                }
            }

            inker.InkPresenter.StrokeContainer.Clear();
        }
    }
}
