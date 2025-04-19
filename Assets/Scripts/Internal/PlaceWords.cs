using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BoardContent;
using Exceptions;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;

namespace BoardContent
{
	enum WordOrientationEnum
	{
		horizontal = 0,
		vertical = 1,
		diagonal = 2,
		horizontalBack = 3,
		verticalBack = 4,
		diagonalBack = 5,
	}
	struct WordPlaceOk
	{
		public int xmod, ymod;
		public bool ok;
	}
	class PlaceWords
	{
		int maxRetries = 100;
		int width, height;
		/// <summary>
		/// keep board aspect ratio (def. 14 x 9 reserving 2 x 9 for UI)
		/// </summary>
		public Vector2 AspectRatio = new Vector2(14, 9);
		/// <summary>
		/// board will not get any smaller than this
		/// </summary>
		public Vector2 MinimumBoardSize;
		List<string> words;
		public Vector2 MinimumRequiredBoardSize { get; private set; }
		/// <summary>
		/// adds Additional dummy Chars to the board size to make the search for words harder
		/// the formula is += letters * AdditionalCharsPercent
		/// </summary>
		float AdditionalCharsPercent = 0f;
		/// <summary>
		/// this could be an option for the player to entice Words to share Letter Tile
		/// </summary>
		public bool PrefferWordsShareLetters;



		SortedDictionary<string, List<WordContainedFrom>> wordsContained;
		public int WordsRemoved { get; private set; }



		public LetterTileScript[,] TilesSript2D {  get; private set; }
		//char[,] tiles2DDummy;
		PlaceWordsOnBoardReturns finalBoard;


		protected PlaceWords(LetterTileScript[,] tilesSript2D)
		{
			this.TilesSript2D = tilesSript2D;
			width = tilesSript2D.GetLength(0);
			height = tilesSript2D.GetLength(1);
			//tiles2DDummy = new char[width, height];
		}

		/// <summary>
		/// Calculates minimum requirements & separates out the contained duplicates
		/// </summary>
		/// <param name="words">In: List of words</param>
		/// <param name="AspectRatio">by default (14:9)</param>
		/// <param name="CreateBoardAtLeast">Pass in the function that makes the board</param>
		/// <param name="wordsInReverse">if true ContainedDuplicate will also match palindroms</param>
		public PlaceWords(List<string> words, Vector2 AspectRatio, Func<int, int, LetterTileScript[,]> CreateBoardAtLeast, bool wordsInReverse = true, float AdditionalCharsPercent=0f)
		{
			if (AspectRatio != null)
				this.AspectRatio = AspectRatio;
			this.AdditionalCharsPercent = AdditionalCharsPercent;
			WordsRemoved = SepareteContainedDuplicateWords(ref words, out wordsContained, out var WordsLeft, wordsInReverse);
			this.words = words;
			MinimumRequiredBoardSize = CalculateMinBoardDims(ref words);
			TilesSript2D = CreateBoardAtLeast(((int)MinimumRequiredBoardSize.x), ((int)MinimumRequiredBoardSize.y));
			width = TilesSript2D.GetLength(0);
			height = TilesSript2D.GetLength(1);
			//tiles2DDummy = new char[width, height];
		}

		public int PlaceWordsOnBoardThreaded(int wordPlaceMaxRetry = 100, int maxThreads = 8)
		{
			maxRetries = wordPlaceMaxRetry;
			if (maxThreads < 1) maxThreads = 1;
			Singleton.wordList.Reset();
			//use the old C like thread wrappers
			List<PlaceWordsOnBoardThrWrapper> triedBoards = new List<PlaceWordsOnBoardThrWrapper>(maxThreads);
			List<Thread> threads = new List<Thread>(maxThreads);
			for (int i = 0; i != maxThreads; i++)
			{
				triedBoards.Add(new PlaceWordsOnBoardThrWrapper(this));
				Thread thread = new(new ThreadStart(triedBoards[i].Run));
				thread.Start();
				threads.Add(thread);
			}

			foreach (var thread in threads)
			{
				thread.Join();
			}
			triedBoards.OrderBy(x => x.boardTry.wordsTimedout);


			finalBoard = triedBoards[0].boardTry;

			//write onto the screen
			var iterSrc = finalBoard.tiles2DDummy.GetEnumerator();
			var iterDst = TilesSript2D.GetEnumerator();
			while (iterSrc.MoveNext() && iterDst.MoveNext())
			{
				(iterDst.Current as LetterTileScript).Letter = (char)iterSrc.Current;
			}

			return 0x00;
		}

		struct PlaceWordsOnBoardReturns
		{
			public int wordsTimedout;
			public char[,] tiles2DDummy;
			public List<WordListEntry> wordList;
			public PlaceWordsOnBoardReturns(int width, int height)
			{
				wordsTimedout = 0;
				tiles2DDummy = new char[width, height];
				wordList = new List<WordListEntry>();
			}
		}

