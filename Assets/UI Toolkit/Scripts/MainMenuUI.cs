using Assets.Scripts.Internal;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using static Singleton;

public class MainMenuUI : MonoBehaviour, ICameraView
{
	UIDocument ui;
	Button buttonStart;
	Button buttonQuit;
	Button buttonSettings;
	private Action<MenuMgr.MenuNavigationEnum> navigateToMenuAction;


	private void OnEnable()
	{
		Application.targetFrameRate = 30;
		ui = GetComponent<UIDocument>();
		ui.enabled = true;
		buttonStart = ui.rootVisualElement.Q<Button>("Start");
		buttonQuit = ui.rootVisualElement.Q<Button>("Quit");
		buttonSettings = ui.rootVisualElement.Q<Button>("Settings");

		buttonQuit.clicked += () => {
			string pathSettings = Singleton.settingsPersistent_GetSavePath();
			Singleton.settingsPersistent_SaveJson(pathSettings);
			Application.Quit();
			Singleton.SceneMgr.SwitchToScene("Assets/Scenes/GameScene.unity");
		};
		buttonStart.clicked += () => navigateToMenuAction(MenuMgr.MenuNavigationEnum.NewGame);
		buttonSettings.clicked += () => navigateToMenuAction(MenuMgr.MenuNavigationEnum.Settings);
	}

	private void Start()
	{
		buttonStart.Focus();
		string pathSettings = Singleton.settingsPersistent_GetSavePath();
		Singleton.settingsPersistent_loadJson(pathSettings);
	}

	public void Hide()
	{
		this.gameObject.SetActive(false);
	}
	public void Show()
	{
		this.gameObject.SetActive(true);
		buttonStart.Focus();
	}
	public void OnNavigateToSet(Action<MenuMgr.MenuNavigationEnum> action) => navigateToMenuAction = action;
}
