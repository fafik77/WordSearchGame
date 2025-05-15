using Assets.Scripts.Internal;
using System;
using System.Collections;
using System.IO;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;
using SFB;
using System.Threading;
using Assets.Scripts.LoadFileContent;
using Assets.UI_Toolkit.Scripts;
using System.Collections.Generic;
using System.Linq;

public class ChooseBoardUI : MonoBehaviour, ICameraView
{
	private UIDocument ui;
	private Action<MenuMgr.MenuNavigationEnum> navigateToMenuAction;
	Button buttonLoadFile;
	Button buttonCreateRandom;
	Button buttonPickRandom;
	DropdownField dropdownLang;
	TextField SearchField;
	SliderInt sliderIntWordLength;
	Label LabelWordLength;
	TreeView treeViewCategories;
	Toggle toggleDiagonal;
	Toggle toggleReversed;


	Coroutine CategoriesForTreeCoroutine;



	private void Awake()
	{
		ui = GetComponent<UIDocument>();
		ui.enabled = true;
	}
	private void OnDisable()
	{
		buttonCreateRandom.clicked -= ButtonCreateRandom_clicked;
		buttonLoadFile.clicked -= ButtonLoadFile_clicked;

		dropdownLang.UnregisterValueChangedCallback(OnLanguageSelectionChange);
		sliderIntWordLength.UnregisterValueChangedCallback(OnWordLengthSliderChange);
	}
	private void OnEnable()
	{
		var root = ui.rootVisualElement;
		buttonLoadFile = root.Q<Button>("LoadFile");
		buttonCreateRandom = root.Q<Button>("CreateRandom");
		dropdownLang = root.Q<DropdownField>("Language");

		var Categories = root.Q<VisualElement>("Categories");
		treeViewCategories = Categories.Q<TreeView>();
		buttonPickRandom = Categories.Q<Button>("PickRandom");
		treeViewCategories.selectionType = SelectionType.Single;
		treeViewCategories.itemsChosen += TreeViewCategories_itemsChosen;
		toggleDiagonal = root.Q<Toggle>("Diagonal");
		toggleReversed = root.Q<Toggle>("Reversed");
		SearchField = root.Q<TextField>("Search");
		toggleDiagonal.value = Singleton.settingsPersistent.diagonalWords;
		toggleReversed.value = Singleton.settingsPersistent.reversedWords;

		///https://github.com/Unity-Technologies/ui-toolkit-manual-code-examples/blob/master/create-listviews-treeviews/PlanetsWindow.cs
		///populate tree
		CategoriesForTreeCoroutine = StartCoroutine(GetCategoriesRootsForTreeLangRutine(0.5f));

		var WordLengthElement = root.Q<VisualElement>("WordLength");
		sliderIntWordLength = WordLengthElement.Q<SliderInt>();
		LabelWordLength = WordLengthElement.Q<Label>("Amount");

		dropdownLang.RegisterValueChangedCallback(OnLanguageSelectionChange);
		buttonLoadFile.clicked += ButtonLoadFile_clicked;

		buttonCreateRandom.clicked += ButtonCreateRandom_clicked;
		sliderIntWordLength.RegisterValueChangedCallback(OnWordLengthSliderChange);
		if (Singleton.settingsPersistent.wordsMaxLenght > 2)
			sliderIntWordLength.value = Singleton.settingsPersistent.wordsMaxLenght;
		else
			Singleton.settingsPersistent.wordsMaxLenght = sliderIntWordLength.value;
		if (Singleton.settingsPersistent.LanguageWords != null)
			dropdownLang.value = Singleton.settingsPersistent.LanguageWords;
		else
			Singleton.settingsPersistent.LanguageWords = dropdownLang.value;
	}