		class PlaceWordsOnBoardThrWrapper
		{
			public PlaceWordsOnBoardThrWrapper(PlaceWords placeWords)
			{
				this.placeWords = placeWords;
				boardTry = new PlaceWordsOnBoardReturns();
			}
			public PlaceWords placeWords;
			public PlaceWordsOnBoardReturns boardTry;
			public void Run()
			{
				Debug.Log($"Starting {Thread.CurrentThread.ManagedThreadId}");
				boardTry = placeWords.PlaceWordsOnBoard();
			}
		}
		PlaceWordsOnBoardReturns PlaceWordsOnBoard()
		{
			//Singleton.wordList.Reset();
			//List<string> words = new List<string>() { "barbara", "ania", "Olaf", "kamil", "ola", "slimak", "Ania" };
			//SortedDictionary<string, List<WordContainedFrom>> wordsContained;
			//before applying that list to board, make sure to sort it from longest to shortest..
			// remove the short words contained in longer ones (only add their positions)..
			// one letter words are not accepted, (2 letters words should not exist)
			//int removedWords = SepareteContainedDuplicateWords(ref words, out var wordsContained, out var WordsLeft, wordsInReverse: true);
			//Vector2 vector2 = CalculateMinBoardDims(ref words);

			//not so simple, if we fail to put the word on board it can not be included here then
			//Singleton.wordList.wordsToFind = WordsLeft;

			///Change This to Be threaded potentially?, try on multiple threads
			System.Random random = new System.Random(Guid.NewGuid().GetHashCode());
			int x, y;
			WordOrientationEnum orientEnum;
			WordPlaceOk wordPlaceOk;


			PlaceWordsOnBoardReturns boardTry = new PlaceWordsOnBoardReturns(width, height);
			//var tiles2DDummyLocal = new char[width, height];
			//List<WordListEntry> wordList = new List<WordListEntry>();

			foreach (var word in words)
			{
				int tries = 0;
				try
				{
					do
					{   //try to place the word up to maxRetries times
						++tries;
						if (tries > maxRetries) throw new RetriesTimeoutException(tries, $"To Many ReTries placing the word: \"{word}\"");
						x = random.Next(0, width);  // nice ducumentation you have there
						y = random.Next(0, height); // https://stackoverflow.com/a/5063289
						orientEnum = (WordOrientationEnum)random.Next(0, 5);
					}
					while (!(wordPlaceOk = CanPlaceWordHere(x, y, boardTry, orientEnum, word)).ok);
				}
				catch (Exception e)
				{
					++boardTry.wordsTimedout;
					Debug.LogWarning(e);
					continue;
				}

				boardTry.wordList.Add(new WordListEntry()
				{
					word = word,
					posFrom = { x = x, y = y },
					posTo = { x = x + wordPlaceOk.xmod * word.Length, y = y + wordPlaceOk.ymod * word.Length }
				});
				for (int i = 0; i != word.Length; i++)
				{
					boardTry.tiles2DDummy[x + (i * wordPlaceOk.xmod), y + (i * wordPlaceOk.ymod)] = word[i];
					//tilesSript2D[x + (i * wordPlaceOk.xmod), y + (i * wordPlaceOk.ymod)].SetLetter(word[i]);
				}
			}


			return boardTry;
		}

		/// <summary>
		/// Based on total count of Letters in words, returns a bare minimum board size in AspectRacio
		/// </summary>
		/// <param name="words"></param>
		/// <returns></returns>
		public Vector2 CalculateMinBoardDims(ref List<string> words)
		{
			//double targetAspectRatio = 14d / 9d;
			double targetAspectRatio = AspectRatio.x / AspectRatio.y;
			var minDim = words[0].Length;
			int letters = 0;
			foreach (var word in words)
			{
				letters += word.Length;
			}
			//add to the minimum board
			if (letters > 0f && AdditionalCharsPercent > 0f)
				letters += (int)((AdditionalCharsPercent * letters));
			// formula: sqrt((1920*1080) *(16/9))
			int prefDimX = (int)Math.Sqrt((double)letters * targetAspectRatio);
			int prefDimY = (int)Math.Sqrt((double)letters / targetAspectRatio);

			if (minDim > prefDimX) prefDimX = minDim;

			return new Vector2(prefDimX, prefDimY);
		}
		Vector2 GetBoardDimsNoLessThanMin(Vector2 minimum)
		{
			return new Vector2(minimum.x < MinimumBoardSize.x ? MinimumBoardSize.x : minimum.x,
				minimum.y < MinimumBoardSize.y ? MinimumBoardSize.y : minimum.y
				);
		}

