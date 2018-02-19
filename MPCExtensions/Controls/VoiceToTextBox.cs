// -----------------------------------------------------------------------
// <copyright file="VoiceToTextBox.cs" company="IBV Informatik AG">
//    Copyright (c) IBV Informatik AG All rights reserved.
// </copyright>
// 
//  
// created: 2017-04-20 @ 20:06
//  edited: 2017-04-24 @ 18:52
// -----------------------------------------------------------------------

#region Using

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Globalization;
using Windows.Media.SpeechRecognition;
using Windows.System.UserProfile;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using MPCExtensions.Common;

#endregion

namespace MPCExtensions.Controls
{
	[TemplatePart(Name = "PART_ERRORMESSAGE", Type = typeof(InkCanvas))]
	public class VoiceToTextBox : TextBox
	{
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
		private static readonly SolidColorBrush DefaultErrorBrush = new SolidColorBrush(Colors.Red);
		private static readonly Random randomizer = new Random();

		#region IsSpeechRecognizingCommand

		/// <summary>
		/// IsSpeechRecognizingCommand Dependency Property
		/// </summary>
		public static readonly DependencyProperty IsSpeechRecognizingCommandProperty =
			 DependencyProperty.Register("IsSpeechRecognizingCommand", typeof(ICommand), typeof(VoiceToTextBox),
				  new PropertyMetadata(null));

		/// <summary>
		/// Gets or sets the IsSpeechRecognizingCommand property. This dependency property 
		/// indicates ....
		/// </summary>
		public ICommand IsSpeechRecognizingCommand
		{
			get { return (ICommand)GetValue(IsSpeechRecognizingCommandProperty); }
			set { SetValue(IsSpeechRecognizingCommandProperty, value); }
		}

		#endregion

		/// <summary>
		/// ErrorMessage Dependency Property
		/// </summary>
		public static readonly DependencyProperty ErrorMessageProperty =
			DependencyProperty.Register(nameof(VoiceToTextBox.ErrorMessage), typeof(string), typeof(VoiceToTextBox), new PropertyMetadata(null, VoiceToTextBox.OnErrorMessageChanged));

		/// <summary>
		/// ErrorBorderBrush Dependency Property
		/// </summary>
		public static readonly DependencyProperty ErrorBorderBrushProperty =
			DependencyProperty.Register(nameof(VoiceToTextBox.ErrorBorderBrush), typeof(Brush), typeof(VoiceToTextBox),
				new PropertyMetadata(DefaultErrorBrush));

		/// <summary>
		/// UseInternalErrorMessage Dependency Property
		/// </summary>
		public static readonly DependencyProperty UseInternalErrorMessageProperty =
			DependencyProperty.Register("UseInternalErrorMessage", typeof(bool), typeof(VoiceToTextBox),
				new PropertyMetadata(true));

        #region VoiceEnabled

        /// <summary>
        /// VoiceEnabled Dependency Property
        /// </summary>
        public static readonly DependencyProperty VoiceEnabledProperty =
            DependencyProperty.Register("VoiceEnabled", typeof(bool), typeof(VoiceToTextBox),
                new PropertyMetadata((bool)true,
                    new PropertyChangedCallback(OnVoiceEnabledChanged)));

        /// <summary>
        /// Gets or sets the VoiceEnabled property. This dependency property 
        /// indicates ....
        /// </summary>
        public bool VoiceEnabled
        {
            get { return (bool)GetValue(VoiceEnabledProperty); }
            set { SetValue(VoiceEnabledProperty, value); }
        }

