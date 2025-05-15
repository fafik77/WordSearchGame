using Assets.Scripts.Internal;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public class SettingsUi : MonoBehaviour, ICameraView
{
	//[SerializeField] private GameObject PreviewTiles;
	//private List<LetterDisplayScript> previewDisplayScripts;
	private Action<MenuMgr.MenuNavigationEnum> navigateAction;

	[SerializeField]
	private DeadCenterDisplay deadCenterDisplay;

	private UIDocument ui;

	private Button letterCaseButton;
	private SliderInt zoomDeadZoneSlider;
	Toggle toggleDiagonal;
	Toggle toggleReversed;

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

		if (Singleton.settingsPersistent.ZoomDeadZoneSize <= 1) Singleton.settingsPersistent.ZoomDeadZoneSize = 64;
		zoomDeadZoneSlider.SetValueWithoutNotify(Singleton.settingsPersistent.ZoomDeadZoneSize);

		toggleDiagonal = root.Q<Toggle>("Diagonal");
		toggleReversed = root.Q<Toggle>("Reversed");
		toggleDiagonal.value = Singleton.wordList.diagonalWords;
		toggleReversed.value = Singleton.wordList.reversedWords;


		LetterCaseLoad(Singleton.settingsPersistent.upperCase);
	}
	void OnZoomDeadZoneSliderChange(ChangeEvent<int> change)
	{
		deadCenterDisplay.ShowForTime(seconds: 2f);
		deadCenterDisplay.SetCenterSize(change.newValue);
	}

	//private void BackwardWordsToggle() => BackwardWordsSet(!backwardWords);

	//private void BackwardWordsSet(bool backward)
	//{
	//	backwardWords = backward;
	//	backwardWordsButton.text = backwardWords ? local_Yes : local_No;
	//	if (!backwardWords && char.ToUpper(previewDisplayScripts[0].Letter) == 'A') return; //ok
	//	if (backwardWords && char.ToUpper(previewDisplayScripts[0].Letter) != 'A') return; //ok
	//	//switch needed
	//	int idx = 1;
	//	if (previewDisplayScripts[1].Letter == ' ') idx = 3;
	//	char letter = previewDisplayScripts[0].Letter;
	//	previewDisplayScripts[0].Letter = previewDisplayScripts[idx].Letter;
	//	previewDisplayScripts[idx].Letter = letter;
	//}

	private void OnDisable()
	{
		if (letterCaseButton != null)
		{
			letterCaseButton.clicked -= LetterCaseToggle;
			//diagonalWordsButton.clicked -= DiagonalWordsToggle;
			//backwardWordsButton.clicked -= BackwardWordsToggle;
		}
		zoomDeadZoneSlider.UnregisterValueChangedCallback(OnZoomDeadZoneSliderChange);
	}

	//private void DiagonalWordsToggle() => DiagonalWordsSet(!diagonalWords);
	//private void DiagonalWordsSet(bool diagonal)
	//{
	//	diagonalWords = diagonal;
	//	diagonalWordsButton.text = diagonalWords ? local_Yes : local_No;
	//	if (diagonal && previewDisplayScripts[3].Letter != ' ') return; //ok
	//	if (!diagonal && previewDisplayScripts[3].Letter == ' ') return; //ok
	//	//switch needed
	//	char temp = previewDisplayScripts[1].Letter;
	//	previewDisplayScripts[1].Letter = previewDisplayScripts[3].Letter;
	//	previewDisplayScripts[3].Letter = temp;
	//}


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
