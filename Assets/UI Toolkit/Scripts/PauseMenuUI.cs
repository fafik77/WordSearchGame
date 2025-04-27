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
		buttonContinue = ui.rootVisualElement.Q<Button>("Continue");
		buttonContinue.Focus();
		buttonNewGame = ui.rootVisualElement.Q<Button>("NewGame");
		buttonSettings = ui.rootVisualElement.Q<Button>("Settings");
		buttonQuit = ui.rootVisualElement.Q<Button>("Quit");
		buttonCCamera = ui.rootVisualElement.Q<Button>("CCamera");

		buttonContinue.clicked += () => navigateAction(MenuMgr.MenuNavigationEnum.Back);
		buttonNewGame.clicked += () => navigateAction(MenuMgr.MenuNavigationEnum.NewGame);
		buttonSettings.clicked += () => navigateAction(MenuMgr.MenuNavigationEnum.Settings);
		buttonCCamera.clicked += () => { mainCameraZoom.ResetCamera(); };

		buttonQuit.clicked += () => { Singleton.scenesStruct.SwitchToScene("Assets/Scenes/MainMenuScene.unity"); };
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