        /// <summary>
        /// Handles changes to the VoiceEnabled property.
        /// </summary>
      	/// <param name="d">Source dependendency o bject</param>
      	/// <param name="e">Event argument</param>
        private static void OnVoiceEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VoiceToTextBox target = (VoiceToTextBox)d;
            bool oldValue = (bool)e.OldValue;
            bool newValue = target.VoiceEnabled;
            target.OnVoiceEnabledChanged(oldValue, newValue);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the VoiceEnabled property.
        /// </summary>
      	/// <param name="oldValue">Parameter old value</param>
      	/// <param name="newValue">Paramenter new value </param>]        
		protected virtual void OnVoiceEnabledChanged(bool oldValue, bool newValue)
        {
            if(this.voiceButton!=null) this.voiceButton.Visibility = newValue ? Visibility.Visible : Visibility.Collapsed;
            this.PlaceholderText = this.VoiceEnabled ? PLACE_HOLDER_TEXT : string.Empty;
        }

        #endregion



        private Brush defaultBorderbrush;
		private TextBlock errorTextBlock;
		private string hypotesis = string.Empty;
		private Task initialization;
		private bool listening;

		private long readOnlyCalbackToken;
		private SpeechRecognizer speechRecognizer;
		private Button stopVoiceButton;
		private readonly DispatcherTimer timer;
		private Button voiceButton;

		public VoiceToTextBox()
		{
			this.DefaultStyleKey = typeof(VoiceToTextBox);
			this.timer = new DispatcherTimer();
			this.timer.Tick += this.Timer_Tick;
			this.timer.Interval = TimeSpan.FromMilliseconds(100);
			this.Unloaded += this.VoiceToTextBox_Unloaded;
		}

		/// <summary>
		/// Gets or sets the ErrorBorderBrush property. This dependency property 
		/// indicates ....
		/// </summary>
		public Brush ErrorBorderBrush
		{
			get { return (Brush) this.GetValue(ErrorBorderBrushProperty); }
			set { this.SetValue(ErrorBorderBrushProperty, value); }
		}

		/// <summary>
		/// Gets or sets the ErrorMessage property. This dependency property 
		/// indicates ....
		/// </summary>
		public string ErrorMessage
		{
			get { return (string) this.GetValue(ErrorMessageProperty); }
			set { this.SetValue(ErrorMessageProperty, value); }
		}

		/// <summary>
		/// Gets or sets the UseInternalErrorMessage property. This dependency property 
		/// indicates ....
		/// </summary>
		public bool UseInternalErrorMessage
		{
			get { return (bool) this.GetValue(UseInternalErrorMessageProperty); }
			set { this.SetValue(UseInternalErrorMessageProperty, value); }
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			this.voiceButton = this.GetTemplateChild(VOICE_BUTTON_NAME) as Button;
			this.stopVoiceButton = this.GetTemplateChild(STOP_VOICE_BUTTON_NAME) as Button;
		    this.voiceButton.Visibility = this.VoiceEnabled ? Visibility.Visible : Visibility.Collapsed;
		    this.PlaceholderText = this.VoiceEnabled ? PLACE_HOLDER_TEXT : string.Empty;

			this.initialization = this.InitializeAsync();

			this.voiceButton.Click += this.VoiceButton_Click;
			this.stopVoiceButton.Click += this.StopVoiceButton_Click;

			this.readOnlyCalbackToken = this.RegisterPropertyChangedCallback(TextBox.IsReadOnlyProperty, this.ReadOnlyCallback);
			this.errorTextBlock = this.GetTemplateChild("PART_ERRORMESSAGE") as TextBlock;
		}

		/// <summary>
		/// Provides derived classes an opportunity to handle changes to the ErrorMessage property.
		/// </summary>
		/// <param name="oldValue">Parameter old value</param>
		/// <param name="newValue">Paramenter new value </param>]        
		protected virtual void OnErrorMessageChanged(string oldValue, string newValue)
		{
			bool isError = !string.IsNullOrEmpty(newValue);
			if (isError && (this.defaultBorderbrush == null)) this.defaultBorderbrush = this.BorderBrush;
			this.BorderBrush = isError ? this.ErrorBorderBrush : this.defaultBorderbrush;
			if (isError)
			{
				if (this.errorTextBlock != null && this.UseInternalErrorMessage)
				{
					this.errorTextBlock.Text = newValue;
					this.errorTextBlock.Visibility = Visibility.Visible;
				}
			}
			else
			{
				this.errorTextBlock.Visibility = Visibility.Collapsed;
			}
		}

