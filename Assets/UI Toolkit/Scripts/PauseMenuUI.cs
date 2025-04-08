using Assets.Scripts.Internal;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public class PauseMenuUI : MonoBehaviour, ICameraView
{
	private UIDocument ui;
	private Button buttonContinue;
	private Button buttonNewGame;
	private Button buttonSettings;
	private Action<MenuMgr.MenuNavigationEnum> navigateAction;


	private void Awake()
	{
		ui = GetComponent<UIDocument>();
		ui.enabled = false;
	}
	private void OnEnable()
	{
		ui = GetComponent<UIDocument>();
		buttonContinue = ui.rootVisualElement.Q<Button>("Continue");
		buttonNewGame= ui.rootVisualElement.Q<Button>("NewGame");
		buttonSettings = ui.rootVisualElement.Q<Button>("Settings");

		buttonContinue.clicked += ButtonContinue_clicked;
		buttonNewGame.clicked += ButtonNewGame_clicked;
		buttonSettings.clicked += ButtonSettings_clicked;
	}
	private void OnDisable()
	{
		buttonContinue.clicked += ButtonContinue_clicked;
		buttonNewGame.clicked += ButtonNewGame_clicked;
		buttonSettings.clicked += ButtonSettings_clicked;
	}

	private void ButtonNewGame_clicked()
	{
		navigateAction(MenuMgr.MenuNavigationEnum.NewGame);
	}

	private void ButtonContinue_clicked()
	{
		navigateAction(MenuMgr.MenuNavigationEnum.Back);
	}

	private void ButtonSettings_clicked()
	{
		navigateAction(MenuMgr.MenuNavigationEnum.Settings);
	}

	
	public void Hide()
	{
		this.enabled = false;
		ui.enabled = false;
		OnDisable();
	}

	public void Show()
	{
		this.enabled = true;
		ui.enabled = true;
		OnEnable();
	}

	public void OnNavigateToSet(Action<MenuMgr.MenuNavigationEnum> action)
	{
		navigateAction = action;
	}
}
