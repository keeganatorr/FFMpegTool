using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfColorFontDialog;

namespace FFMpegTool
{
	public class FileManager
	{
		public bool Loaded = false;
		public string FullFilename;
		public string Filename;
		public string FilenameNoExt;
		public string FileExt;
		public string WorkingDirectory;
		public string outputFileType;
		public FontInfo FontInfo;
		public string FileDuration;

		public bool FontBold = false;
		public bool FontItalic = false;

		public FileManager()
		{
			FullFilename = "";
			Filename = "";
			FilenameNoExt = "";
			FileExt = "";
			WorkingDirectory = "";
			outputFileType = "";
		}

		public void LoadFile(string filename)
		{
			FullFilename = filename;
			int filenameLength = FullFilename.LastIndexOf("\\") + 1;
			Filename = FullFilename.Substring(filenameLength, FullFilename.Length - filenameLength);
			WorkingDirectory = FullFilename.Substring(0, filenameLength);
			int FilenameExtPoint = Filename.LastIndexOf(".");
			FilenameNoExt = Filename.Substring(0, FilenameExtPoint);
			FileExt = Filename.Substring(FilenameExtPoint, Filename.Length - FilenameExtPoint);
			Loaded = true;
		}

		public void SetFont(FontInfo fontInfo)
		{
			FontInfo = fontInfo;
		}
	}
}
