using Assets.Scripts.Internal;
using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DeadCenterDisplay : MonoBehaviour, ICameraView
{
	private UIDocument ui;
	private VisualElement DeadCenterVisual;


	float timeEndZoomDeadZoneSliderDisplay;

	private void Awake()
	{
		ui = GetComponent<UIDocument>();
	}
	private void OnEnable()
	{
		DeadCenterVisual = ui.rootVisualElement.Q<VisualElement>("DeadCenter");
	}
	private void OnDisable()
	{
		DeadCenterVisual = null;
	}

	public void ShowForTime(float seconds)
	{
		Show();
		timeEndZoomDeadZoneSliderDisplay = Time.time + seconds;
	}

	private void Update()
	{
		if(timeEndZoomDeadZoneSliderDisplay <= Time.time)
		{
			Hide();
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

	public void OnNavigateToSet(Action<MenuMgr.MenuNavigationEnum> action)
	{
		throw new NotImplementedException();
	}

	public void SetCenterSize(int size)
	{
		Singleton.settingsPersistent.ZoomDeadZoneSize = size;
		DeadCenterVisual.style.width = size;
		DeadCenterVisual.style.height = size;
	}
}
