using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.LoadFileContent
{
	public class LoadFileContentLocal : ILoadFileContent
	{
		public List<FileDir> GetDirectory(string pathOnly, string searchPattern = null)
		{
			if (searchPattern == null || searchPattern.Length == 0)
				searchPattern = new string("*.*");
			List<FileDir> entries = new List<FileDir>();
			entries.AddRange(System.IO.Directory.GetDirectories(pathOnly).Select(x => new FileDir { Name = x, IsDirectory = true }));
			entries.AddRange(System.IO.Directory.GetFiles(pathOnly, searchPattern).Select(x => new FileDir { Name = x, IsDirectory = false }));
			return entries;
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
