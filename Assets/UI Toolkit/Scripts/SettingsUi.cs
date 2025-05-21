using Assets.Scripts.Internal;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

public class SettingsUi : MonoBehaviour, ICameraView
{
	private Action<MenuMgr.MenuNavigationEnum> navigateAction;

	[SerializeField]
	private DeadCenterDisplay deadCenterDisplay;
	[SerializeField]
	private UIDocument PauseMenuHelp;

	private UIDocument ui;

	private Button letterCaseButton;
	private SliderInt zoomDeadZoneSlider;
	Label DiagonalVisualText;
	Label ReversedVisualText;
	DropdownField dropdownLang;
	DropdownField dropdownResolution;

	static readonly string[] CrossTickMarks = new string[] { "☓", "✔" };
	static readonly Color[] CrossTickMarksColor = new Color[]{
		ColorExtension.FromHexRGB(new Color(),0xF55D22),
		ColorExtension.FromHexRGB(new Color(),0x00D9FF)
	};

	private void Awake()
	{
		ui = GetComponent<UIDocument>();
		ui.enabled = true;
	}

	private void OnEnable()
	{
		ui = GetComponent<UIDocument>();

		if (ui == null || ui.rootVisualElement == null) return;
		var root = ui.rootVisualElement;
		letterCaseButton = root.Q<Button>("CaseButton");
		letterCaseButton.clicked += LetterCaseToggle;

		zoomDeadZoneSlider = root.Q<SliderInt>("ZoomDeadZoneSlider");
		zoomDeadZoneSlider.RegisterValueChangedCallback(OnZoomDeadZoneSliderChange);
		zoomDeadZoneSlider.highValue = (int)(0.2 * Screen.width);

		if (Singleton.settingsPersistent.ZoomDeadZoneSize <= 1)
			Singleton.settingsPersistent.ZoomDeadZoneSize = 64;
		zoomDeadZoneSlider.SetValueWithoutNotify(Singleton.settingsPersistent.ZoomDeadZoneSize);

		dropdownLang = root.Q<DropdownField>("Language");
		dropdownLang.choices = LanguagesManager.Languages.Select(item => item.Key).ToList();
		dropdownLang.RegisterValueChangedCallback(OnLangChange);
		dropdownResolution = root.Q<DropdownField>("Resolution");
		dropdownResolution.RegisterValueChangedCallback(OnResolutionChange);
		if (!string.IsNullOrEmpty(Singleton.settingsPersistent.LanguageUi))
			dropdownLang.SetValueWithoutNotify(Singleton.settingsPersistent.LanguageUi);

		DiagonalVisualText = root.Q<VisualElement>("Diagonal").Q<Label>("Tick");
		ReversedVisualText = root.Q<VisualElement>("Reversed").Q<Label>("Tick");
		DiagonalVisualText.text = CrossTickMarks[Singleton.wordList.diagonalWords ? 1 : 0];
		DiagonalVisualText.style.color = CrossTickMarksColor[Singleton.wordList.diagonalWords ? 1 : 0];
		ReversedVisualText.text = CrossTickMarks[Singleton.wordList.reversedWords ? 1 : 0];
		ReversedVisualText.style.color = CrossTickMarksColor[Singleton.wordList.reversedWords ? 1 : 0];


		LetterCaseLoad(Singleton.settingsPersistent.upperCase);
		if (PauseMenuHelp) { PauseMenuHelp.gameObject.SetActive(true); }
	}

	private void OnResolutionChange(ChangeEvent<string> evt)
	{
		throw new NotImplementedException("Settings > OnResolutionChange");
		//var resol = dropdownResolution.value;
		//Singleton.settingsPersistent.resolution
	}

	private void OnLangChange(ChangeEvent<string> evt)
	{
		Singleton.settingsPersistent.LanguageUi = dropdownLang.value;
		//var curr = LocalizationSettings.SelectedLocale;
		//var locales = LocalizationSettings.AvailableLocales.Locales;

		LanguagesManager.SetLocale(dropdownLang.value);
	}

	void OnZoomDeadZoneSliderChange(ChangeEvent<int> change)
	{
		deadCenterDisplay.ShowForTime(seconds: 2f);
		deadCenterDisplay.SetCenterSize(change.newValue);
	}

	private void OnDisable()
	{
		if (letterCaseButton != null)
		{
			letterCaseButton.clicked -= LetterCaseToggle;
		}
		zoomDeadZoneSlider.UnregisterValueChangedCallback(OnZoomDeadZoneSliderChange);
		if (PauseMenuHelp) { PauseMenuHelp.gameObject.SetActive(false); }
	}


	private void LetterCaseToggle() => LetterCaseSet(!Singleton.settingsPersistent.upperCase);

	private void LetterCaseSet(bool isUpperCase)
	{
		Singleton.settingsPersistent.upperCase = isUpperCase;
		LetterCaseLoad(isUpperCase);
		Singleton.boardUiEvents.BoardSetCase(isUpperCase);
	}
	private void LetterCaseLoad(bool isUpperCase)
	{
		if (isUpperCase)
			letterCaseButton.text = "B";
		else
			letterCaseButton.text = "b";
	}


	public void Hide()
	{
		this.gameObject.SetActive(false);
		deadCenterDisplay.Hide();
	}

	public void Show()
	{
		this.gameObject.SetActive(true);
	}

	public void OnNavigateToSet(Action<MenuMgr.MenuNavigationEnum> action) => navigateAction = action;
}
