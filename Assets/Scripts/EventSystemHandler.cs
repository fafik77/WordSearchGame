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

    private void Awake()
    {
        QualitySettings.vSyncCount = 1;
    }

    private void Update()
	{
		//settings
		if (Keyboard.current.escapeKey.wasReleasedThisFrame)
		{
			menuMgr.MenuEscKey();
		}
	}
}
