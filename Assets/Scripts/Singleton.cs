using Assets.Scripts.Internal;
using BoardContent;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public static class Singleton
{
	// public static Singleton Instance { get; private set; }
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
	public static ClickAndDragStruct clickAndDrag = new();

	public struct BoardUiEvents
	{
		public event EventHandler<string> FoundWordEvent;
		public event Action BoardRefreshUiEvent;
		/// <summary>
		/// Set to bool UpperCase
		/// </summary>
		public event Action<bool> BoardSetCaseEvent;
		/// <summary>
		/// predefined Board?
		/// </summary>
		public event Action<bool> CreateBoardEvent;

		public void FoundWord(string word) => FoundWordEvent?.Invoke(this, word);
		public void RefreshBoardUi() => BoardRefreshUiEvent?.Invoke();
		public void BoardSetCase(bool UpperCase) => BoardSetCaseEvent?.Invoke(UpperCase);
		public void CreateBoard(bool predefined) { CreateBoardEvent?.Invoke(predefined); }

		public OnScreenNotification onScreenNotification;
	}
	public static BoardUiEvents boardUiEvents = new();


	public static WordList wordList = new();
	public static LetterTileScript[,] TilesSript2D { get; set; }

	public struct ScenesStruct
	{
		public Scene GameScene;
		public Scene MainMenuScene;
		public void SwitchToScene(string sceneName)
		{
			var currScene = SceneManager.GetActiveScene();
			if (currScene.path == sceneName || currScene.name == sceneName || currScene.path.Contains(sceneName))
				return;
			SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
		}
	}
	public static ScenesStruct SceneMgr = new();

	public static ChooseBoardDispatcher choosenBoard = new();

	public struct SettingsPersistent
	{
		public bool upperCase;
		public int ZoomDeadZoneSize;
		public int wordsMaxLenght;
		public string LanguageUi;
		public string LanguageWords;
		public bool reversedWords;
		public bool diagonalWords;
	}
	public static SettingsPersistent settingsPersistent = new();
}
