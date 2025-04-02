using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BoardContent;
using Unity.VisualScripting;

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
		int width, height;
		LetterTileScript[,] tilesSript2D;
		char[,] tilesSript2DDummy;
		public PlaceWords(LetterTileScript[,] tilesSript2D)
		{
			this.tilesSript2D = tilesSript2D;
			width = tilesSript2D.GetLength(0);
			height = tilesSript2D.GetLength(1);
			tilesSript2DDummy = new char[width, height];
		}

		public void PlaceWordsOnBoard()
		{
			List<string> words = new List<string>() { "barbara", "ania", "olaf", "kamil" };

			Singleton.wordList.Reset();
			Random random = new Random();
			int x, y;
			WordOrientationEnum orientEnum;
			WordPlaceOk wordPlaceOk;

			foreach (var word in words)
			{
				do
				{
					x = random.Next(0, width - 1);
					y = random.Next(0, height - 1);
					orientEnum = (WordOrientationEnum)random.Next(0, 5);
				}
				while (!(wordPlaceOk=CanPlaceWordHere(x, y, orientEnum, word)).ok);

				//Singleton.wordList.list.Add(new WordListEntry{ })
				for (int i = 0; i < word.Length; i++)
				{
					tilesSript2DDummy[x + (i * wordPlaceOk.xmod), y + (i * wordPlaceOk.ymod)] = word[i];
					tilesSript2D[x + (i * wordPlaceOk.xmod), y + (i * wordPlaceOk.ymod)].SetLetter(word[i]);
				}
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
				var tmp = tilesSript2DDummy[x + (i * wordPlaceOk.xmod), y + (i * wordPlaceOk.ymod)];
				if (tmp != 0x00)
					if (tmp != word[i])
					{
						wordPlaceOk.ok = false;
						break;
					}
			}
			return wordPlaceOk;
		}
	}
}