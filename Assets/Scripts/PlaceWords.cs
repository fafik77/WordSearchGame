using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Assets.Scripts.Internal;
using Exceptions;
using UnityEngine;

namespace BoardContent
{
	enum WordOrientationEnum
	{
		horizontal = 0,
		vertical = 1,
		diagonal = 2,
		backDiagonal = 3,

		horizontalReverse = 4,
		verticalReverse = 5,
		diagonalReverse = 6,
		backDiagonalReverse = 7,
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
		/// adds Additional dummy Chars to the board size to make the search for words harder.
		/// The formula is += letters * AdditionalCharsPercent
		/// </summary>
		float AdditionalCharsPercent = 0f;
		/// <summary>
		/// this could be an option for the player to entice Words to share Letter Tile
		/// </summary>
		public bool PrefferWordsShareLetters;
		public float MaxWaitTimeForThreadsSec = 5f;

		SortedSet<WordOrientationEnum> allowedOrientationsSet;

		bool _TerminatingThreads;



		SortedDictionary<string, List<WordContainedFrom>> wordsContained;
		Dictionary<char, int> uniqueLettersFrequency;

		public int WordsRemoved { get; private set; }



		public LetterTileScript[,] TilesSript2D {  get; private set; }
		PlaceWordsOnBoardReturns finalBoard;


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
			MinimumBoardSize = this.AspectRatio;
			allowedOrientationsSet = new();
			this.AdditionalCharsPercent = AdditionalCharsPercent;
			WordsRemoved = SepareteContainedDuplicateWords(ref words, out wordsContained, out uniqueLettersFrequency, wordsInReverse);
			this.words = words;
			MinimumRequiredBoardSize = CalculateMinBoardDims(ref words);
			MinimumRequiredBoardSize = GetBoardDimsNoLessThanMin(MinimumRequiredBoardSize);
			TilesSript2D = CreateBoardAtLeast(((int)MinimumRequiredBoardSize.x), ((int)MinimumRequiredBoardSize.y));
			width = TilesSript2D.GetLength(0);
			height = TilesSript2D.GetLength(1);
		}

		public int PlaceWordsOnBoardThreaded(int wordPlaceMaxRetry = 100, int maxThreads = 8)
		{
			maxRetries = wordPlaceMaxRetry;
			if (maxThreads < 1) maxThreads = 1;
			Singleton.wordList.Reset();
			_TerminatingThreads = false;

			allowedOrientationsSet.Add(WordOrientationEnum.horizontal);
			allowedOrientationsSet.Add(WordOrientationEnum.vertical);
			Singleton.wordList.diagonalWords = Singleton.settingsPersistent.diagonalWords;
			Singleton.wordList.reversedWords = Singleton.settingsPersistent.reversedWords;
			if (Singleton.wordList.diagonalWords)
			{
				allowedOrientationsSet.Add(WordOrientationEnum.diagonal);
				allowedOrientationsSet.Add(WordOrientationEnum.backDiagonal);
			}
			if (Singleton.wordList.reversedWords)
			{
				allowedOrientationsSet.Add(WordOrientationEnum.horizontalReverse);
				allowedOrientationsSet.Add(WordOrientationEnum.verticalReverse);
				if (Singleton.wordList.diagonalWords)
				{
					allowedOrientationsSet.Add(WordOrientationEnum.diagonal);
					allowedOrientationsSet.Add(WordOrientationEnum.backDiagonal);
				}
			}



			List<Task<PlaceWordsOnBoardReturns>> tryBoardTasks = new(maxThreads);
			List<PlaceWordsOnBoardReturns> triedBoards = new(maxThreads);
			for (int i = 0; i != maxThreads; i++)
			{
				var task = Task<PlaceWordsOnBoardReturns>.Run(() => { return PlaceWordsOnBoard(); });
				tryBoardTasks.Add(task);
			}
			Task.WaitAll(tryBoardTasks.ToArray(), (int)(MaxWaitTimeForThreadsSec * 1000));
			_TerminatingThreads = true;
			foreach (var task in tryBoardTasks)
			{
				try
				{
					var res = task.Result;
					if (res.fullyComplete) triedBoards.Add(res);
				}
				catch (Exception ex)
				{
					Debug.LogWarning(ex);
				}
			}
			triedBoards.OrderBy(x => x.wordsTimedout);
			if (triedBoards.Count == 0)
			{
				Debug.LogWarning($"failed to complete any of {maxThreads} threads. Retrying single.");
				finalBoard = PlaceWordsOnBoard();
			}
			else
			{
				finalBoard = triedBoards[0];
			}
			Singleton.wordList.list = finalBoard.wordList;
			Singleton.wordList.wordsToFind = finalBoard.wordsToFind.ToList();

			//write onto the screen
			WriteContentOntoScreen(finalBoard.tiles2DDummy, TilesSript2D);
			return 0x00;
		}
		/// <summary>
		/// Writes onto the screen
		/// </summary>
		/// <param name="content">Src</param>
		/// <param name="screen">Dst</param>
		public static void WriteContentOntoScreen(char[,] content, LetterTileScript[,] screen)
		{
			//write onto the screen
			var iterSrc = content.GetEnumerator();
			var iterDst = screen.GetEnumerator();
			if (Singleton.settingsPersistent.upperCase)
			{
				while (iterSrc.MoveNext() && iterDst.MoveNext())
				{
					(iterDst.Current as LetterTileScript).Letter = char.ToUpper((char)iterSrc.Current);
				}
			}
			else
			{
				while (iterSrc.MoveNext() && iterDst.MoveNext())
				{
					(iterDst.Current as LetterTileScript).Letter = char.ToLower((char)iterSrc.Current);
				}
			}
		}

