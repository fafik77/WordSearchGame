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
		ui.enabled = true;
	}
	private void OnEnable()
	{
		ui = GetComponent<UIDocument>();
	}
	private void OnDisable()
	{
		
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
