using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BoardContent;

namespace BoardContent
{
	enum WordOrientationEnum
	{
		none = 0,
		horizontal = 0x01,
		vertical = 0x02,
		diagonal = 0x04,
		backwards = 0x08,
		horizontalBack = 0x01 | backwards,
		verticalBack = 0x02 | backwards,
		diagonalBack = 0x04 | backwards,
	}
	class PlaceWords
	{
		int width = 40, height = 28;

		public void PlaceWordsOnBoard( LetterTileScript[,] tilesSript2D)
		{
			List<string> words = new List<string>() { "barbara", "ania"};

			Singleton.wordList.Reset();

			foreach (var word in words)
			{
				Random random = new Random();
				var x = random.Next(0, width - 10);
				var y = random.Next(0, height - 1);
				int orient = 1 << random.Next(0, 2);
				WordOrientationEnum orientEnum = (WordOrientationEnum)orient;

				//Singleton.wordList.list.Add(new WordListEntry{ })
				for (int i = 0; i < word.Length; i++)
				{
					tilesSript2D[x+i, y].SetLetter(word[i]);
				}
            }

		}

	}

}