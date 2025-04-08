using Assets.Scripts.Internal;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public class ChooseBoardUI : MonoBehaviour, ICameraView
{
	private UIDocument ui;
	private Action<MenuMgr.MenuNavigationEnum> navigateAction;

	private void Awake()
	{
		ui = GetComponent<UIDocument>();
		ui.enabled = false;
	}
	public void Hide()
	{
		ui.enabled = false;
	}

	public void Show()
	{
		ui.enabled = true;
	}

	public void OnNavigateToSet(Action<MenuMgr.MenuNavigationEnum> action)
	{
		navigateAction = action;
	}
}
