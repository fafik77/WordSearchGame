using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class EventSystemHandler : MonoBehaviour
{
	[SerializeField] private Camera mainCamera;
	[SerializeField] private Camera settingsCamera;

	private void Update()
	{
		//settings
		if (Keyboard.current.escapeKey.wasReleasedThisFrame)
		{
			Singleton.clickAndDrag.CancelClickPoints(null);
			bool mainVisible = mainCamera.gameObject.activeSelf;
			mainCamera.gameObject.SetActive(!mainVisible);
			settingsCamera.gameObject.SetActive(mainVisible);
		}
	}
}
