using Assets.Scripts.Internal;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class EventSystemHandler : MonoBehaviour
{
	[SerializeField] private Camera mainCamera;
	[SerializeField] private Camera settingsCamera;
	[SerializeField] private MenuMgr menuMgr;

	private void Update()
	{
		//settings
		if (Keyboard.current.escapeKey.wasReleasedThisFrame)
		{
			menuMgr.MenuEscKey();

			//Singleton.clickAndDrag.CancelClickPoints(null);
			//bool mainVisible = mainCamera.gameObject.activeSelf;
			//mainCamera.gameObject.SetActive(!mainVisible);
			////Time.timeScale = mainVisible ? 0 : 1;
			//settingsCamera.gameObject.SetActive(mainVisible);
		}
	}
}
