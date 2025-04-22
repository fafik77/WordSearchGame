using Assets.Scripts.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using static UnityEditor.Progress;

public class InGameUI : MonoBehaviour, ICameraView
{
	private UIDocument ui;
	private ListView listViewWordsLeft;
	private ListView listViewWordsFound;
	private Label TimeCounterLabel;
	private float timeCounter;
	private Action<MenuMgr.MenuNavigationEnum> navigateAction;
	public float TimeCounter
	{
		get { return timeCounter; }
		set { timeCounter = value; }
	}
	private Coroutine timeCounterCoroutine;
	Camera mainCamera;

	/// The "makeItem" function will be called as needed when the ListView needs more items to render
	Func<VisualElement> makeItem = () => new Label();
	private void Awake()
	{
		if (Singleton.scenesStruct.GameScene.path == null)
			Singleton.scenesStruct.GameScene = SceneManager.GetSceneByName("GameScene");
		if (Singleton.scenesStruct.MainMenuScene.path == null)
			Singleton.scenesStruct.MainMenuScene = SceneManager.GetSceneByName("MainMenuScene");
		mainCamera = Camera.main;
	}
	private void Start()
	{
	}

	private void OnEnable()
	{
		/// As the user scrolls through the list, the ListView object will recycle elements created by the "makeItem" and invoke the "bindItem" callback to associate the element with the matching data item (specified as an index in the list)
		Action<VisualElement, int> bindItemLeft = (e, i) => (e as Label).text = Singleton.wordList.wordsToFind[i];
		Action<VisualElement, int> bindItemFound = (e, i) => (e as Label).text = Singleton.wordList.wordsFound[i];
		if (Singleton.wordList.wordsToFind == null)
			Singleton.wordList.wordsToFind = new List<string>();
		if (Singleton.wordList.wordsFound == null)
			Singleton.wordList.wordsFound = new List<string>();


		ui = GetComponent<UIDocument>();
		TimeCounterLabel = ui.rootVisualElement.Q<Label>("TimeCounter");
		listViewWordsLeft = ui.rootVisualElement.Q<ListView>("WordsLeft");
		listViewWordsLeft.makeItem = makeItem;
		listViewWordsLeft.bindItem = bindItemLeft;
		listViewWordsLeft.itemsSource = Singleton.wordList.wordsToFind;
		listViewWordsLeft.selectionType = SelectionType.Single;
		listViewWordsFound = ui.rootVisualElement.Q<ListView>("WordsFound");
		listViewWordsFound.makeItem = makeItem;
		listViewWordsFound.bindItem = bindItemFound;
		listViewWordsFound.itemsSource = Singleton.wordList.wordsFound;
		listViewWordsFound.selectionType = SelectionType.None;
		///update the timer every 1 second
		timeCounterCoroutine = StartCoroutine(TimeCounterSecond());


		listViewWordsLeft.itemsChosen += (selectedItems) =>
		{
			string item = selectedItems.First() as string;
			Debug.Log("Items chosen: " + item);
		};

		Singleton.boardUiEvents.FoundWordEvent += FoundWordEventHandler;
		Singleton.boardUiEvents.BoardRefreshUiEvent += BoardRefreshUiEvent;
	}
	private void BoardRefreshUiEvent()
	{
		Hide();
		Show();
	}

	private void FoundWordEventHandler(object sender, string word)
	{
		Singleton.wordList.wordsFound.Insert(0, word);
		Singleton.wordList.wordsToFind.Remove(word);
		RefreshItems();
	}
	private void RefreshItems()
	{
		listViewWordsLeft.RefreshItems();
		listViewWordsFound.RefreshItems();
	}


	private void OnDisable()
	{
		StopCoroutine(timeCounterCoroutine);
		Singleton.boardUiEvents.FoundWordEvent -= FoundWordEventHandler;
		Singleton.boardUiEvents.BoardRefreshUiEvent -= RefreshItems;
	}

	private void FixedUpdate()
	{
		if (ui.enabled)
			timeCounter += Time.fixedDeltaTime;
	}
	private void Update()
	{
		if (!ui.enabled)
			return;
	}



/// <summary>
/// this updates the Time display
/// </summary>
/// <param name="seconds"></param>
IEnumerator TimeCounterSecond(int seconds = 1)
	{
		while (this.gameObject.activeInHierarchy)
		{
			UpdateTimeLabel();
			yield return new WaitForSeconds(seconds);
		}
	}

	private void UpdateTimeLabel()
	{
		TimeSpan timeSpan = new TimeSpan(0,0,seconds: (int)(timeCounter));
		TimeCounterLabel.text = $"{timeSpan.Hours:0}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
	}

	public void Hide()
	{
		Singleton.clickAndDrag.CancelClickPoints(null);
		this.gameObject.SetActive(false);
	}

	public void Show()
	{
		this.gameObject.SetActive(true);
	}
	public void OnNavigateToSet(Action<MenuMgr.MenuNavigationEnum> action) => navigateAction = action;
}
