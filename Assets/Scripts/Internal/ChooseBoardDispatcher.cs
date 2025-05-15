using Assets.Scripts.LoadFileContent;
using Assets.UI_Toolkit.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace Assets.Scripts.Internal
{
	public class ChooseBoardDispatcher
	{
		protected Dictionary<string, List<string>> LangDictOfWords = new Dictionary<string, List<string>>();
		protected Dictionary<string, CategoryOrGroup> LangDictOfCategories = new Dictionary<string, CategoryOrGroup>();
		LoadFileContent.LoadFileContent loadFileContent = new();
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
			List<string> lines = new List<string>();
			foreach (var line in loadFileContent.ReadLines(fileName, Encoding.UTF8))
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

		public CategoryOrGroup CategoriesInCurrLang { get; private set; }
		public IList<TreeViewItemData<ICategoryOrGroup>> GetCategoriesRootsForLang(string lang)
		{
			lang = lang.ToLower();
			var categories = LangDictOfCategories.GetOrCreate(lang);
			CategoriesInCurrLang = categories;
			int id = 0;
			if (categories.HasAnyContent == false)
			{
				string path = System.IO.Path.Combine(Application.streamingAssetsPath, "Categories", lang);
				LoadCategoriesRecursiveForPath(path, ref categories);
			}
			CategoriesInCurrLang = categories;
			return categories.GetRoots(ref id);
		}

		void LoadCategoriesRecursiveForPath(string path, ref CategoryOrGroup into)
		{
			foreach (var item in loadFileContent.GetDirectory(path, "*.txt"))
			{
				if (item.IsDirectory)
				{
					string nameOnly = System.IO.Path.GetFileName(item.Name);
					if (into.SubCategories == null) into.SubCategories = new();
					CategoryOrGroup subCategory = new CategoryOrGroup(nameOnly);
					LoadCategoriesRecursiveForPath(item.Name, ref subCategory);
					into.SubCategories.Add(subCategory);
				}
				else
				{
					List<string> words = new List<string>();
					foreach (var line in loadFileContent.ReadLines(item.Name))
					{
						var lineTrim = line.Trim().ToLower();
						if (lineTrim.Length > 1)
						{
							words.Add(lineTrim);
						}
					}
					string nameOnly = System.IO.Path.GetFileNameWithoutExtension(item.Name);
					if (into.Categories == null) into.Categories = new();
					into.Categories.Add(new CategoryOnly(nameOnly, words));
				}
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
		/// <param name="amountMax">Limit maximum amount of words to</param>
		/// <returns>amount of words found</returns>
		public int LoadProvidedWords(string words, int amountMax = -1)
		{
			var wordList = TokenizeProvidedWords(words);
			return LoadProvidedWords(wordList, amountMax);
		}
		/// <summary>
		/// tries to make a board out of provided wordList
		/// </summary>
		/// <param name="wordList">List composed of words</param>
		/// <param name="amountMax">Limit maximum amount of words to</param>
		/// <returns>amount of words</returns>
		public int LoadProvidedWords(List<string> wordList, int amountMax = -1)
		{
			if (wordList == null || wordList.Count == 0) return 0;
			PredefinedBoard2D = null;


			if (amountMax > 0)
			{
				var totalWords = wordList.Count;
				if (totalWords <= amountMax)
				{
					WordsOnBoard = wordList;
				}
				else
				{
					if (WordsOnBoard == null) WordsOnBoard = new();
					WordsOnBoard.Clear();
					System.Random random = new System.Random(Guid.NewGuid().GetHashCode());
					var amount = random.Next(10, amountMax);
					for (int i = 0; i != amount; ++i)
						WordsOnBoard.Add(wordList[random.Next(0, totalWords)]);
				}
			}
			else
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
			int colsInLineWidth = 0;
			List<string> BoardRowsHeight = new List<string>();
			var rows = lines.Skip(1);
			Regex lettersReg = new("\\w", RegexOptions.IgnoreCase);
			foreach (var row in rows)
			{
				///skip empty lines
				if (row.Length < 3) continue;
				///process each line extracting only letters?
				StringBuilder thisRowCharsBuilder = new StringBuilder(row.Length);
				foreach (Match LetterInWord in lettersReg.Matches(row))
				{
					thisRowCharsBuilder.Append(LetterInWord.Value);
				}
				if (colsInLineWidth == 0)
					colsInLineWidth = thisRowCharsBuilder.Length;
				else if (thisRowCharsBuilder.Length != colsInLineWidth) ///error cols dont match
				{
					throw new ArrayTypeMismatchException($"Column lenght mismatch, starting at line {BoardRowsHeight.Count}");
				}
				BoardRowsHeight.Add(thisRowCharsBuilder.ToString().ToLower());
			}
			///save each char into the board
			PredefinedBoard2D = new char[colsInLineWidth, BoardRowsHeight.Count];
			for (int i = 0; i != BoardRowsHeight.Count; ++i)
			{
				for (int ii = 0; ii != colsInLineWidth; ++ii)
				{
					PredefinedBoard2D[ii,i] = BoardRowsHeight[i][ii]; //reverse the order couse thats a thing
				}
			}
			var words = TokenizeProvidedWords(lines[0]);
			WordsOnBoard = words.Select(x => x.ToLower()).ToList();
			ApplyBoard();
			return true;
		}





		private List<string> GetWordListInLang(string lang)
		{
			lang = lang.ToLower();
			var words = LangDictOfWords.GetOrCreate(lang);
			if (words.Count == 0)
			{
				string pathOnly = System.IO.Path.Combine(Application.streamingAssetsPath, "Dictionary");
				LoadFileLikeExact fileLike = new LoadFileLikeExact() { like = lang + " *.txt", matchFile = new Regex(lang + ".*\\.txt") };

				foreach(var line in loadFileContent.ReadLinesFromMultipleFiles(pathOnly, fileLike))
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