		private async Task InitializeAsync()
		{
			// if user haven't give permission to speec or app is running on a phone then the voice button has not to be shown
			if (await AudioCapturePermissions.RequestMicrophonePermission())
				//if (await Template10.Utils.AudioUtils.RequestMicrophonePermission() == false || DeviceUtils.Current().DeviceDisposition() == DeviceUtils.DeviceDispositions.Phone
				//    || DeviceUtils.Current().DeviceDisposition() == DeviceUtils.DeviceDispositions.Continuum)
				VisualStateManager.GoToState(this, VISUAL_STATE_VOICE_DISABLED, true);

			// if textbox is readonly there should not be possible to use voice recognition
			if (this.IsReadOnly)
			{
				this.voiceButton.IsEnabled = false;
				this.stopVoiceButton.IsEnabled = false;
			}
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
				.Select(s => s[randomizer.Next(s.Length)])
				.ToArray());
		}

		private void ReadOnlyCallback(DependencyObject sender, DependencyProperty dp)
		{
			if (dp == TextBox.IsReadOnlyProperty)
			{
				if (((TextBox) sender).IsReadOnly && this.listening == false)
				{
					this.voiceButton.IsEnabled = false;
					this.stopVoiceButton.IsEnabled = false;
				}
				else if (((TextBox) sender).IsReadOnly == false && this.listening == false)
				{
					this.voiceButton.IsEnabled = true;
					this.stopVoiceButton.IsEnabled = true;
				}
			}
		}

