using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.LoadFileContent
{
	public class LoadFileContentLocal : ILoadFileContent
	{
		public List<string> GetDirectory(string pathOnly, string searchPattern = null)
		{
			if (searchPattern == null || searchPattern.Length == 0)
				searchPattern = "*.*";
			return System.IO.Directory.GetFiles(pathOnly, searchPattern).ToList();
		}

		public IEnumerable<string> ReadLines(string filePath, Encoding encoding = null)
		{
			if (encoding == null)
				encoding = Encoding.UTF8;
			foreach (var line in System.IO.File.ReadLines(filePath, encoding))
			{
				yield return line;
			}
		}
	}
}
