using Assets.Scripts.Internal;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using SFB;
using System.IO;
using Exceptions;

public class PauseMenuUI : MonoBehaviour, ICameraView
{
	[SerializeField]
	private UIDocument PauseMenuHelp;
	[SerializeField]
	private BoardTiles boardTiles;


	private UIDocument ui;
	private Button buttonContinue;
	private Button buttonNewGame;
	private Button buttonSettings;
	private Button buttonQuit;
	private Button buttonCCamera;
	private Button buttonSaveBoard;
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
	private void OnDisable()
	{
		if (PauseMenuHelp) { PauseMenuHelp.gameObject.SetActive(false); }
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
		buttonSaveBoard = root.Q<Button>("SaveBoard");

		buttonContinue.clicked += () => navigateAction(MenuMgr.MenuNavigationEnum.Back);
		buttonNewGame.clicked += () => navigateAction(MenuMgr.MenuNavigationEnum.NewGame);
		buttonSettings.clicked += () => navigateAction(MenuMgr.MenuNavigationEnum.Settings);
		buttonCCamera.clicked += () => { mainCameraZoom.ResetCamera(); };
		buttonSaveBoard.clicked += ButtonSaveBoard_clicked;

		buttonQuit.clicked += () => {
			string pathSettings = Singleton.settingsPersistent_GetSavePath();
			Singleton.settingsPersistent_SaveJson(pathSettings);
			Singleton.SceneMgr.SwitchToScene("Assets/Scenes/MainMenuScene.unity"); 
		};
		if (PauseMenuHelp) { PauseMenuHelp.gameObject.SetActive(true); }
	}

	private void ButtonSaveBoard_clicked()
	{
		if (!boardTiles) throw new NotFoundException("boardTiles not set");

		var timeNowStr = DateTime.Now.ToString("HH-mm-ss");
		var userFile = StandaloneFileBrowser.SaveFilePanel("Save Board", ".", $"{timeNowStr}.txt", "txt");
		var userDir = System.IO.Path.GetDirectoryName(userFile);
		if (userFile.Length!=0 && Directory.Exists(userDir))
		{
			var boardContent = boardTiles.ExportBoard();
			File.WriteAllText(userFile, boardContent);
		}
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
