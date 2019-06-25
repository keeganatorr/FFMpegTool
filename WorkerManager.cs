using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FFMpegTool
{
	public class WorkerManager
	{
		public BackgroundWorker backgroundWorker;
		private ProgressBar progressBar;
		private TextBox textBox;

		private string ConsoleOutput;

		public WorkerManager(ProgressBar progressBarInput, TextBox textBoxInput)
		{
			if(backgroundWorker == null)
			{
				backgroundWorker = new BackgroundWorker();
				backgroundWorker.DoWork += BackgroundWorker_DoWork;
				backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
				backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
				backgroundWorker.WorkerReportsProgress = true;
				backgroundWorker.WorkerSupportsCancellation = true;
				progressBar = progressBarInput;
				textBox = textBoxInput;
				ConsoleOutput = "";
			}
		}

		public void RunWorker()
		{
			backgroundWorker.RunWorkerAsync();
		}

		private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			progressBar.Value = e.ProgressPercentage;
			textBox.Text = ConsoleOutput;
			//throw new NotImplementedException();
		}

		private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			//throw new NotImplementedException();
		}

		private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			FFMpegProcessManager();
			backgroundWorker.ReportProgress(50);
			//throw new NotImplementedException();
		}

		private void FFMpegProcessManager()
		{
			Process process;
			process = new Process();
			process.StartInfo.FileName = @"C:\ProgramData\chocolatey\bin\ffmpeg.exe";

			process.StartInfo.UseShellExecute = false;
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.RedirectStandardOutput = true;


			process.StartInfo.RedirectStandardInput = true;


			//process.
			process.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);

			// Start the process.
			process.Start();

			// Start the asynchronous read
			process.BeginOutputReadLine();

			process.WaitForExit();


		}

		void OutputHandler(object sender, DataReceivedEventArgs e)
		{
			//Trace.WriteLine(e.Data);
			ConsoleOutput = e.Data;
			//WriteToTextBox(e.Data);
		}
	}
}
