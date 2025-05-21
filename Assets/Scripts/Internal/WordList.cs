using System.Collections.Generic;
using UnityEngine;

namespace BoardContent
{
	public struct WordListEntry
	{
		public string word;
		public Vector2 posFrom, posTo;
		public bool found;
		public bool Found {  get { return found; } set { found = value; }}
		public bool CompareTo(Vector2 posFrom2, Vector2 posTo2)
		{
			if ((posFrom == posFrom2 && posTo == posTo2) ||
				  (posFrom == posTo2 && posTo == posFrom2))
				return true;
			return false;
		}
	}

	public struct WordList
	{
		public List<WordListEntry> list;
		public List<WordListEntry> listUnintended;
		public List<string> wordsToFind;
		public List<string> wordsFound;
		public bool reversedWords;
		public bool diagonalWords;


		public void Reset()
		{
			if (list == null) list = new List<WordListEntry>();
			list.Clear();
			if (wordsToFind == null) wordsToFind = new List<string>();
			wordsToFind.Clear();
			if (wordsFound == null) wordsFound = new List<string>();
			wordsFound.Clear();
			if (listUnintended == null) listUnintended = new List<WordListEntry>();
			listUnintended.Clear();
		}
	}

}