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

	Stack<MonoBehaviour> menusStack = new Stack<MonoBehaviour>();

	public enum MenuNavigationEnum
	{
		None,
		Back,
		Settings,
		PauseMenu,
		NewGame,
	}

	private void Awake()
	{
		if (!ingameUI)
			ingameUI = this.gameObject.GetComponentInChildren<InGameUI>(true);
		if (!pauseMenuUI)
			pauseMenuUI = this.gameObject.GetComponentInChildren<PauseMenuUI>(true);
		if (!chooseBoardUI)
			chooseBoardUI = this.gameObject.GetComponentInChildren<ChooseBoardUI>(true);
		if (!settingsUi)
			settingsUi = this.gameObject.GetComponentInChildren<SettingsUi>(true);
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
	}
	public void MenuEscKey()
	{
		if (menusStack.Count == 0)
		{
			ingameUI.Hide();
			menusStack.Push(pauseMenuUI);
			pauseMenuUI.Show();
		}
		else
		{
			NavigateBack();
		}
	}
	public void NavigateBack()
	{
		if (menusStack.Count == 0)
			return;
		var last = menusStack.Pop() as ICameraView;
		last.Hide();
		if (menusStack.Count != 0)	//go back or show the in game ui back
		{
			var prev = menusStack.Peek() as ICameraView;
			prev.Show();
		}
		else
			ingameUI.Show();
	}
	public void NavigateForwardTo(MonoBehaviour menu)
	{
		var prev = menusStack.Peek();
		(prev as ICameraView).Hide();
		menusStack.Push(menu);
		(menu as ICameraView).Show();
	}

	public void NavigateTo(MenuNavigationEnum navigationEnum)
	{
		switch (navigationEnum)
		{
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
