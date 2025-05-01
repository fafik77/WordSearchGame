using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Assets.Scripts.Internal
{
	public class ChooseBoardDispatcher
	{
		protected Dictionary<string, List<string>> LangDictOfWords = new Dictionary<string, List<string>>();
		public bool WaitingForApply { get; set; }
		public string Lang;
		public List<string> WordsOnBoard;
		public char[,] PredefinedBoard2D;

		/// <summary>
		/// Transforms provided data into Board
		/// </summary>
		protected void ApplyBoard()
		{
			Singleton.SceneMgr.SwitchToScene("Assets/Scenes/GameScene.unity");
			WaitingForApply = true;
			Singleton.boardUiEvents.CreateBoard(predefined: PredefinedBoard2D != null); //is it random or predefined ?
		}

		/// <summary>
		/// Chooses random words in random amount[10,16) in the provided language
		/// </summary>
		/// <param name="lang"></param>
		/// <param name="maxWordLen"></param>
		/// <returns>Amount of words chosen</returns>
		public int CreateRandom(string lang, int maxWordLen)
		{
			PredefinedBoard2D = null;
			var words = GetWordListInLang(lang);
			if (maxWordLen < 3) maxWordLen = 3;


			System.Random random = new System.Random(Guid.NewGuid().GetHashCode());
			var amount = random.Next(10, 16);
			
			List<string> wordsChosen = new List<string>();
			var wordsNoLongerThan = words.Where(w => { return w.Length <= maxWordLen; }).ToList();
			var totalWords = wordsNoLongerThan.Count;

			for (int i = 0; i != amount; ++i)
			{
				wordsChosen.Add(wordsNoLongerThan[random.Next(0, totalWords)]);
			}
			WordsOnBoard = wordsChosen;
			ApplyBoard();
			return wordsChosen.Count;
		}


		/// <summary>
		/// Loads a file where first line is words separated by space or comma, [other lines: row x col]
		/// </summary>
		/// <param name="fileName">File to load</param>
		/// <returns>true on success</returns>
		public bool LoadFromFile(string fileName)
		{
			LoadFileContent loadFileContent = new LoadFileContent();
			List<string> lines = new List<string>();
			foreach (var line in loadFileContent.ReadLineLikeExactFile(fileName, Encoding.UTF8))
			{
				lines.Add(line);
			}
			if (lines.Count > 1) //predefined board
			{
				return Load2DPredefinedBoard(lines);
			}
			else //only words
			{
				return LoadProvidedWords(lines[0]) != 0;
			}
		}


		/// <summary>
		/// Tokenizes Str:words into a list of words
		/// </summary>
		/// <param name="words">Words separated by non letter (\w+)</param>
		/// <returns>List of tokenized words(\w+)</returns>
		public List<string> TokenizeProvidedWords(string words)
		{
			Regex regexWord = new("\\w+", RegexOptions.IgnoreCase);
			List<string> wordList = new List<string>();
			foreach (Match word in regexWord.Matches(words))
			{
				wordList.Add(word.Value);
			}
			return wordList;
		}

		/// <summary>
		/// Tokenizes Str:words into a list of words and tries to make a board out of them
		/// </summary>
		/// <param name="words">Words separated by non letter (\w+)</param>
		/// <returns>amount of words found</returns>
		public int LoadProvidedWords(string words)
		{
			var wordList = TokenizeProvidedWords(words);
			return LoadProvidedWords(wordList);
		}
		/// <summary>
		/// tries to make a board out of provided wordList
		/// </summary>
		/// <param name="wordList">List composed of words</param>
		/// <returns>amount of words</returns>
		public int LoadProvidedWords(List<string> wordList)
		{
			if (wordList == null || wordList.Count == 0) return 0;
			PredefinedBoard2D = null;
			WordsOnBoard = wordList;
			ApplyBoard();
			return wordList.Count;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="lines">First line contains all words</param>
		/// <returns></returns>
		public bool Load2DPredefinedBoard(List<string> lines)
		{
			int colAmount = 0;
			List<string> BoardRows = new List<string>();
			var rows = lines.Skip(1);
			foreach (var row in rows)
			{
				if (colAmount != 0)
				{
					if (row.Length == colAmount) BoardRows.Add(row);
					else if (row.Trim().Length == colAmount) BoardRows.Add(row.Trim());
					else 
						throw new ArrayTypeMismatchException($"Column lenght mismatch, starting at line {BoardRows.Count}");
				}
				else
					BoardRows.Add(row);
			}
			var wordsList = TokenizeProvidedWords(lines[0]);
			return false;
		}





		private List<string> GetWordListInLang(string lang)
		{
			lang = lang.ToLower();
			var words = LangDictOfWords.GetOrCreate(lang);
			if (words.Count == 0)
			{
				string pathOnly = System.IO.Path.Combine(Application.streamingAssetsPath, "Dictionary");
				LoadFileContent loadFileContent = new LoadFileContent();
				LoadFileLikeExact fileLike = new LoadFileLikeExact() { like = lang + " *.txt" };

				
				//var path = AppContext.BaseDirectory;	//unity editor path
				//path = Application.persistentDataPath; //for per user config
				//var path = Application.dataPath;        // (project)/Assets
				//path = Path.Combine(path, "Data");
				//foreach (var file in Directory.GetFiles(pathOnly, lang + " *.txt"))
				//{
				//	foreach(var line in File.ReadLines(file, Encoding.UTF8))
				//	{
				//		var lineTrim = line.Trim();
				//		if (lineTrim.Length > 2)
				//			words.Add(lineTrim);
				//	}
				//}
				foreach(var line in loadFileContent.ReadLine(pathOnly, fileLike))
				{
					var lineTrim = line.Trim();
					if (lineTrim.Length > 2)
						words.Add(lineTrim);
				}
			}
			return words;
		}
	}
}
