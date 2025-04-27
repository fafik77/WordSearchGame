using Assets.Scripts.Internal;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public class ChooseBoardUI : MonoBehaviour, ICameraView
{
	private UIDocument ui;
	private Action<MenuMgr.MenuNavigationEnum> navigateAction;
	Button buttonLoadFile;
	Button buttonCreateRandom;
	Button buttonPickRandom;
	DropdownField dropdownLang;
	TextField SearchField;

	private void Awake()
	{
		ui = GetComponent<UIDocument>();
		ui.enabled = true;
	}
	private void OnEnable()
	{
		var root = ui.rootVisualElement;
		buttonLoadFile = root.Q<Button>("LoadFile");
		buttonCreateRandom = root.Q<Button>("CreateRandom");
		buttonPickRandom = root.Q<Button>("PickRandom");
		dropdownLang = root.Q<DropdownField>("Language");
		SearchField = root.Q<TextField>("Search");

		buttonCreateRandom.clicked += ButtonCreateRandom_clicked;
	}

	private void ButtonCreateRandom_clicked()
	{

	}

	private void OnDisable()
	{
		buttonCreateRandom.clicked -= ButtonCreateRandom_clicked;
	}
	public void Hide() => this.gameObject.SetActive(false);

	public void Show() => this.gameObject.SetActive(true);

	public void OnNavigateToSet(Action<MenuMgr.MenuNavigationEnum> action) => navigateAction = action;
}
