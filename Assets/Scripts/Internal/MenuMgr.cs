using Assets.Scripts.Internal;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;

public class MenuMgr : MonoBehaviour
{
	[SerializeField] private InGameUI ingameUI;
	[SerializeField] private PauseMenuUI pauseMenuUI;
	[SerializeField] private ChooseBoardUI chooseBoardUI;
	[SerializeField] private SettingsUi settingsUi;
	[SerializeField] private MainMenuUI mainMenuUI;
	[SerializeField] private OnScreenNotification onScreenNotification;

	Stack<MonoBehaviour> menusStack = new Stack<MonoBehaviour>();
	public OnScreenNotification OnScreenNotification { get { return onScreenNotification; } }

	public enum MenuNavigationEnum
	{
		None,
		Home,
		Back,
		Settings,
		PauseMenu,
		NewGame,
	}

	private void Awake()
	{
		Singleton.clickAndDrag.menuMgrInGame = this;
		if (!ingameUI)
			ingameUI = this.gameObject.GetComponentInChildren<InGameUI>(true);
		if (!pauseMenuUI)
			pauseMenuUI = this.gameObject.GetComponentInChildren<PauseMenuUI>(true);
		if (!chooseBoardUI)
			chooseBoardUI = this.gameObject.GetComponentInChildren<ChooseBoardUI>(true);
		if (!settingsUi)
			settingsUi = this.gameObject.GetComponentInChildren<SettingsUi>(true);
		if (!mainMenuUI)
			mainMenuUI = this.gameObject.GetComponentInChildren<MainMenuUI>(true);
		if (!onScreenNotification)
			onScreenNotification = this.gameObject.GetComponentInChildren<OnScreenNotification>(true);
		Singleton.boardUiEvents.onScreenNotification = onScreenNotification;
	}
	private void OnDestroy()
	{
		Singleton.boardUiEvents.onScreenNotification = null;
	}

	private void Start()
	{
		if (ingameUI){
			ingameUI.gameObject.SetActive(true);
			ingameUI.OnNavigateToSet(NavigateTo);
		}
		if (pauseMenuUI){
			pauseMenuUI.gameObject.SetActive(false);
			pauseMenuUI.OnNavigateToSet(NavigateTo);
		}
		if (chooseBoardUI){
			chooseBoardUI.gameObject.SetActive(false);
			chooseBoardUI.OnNavigateToSet(NavigateTo);
		}
		if (settingsUi){
			settingsUi.gameObject.SetActive(false);
			settingsUi.OnNavigateToSet(NavigateTo);
		}
		if (mainMenuUI)
		{
			mainMenuUI.OnNavigateToSet(NavigateTo);
		}
	}
	public void MenuEscKey()
	{
		if (menusStack.Count == 0)
		{
			if (mainMenuUI == null)
			{
				if (ingameUI)
					ingameUI.Hide();
				if (pauseMenuUI)
				{
					menusStack.Push(pauseMenuUI);
					pauseMenuUI.Show();
				}
			}
			else
			{
				mainMenuUI.Show();
			}
		}
		else
		{
			NavigateBack();
		}
	}
	public bool IsIngame() { return menusStack.Count == 0; }

	private void NavigateHome()
	{
		while (menusStack.Count != 0)
		{
			var last = menusStack.Pop() as ICameraView;
			last.Hide();
		}
		if (mainMenuUI != null)
			mainMenuUI.Show();
		else
			ingameUI.Show();
	}
	public void NavigateBack()
	{
		if (menusStack.Count == 0)
			return;
		var last = menusStack.Pop() as ICameraView;
		last.Hide();
		if (menusStack.Count != 0)  //go back or show the in game ui back
		{
			var prev = menusStack.Peek() as ICameraView;
			prev.Show();
		}
		else
		{
			if (mainMenuUI != null)
				mainMenuUI.Show();
			else
				ingameUI.Show();
		}
	}
	public void NavigateForwardTo(MonoBehaviour menu)
	{
		if (menusStack.Count == 0)
		{
			if (mainMenuUI != null) menusStack.Push(mainMenuUI);
			if (ingameUI) ingameUI.Hide();
		}
		if (menusStack.Count != 0)
		{
			var prev = menusStack.Peek();
			(prev as ICameraView).Hide();
		}
		menusStack.Push(menu);
		(menu as ICameraView).Show();
	}

	/// <summary>
	/// Goes into provided menu, pushing previous menu onto stack
	/// </summary>
	/// <param name="navigationEnum"></param>
	public void NavigateTo(MenuNavigationEnum navigationEnum)
	{
		switch (navigationEnum)
		{
			case MenuNavigationEnum.Home:
				{
					NavigateHome();
					break;
				}
			case MenuNavigationEnum.Back:
				{
					NavigateBack();
					break;
				}
			case MenuNavigationEnum.PauseMenu:
				{
					NavigateForwardTo(pauseMenuUI);
					break;
				}
			case MenuNavigationEnum.NewGame:
				{
					NavigateForwardTo(chooseBoardUI);
					break;
				}
			case MenuNavigationEnum.Settings:
				{
					NavigateForwardTo(settingsUi);
					break;
				}
		}
	}

   
}
