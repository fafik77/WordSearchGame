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
		ui.enabled = true;
	}
	private void OnEnable()
	{
		ui = GetComponent<UIDocument>();

		if (ui == null || ui.rootVisualElement == null) return;
		buttonContinue = ui.rootVisualElement.Q<Button>("Continue");
		buttonNewGame= ui.rootVisualElement.Q<Button>("NewGame");
		buttonSettings = ui.rootVisualElement.Q<Button>("Settings");

		buttonContinue.clicked += () => navigateAction(MenuMgr.MenuNavigationEnum.Back);
		buttonNewGame.clicked += () => navigateAction(MenuMgr.MenuNavigationEnum.NewGame);
		buttonSettings.clicked += () => navigateAction(MenuMgr.MenuNavigationEnum.Settings);
	}

	public void Hide()
	{
		this.gameObject.SetActive(false);
		//ui.enabled = false;
	}

	public void Show()
	{
		//ui.enabled = true;
		this.gameObject.SetActive(true);
		//OnEnable();
	}

	public void OnNavigateToSet(Action<MenuMgr.MenuNavigationEnum> action) => navigateAction = action;
}
