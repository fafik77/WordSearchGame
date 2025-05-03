using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.LoadFileContent
{
	public interface ILoadFileContent
	{
		/// <summary>
		/// Reads up to all lines in given file
		/// </summary>
		/// <param name="filePath">Directory in which file resides?</param>
		/// <param name="encoding">UTF-8 By default</param>
		/// <returns>One Line per request</returns>
		public IEnumerable<string> ReadLines(string filePath, Encoding encoding = null);
		/// <summary>
		/// Gets files and folders in directory
		/// </summary>
		/// <param name="pathOnly">Directory to search in</param>
		/// <param name="searchPattern">find matching files</param>
		/// <returns>List of files</returns>
		public List<FileDir> GetDirectory(string pathOnly, string searchPattern = null);
	}
}