		struct CoordinatesXY
		{
			public int X;
			public int Y;
			public CoordinatesXY(int x, int y) { X = x; Y = y; }
		}

		/// <summary>
		/// Finds words on board in less than O(#words*(W*H))
		/// </summary>
		/// <param name="boardContent">lowercase</param>
		/// <param name="words">lowercase</param>
		/// <returns>List of all directions the words are present in</returns>
		public static SortedSet<WordOrientationEnum> FindWordsOnBoard(char[,] boardContent, List<string> words)
		{
			Singleton.wordList.Reset();
			///make a copy early so we can take out the provided words
			Singleton.wordList.wordsToFind = words.Select(x => x.ToLower()).Distinct().ToList();
			Singleton.wordList.wordsToFind.Sort();
			words = Singleton.wordList.wordsToFind.ToList();
			SortedSet<WordOrientationEnum> wordOrientations = new SortedSet<WordOrientationEnum>();
			List<WordListEntry> wordList = new List<WordListEntry>();

			int width = boardContent.GetLength(0), height = boardContent.GetLength(1);


			PlaceWordsOnBoardReturns boardReturns = new PlaceWordsOnBoardReturns(width, height);
			boardReturns.tiles2DDummy = boardContent;
			//CoordinatesXY posTo = new CoordinatesXY();
			//List<string> wordsToRemove = new List<string>();
			//for (int i = 0; i!= width; ++i)
			//{
			//	for (int ii = 0; ii != height; ++ii)
			//	{
			//		foreach(var word in words)
			//		{
			//			bool success = false;
			//			if (word[0] == boardContent[i, ii])
			//			{
			//				WordOrientationEnum foundLike;
			//				(success, foundLike) = FindWordOnBoard(boardReturns, word, new CoordinatesXY(i, ii), ref posTo);
			//				if (success)
			//				{
			//					wordList.Add(new WordListEntry()
			//					{
			//						word = word,
			//						posFrom = new Vector2(i, ii),
			//						posTo = new Vector2(posTo.X, posTo.Y)
			//					});
			//					wordOrientations.Add(foundLike);
			//					//words.Remove(word);
			//					//break;
			//				}
			//			}
			//		}
			//	}
			//}
			StringBuilder stringBuilder = new StringBuilder();
			List<WordContainedFrom> wordsFound;
			///proccess each row
			for (int h = 0; h != height; ++h)
			{
				for (int w = 0; w != width; ++w)
					stringBuilder.Append(boardContent[w, h]);
				///horizontal normal
				string lineStr = stringBuilder.ToString().ToLower();
				FindWordsInString(lineStr, out wordsFound, ref words);
				foreach (var word in wordsFound)
				{
					wordList.Add(new WordListEntry()
					{
						word = word.wordContaied,
						posFrom = new Vector2(word.startOffset, h),
						posTo = new Vector2(word.startOffset + word.wordContaied.Length, h)
					});
				}
				if (wordsFound.Count != 0)
					wordOrientations.Add(WordOrientationEnum.horizontal);
				///horizontal reverse
				lineStr = lineStr.Reverse().ToString();
				FindWordsInString(lineStr, out wordsFound, ref words);
				foreach (var word in wordsFound)
				{
					wordList.Add(new WordListEntry()
					{
						word = word.wordContaied,
						posFrom = new Vector2(width - word.startOffset, h),
						posTo = new Vector2(width - (word.startOffset + word.wordContaied.Length), h)
					});
				}
				if (wordsFound.Count != 0)
					wordOrientations.Add(WordOrientationEnum.horizontalReverse);

				stringBuilder.Clear();
			}
			///proccess each coll
			for (int w = 0; w != width; ++w) 
			{
				for (int h = 0; h != height; ++h)
					stringBuilder.Append(boardContent[w, h]);
				///vertical normal
				string lineStr = stringBuilder.ToString().ToLower();
				FindWordsInString(lineStr, out wordsFound, ref words);
				foreach (var word in wordsFound)
				{
					wordList.Add(new WordListEntry()
					{
						word = word.wordContaied,
						posFrom = new Vector2(w, word.startOffset),
						posTo = new Vector2(w, word.startOffset + word.wordContaied.Length)
					});
				}
				if (wordsFound.Count != 0)
					wordOrientations.Add(WordOrientationEnum.vertical);
				///vertical reverse
				lineStr = lineStr.Reverse().ToString();
				FindWordsInString(lineStr, out wordsFound, ref words);
				foreach (var word in wordsFound)
				{
					wordList.Add(new WordListEntry()
					{
						word = word.wordContaied,
						posFrom = new Vector2(w, height - word.startOffset),
						posTo = new Vector2(w, height - (word.startOffset + word.wordContaied.Length))
					});
				}
				if (wordsFound.Count != 0)
					wordOrientations.Add(WordOrientationEnum.verticalReverse);

				stringBuilder.Clear();
			}
            ///proccess each diagonal /
            
			///proccess each backDiagonal \
			
            /*	123
			 *	456
			 *	789
			 *	from 2D board get all (Rows + Cols) then reverse it (that is O(R+C) instead of O(R*C) )
			 *	for diagonal that is sqrt(R^2+C^2) and reverse it.
			 *	from those (exactly 8) lists take out all the words ONCE.
			 *	  additionall benefit of that: if word exists in said list in marks WordOrientationEnum
			 *	in the end the complexity is O(8*(R+C)* ?words * ?avgLetters)
			 *	memory usage is 4*RC+4*Diagonals
			 *	++memory will be continous, just with this benefit it will be faster than accessing a random point in memory
			 */
            Singleton.wordList.list = wordList;
			return wordOrientations;
		}

