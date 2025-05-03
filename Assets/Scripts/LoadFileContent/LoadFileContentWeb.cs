using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace Assets.Scripts.LoadFileContent
{
	public class LoadFileContentWeb : ILoadFileContent
	{
		Dictionary<string, List<string>> WebDirectoryContentCache = new Dictionary<string, List<string>>();


		public IEnumerable<string> ReadLines(string filePath, Encoding encoding = null)
		{
			var request = GetWebFileContent(System.IO.Path.Combine(filePath));
			while (request.MoveNext())
			{
				if (request.Current != null)
				{
					string result = request.Current as string;
					var reader = new StringReader(result);
					string line;
					while ((line = reader.ReadLine()) != null)
					{
						yield return line;
					}
					reader.Dispose();
				}
				else
				{
					Thread.Sleep(1);
				}
			}
		}

		public List<FileDir> GetDirectory(string pathOnly, string searchPattern)
		{
			throw new NotImplementedException();
		}



		private IEnumerator GetWebFileContent(string filePath)
		{
			/// Special case to access StreamingAsset content on Android and Web
			UnityWebRequest request = UnityWebRequest.Get(filePath);
			request.SendWebRequest();
			while (!request.isDone)
				yield return null;
			string jsonData = string.Empty;
			if (request.result == UnityWebRequest.Result.Success)
			{
				jsonData = request.downloadHandler.text;
			}
			yield return jsonData;
		}
	}
}
