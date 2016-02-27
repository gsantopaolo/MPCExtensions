using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.Graphics.Display;
using Windows.Media.Capture;
using Windows.Media.SpeechRecognition;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MPCExtensions.Controls
{
    public class VoiceToTextBox : TextBox
    {
        #region private fields
        private const string VOICE_BUTTON_NAME = "VoiceButton";
        private const string STOP_VOICE_BUTTON_NAME = "StopVoiceButton";
        private const string PART_SYMBOL_ICON_NAME = "SymbolVoice";
        private const string VISUAL_STATE_LISTENING = "Listening";
        private const string VISUAL_STATE_NOT_LISTENING = "NotListening";
        private const string VISUAL_STATE_VOICE_DISABLED = "VoiceDisabled";
        private const string WEB_SEARCH = "web search";
        private const string PLACE_HOLDER_TEXT = "Type or Speech";
        private const string SPEECH_RECOGNITION_FAILED = "Speech Recognition Failed";
        private const string LISTENING_TEXT = "Listening..";
        private Button voiceButton;
        private Button stopVoiceButton;
        private SpeechRecognizer speechRecognizer;
        private Task initialization;
        private DispatcherTimer timer;
        private string hypotesis = string.Empty;
        private bool listening;
        private static Random randomizer = new Random();
        private bool firstStopAttemptDone;
        #endregion

        #region ctor
        public VoiceToTextBox()
        {
            this.DefaultStyleKey = typeof(VoiceToTextBox);
            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromMilliseconds(100);
            this.Unloaded += VoiceToTextBox_Unloaded;
        }
        #endregion

        #region override OnApplyTemplate and async initialization
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            voiceButton = GetTemplateChild(VOICE_BUTTON_NAME) as Button;
            stopVoiceButton = GetTemplateChild(STOP_VOICE_BUTTON_NAME) as Button;
            this.PlaceholderText = PLACE_HOLDER_TEXT;

            initialization = InitializeAsync();

            voiceButton.Click += VoiceButton_Click;
            stopVoiceButton.Click += StopVoiceButton_Click;

            readOnlyCalbackToken = this.RegisterPropertyChangedCallback(TextBox.IsReadOnlyProperty, ReadOnlyCallback);
        }

        private async Task InitializeAsync()
        {
            // if user haven't give permission to speec or app is running on a phone then the voice button has not to be shown
            if (await Common.AudioCapturePermissions.RequestMicrophonePermission())
                //if (await Template10.Utils.AudioUtils.RequestMicrophonePermission() == false || DeviceUtils.Current().DeviceDisposition() == DeviceUtils.DeviceDispositions.Phone
                //    || DeviceUtils.Current().DeviceDisposition() == DeviceUtils.DeviceDispositions.Continuum)
                VisualStateManager.GoToState(this, VISUAL_STATE_VOICE_DISABLED, true);

            // if textbox is readonly there should not be possible to use voice recognition
            if (IsReadOnly)
            {
                voiceButton.IsEnabled = false;
                stopVoiceButton.IsEnabled = false;
            }
        }
        #endregion

        private long readOnlyCalbackToken;

        #region private methods
        private void VoiceToTextBox_Unloaded(object sender, RoutedEventArgs e)
        {
            this.UnregisterPropertyChangedCallback(TextBox.IsReadOnlyProperty, readOnlyCalbackToken);
            this.Unloaded -= VoiceToTextBox_Unloaded;
            voiceButton.Click -= VoiceButton_Click;
            stopVoiceButton.Click -= StopVoiceButton_Click;
            timer.Tick -= Timer_Tick;
        }

        private async void StopVoiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (speechRecognizer?.State != SpeechRecognizerState.Idle)
            {
                // Cancelling recognition prevents any currently recognized speech from
                // generating a ResultGenerated event. StopAsync() will allow the final session to 
                // complete.
                try
                {
                    if (firstStopAttemptDone == false)
                    {
                        await speechRecognizer?.StopRecognitionAsync();
                        firstStopAttemptDone = true;
                    }
                    else
                    {
                        speechRecognizer?.Dispose();
                        speechRecognizer = null;
                    }
                    //await TryDisposeSpeech();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
        }

        private async void VoiceButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the top user-preferred language and its display name.
                var topUserLanguage = Windows.System.UserProfile.GlobalizationPreferences.Languages[0];
                var language = new Windows.Globalization.Language(topUserLanguage);

                firstStopAttemptDone = false;
                listening = true;
                using (speechRecognizer = new SpeechRecognizer(language))
                {

                    var dictationConstraint = new SpeechRecognitionTopicConstraint(SpeechRecognitionScenario.WebSearch, WEB_SEARCH);
                    speechRecognizer.Constraints.Add(dictationConstraint);
                    SpeechRecognitionCompilationResult compilationResult = await speechRecognizer.CompileConstraintsAsync();

                    // setting timeouts
                    speechRecognizer.Timeouts.InitialSilenceTimeout = TimeSpan.FromSeconds(4.0);
                    speechRecognizer.Timeouts.BabbleTimeout = TimeSpan.FromSeconds(4.0);
                    speechRecognizer.Timeouts.EndSilenceTimeout = TimeSpan.FromSeconds(1.0);

                    speechRecognizer.HypothesisGenerated += SpeechRecognizer_HypothesisGenerated;

                    if (compilationResult.Status != SpeechRecognitionResultStatus.Success)
                        return;

                    VisualStateManager.GoToState(this, VISUAL_STATE_LISTENING, true);
                    this.IsReadOnly = true;
                    this.Text = LISTENING_TEXT;

                    SpeechRecognitionResult speechRecognitionResult = await speechRecognizer.RecognizeAsync();
                    if (speechRecognitionResult.Status == SpeechRecognitionResultStatus.Success)
                        Text = speechRecognitionResult.Text;
                    else
                        Text = SPEECH_RECOGNITION_FAILED;

                   
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                Text = string.Empty;
            }
            finally
            {
                timer.Stop();
                hypotesis = string.Empty;
                VisualStateManager.GoToState(this, VISUAL_STATE_NOT_LISTENING, true);
                this.IsReadOnly = false;
                listening = false;
            }
            //try
            //{
            //    listening = true;

            //    // if SpeechRecognizer inizialization failed notthing else to do
            //    if (await TryInitSpeech() == false)
            //        return;

            //    VisualStateManager.GoToState(this, VISUAL_STATE_LISTENING, true);
            //    this.IsReadOnly = true;
            //    this.Text = LISTENING_TEXT;

            //    SpeechRecognitionResult speechRecognitionResult = await speechRecognizer.RecognizeAsync();
            //    if (speechRecognitionResult?.Status == SpeechRecognitionResultStatus.Success)
            //    {
            //        // remove last chat of recognized text if it is a point (never saw a text box filled like a sentence with the point at the end)
            //        if (speechRecognitionResult.Text.Length > 1 && speechRecognitionResult.Text.Substring(speechRecognitionResult.Text.Length - 1, 1) == ".")
            //            Text = speechRecognitionResult.Text.Remove(speechRecognitionResult.Text.Length - 1);
            //        else
            //            Text = speechRecognitionResult.Text;
            //    }
            //    else
            //        Text = SPEECH_RECOGNITION_FAILED;

            //    hypotesis = string.Empty;
            //}
            //catch (Exception ex)
            //{
            //    System.Diagnostics.Debug.WriteLine(ex.Message);
            //    Text = SPEECH_RECOGNITION_FAILED;
            //}
            //finally
            //{
            //    timer.Stop();
            //    await TryDisposeSpeech();

            //    VisualStateManager.GoToState(this, VISUAL_STATE_NOT_LISTENING, true);
            //    this.IsReadOnly = false;
            //    listening = false;
            //}
        }

        private async void SpeechRecognizer_HypothesisGenerated(SpeechRecognizer sender, SpeechRecognitionHypothesisGeneratedEventArgs args)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (timer.IsEnabled == false)
                    timer.Start();
                hypotesis = args.Hypothesis.Text;
                Text = args.Hypothesis.Text;
            });
        }

        /// <summary>
        /// Generates a random string of a given length
        /// </summary>
        /// <param name="length">length of the reandom string to be generated</param>
        /// <returns>random string generated</returns>
        private string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";

            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[randomizer.Next(s.Length)]).ToArray());
        }

        private void Timer_Tick(object sender, object e)
        {
            Text = hypotesis + " " + RandomString(8);
        }

        private void ReadOnlyCallback(DependencyObject sender, DependencyProperty dp)
        {
            if (dp == TextBox.IsReadOnlyProperty)
            {

                System.Diagnostics.Debug.WriteLine("ReaOnly has been set to " + ((TextBox)sender).IsReadOnly);
                //This line produce the same result as above.
                //System.Diagnostics.Debug.WriteLine("ReaOnlyhas been set to " + sender.GetValue(dp));

                // if listening == false and the IsReadOnly value is changing means that the vaule has been changed form outside
                // and voiceButton / stopVoiceButton have to be set accordingly
                if (((TextBox)sender).IsReadOnly && listening == false)
                {
                    voiceButton.IsEnabled = false;
                    stopVoiceButton.IsEnabled = false;
                }
                else if (((TextBox)sender).IsReadOnly == false && listening == false)
                {
                    voiceButton.IsEnabled = true;
                    stopVoiceButton.IsEnabled = true;
                }
            }
        }
        #endregion
    }
}
