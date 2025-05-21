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
		PickingMode_Ignore(ui.rootVisualElement);
	}
	void PickingMode_Ignore(VisualElement element)
	{
		element.pickingMode = PickingMode.Ignore;
		foreach(var subElem in element.Children())
		{
			PickingMode_Ignore(subElem);
		}
	}
}