		WordPlaceOk CanPlaceWordHere(int x, int y, PlaceWordsOnBoardReturns boardTry, WordOrientationEnum orientEnum, string word)
		{
			int xmod = 0, ymod = 0;
			switch (orientEnum)
			{
				case WordOrientationEnum.horizontal:
					{
						xmod = 1;
						break;
					}
				case WordOrientationEnum.vertical:
					{
						ymod = 1;
						break;
					}
				case WordOrientationEnum.diagonal:
					{
						xmod = 1;
						ymod = 1;
						break;
					}
				case WordOrientationEnum.horizontalBack:
					{
						xmod = -1;
						break;
					}
				case WordOrientationEnum.verticalBack:
					{
						ymod = -1;
						break;
					}
				case WordOrientationEnum.diagonalBack:
					{
						xmod = -1;
						ymod = -1;
						break;
					}
			}
			WordPlaceOk wordPlaceOk = new WordPlaceOk() { xmod = xmod, ymod = ymod, ok = false };
			int xok = x + xmod * word.Length;
			int yok = y + ymod * word.Length;
			if (xok > 0 && yok > 0)
				if (xok < width && yok < height) wordPlaceOk.ok = true;
			if (!wordPlaceOk.ok) return wordPlaceOk;

			for (int i = 0; i < word.Length; i++)
			{
				var tmp = boardTry.tiles2DDummy[x + (i * wordPlaceOk.xmod), y + (i * wordPlaceOk.ymod)];
				if (tmp != 0x00)
					if (tmp != word[i])
					{
						wordPlaceOk.ok = false;
						break;
					}
			}
			return wordPlaceOk;
		}

		struct WordContainedFrom
		{
			public string wordContaied;
			public int startOffset;
		}

		/// <summary>
		/// this function takes in a list of words to separate out ones that are contained in other ones
		/// </summary>
		/// <param name="words">in-out: list of words to put on board</param>
		/// <param name="outSeparetedWords">out: a map of longer words that contain shorter words and their relative offset</param>
		/// <param name="wordsToFind">out: the full list of words to find (should contain words.Len() - return)</param>
		/// <param name="wordsInReverse">checks the input words list for palindroms and their partial matches</param>
		/// <returns>amount of 1 letter words or exact duplicates removed</returns>
		static int SepareteContainedDuplicateWords(ref List<string> words, out SortedDictionary<string, List<WordContainedFrom>> outSeparetedWords, out List<string> wordsToFind, bool wordsInReverse)
		{
			int amountRemoved = words.RemoveAll(s => s.Length <= 1);    //do not allow single letters
			outSeparetedWords = new SortedDictionary<string, List<WordContainedFrom>>();
			var wordsLower = words.ConvertAll(x => new string(x.ToLower()));

			wordsLower.Sort(); // sort alpabetical
			wordsLower.Sort((x, y) => -x.Length.CompareTo(y.Length)); // sort it from longest to shortest (keep previous alpabetical)
			
			var wordsMinToMax = wordsLower.ToList(); //makes a copy
			wordsMinToMax.Reverse();
			int wordCount = wordsMinToMax.Count;
			List<string> wordsNotRemoved = new List<string>();
			wordsToFind = new List<string>();

			bool removeWord, removeWordExactDup;
			for (int idx = 0; idx < wordsMinToMax.Count-1; ++idx)
			{
				removeWord = removeWordExactDup = false;
				string shorterWord = wordsMinToMax[idx];
				Regex regexContains = new Regex(shorterWord, RegexOptions.IgnoreCase);

				for (int idx2 = idx+1; idx2 < wordsMinToMax.Count; ++idx2)
				{
					string longerWord = wordsMinToMax[idx2];
					if (longerWord == shorterWord)	//same word appeared multiple times, that is not allowed
					{
						++amountRemoved;
						removeWord = removeWordExactDup = true;
						break;
					}

					var matches = regexContains.Matches(longerWord);

					if (matches.Count!=0)	//we have a contained duplicate
						removeWord = true;
					foreach (Match match in matches)
					{
						outSeparetedWords.GetOrCreate(longerWord)
						.Add(new() { wordContaied = shorterWord, startOffset = match.Groups[0].Index });
					}

					if (!wordsInReverse) continue; //only when words can be backwards on the board

					string longerWordRev;
					char[] charArray = longerWord.ToCharArray();
					Array.Reverse(charArray);
					longerWordRev = new string(charArray);
					var matchesRev = regexContains.Matches(longerWordRev);

					if (matchesRev.Count != 0) //we have a contained Reverse duplicate
						removeWord = true;
					foreach (Match match in matchesRev)
					{
						outSeparetedWords.GetOrCreate(longerWord)
						.Add(new() { wordContaied = shorterWord, startOffset = -(match.Groups[0].Index+1) });
					}
				}

				if (!removeWord)
					wordsNotRemoved.Add(shorterWord);
				if (!removeWordExactDup)
					wordsToFind.Add(shorterWord);
			}
			wordsNotRemoved.Add(wordsLower[0]);
			wordsNotRemoved.Reverse();
			words = wordsNotRemoved;
			return amountRemoved;
		}
	}
}