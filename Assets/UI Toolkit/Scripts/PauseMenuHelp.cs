using Assets.Scripts.Internal;
using UnityEngine;
using UnityEngine.UIElements;

public class PauseMenuHelp : MonoBehaviour
{
	UIDocument ui;
	private void Awake()
	{
		ui = GetComponent<UIDocument>();
	}
	private void OnEnable()
	{
		ui.PickingMode_Ignore();
	}
}
