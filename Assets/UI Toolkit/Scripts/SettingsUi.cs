using Assets.Scripts.Internal;
using Exceptions;
using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
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
	private bool letterCaseUpper = true;

	private SliderInt zoomDeadZoneSlider;
	private int zoomDeadZoneSliderValue;

	//private Button diagonalWordsButton;
	//private bool diagonalWords = false;

	//private Button backwardWordsButton;
	//private bool backwardWords = false;
	//private string local_Yes, local_No;

	private void Awake()
	{
		ui = GetComponent<UIDocument>();
		ui.enabled = true;
	}

	private void OnEnable()
	{
		ui = GetComponent<UIDocument>();

		if (ui == null || ui.rootVisualElement == null) return;

		letterCaseButton = ui.rootVisualElement.Q<Button>("CaseButton");
		letterCaseButton.clicked += LetterCaseToggle;

		zoomDeadZoneSlider = ui.rootVisualElement.Q<SliderInt>("ZoomDeadZoneSlider");
		zoomDeadZoneSlider.RegisterValueChangedCallback(OnZoomDeadZoneSliderChange);

		//diagonalWordsButton = ui.rootVisualElement.Q<Button>("DiagonalButton");
		//diagonalWordsButton.clicked += DiagonalWordsToggle;

		//backwardWordsButton = ui.rootVisualElement.Q<Button>("BackwardsButton");
		//backwardWordsButton.clicked += BackwardWordsToggle;
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


	private void LetterCaseToggle() => LetterCaseSet(!letterCaseUpper);

	private void LetterCaseSet(bool isUpperCase)
	{
		letterCaseUpper = isUpperCase;
		if (isUpperCase)
		{
			letterCaseButton.text = "B";
			Singleton.boardUiEvents.BoardSetCase(Singleton.CaseEnum.UpperCase);
		}
		else
		{
			letterCaseButton.text = "b";
			Singleton.boardUiEvents.BoardSetCase(Singleton.CaseEnum.LowerCase);
		}
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
