using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

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
		public void ApplyBoard()
		{

		}

		/// <summary>
		/// Chooses random words in random amount[10,16) in the provided language
		/// </summary>
		/// <param name="lang"></param>
		/// <param name="maxWordLen"></param>
		/// <returns></returns>
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
			Singleton.SceneMgr.SwitchToScene("Assets/Scenes/GameScene.unity");
			WaitingForApply = true;
			Singleton.boardUiEvents.CreateBoard(predefined: false);	//its random, not predefined
			return wordsChosen.Count;
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
