using Assets.Scripts.Internal;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;
using SFB;
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
	Label TreeEmptyLabel;


	Coroutine CategoriesForTreeCoroutine;
	CategoryOrGroup treeViewItemDatasOrig;
	IList<TreeViewItemData<ICategoryOrGroup>> treeViewItemDatasFiltered;
	string SearchField_oldValue;

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
		root.Focus();
		buttonLoadFile = root.Q<Button>("LoadFile");
		buttonLoadFile.Focus();
		buttonCreateRandom = root.Q<Button>("CreateRandom");
		dropdownLang = root.Q<DropdownField>("Language");

		var Categories = root.Q<VisualElement>("Categories");
		treeViewCategories = Categories.Q<TreeView>();
		TreeEmptyLabel = Categories.Q<Label>("TreeEmpty");
		TreeEmptyLabel.style.display = DisplayStyle.None;
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
		buttonPickRandom.clicked += ButtonPickRandom_clicked;
		SearchField.RegisterCallback<KeyDownEvent>(evt =>
		{
			if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Tab)
			{
				if (SearchField_oldValue != SearchField.value)
				{
					SearchField_oldValue = SearchField.value;
					SearchFieldOnSubmit(SearchField.value);
				}
			}
		}, TrickleDown.TrickleDown);
		root.RegisterCallback<KeyDownEvent>(rootOnKeyDown, TrickleDown.TrickleDown);


		if (Singleton.settingsPersistent.wordsMaxLenght > 2)
			sliderIntWordLength.value = Singleton.settingsPersistent.wordsMaxLenght;
		else
			Singleton.settingsPersistent.wordsMaxLenght = sliderIntWordLength.value;
		if (Singleton.settingsPersistent.LanguageWords != null)
			dropdownLang.value = Singleton.settingsPersistent.LanguageWords;
		else
			Singleton.settingsPersistent.LanguageWords = dropdownLang.value;
	}

	private void ButtonPickRandom_clicked()
	{
		var catCount = Singleton.choosenBoard.CategoriesInCurrLang.AllCategories.Count;
		System.Random rand = new System.Random();
		var randomCatId = rand.Next(0, catCount);
		var randomCat = Singleton.choosenBoard.CategoriesInCurrLang.AllCategories[randomCatId];
		SearchFieldOnSubmit(randomCat.Name);
	}

	private void SearchFieldOnSubmit(string text) => PopulateTreeViewCategoriesFiltered(text);
	private void rootOnKeyDown(KeyDownEvent evt)
	{
		if ((evt.commandKey || evt.ctrlKey))
		{
			if (evt.keyCode == KeyCode.F)
			{
				SearchField.Focus();
				evt.StopPropagation();
			}
			else if (evt.keyCode == KeyCode.O)
			{
#if UNITY_EDITOR
				Debug.LogWarning("Ctr+O was pressed in editor, opening TWO: open file dialogs, and They were confused");
#endif
				evt.StopPropagation();
				ButtonLoadFile_clicked();
			}
			else if (evt.keyCode == KeyCode.F5)
			{
				evt.StopPropagation();
				StopCoroutine(CategoriesForTreeCoroutine);
				CategoriesForTreeCoroutine = StartCoroutine(GetCategoriesRootsForTreeLangRutine(delaySec: 0.1f, forceRefresh: true));
			}
		}
	}



	private void TreeViewCategories_itemsChosen(IEnumerable<object> obj)
	{
		var item = obj.First() as ICategoryOrGroup;
		var category = item as CategoryOnly;

		if (category == null)
		{   ///group
			//treeViewCategories.ExpandItem();
		}
		else
		{   ///category
			LoadCategoryForBoard(category);
		}
	}
	private bool LoadCategoryForBoard(CategoryOnly category)
	{
		Save_choosenBoard();
		try
		{
			Singleton.choosenBoard.LoadProvidedWords(category.words, 16);
		}
		catch (Exception e)
		{
			var err_load = LocalizationHelper.GetTranslation("error.failed_to_load_category");
			if (err_load.Length == 0) err_load = $"Could not load {category.Name} in {Singleton.settingsPersistent.LanguageWords}!";
			Singleton.boardUiEvents.onScreenNotification.setText(err_load);
			Debug.LogError(err_load + ": " + e.GetType());
			return false;
		}
		navigateToMenuAction(MenuMgr.MenuNavigationEnum.Home);
		return true;
	}

	private IEnumerator GetCategoriesRootsForTreeLangRutine(float delaySec = 0, bool forceRefresh = false)
	{
		yield return new WaitForSeconds(delaySec);
		treeViewItemDatasOrig = null;
		try
		{
			treeViewItemDatasOrig = Singleton.choosenBoard.GetCategoriesForLang(Singleton.settingsPersistent.LanguageWords, forceRefresh);
		}
		catch
		{	///show empty tree and re-throw
			Singleton.boardUiEvents.onScreenNotification.setText($"No Categories found for {Singleton.settingsPersistent.LanguageWords}!");
			throw; //pass higher up
		}
		finally
		{
			PopulateTreeViewCategoriesFiltered(string.Empty);
		}
	}
	private void PopulateTreeViewCategoriesFiltered(string searchFilter)
	{
		int id = 0;
		if (treeViewItemDatasFiltered == null)
			treeViewItemDatasFiltered = new List<TreeViewItemData<ICategoryOrGroup>>();
		treeViewItemDatasFiltered.Clear();

		if (treeViewItemDatasOrig != null)
			treeViewItemDatasFiltered = treeViewItemDatasOrig.GetRoots(ref id, searchFilter.ToLower());

		///show: "Tree has no items to display"
		if (treeViewItemDatasFiltered.Count == 0)
		{
			TreeEmptyLabel.style.display = DisplayStyle.Flex;
		}
		else TreeEmptyLabel.style.display = DisplayStyle.None;
		treeViewCategories.SetRootItems(treeViewItemDatasFiltered);
		treeViewCategories.makeItem = () => new Label();
		treeViewCategories.bindItem = (VisualElement element, int index) =>
			(element as Label).text = treeViewCategories.GetItemDataForIndex<ICategoryOrGroup>(index).Name;
		treeViewCategories.Rebuild();
		///Tree has filtered items
		if (!string.IsNullOrEmpty(searchFilter) && treeViewItemDatasFiltered.Count <= 5)
		{
			treeViewCategories.ExpandRootItems();
		}
	}

	private void Save_choosenBoard()
	{
		Singleton.settingsPersistent.reversedWords = toggleReversed.value;
		Singleton.settingsPersistent.diagonalWords = toggleDiagonal.value;
	}


	private void ButtonLoadFile_clicked()
	{
		string loadfile_pick_board_file = LocalizationHelper.GetTranslation("menu.board.loadfile_pick_board_file");
		if (loadfile_pick_board_file.Length == 0) loadfile_pick_board_file = "Pick Board File";

		var filePicked = StandaloneFileBrowser.OpenFilePanel(loadfile_pick_board_file, ".", "txt", false);
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
				string error_failed = LocalizationHelper.GetTranslation("error.failed_to_load_file", file);
				if (error_failed.Length == 0) error_failed = $"Failed to load: {file}";
				Singleton.boardUiEvents.onScreenNotification.setText(error_failed);
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
			var err_no_dict = LocalizationHelper.GetTranslation("error.no_dictionary", Singleton.settingsPersistent.LanguageWords);
			if (err_no_dict.Length == 0) err_no_dict = $"No Dictionary Found For {Singleton.settingsPersistent.LanguageWords}!";
			Singleton.boardUiEvents.onScreenNotification.setText(err_no_dict);
			Debug.LogError(err_no_dict + ": " + e.GetType());
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
		Singleton.settingsPersistent.LanguageWords = dropdownLang.value;
		///populate tree again
		StopCoroutine(CategoriesForTreeCoroutine);
		CategoriesForTreeCoroutine = StartCoroutine(GetCategoriesRootsForTreeLangRutine(0.1f));
	}

	
	public void Hide() => this.gameObject.SetActive(false);

	public void Show() => this.gameObject.SetActive(true);

	public void OnNavigateToSet(Action<MenuMgr.MenuNavigationEnum> action) => navigateToMenuAction = action;
}
