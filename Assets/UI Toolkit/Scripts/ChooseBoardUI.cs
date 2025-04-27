using Assets.Scripts.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

public class ChooseBoardUI : MonoBehaviour, ICameraView
{
	private UIDocument ui;
	private Action<MenuMgr.MenuNavigationEnum> navigateAction;
	Button buttonLoadFile;
	Button buttonCreateRandom;
	Button buttonPickRandom;
	DropdownField dropdownLang;
	TextField SearchField;

	private void Awake()
	{
		ui = GetComponent<UIDocument>();
		ui.enabled = true;
	}
	private void OnEnable()
	{
		var root = ui.rootVisualElement;
		buttonLoadFile = root.Q<Button>("LoadFile");
		buttonCreateRandom = root.Q<Button>("CreateRandom");
		buttonPickRandom = root.Q<Button>("PickRandom");
		dropdownLang = root.Q<DropdownField>("Language");
		SearchField = root.Q<TextField>("Search");

		buttonCreateRandom.clicked += ButtonCreateRandom_clicked;
	}

	private void ButtonCreateRandom_clicked()
	{
		if (Singleton.EngWordsList == null)
			Singleton.EngWordsList = new();
		if (Singleton.EngWordsList.Count == 0)
		{
			///wrong: this is cwd: should be executionPath/Data...
			foreach (var line in File.ReadLines(@"Data\words_alpha english.txt"))
			{
				if (line.Length > 2)
					Singleton.EngWordsList.Add(line);
			}
		}

		System.Random random = new System.Random(Guid.NewGuid().GetHashCode());
		var amount = random.Next(10, 16);
		var totalWords = Singleton.EngWordsList.Count;
		List<string> wordsChosen = new List<string>();
		for(int i=0; i < amount; ++i)
		{
			wordsChosen.Add(Singleton.EngWordsList[random.Next(0, totalWords)]);
		}
		Singleton.choosenBoard.wordsOnBoard = wordsChosen;
		Singleton.boardUiEvents.CreateBoard(predefined: false);
		navigateAction(MenuMgr.MenuNavigationEnum.Home);
	}

	private void OnDisable()
	{
		buttonCreateRandom.clicked -= ButtonCreateRandom_clicked;
	}
	public void Hide() => this.gameObject.SetActive(false);

	public void Show() => this.gameObject.SetActive(true);

	public void OnNavigateToSet(Action<MenuMgr.MenuNavigationEnum> action) => navigateAction = action;
}
