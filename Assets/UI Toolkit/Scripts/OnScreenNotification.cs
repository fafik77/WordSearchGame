using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class OnScreenNotification : MonoBehaviour
{
	UIDocument ui;
	Label label;
	Coroutine coroutineHideText;
	private void Awake() => ui = GetComponent<UIDocument>();

	private void OnEnable()
	{
		var root = ui.rootVisualElement;
		label = root.Q<Label>();
	}
	private void OnDisable()
	{
		if (coroutineHideText != null)
			StopCoroutine(coroutineHideText);
	}

	public void setText(string text)
	{
		label.text = text;
		label.style.opacity = 100;
		if (coroutineHideText != null)
			StopCoroutine(coroutineHideText);
		coroutineHideText = StartCoroutine(HideText());
	}

	private IEnumerator HideText()
	{
		yield return new WaitForSeconds(2);
		label.style.opacity = 0;
	}
}
