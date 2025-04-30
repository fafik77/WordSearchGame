using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace Assets.Scripts.Internal
{
	public struct LoadFileLikeExact
	{
		public string like;
		public string exact;
	}
	public class LoadFileContent
	{
		/// <summary>
		/// Reads up to all lines in given file
		/// </summary>
		/// <param name="pathOnly">Directory in which file resides?</param>
		/// <param name="fileName">Exact or wildard file name</param>
		/// <returns>One Line per request</returns>
		public IEnumerable<string> ReadLine(string pathOnly, LoadFileLikeExact fileName, Encoding encoding = null)
		{
			if (pathOnly.StartsWith("jar") || pathOnly.StartsWith("http"))
			{
				// Special case to access StreamingAsset content on Android and Web
				GetWebDirectoryContent(pathOnly, fileName);
				//GetWebFileContent(pathOnly, fileName);
			}
			else
			{
				if (encoding == null) encoding = Encoding.UTF8;
				foreach (var file in GetLocalDirectoryContent(pathOnly, fileName))
				{	//get all files that match
					// This is a regular file path on most platforms and in playmode of the editor
					foreach (var line in System.IO.File.ReadLines(file, encoding))
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
		public IEnumerable<string> ReadLineLikeExactFile(string filePath, Encoding encoding = null)
		{
			string path = System.IO.Path.GetDirectoryName(filePath);
			string filename = System.IO.Path.GetFileName(filePath);
			LoadFileLikeExact loadFileLikeExact = new LoadFileLikeExact();
			if (filename.Contains('*')) loadFileLikeExact.like = filename;
			else loadFileLikeExact.exact = filename;
			foreach(var line in ReadLine(path, loadFileLikeExact, encoding)) {  yield return line; }
		}


		private string GetWebFileContent(string filePath)
		{
			throw new NotImplementedException();
			// Special case to access StreamingAsset content on Android and Web
			//UnityWebRequest request = UnityWebRequest.Get(pathOnly);
			//yield return request.SendWebRequest();

			//if (request.result == UnityWebRequest.Result.Success)
			//{
			//    jsonData = request.downloadHandler.text;
			//}
		}
		private List<string> GetWebDirectoryContent(string pathOnly, LoadFileLikeExact filterFile)
		{
			throw new NotImplementedException();
		}

		private List<string> GetLocalDirectoryContent(string pathOnly, LoadFileLikeExact filterFile)
		{
			if(filterFile.exact != null && filterFile.exact.Length != 0)
			{
				List<string> files = new List<string>();
				files.Add(System.IO.Path.Combine(pathOnly, filterFile.exact));
				return files;
			}
			return System.IO.Directory.GetFiles(pathOnly, filterFile.like).ToList();
		}


	}
}
