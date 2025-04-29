using Assets.Scripts.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Localization.Settings;
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
	SliderInt sliderIntWordLength;
	Label LabelWordLength;


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
		var WordLengthElement = root.Q<VisualElement>("WordLength");

		sliderIntWordLength = WordLengthElement.Q<SliderInt>();
		LabelWordLength = WordLengthElement.Q<Label>("Amount");

		dropdownLang.RegisterValueChangedCallback(OnLanguageSelectionChange);

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

	private void ButtonCreateRandom_clicked()
	{
		var locale = LocalizationSettings.SelectedLocale; //by default pl-PL

		try
		{
			Singleton.choosenBoard.CreateRandom(Singleton.settingsPersistent.LanguageWords, Singleton.settingsPersistent.wordsMaxLenght);
		}
		catch (Exception e)
		{
			Singleton.boardUiEvents.onScreenNotification.setText($"No Dictionary found for {Singleton.settingsPersistent.LanguageWords}!");
			Debug.LogError($"No Dictionary found for {Singleton.settingsPersistent.LanguageWords}: " + e);
			return;
		}
		navigateAction(MenuMgr.MenuNavigationEnum.Home);
	}
	private void OnWordLengthSliderChange(ChangeEvent<int> change)
	{
		Singleton.settingsPersistent.wordsMaxLenght = change.newValue;
		LabelWordLength.text = change.newValue.ToString();
	}

	private void OnLanguageSelectionChange(ChangeEvent<string> change)
	{
		Singleton.settingsPersistent.LanguageWords = change.newValue;
	}

	private void OnDisable()
	{
		buttonCreateRandom.clicked -= ButtonCreateRandom_clicked;

		dropdownLang.UnregisterValueChangedCallback(OnLanguageSelectionChange);
		sliderIntWordLength.UnregisterValueChangedCallback(OnWordLengthSliderChange);
	}
	public void Hide() => this.gameObject.SetActive(false);

	public void Show() => this.gameObject.SetActive(true);

	public void OnNavigateToSet(Action<MenuMgr.MenuNavigationEnum> action) => navigateAction = action;
}
