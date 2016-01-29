using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.SpeechRecognition;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MPCExtensions.Controls
{
    [TemplatePart(Name = PART_ROOT_NAME, Type = typeof(StackPanel))]
    [TemplatePart(Name = PART_TEXT_NAME, Type = typeof(TextBox))]
    [TemplatePart(Name = PART_BUTTON_NAME, Type = typeof(Button))]
    public class SpeechableTextBox : Control
    { 
        private const string PART_ROOT_NAME = "PART_ROOT";
        private const string PART_TEXT_NAME = "PART_TEXT";
        private const string PART_BUTTON_NAME = "PART_BUTTON";  
        private TextBox _textBox;
        private Button _button;

        private SpeechRecognizer speechRecognizer;
        private CoreDispatcher dispatcher;

        // Text
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(SpeechableTextBox),
            new PropertyMetadata(DependencyProperty.UnsetValue));
        public string Text { get { return (string)GetValue(TextProperty); } set { SetValue(TextProperty, value); } }

        // PlaceholderText
        public static readonly DependencyProperty PlaceholderTextProperty = DependencyProperty.Register(nameof(PlaceholderText), typeof(string), typeof(SpeechableTextBox),
            new PropertyMetadata("Type or Speech"));
        public string PlaceholderText { get { return (string)GetValue(PlaceholderTextProperty); } set { SetValue(PlaceholderTextProperty, value); } }

        public SpeechableTextBox()
        {
            this.DefaultStyleKey = typeof(SpeechableTextBox);
        }

        protected override void OnApplyTemplate()
        { 
            _textBox = GetTemplateChild(PART_TEXT_NAME) as TextBox;
            _button = GetTemplateChild(PART_BUTTON_NAME) as Button;
            InitEvents();
            
        }

        private void InitEvents()
        {
            if (_button != null)
                _button.Click += Button_Click;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await InitSpeech();

            // Disable the UI while recognition is occurring, and provide feedback to the user about current state.
            VisualStateManager.GoToState(this, "Listening", true);

            // Start recognition.
            try
            {
                //IAsyncOperation<SpeechRecognitionResult> recognitionOperation = speechRecognizer.RecognizeAsync();
                SpeechRecognitionResult speechRecognitionResult = await speechRecognizer.RecognizeAsync();
                //SpeechRecognitionResult speechRecognitionResult = await recognitionOperation;
                // If successful, display the recognition result.
                if (speechRecognitionResult.Status == SpeechRecognitionResultStatus.Success)
                {
                    Text = speechRecognitionResult.Text;
                }
                else
                {
                    Text = string.Format("Speech Recognition Failed, Status: {0}", speechRecognitionResult.Status.ToString());
                }
            }
            catch (TaskCanceledException exception)
            {
                // TaskCanceledException will be thrown if you exit the scenario while the recognizer is actively
                // processing speech. Since this happens here when we navigate out of the scenario, don't try to 
                // show a message dialog for this exception.
                System.Diagnostics.Debug.WriteLine("TaskCanceledException caught while recognition in progress (can be ignored):");
                System.Diagnostics.Debug.WriteLine(exception.ToString());
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.ToString());
                //// Handle the speech privacy policy error.
                //if ((uint)exception.HResult == HResultPrivacyStatementDeclined)
                //{
                //    hlOpenPrivacySettings.Visibility = Visibility.Visible;
                //}
                //else
                //{
                //    var messageDialog = new Windows.UI.Popups.MessageDialog(exception.Message, "Exception");
                //    await messageDialog.ShowAsync();
                //}
            }
            finally
            {
                // Reset UI state.
                VisualStateManager.GoToState(this, "NotListening", true);
            }
        }


        /// <summary>
        /// When activating the scenario, ensure we have permission from the user to access their microphone, and
        /// provide an appropriate path for the user to enable access to the microphone if they haven't
        /// given explicit permission for it.
        /// </summary>
        /// <param name="e">The navigation event details</param>
        private async Task InitSpeech()
        {
            // Save the UI thread dispatcher to allow speech status messages to be shown on the UI.
            dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

            bool permissionGained = await Common.AudioCapturePermissions.RequestMicrophonePermission();
            if (permissionGained)
            {
                // Enable the recognition buttons.
                _button.IsEnabled = true;

                if (speechRecognizer != null)
                {
                    // cleanup prior to re-initializing this scenario.
                    //speechRecognizer.StateChanged -= SpeechRecognizer_StateChanged;

                    this.speechRecognizer.Dispose();
                    this.speechRecognizer = null;
                }

                // Create an instance of SpeechRecognizer.
                speechRecognizer = new SpeechRecognizer();

                // Provide feedback to the user about the state of the recognizer.
                //speechRecognizer.StateChanged += SpeechRecognizer_StateChanged;

                // Compile the dictation topic constraint, which optimizes for dictated speech.
                var dictationConstraint = new SpeechRecognitionTopicConstraint(SpeechRecognitionScenario.Dictation, "dictation");
                speechRecognizer.Constraints.Add(dictationConstraint);
                SpeechRecognitionCompilationResult compilationResult = await speechRecognizer.CompileConstraintsAsync();

                speechRecognizer.HypothesisGenerated += SpeechRecognizer_HypothesisGenerated;

                // Check to make sure that the constraints were in a proper format and the recognizer was able to compile it.
                if (compilationResult.Status != SpeechRecognitionResultStatus.Success)
                {
                    // Disable the recognition buttons.
                    _button.IsEnabled = false;

                    // Let the user know that the grammar didn't compile properly.
                    //resultTextBlock.Visibility = Visibility.Visible;
                    //resultTextBlock.Text = "Unable to compile grammar.";
                }

            }
            else
            {
                // "Permission to access capture resources was not given by the user; please set the application setting in Settings->Privacy->Microphone.";
                _button.IsEnabled = false;
            }

            await Task.Yield();
        }

        private async void SpeechRecognizer_HypothesisGenerated(SpeechRecognizer sender, SpeechRecognitionHypothesisGeneratedEventArgs args)
        {
            await _textBox.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                Text = args.Hypothesis.Text;
            });
        }
    }
}
