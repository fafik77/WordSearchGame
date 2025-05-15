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
	private Action<MenuMgr.MenuNavigationEnum> navigateToMenuAction;


	private void OnEnable()
	{
		Application.targetFrameRate = 30;
		ui = GetComponent<UIDocument>();
		ui.enabled = true;
		buttonStart = ui.rootVisualElement.Q<Button>("Start");
		buttonQuit = ui.rootVisualElement.Q<Button>("Quit");

		buttonQuit.clicked += () => {
			string pathSettings = Singleton.settingsPersistent_GetSavePath();
			Singleton.settingsPersistent_SaveJson(pathSettings);
			Application.Quit();
			Singleton.SceneMgr.SwitchToScene("Assets/Scenes/GameScene.unity");
		};
		buttonStart.clicked += ButtonStart_clicked;
	}

	private void ButtonStart_clicked()
	{
		navigateToMenuAction(MenuMgr.MenuNavigationEnum.NewGame);
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