		/// <summary>
		/// finds given 'wordsToFind' in 'inString' removing them from the 1st list and adding them with their relative position to wordsFound
		/// </summary>
		/// <param name="inString">Lower</param>
		/// <param name="wordsFound">Lower</param>
		/// <param name="wordsToFind">Lower</param>
		private static void FindWordsInString(string inString, out List<WordContainedFrom> wordsFound, ref List<string> wordsToFind)
		{
			wordsFound = new();
			for (int i = wordsToFind.Count - 1; i >= 0; --i)
			{
				Regex wordReg = new Regex(wordsToFind[i]);
				bool found = false;
				foreach(Match foundPos in wordReg.Matches(inString))
				{
					wordsFound.Add(new WordContainedFrom() { wordContaied = wordsToFind[i], startOffset = foundPos.Groups[0].Index });
					found = true;
				}
				if (found)
					wordsToFind.RemoveAt(i);
			}
		}


		struct PlaceWordsOnBoardReturns
		{
			public int wordsTimedout;
			public char[,] tiles2DDummy;
			public List<WordListEntry> wordList;
			public SortedSet<string> wordsToFind;
			public bool fullyComplete;
			public int width, height;
			public PlaceWordsOnBoardReturns(int width, int height)
			{
				fullyComplete = false;
				wordsTimedout = 0;
				tiles2DDummy = new char[width, height];
				this.width = width;
				this.height = height;
				wordList = new List<WordListEntry>();
				wordsToFind = new SortedSet<string>();
			}
		}

