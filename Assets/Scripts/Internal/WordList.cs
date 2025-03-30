using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BoardContent
{
	public enum WordListDirectionEnum
	{
		None = 0,
		Horizontal,
		Vertical,
		Crossed
	}

	public struct WordListEntry
	{
		public WordListDirectionEnum direction;
		public string word;
		public Vector2 posFrom, posTo;
	}

	public struct WordList
	{
		public List<WordListEntry> list;
		public List<string> wordsToFind;
		public List<string> wordsFound;
		// that wont work when two words share the same Letter Tile
		//public Dictionary<Vector2, WordListEntry> tilesTaken;
		// this could be an option for the player to entice Words to share Letter Tile
		bool PrefferWordsShareLetters;


		public void Reset()
		{
			list.Clear();
			//tilesTaken.Clear();
		}
	}

}