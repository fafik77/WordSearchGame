using BoardContent;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Singleton
{
	public static Singleton Instance { get; private set; }
	public struct ClickAndDragStruct
	{
		private float lastClickTime;
		public event EventHandler<LetterTileScript> StartDrawingLine;
		public static float ClickFromDragRegisterSeconds = 0.2f;
		public static float ReClickRegisterDelaySeconds = 0.3f;
		/**
			the event args should be the actual positions
		*/
		public event EventHandler<LetterTileScript[]> FinishDrawingLine;
		//for when there was a cancel
		public event EventHandler<LetterTileScript> CancelDrawingLine;
		private LetterTileScript tileStart;
		private LetterTileScript tileEnd;
		public LetterTileScript TileStart
		{
			get => tileStart;
			set
			{
				if (!tileStart)
					tileStart = value;
			}
		}


		static private async void _FinishLineDelayed()
		{
			//FinishDrawingLine?.Invoke(this, tileEnd);
			await Task.Delay((int)ReClickRegisterDelaySeconds * 1000);
			Singleton.clickAndDrag.tileStart = null;
			Singleton.clickAndDrag.clickLocked = false;
		}

		private bool WasShortClick
		{
			get
			{
				if ((Time.fixedTime - lastClickTime) < ClickFromDragRegisterSeconds)
				{
					lastClickTime = Time.fixedTime;
					return true;
				}
				return false;
			}
			set => lastClickTime = Time.fixedTime;
		}
		private bool clickLocked;
		public MenuMgr menuMgrInGame;
		/// <summary>
		/// track letter tiles selected by clicking/draging
		/// </summary>
		/// <param name="letterTileTouched">Tile to register as Start/End</param>
		/// <param name="finishPointOnly">Set only the End tile</param>
		public void AddClickPoint(LetterTileScript letterTileTouched, PointerEventData eventData, bool finishPointOnly = false)
		{
			if (menuMgrInGame != null && !menuMgrInGame.IsIngame())
				return;
			
			if (clickLocked) return;

			if (tileStart)
			{
				if (!WasShortClick)
				{
					tileEnd = letterTileTouched;
					clickLocked = true;
					FinishDrawingLine?.Invoke(this, new LetterTileScript[] { tileStart, tileEnd });
					Thread thread = new Thread(new ThreadStart(_FinishLineDelayed));
					thread.Start();
				}
			}
			else if (finishPointOnly == false && (!tileStart || WasShortClick)) 
			{
				tileStart = letterTileTouched;
				WasShortClick = true;
				tileEnd = null;

				//Thread thread = new Thread(new ThreadStart(_StartDrawingLineAsync));
				//thread.Start();
				StartDrawingLine?.Invoke(this, tileStart);
			}

		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="requester">only if present</param>
		public void CancelClickPoints(LetterTileScript requester)
		{
			CancelDrawingLine?.Invoke(this, requester);

			Singleton.clickAndDrag.tileStart = null;
			Singleton.clickAndDrag.clickLocked = false;

		}

	}
	public static ClickAndDragStruct clickAndDrag;
	public enum CaseEnum
	{
		UpperCase, LowerCase
	}
	public struct BoardUiEvents
	{
		public event EventHandler<string> FoundWordEvent;
		public event Action BoardRefreshUiEvent;
		public event Action<CaseEnum> BoardSetCaseEvent;

		public void FoundWord(string word) => FoundWordEvent?.Invoke(this, word);
		public void RefreshBoardUi() => BoardRefreshUiEvent?.Invoke();
		public void BoardSetCase(CaseEnum Case) => BoardSetCaseEvent?.Invoke(Case);
	}
	public static BoardUiEvents boardUiEvents;

	
	public static WordList wordList;
	public static LetterTileScript[,] TilesSript2D { get; set; }

	public struct ScenesStruct
	{
		public Scene GameScene;
		public Scene MainMenuScene;
		public void SwitchToScene(string sceneName)
		{
			var currScene = SceneManager.GetActiveScene();
			if (currScene.path == sceneName || currScene.name == sceneName)
				return;
			SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
		}
	}
	public static ScenesStruct scenesStruct;

	public struct ChooseBoardSStruct
	{
		public string Lang;
		public string file;
		public string board;
	}
	public static ChooseBoardSStruct chooseBoardSStruct;

	public struct SettingsPersistent
	{
		public bool upperCase;
		public int ZoomDeadZoneSize;

	}
}