		/// <summary>
		/// Places words on board (Thread safe, read only access)
		/// </summary>
		/// <returns>Board proposition</returns>
		/// <exception cref="ThreadTerminationException">when '_TerminatingThreads==1'</exception>
		PlaceWordsOnBoardReturns PlaceWordsOnBoard()
		{
			//not so simple, if we fail to put the word on board it can not be included here then
			//Singleton.wordList.wordsToFind = WordsLeft;

			System.Random random = new System.Random(Guid.NewGuid().GetHashCode());
			int x, y;
			WordOrientationEnum orientEnum;
			WordPlaceOk wordPlaceOk;
			PlaceWordsOnBoardReturns boardTry = new PlaceWordsOnBoardReturns(width, height);

			foreach (var word in words)
			{
				int tries = 0;
				//try to place the word
				try
				{	
					do
					{   //try to place the word up to maxRetries times
						if (_TerminatingThreads) throw new ThreadTerminationException();
						++tries;
						if (tries > maxRetries) throw new RetriesTimeoutException(tries, $"To Many ReTries placing the word: \"{word}\"");
						x = random.Next(0, width);  // nice ducumentation you have there MS
						y = random.Next(0, height); // https://stackoverflow.com/a/5063289
						orientEnum = allowedOrientationsSet.ElementAt(random.Next(0, allowedOrientationsSet.Count));
					}
					while (!(wordPlaceOk = CanPlaceWordHere(x, y, boardTry, orientEnum, word)).ok);
				}
				catch (RetriesTimeoutException e)
				{	//could not place the word in n retries
					++boardTry.wordsTimedout;
					Debug.LogWarning(e);
					continue;
				}
				//save the word to the board
				for (int i = 0; i != word.Length; i++)
				{
					if (_TerminatingThreads) throw new ThreadTerminationException();
					boardTry.tiles2DDummy[x + (i * wordPlaceOk.xmod), y + (i * wordPlaceOk.ymod)] = word[i];
				}

				//save this and all contained words to the list
				var myWordListEntry = new WordListEntry()
				{
					word = word,
					posFrom = { x = x, y = y },
					posTo = { x = x + wordPlaceOk.xmod * (word.Length - 1), y = y + wordPlaceOk.ymod * (word.Length - 1) }
				};
				boardTry.wordList.Add(myWordListEntry);
				boardTry.wordsToFind.Add(word);
				// save all contained words to the list
				if (wordsContained.TryGetValue(word, out var value))
				{
					foreach(var item in value)
					{
						var containedWordListEntry = new WordListEntry()
						{
							word = item.wordContaied
						};

						if (item.startOffset < 0) //from the end
						{
							var back = -item.startOffset;
							var len = item.wordContaied.Length - 1;
							containedWordListEntry.posFrom = new Vector2(x + wordPlaceOk.xmod * (word.Length - back), y + wordPlaceOk.ymod * (word.Length - back));
							containedWordListEntry.posTo = new Vector2(x + wordPlaceOk.xmod * (word.Length - back - len), y + wordPlaceOk.ymod * (word.Length - back - len));
						}
						else
						{
							containedWordListEntry.posFrom = new Vector2(x, y);
							containedWordListEntry.posTo = new Vector2(x + wordPlaceOk.xmod * (item.wordContaied.Length - 1), y + wordPlaceOk.ymod * (item.wordContaied.Length - 1));
						}
						boardTry.wordList.Add(containedWordListEntry);
						boardTry.wordsToFind.Add(item.wordContaied);
					}
				}

			} //for all words
			FillinBoardWithRandomLetters(ref boardTry);

			boardTry.fullyComplete = true;
			return boardTry;
		}

		void FillinBoardWithRandomLetters(ref PlaceWordsOnBoardReturns boardTry)
		{
			var keys = uniqueLettersFrequency.Keys.ToList();
			int keysLen = keys.Count;
			System.Random random = new System.Random(Guid.NewGuid().GetHashCode());

			for (int row = 0; row < boardTry.tiles2DDummy.GetLength(0); ++row)
			{
				for (int col = 0; col < boardTry.tiles2DDummy.GetLength(1); ++col)
				{
					if (boardTry.tiles2DDummy[row, col] == 0x00)
						boardTry.tiles2DDummy[row, col] = keys[random.Next(0, keysLen)];
				}
			}
		}

