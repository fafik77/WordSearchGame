using Assets.Scripts.Internal;
using UnityEngine;
using UnityEngine.UIElements;

public class PauseMenuUI : MonoBehaviour, ICameraView
{
	private UIDocument ui;

	private void Awake()
	{
		ui = GetComponent<UIDocument>();
		ui.enabled = false;
	}
	private void OnEnable()
	{
		ui = GetComponent<UIDocument>();
	}
	public void Hide()
	{
		ui.enabled = false;
	}

	public void Show()
	{
		ui.enabled = true;
	}
}
