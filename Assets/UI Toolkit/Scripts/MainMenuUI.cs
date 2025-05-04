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
	private Action<MenuMgr.MenuNavigationEnum> navigateAction;


	private void OnEnable()
	{
		Application.targetFrameRate = 30;
		ui = GetComponent<UIDocument>();
		ui.enabled = true;
		buttonStart = ui.rootVisualElement.Q<Button>("Start");
		buttonQuit = ui.rootVisualElement.Q<Button>("Quit");

		buttonQuit.clicked += () => {
			Singleton.SceneMgr.SwitchToScene("Assets/Scenes/GameScene.unity");
			Application.Quit();
		};
		buttonStart.clicked += ButtonStart_clicked;
	}

	private void ButtonStart_clicked()
	{
		navigateAction(MenuMgr.MenuNavigationEnum.NewGame);
		//SceneManager.LoadScene("Scenes/GameScene", LoadSceneMode.Additive);
	}

	private void Start()
	{
		buttonStart.Focus();
	}

	//IEnumerator LoadSceneAsync()
	//{
	//	//var scene = SceneManager.LoadSceneAsync("GameScene", LoadSceneMode.Additive);
	//	//if (scene.isDone)
	//	//	LoadScene_completed(null);
	//	//else
	//	//	scene.completed += LoadScene_completed;
	//	yield return null;
	//}

	private void LoadScene_completed(AsyncOperation obj)
	{
		//if (Singleton.scenesStruct.GameScene.path == null)
		//{
		//	var a = SceneManager.GetSceneByPath("Assets/Scenes/GameScene.unity");
		//	Singleton.scenesStruct.GameScene = a;
		//}
		//if (Singleton.scenesStruct.MainMenuScene.path == null)
		//{
		//	var a = SceneManager.GetSceneByName("MainMenuScene");
		//	Singleton.scenesStruct.MainMenuScene = a;
		//}
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
	public void OnNavigateToSet(Action<MenuMgr.MenuNavigationEnum> action) => navigateAction = action;
}