		/// <summary>
		/// Based on total count of Letters in words, returns a bare (minimum + AdditionalCharsPercent) board size in AspectRacio
		/// </summary>
		/// <param name="words">Sorted by lenght desc</param>
		/// <returns>minimum W : H</returns>
		public Vector2 CalculateMinBoardDims(ref List<string> words)
		{
			double targetAspectRatio = AspectRatio.x / AspectRatio.y;   // 14 : 9
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

		/// <summary>
		/// Checks if word from x,y in direction can be placed on board
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="boardTry">the board to check</param>
		/// <param name="orientEnum"></param>
		/// <param name="word"></param>
		/// <returns></returns>
		static WordPlaceOk CanPlaceWordHere(int x, int y, PlaceWordsOnBoardReturns boardTry, WordOrientationEnum orientEnum, string word)
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
				case WordOrientationEnum.diagonal:	// /
					{
						xmod = 1;
						ymod = -1;
						break;
					}
				case WordOrientationEnum.backDiagonal:	// \
					{
						xmod = 1;
						ymod = 1;
						break;
					}
				case WordOrientationEnum.horizontalReverse:
					{
						xmod = -1;
						break;
					}
				case WordOrientationEnum.verticalReverse:
					{
						ymod = -1;
						break;
					}
				case WordOrientationEnum.diagonalReverse:	// /
                    {
						xmod = -1;
						ymod = 1;
						break;
					}
				case WordOrientationEnum.backDiagonalReverse:   // \
                    {
						xmod = -1;
						ymod = -1;
						break;
					}
			}
			WordPlaceOk wordPlaceOk = new WordPlaceOk() { xmod = xmod, ymod = ymod, ok = false };
			int xok = x + xmod * (word.Length - 1);
			int yok = y + ymod * (word.Length - 1);
			if (xok >= 0 && yok >= 0)
				if (xok < boardTry.width && yok < boardTry.height) wordPlaceOk.ok = true;
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
			/// <summary>
			/// startOffset: ..-1 means backwards
			/// </summary>
			public int startOffset;
		}

		/// <summary>
		/// takes in a list of words to separate out ones that are contained in other ones
		/// </summary>
		/// <param name="words">in-out: list of words to put on board</param>
		/// <param name="outSeparetedWords">out: a map of longer words that contain shorter words and their relative offset</param>
		/// <param name="wordsToFind">out: the full list of words to find (should contain words.Len() - return)</param>
		/// <param name="wordsInReverse">checks the input words list for palindroms and their partial matches</param>
		/// <returns>amount of 1 letter words or exact duplicates removed</returns>
		static int SepareteContainedDuplicateWords(ref List<string> words, out SortedDictionary<string, List<WordContainedFrom>> outSeparetedWords, out Dictionary<char,int> uniqueLettersFrequency, bool wordsInReverse)
		{
			int amountRemoved = words.RemoveAll(s => s.Length <= 1);    //do not allow single letters
			outSeparetedWords = new SortedDictionary<string, List<WordContainedFrom>>();
			///remove duplicate words, that is not allowed
			var wordsLower = words.ConvertAll(x => new string(x.ToLower())).Distinct().ToList();
			amountRemoved += words.Count - wordsLower.Count;
			wordsLower.Sort(); // sort alpabetical
			wordsLower.Sort((x, y) => -x.Length.CompareTo(y.Length)); // sort it from longest to shortest (keep previous alpabetical)

			var wordsMinToMax = wordsLower.ToList(); //makes a copy
			wordsMinToMax.Reverse();
			int wordCount = wordsMinToMax.Count;
			List<string> wordsNotRemoved = new List<string>();
			uniqueLettersFrequency = new Dictionary<char, int>();


			bool removeWord;
			for (int idx = 0; idx < wordsMinToMax.Count-1; ++idx)
			{
				removeWord = false;
				string shorterWord = wordsMinToMax[idx];
				Regex regexContains = new Regex(shorterWord, RegexOptions.IgnoreCase);

				for (int idx2 = idx+1; idx2 < wordsMinToMax.Count; ++idx2)
				{
					string longerWord = wordsMinToMax[idx2];
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
			}
			wordsNotRemoved.Add(wordsLower[0]);
			wordsNotRemoved.Reverse();
			words = wordsNotRemoved;
			foreach(var word in words)
			{
				foreach (char letter in word)
				{
					uniqueLettersFrequency.TryAdd(char.ToLower(letter),0);
					uniqueLettersFrequency[char.ToLower(letter)]++;
				}
			}
			return amountRemoved;
		}
	}
}