		private async void SpeechRecognizer_HypothesisGenerated(SpeechRecognizer sender, SpeechRecognitionHypothesisGeneratedEventArgs args)
		{
			await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				if (this.timer.IsEnabled == false)
					this.timer.Start();
				this.hypotesis = args.Hypothesis.Text;
				this.Text = args.Hypothesis.Text;
			});
		}

		private async void StopVoiceButton_Click(object sender, RoutedEventArgs e)
		{
			if (this.speechRecognizer?.State != SpeechRecognizerState.Idle)
			{
				// Cancelling recognition prevents any currently recognized speech from
				// generating a ResultGenerated event. StopAsync() will allow the final session to 
				// complete.
				try
				{
					if (this.listening) this.IsSpeechRecognizingCommand?.Execute(false);
					await this.speechRecognizer?.StopRecognitionAsync();
					//await TryDisposeSpeechAsync();
				}
				catch (Exception ex)
				{
					VisualStateManager.GoToState(this, "NotListening", true);
				}
			}
		}

		private void Timer_Tick(object sender, object e)
		{
			this.Text = this.hypotesis + " " + this.RandomString(8);
		}

		/// <summary>
		/// Tries to dispose the SpeechRecognizer object
		/// </summary>
		private async Task TryDisposeSpeechAsync()
		{
			try
			{
				if(this.listening) this.IsSpeechRecognizingCommand?.Execute(false);
				if (this.speechRecognizer != null)
				{
					this.speechRecognizer.HypothesisGenerated -= this.SpeechRecognizer_HypothesisGenerated;
					this.speechRecognizer.Dispose();
					this.speechRecognizer = null;
				}
			}
			catch
			{
				// nothig to do..
			}
			await Task.Yield();
		}

		private async void VoiceButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				// Get the top user-preferred language and its display name.
				var topUserLanguage = GlobalizationPreferences.Languages[0];
				var language = new Language(topUserLanguage);
				var displayName = language.DisplayName;

				this.listening = true;
				this.IsSpeechRecognizingCommand?.Execute(true);
				this.speechRecognizer = new SpeechRecognizer(SpeechRecognizer.SystemSpeechLanguage);
				
				var dictationConstraint = new SpeechRecognitionTopicConstraint(SpeechRecognitionScenario.WebSearch, WEB_SEARCH);
				this.speechRecognizer.Constraints.Add(dictationConstraint);
				SpeechRecognitionCompilationResult compilationResult = await this.speechRecognizer.CompileConstraintsAsync();

				// setting timeouts
				this.speechRecognizer.Timeouts.InitialSilenceTimeout = TimeSpan.FromSeconds(6.0);
				this.speechRecognizer.Timeouts.BabbleTimeout = TimeSpan.FromSeconds(4.0);
				this.speechRecognizer.Timeouts.EndSilenceTimeout = TimeSpan.FromSeconds(1.2);

				this.speechRecognizer.HypothesisGenerated += this.SpeechRecognizer_HypothesisGenerated;

				if (compilationResult.Status != SpeechRecognitionResultStatus.Success)
					return;

				VisualStateManager.GoToState(this, VISUAL_STATE_LISTENING, true);
				this.IsReadOnly = true;
				this.Text = LISTENING_TEXT;
				
				try
				{
					SpeechRecognitionResult speechRecognitionResult = await this.speechRecognizer.RecognizeAsync();
					if (speechRecognitionResult.Status == SpeechRecognitionResultStatus.Success)
					{
						// remove last chat of recognized text if it is a point (never saw a text box filled like a sentence with the point at the end)
						if (speechRecognitionResult.Text.Length > 1 && speechRecognitionResult.Text.Substring(speechRecognitionResult.Text.Length - 1, 1) == ".")
							this.Text = speechRecognitionResult.Text.Remove(speechRecognitionResult.Text.Length - 1);
						else
							this.Text = speechRecognitionResult.Text;
					}
					else
						this.Text = SPEECH_RECOGNITION_FAILED;

					this.hypotesis = string.Empty;
					this.speechRecognizer.Dispose();
				}
				catch (Exception ex)
				{
					this.IsSpeechRecognizingCommand?.Execute(false);
					const int privacyPolicyHResult = unchecked((int) 0x80045509);
					if (ex.HResult == privacyPolicyHResult)
					{
						//Handle missing privacy policy 
					}
					else
					{
						// Handle other types of errors here.
					}
				}
			}
			catch (Exception)
			{
			}
			finally
			{
				this.IsSpeechRecognizingCommand?.Execute(false);
				this.timer.Stop();
				VisualStateManager.GoToState(this, VISUAL_STATE_NOT_LISTENING, true);
				this.IsReadOnly = false;
				this.listening = false;
			}
		}

		private async void VoiceToTextBox_Unloaded(object sender, RoutedEventArgs e)
		{
			if (this.voiceButton == null) return;

			// 03/06/2016 workarround: when control is unloaded before being displayed
			// it that case this generates a null reference exception
			this?.UnregisterPropertyChangedCallback(TextBox.IsReadOnlyProperty, this.readOnlyCalbackToken);
			if (this != null)
			{
				this.Unloaded -= this.VoiceToTextBox_Unloaded;
			}

			this.voiceButton.Click -= this.VoiceButton_Click;
			this.stopVoiceButton.Click -= this.StopVoiceButton_Click;
			this.timer.Tick -= this.Timer_Tick;
			await this.TryDisposeSpeechAsync();
		}

		#region Dependency properties

		/// <summary>
		/// Handles changes to the ErrorMessage property.
		/// </summary>
		/// <param name="d">Source dependendency o bject</param>
		/// <param name="e">Event argument</param>
		private static void OnErrorMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			VoiceToTextBox target = (VoiceToTextBox) d;
			string oldValue = (string) e.OldValue;
			string newValue = target.ErrorMessage;
			target.OnErrorMessageChanged(oldValue, newValue);
		}

		#endregion
	}
}