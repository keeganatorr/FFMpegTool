﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfColorFontDialog;

namespace FFMpegTool
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{

		public FileManager fileManager;
		public WorkerManager workerManager;
		public string OutputFileType;
		Process process;
		public Stopwatch stopwatch;



		bool FirstLine = false;
		public TimeSpan VideoDuration;
		public TimeSpan VideoParsed;
		public TimeSpan TimeFromStart;
		public TimeSpan TimeRemaining;
		public double ConvertSpeed;

		public MainWindow()
		{
			InitializeComponent();
			fileManager = new FileManager();
			fileManager.SetFont(FontInfo.GetControlFont(LoadedFontFamily));
			//process = new Process();
			stopwatch = new Stopwatch();
			//workerManager = new WorkerManager(FFMpegProgress, ConsoleBox);
		}



		protected override void OnClosing(CancelEventArgs e)
		{
			// Kill all ffMpeg
			KillFFMPEG();

			base.OnClosing(e);
		}

		private void OpenFile_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Video Files(*.mp4;*.avi;*.mkv;*.gif)|*.mp4;*.avi;*.mkv;*.gif|All files (*.*)|*.*";
			openFileDialog.FilterIndex = 0;
			if (openFileDialog.ShowDialog() == true)
			{
				fileManager.LoadFile(openFileDialog.FileName);
				LoadedFileName.Text = fileManager.Filename;
				LoadedFileType.Text = fileManager.FileExt;
				WorkingDirectory.Text = fileManager.WorkingDirectory;
				OutputFileType = fileManager.FileExt;

				OutputFilenameTextbox.Text = fileManager.FilenameNoExt + "-Output";
				ResultFilenameTextbox.Text = fileManager.FilenameNoExt + "-Output" + fileManager.FileExt;
				//Duration.Text = fileManager.FileDuration;
				//OutputType.SelectedItem = OutputFileType;
				// Set initial font for picker

			}


			// Enable next UI sections
			SubtitleCB.IsEnabled = true;
			OutputFileTypeCombo.IsEnabled = true;
			CustomFilenameOutputCB.IsEnabled = true;
		}

		private void OpenFont_Click(object sender, RoutedEventArgs e)
		{
			ColorFontDialog colorFontDialog = new ColorFontDialog(false, false);
			colorFontDialog.Font = fileManager.FontInfo;
			if (colorFontDialog.ShowDialog() == true)
			{
				FontInfo font = colorFontDialog.Font;
				if (font != null)
				{
					fileManager.SetFont(font);
					LoadedFontFamily.Text = fileManager.FontInfo.Family.ToString();
					LoadedFontStyle.Text = fileManager.FontInfo.Style.ToString();
					LoadedFontWeight.Text = fileManager.FontInfo.Weight.ToString();
					LoadedFontSize.Text = fileManager.FontInfo.Size.ToString();
					LoadedFontColor.Text = fileManager.FontInfo.BrushColor.ToString();
				}
			}
		}

		private void ConvertButton_Click(object sender, RoutedEventArgs e)
		{
			if (process == null)
			{
				process = new Process();
				process.StartInfo.FileName = "ffmpeg";
				process.StartInfo.WorkingDirectory = fileManager.WorkingDirectory;

				string ffmpegArguments = $"-i {fileManager.Filename} -vf subtitles=\"f={fileManager.Filename}:si=0:force_style='FontName={fileManager.FontInfo.Family},FontSize={fileManager.FontInfo.Size},WrapStyle=2,Borderstyle=1,Outline=1,Bold={fileManager.FontBold.ToInt()},Italic={fileManager.FontItalic.ToInt()}'\" {ResultFilenameTextbox.Text} -y";
				// {fileManager.FileExt} // fix output type
				process.StartInfo.Arguments = ffmpegArguments;

				FFMpegOutputCommand.Text = "ffmpeg " + ffmpegArguments;
				//process.StartInfo.Arguments = "google.com";
				//process.StartInfo.Arguments = "/c DIR";
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.CreateNoWindow = true;
				//process.StartInfo.RedirectStandardInput = true;
				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.RedirectStandardError = true;
				//* Set your output and error (asynchronous) handlers
				process.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
				process.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);
				//* Start process and handlers
				process.Start();
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();

				//process.WaitForExit();


				//workerManager.RunWorker();
				/*Thread ffmpegProcessThread = new Thread(FFMpegProcessManager);
				ffmpegProcessThread.Start();*/

				ConvertButton.IsEnabled = false;
				CancelButton.IsEnabled = true;
				ConsoleBox.Text = "";
				stopwatch.Reset();
				stopwatch.Start();
			}
		}

		private void ExitProcess()
		{
			ConvertButton.IsEnabled = true;
			CancelButton.IsEnabled = false;
			stopwatch.Stop();
			Percentage.Text = "100";
			FFMpegProgress.Value = 100;
			process = null;
		}

		void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
		{
			//* Do your stuff with the output (write to console/log/StringBuilder)
			//ConsoleBox.Text = outLine.Data;
			string output;
			if (!FirstLine)
			{
				output = outLine.Data;
				FirstLine = true;
			}
			else
			{
				output = "\n" + outLine.Data;
			}
			//Console.WriteLine(outLine.Data);
			ConsoleBox.Dispatcher.BeginInvoke(new Action(() =>
			{
				ConsoleBox.Text += output;
				ConsoleBox.CaretIndex = ConsoleBox.Text.Length;
				ConsoleBox.ScrollToEnd();
			}), null);
			LastLine.Dispatcher.BeginInvoke(new Action(() =>
			{
				LastLine.Text = output;
			}), null);
			/*string test = output + "a";
			Time.Dispatcher.BeginInvoke(new Action(() => {
				Time.Text = test;
			}), null);*/
		}

		internal const int CTRL_C_EVENT = 0;
		[DllImport("kernel32.dll")]
		internal static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);
		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern bool AttachConsole(uint dwProcessId);
		[DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
		internal static extern bool FreeConsole();
		[DllImport("kernel32.dll")]
		static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate HandlerRoutine, bool Add);
		// Delegate type to be used as the Handler Routine for SCCH
		delegate Boolean ConsoleCtrlDelegate(uint CtrlType);

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			stopwatch.Stop();
			KillFFMPEG();
			ExitProcess();
		}

		private void KillFFMPEG()
		{
			if (process != null)
			{
				process.CancelOutputRead();
				process.CancelErrorRead();
				//https://stackoverflow.com/questions/283128/how-do-i-send-ctrlc-to-a-process-in-c
				if (AttachConsole((uint)process.Id))
				{
					SetConsoleCtrlHandler(null, true);
					try
					{
						if (!GenerateConsoleCtrlEvent(CTRL_C_EVENT, 0))
							Debug.WriteLine("false");
						process.WaitForExit();
					}
					finally
					{
						FreeConsole();
						SetConsoleCtrlHandler(null, false);
					}
					Debug.WriteLine("true");
				}
				if (!process.HasExited)
				{
					process.Kill();
				}
			}
		}

		private void LastLine_TextChanged(object sender, TextChangedEventArgs e)
		{

			// Collect Duration from start of ffmpeg output // flag with bool

			Regex RegexDuration = new Regex(@"Duration: (\d*):(\d*):(\d*)\.(\d*)");

			Match DurationMatch = RegexDuration.Match(LastLine.Text);

			string DurationFound = "";

			if (DurationMatch.Success)
			{

				int Hours = Convert.ToInt32(DurationMatch.Groups[1].Value);
				int Minutes = Convert.ToInt32(DurationMatch.Groups[2].Value);
				int Seconds = Convert.ToInt32(DurationMatch.Groups[3].Value);
				int Milliseconds = Convert.ToInt32(DurationMatch.Groups[4].Value) * 10;

				DurationFound = $"{Hours}:{Minutes}:{Seconds}:{Milliseconds}";

				Duration.Text = DurationFound;

				VideoDuration = new TimeSpan(0, Hours, Minutes, Seconds, Milliseconds);

			}

			// Collect Speed from each running line

			Regex RegexSpeed = new Regex(@"speed=(.*)x");

			Match SpeedMatch = RegexSpeed.Match(LastLine.Text);

			string SpeedFound = "";

			if (SpeedMatch.Success)
			{

				ConvertSpeed = Convert.ToDouble(SpeedMatch.Groups[1].Value);

				SpeedFound = $"{ConvertSpeed}";

				Speed.Text = SpeedFound;

			}

			// Collect time convert from each running line

			Regex RegexTime = new Regex(@"time=(\d*):(\d*):(\d*)\.(\d*)");

			Match TimeMatch = RegexTime.Match(LastLine.Text);

			string TimeFound = "";

			if (TimeMatch.Success)
			{

				int Hours = Convert.ToInt32(TimeMatch.Groups[1].Value);
				int Minutes = Convert.ToInt32(TimeMatch.Groups[2].Value);
				int Seconds = Convert.ToInt32(TimeMatch.Groups[3].Value);
				int Milliseconds = Convert.ToInt32(TimeMatch.Groups[4].Value) * 10;

				VideoParsed = new TimeSpan(0, Hours, Minutes, Seconds, Milliseconds);

				TimeFromStart = new TimeSpan(0, stopwatch.Elapsed.Hours, stopwatch.Elapsed.Minutes, stopwatch.Elapsed.Seconds, stopwatch.Elapsed.Milliseconds);

				TimeRemaining = (VideoDuration - VideoParsed).Divide(ConvertSpeed);

				TimeSpan TotalConversionTime = VideoDuration.Divide(ConvertSpeed);


				TimeFound = $"Video Time Parsed: {VideoParsed}\n" +
							$"Current Running Time: {TimeFromStart}\n" +
							$"Time Remaining: {TimeRemaining}\n" +
							$"TotalConvertTime: {TotalConversionTime}\n";

				// estimate total conversion time

				decimal percentLeft = (1 - TimeRemaining.Divide(TotalConversionTime)) * 100;

				Percentage.Text = percentLeft.ToString();

				FFMpegProgress.Value = Convert.ToDouble(percentLeft);

			}

			Time.Text = TimeFound;

			if (process != null)
			{
				if (process.HasExited)
				{
					ExitProcess();
				}
			}

		}

		private void CustomFilenameOutputCB_Click(object sender, RoutedEventArgs e)
		{
			OutputFilenameTextbox.IsEnabled = CustomFilenameOutputCB.IsChecked.Value;
			if (!CustomFilenameOutputCB.IsChecked.Value)
			{
				OutputFilenameTextbox.Text = fileManager.FilenameNoExt + "-Output";
				if (OutputFileTypeCombo.SelectedItem != null)
				{
					ResultFilenameTextbox.Text = OutputFilenameTextbox.Text + ((ComboBoxItem)OutputFileTypeCombo.SelectedItem).Content.ToString();
				}
			}
		}

		private void OutputFileTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!CustomFilenameOutputCB.IsChecked.Value)
			{
				OutputFilenameTextbox.Text = fileManager.FilenameNoExt + "-Output";
				if (OutputFileTypeCombo.SelectedItem != null)
				{
					ResultFilenameTextbox.Text = OutputFilenameTextbox.Text + ((ComboBoxItem)OutputFileTypeCombo.SelectedItem).Content.ToString();
				}
			}
			else
			{
				ResultFilenameTextbox.Text = OutputFilenameTextbox.Text + ((ComboBoxItem)OutputFileTypeCombo.SelectedItem).Content.ToString();
			}
			//OutputFilenameTextbox.Text = ;
			CheckConvertButtonReady();
		}

		private void OutputFilenameTextbox_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (OutputFileTypeCombo.SelectedItem != null)
			{
				ResultFilenameTextbox.Text = OutputFilenameTextbox.Text + ((ComboBoxItem)OutputFileTypeCombo.SelectedItem).Content.ToString();
			}
		}

		private void CheckConvertButtonReady()
		{
			if (fileManager.Loaded)
			{
				if (OutputFileTypeCombo.SelectedItem != null)
				{
					ConvertButton.IsEnabled = true;
				}
				else
					ConvertButton.IsEnabled = false;
			}
			else
				ConvertButton.IsEnabled = false;
		}

		private void SubtitleCB_Click(object sender, RoutedEventArgs e)
		{
			OpenFont.IsEnabled = SubtitleCB.IsChecked.Value;
		}

		private void LoadedFontStyle_TextChanged(object sender, TextChangedEventArgs e)
		{
			switch (fileManager.FontInfo.Style.ToString())
			{
				case "Oblique":
				case "Italic":
					fileManager.FontItalic = true;
					break;
				case "Normal":
				default:
					fileManager.FontItalic = false;
					break;
			}
		}

		private void LoadedFontWeight_TextChanged(object sender, TextChangedEventArgs e)
		{
			switch (fileManager.FontInfo.Weight.ToString())
			{
				case "SemiBold":
				case "Bold":
				case "Black":
					fileManager.FontBold = true;
					break;
				default:
					fileManager.FontBold = false;
					break;
			}
		}
	}

	public static class TimeSpanExtension
	{
		/// <summary>
		/// Multiplies a timespan by an integer value
		/// </summary>
		public static TimeSpan Multiply(this TimeSpan multiplicand, int multiplier)
		{
			return TimeSpan.FromTicks(multiplicand.Ticks * multiplier);
		}

		/// <summary>
		/// Multiplies a timespan by a double value
		/// </summary>
		public static TimeSpan Multiply(this TimeSpan multiplicand, double multiplier)
		{
			return TimeSpan.FromTicks((long)(multiplicand.Ticks * multiplier));
		}

		/// <summary>
		/// Divides a timespan by an integer value
		/// </summary>
		public static TimeSpan Divide(this TimeSpan multiplicand, int divider)
		{
			return TimeSpan.FromTicks(multiplicand.Ticks / divider);
		}

		/// <summary>
		/// Divides a timespan by an integer value
		/// </summary>
		public static decimal Divide(this TimeSpan multiplicand, TimeSpan divider)
		{
			return ((decimal)multiplicand.Ticks / (decimal)divider.Ticks);
		}

		/// <summary>
		/// Divides a timespan by a double value
		/// </summary>
		public static TimeSpan Divide(this TimeSpan multiplicand, double divider)
		{
			return TimeSpan.FromTicks((long)(multiplicand.Ticks / divider));
		}
	}

	public static class BooleanExtention
	{
		/// <summary>
		/// Multiplies a timespan by an integer value
		/// </summary>
		public static int ToInt(this bool boolean)
		{
			if (boolean) return 1;
			else return 0;
		}
	}
}
