using Assets.Scripts.Internal;
using UnityEngine;
using UnityEngine.UIElements;

public class ChooseBoardUI : MonoBehaviour, ICameraView
{
	private UIDocument ui;

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

}
