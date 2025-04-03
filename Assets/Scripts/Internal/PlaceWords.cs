using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BoardContent;
using Unity.VisualScripting;
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
		LetterTileScript[,] tilesSript2D;
		char[,] tiles2DDummy;
		public PlaceWords(LetterTileScript[,] tilesSript2D)
		{
			this.tilesSript2D = tilesSript2D;
			width = tilesSript2D.GetLength(0);
			height = tilesSript2D.GetLength(1);
			tiles2DDummy = new char[width, height];
		}

		public void PlaceWordsOnBoard()
		{
			List<string> words = new List<string>() { "barbara", "ania", "olaf", "kamil" };
			List<string> wordsContained;
			int removedWords = SepareteContainedDuplicateWords(ref words, out wordsContained);
			//before applying that list to board, make sure to sort it from longest to shortest..
			// remove the short words contained in longer ones (only add their positions)
			// one letter words are not accepted
			words.Sort((x, y) => x.Length.CompareTo(y.Length)); //sort in order

			Singleton.wordList.Reset();
			System.Random random = new System.Random();
			int x, y;
			WordOrientationEnum orientEnum;
			WordPlaceOk wordPlaceOk;

			foreach (var word in words)
			{
				int tries = 0;
				try
				{
					do
					{
						++tries;
						if (tries > maxRetries) throw new Exception($"To Many ReTries placing the word: \"{word}\"");
						x = random.Next(0, width - 1);
						y = random.Next(0, height - 1);
						orientEnum = (WordOrientationEnum)random.Next(0, 5);
					}
					while (!(wordPlaceOk = CanPlaceWordHere(x, y, orientEnum, word)).ok);
				}
				catch (Exception e)
				{
					Debug.LogWarning(e);
					continue;
				}

				Singleton.wordList.list.Add(new WordListEntry()
				{
					word = word,
					posFrom = { x = x, y = y },
					posTo = { x = x + wordPlaceOk.xmod * word.Length, y = y + wordPlaceOk.ymod * word.Length }
				});
				for (int i = 0; i != word.Length; i++)
				{
					tiles2DDummy[x + (i * wordPlaceOk.xmod), y + (i * wordPlaceOk.ymod)] = word[i];
					//tilesSript2D[x + (i * wordPlaceOk.xmod), y + (i * wordPlaceOk.ymod)].SetLetter(word[i]);
				}
			}
			//write onto the screen
			var iterSrc = tiles2DDummy.GetEnumerator();
			var iterDst = tilesSript2D.GetEnumerator();
			while (iterSrc.MoveNext() && iterDst.MoveNext())
			{
				(iterDst.Current as LetterTileScript).Letter = (char)iterSrc.Current;
			}
		}

		WordPlaceOk CanPlaceWordHere(int x, int y, WordOrientationEnum orientEnum, string word)
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
				var tmp = tiles2DDummy[x + (i * wordPlaceOk.xmod), y + (i * wordPlaceOk.ymod)];
				if (tmp != 0x00)
					if (tmp != word[i])
					{
						wordPlaceOk.ok = false;
						break;
					}
			}
			return wordPlaceOk;
		}

		/// <summary>
		/// this function takes in a list of words to separate out ones that are contained in other ones
		/// </summary>
		/// <param name="words">in-out list of words</param>
		/// <param name="outContainedDuplicates">out list of cuplicates</param>
		/// <returns>amount of 1 letter words removed</returns>
		static int SepareteContainedDuplicateWords(ref List<string> words, out List<string> outContainedDuplicates)
		{
			int amountRemoved = words.RemoveAll(s => s.Length <= 1);    //do not allow single letters
			outContainedDuplicates = new List<string>();

			words.Sort((x, y) => x.Length.CompareTo(y.Length)); // sort it from longest to shortest
			
			var wordsMinToMax = words.ToList();
			wordsMinToMax.Reverse();
			int indexWord = 0;
			foreach (var word in wordsMinToMax)
			{
				++indexWord;


			}



			return amountRemoved;
		}
	}
}