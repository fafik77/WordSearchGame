using Assets.Scripts.Internal;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PauseMenuUI : MonoBehaviour, ICameraView
{
	private UIDocument ui;
	private Button buttonContinue;
	private Button buttonNewGame;
	private Button buttonSettings;
	private Button buttonQuit;
	private Button buttonCCamera;
	private Action<MenuMgr.MenuNavigationEnum> navigateAction;
	private Camera mainCamera;
	private CameraZoom mainCameraZoom;


	private void Awake()
	{
		ui = GetComponent<UIDocument>();
		ui.enabled = true;
		mainCamera = Camera.main;
		mainCameraZoom = mainCamera.GetComponent<CameraZoom>();
	}
	private void OnEnable()
	{
		ui = GetComponent<UIDocument>();

		if (ui == null || ui.rootVisualElement == null) return;
		var root = ui.rootVisualElement;
		buttonContinue = root.Q<Button>("Continue");
		buttonContinue.Focus();
		buttonNewGame = root.Q<Button>("NewGame");
		buttonSettings = root.Q<Button>("Settings");
		buttonQuit = root.Q<Button>("Quit");
		buttonCCamera = root.Q<Button>("CCamera");

		buttonContinue.clicked += () => navigateAction(MenuMgr.MenuNavigationEnum.Back);
		buttonNewGame.clicked += () => navigateAction(MenuMgr.MenuNavigationEnum.NewGame);
		buttonSettings.clicked += () => navigateAction(MenuMgr.MenuNavigationEnum.Settings);
		buttonCCamera.clicked += () => { mainCameraZoom.ResetCamera(); };

		buttonQuit.clicked += () => {
			string pathSettings = Singleton.settingsPersistent_GetSavePath();
			Singleton.settingsPersistent_SaveJson(pathSettings);
			Singleton.SceneMgr.SwitchToScene("Assets/Scenes/MainMenuScene.unity"); 
		};
	}

	public void Hide()
	{
		this.gameObject.SetActive(false);
	}

	public void Show()
	{
		this.gameObject.SetActive(true);
	}

	public void OnNavigateToSet(Action<MenuMgr.MenuNavigationEnum> action) => navigateAction = action;
}
