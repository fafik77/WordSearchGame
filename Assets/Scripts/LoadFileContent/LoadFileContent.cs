using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Assets.Scripts.LoadFileContent
{
	public struct LoadFileLikeExact
	{
		/// <summary>
		/// Wildcards for Local: (*.*) (*.txt)
		/// </summary>
		public string like;
		public string exact;
		/// <summary>
		/// Apply regex filtering after getting files
		/// </summary>
		public Regex matchFile;
	}
	/// <summary>
	/// Caches Web requests
	/// </summary>
	public class LoadFileContent
	{
		/// <summary>
		/// Cached Web request
		/// </summary>
		LoadFileContentWeb loadFileContentWebCached = new LoadFileContentWeb();

		/// <summary>
		/// Automaticly chooses handler based on provided path.
		/// Reads up to all lines in given file
		/// </summary>
		/// <param name="filePath">Directory in which file resides?</param>
		/// <param name="encoding">UTF-8 By default</param>
		/// <returns>One Line per request</returns>
		public IEnumerable<string> ReadLines(string filePath, Encoding encoding = null)
		{
			ILoadFileContent loadFileContentInterface = ChooseInterfaceForGet(filePath);

			foreach(var line in loadFileContentInterface.ReadLines(filePath, encoding))
			{
				yield return line;
			}
		}

		/// <param name="filePath">path containing key words:[jar, http] is considered Web</param>
		/// <returns>Interface for Web/Local</returns>
		ILoadFileContent ChooseInterfaceForGet(string filePath)
		{
			ILoadFileContent loadFileContentInterface;
			if (filePath.StartsWith("jar") || filePath.StartsWith("http"))
			{
				/// Special case to access StreamingAsset content on Android and Web
				loadFileContentInterface = loadFileContentWebCached;
			}
			else
			{
				/// This is a regular file path on most platforms and in playmode of the editor
				LoadFileContentLocal loadFileContent = new LoadFileContentLocal();
				loadFileContentInterface = loadFileContent;
			}
			return loadFileContentInterface;
		}



		/// <summary>
		/// Reads up to all lines in given file(s)
		/// </summary>
		/// <param name="pathOnly">Directory in which file resides?</param>
		/// <param name="fileName">Exact or like = wildard file type, matchFile = match only certain files</param>
		/// <param name="encoding">UTF-8 By default</param>
		/// <returns>One Line per request</returns>
		public IEnumerable<string> ReadLinesFromMultipleFiles(string pathOnly, LoadFileLikeExact fileName, Encoding encoding = null)
		{
			ILoadFileContent loadFileContentInterface = ChooseInterfaceForGet(pathOnly);

			if (fileName.exact != null && fileName.exact.Length != 0)
			{   ///one file
				foreach (var line in loadFileContentInterface.ReadLines(Path.Combine(pathOnly, fileName.exact), encoding))
				{
					yield return line;
				}
			}
			else
			{   ///more files
				var files = loadFileContentInterface.GetDirectory(pathOnly, fileName.like);
				Regex matcher;
				if (fileName.matchFile != null) matcher = fileName.matchFile;
				else matcher = new Regex(".*");

				foreach (var file in files)
				{
					if (!matcher.Match(file).Success) continue;
					foreach (var line in loadFileContentInterface.ReadLines(file, encoding))
					{
						yield return line;
					}
				}
			}
		}
		/// <summary>
		/// Reads up to all lines in given file or wildard files
		/// </summary>
		/// <param name="filePath">Full path to file with file (might fail when providing file likeness)</param>
		/// <returns>One Line per request</returns>
		public IEnumerable<string> ReadLinesLikeExactFile(string filePath, Encoding encoding = null)
		{
			string path = System.IO.Path.GetDirectoryName(filePath);
			string filename = System.IO.Path.GetFileName(filePath);
			LoadFileLikeExact loadFileLikeExact = new LoadFileLikeExact();
			if (filename.Contains('*')) loadFileLikeExact.like = filename;
			else loadFileLikeExact.exact = filename;
			foreach(var line in ReadLinesFromMultipleFiles(path, loadFileLikeExact, encoding)) {  yield return line; }
		}

		public List<string> GetDirectory(string pathOnly, string searchPattern = null)
		{
			ILoadFileContent loadFileContentInterface = ChooseInterfaceForGet(pathOnly);
			return loadFileContentInterface.GetDirectory(pathOnly, searchPattern);
		}
	}
}