	private void TreeViewCategories_itemsChosen(IEnumerable<object> obj)
	{
		var item = obj.First() as ICategoryOrGroup;
		var category = item as CategoryOnly;

		if (category == null)
		{	///group
			//treeViewCategories.ExpandItem()
		}
		else
		{   ///category
			Save_choosenBoard();
			try
			{
				Singleton.choosenBoard.LoadProvidedWords(category.words, 16);
			}
			catch (Exception e)
			{
				Singleton.boardUiEvents.onScreenNotification.setText($"Could not load {category.Name} in {Singleton.settingsPersistent.LanguageWords}!");
				Debug.LogError($"Could not load {category.Name} in {Singleton.settingsPersistent.LanguageWords}: " + e.GetType());
				return;
			}
			navigateToMenuAction(MenuMgr.MenuNavigationEnum.Home);
		}
		//Debug.Log(item);
	}

	private IEnumerator GetCategoriesRootsForTreeLangRutine(float delaySec = 0)
	{
		yield return new WaitForSeconds(delaySec);
		try
		{
			treeViewCategories.SetRootItems(Singleton.choosenBoard.GetCategoriesRootsForLang(Singleton.settingsPersistent.LanguageWords));
		}
		catch
		{	///show empty tree and re-throw
			treeViewCategories.SetRootItems(new List<TreeViewItemData<ICategoryOrGroup>>());
			treeViewCategories.Rebuild();
			Singleton.boardUiEvents.onScreenNotification.setText($"No Categories found for {Singleton.settingsPersistent.LanguageWords}!");

			throw; //pass higher up
		}

		treeViewCategories.makeItem = () => new Label();
		treeViewCategories.bindItem = (VisualElement element, int index) =>
			(element as Label).text = treeViewCategories.GetItemDataForIndex<ICategoryOrGroup>(index).Name;
		treeViewCategories.Rebuild();
	}
	private void Save_choosenBoard()
	{
		Singleton.settingsPersistent.reversedWords = toggleReversed.value;
		Singleton.settingsPersistent.diagonalWords = toggleDiagonal.value;
	}


	private void ButtonLoadFile_clicked()
	{
		var filePicked = StandaloneFileBrowser.OpenFilePanel("Pick Board File", ".", "txt", false);
		//--> https://docs.unity3d.com/Manual/Input.html (change the input system to the new one to fix small annoyance: when double clicking a file it loads it and clicks on a tile)
		//user has sellected a file?, is it correct ?
		foreach (var file in filePicked)
		{
			Save_choosenBoard();
			//Debug.Log($"{file}");
			var success = Singleton.choosenBoard.LoadFromFile(file);
			if (success)
			{
				navigateToMenuAction(MenuMgr.MenuNavigationEnum.Home);
			}
			else
			{
				Singleton.boardUiEvents.onScreenNotification.setText($"Failed to load: {file}");
			}
			return; //accept only 1 file
		}
	}

	private void ButtonCreateRandom_clicked()
	{
		var locale = LocalizationSettings.SelectedLocale; //by default pl-PL
		Save_choosenBoard();
		try
		{
			Singleton.choosenBoard.CreateRandom(Singleton.settingsPersistent.LanguageWords, Singleton.settingsPersistent.wordsMaxLenght);
		}
		catch (Exception e)
		{
			Singleton.boardUiEvents.onScreenNotification.setText($"No Dictionary found for {Singleton.settingsPersistent.LanguageWords}!");
			Debug.LogError($"No Dictionary found for {Singleton.settingsPersistent.LanguageWords}: " + e.GetType());
			return;
		}
		navigateToMenuAction(MenuMgr.MenuNavigationEnum.Home);
	}
	private void OnWordLengthSliderChange(ChangeEvent<int> change)
	{
		Singleton.settingsPersistent.wordsMaxLenght = change.newValue;
		LabelWordLength.text = change.newValue.ToString();
	}

	private void OnLanguageSelectionChange(ChangeEvent<string> change)
	{
		Singleton.settingsPersistent.LanguageWords = change.newValue;
		///populate tree again
		StopCoroutine(CategoriesForTreeCoroutine);
		CategoriesForTreeCoroutine = StartCoroutine(GetCategoriesRootsForTreeLangRutine(0.1f));
	}

	
	public void Hide() => this.gameObject.SetActive(false);

	public void Show() => this.gameObject.SetActive(true);

	public void OnNavigateToSet(Action<MenuMgr.MenuNavigationEnum> action) => navigateToMenuAction = action;
}
