using UnityEngine;
using UnityEngine.InputSystem;

public class CameraZoom : MonoBehaviour
{
	[SerializeField] private float zoomSpeed = 5f;
	[SerializeField] private float minZoom = 2f;
	[SerializeField] private float maxZoom = 20f;
	[SerializeField] private float moveSpeed = 0.1f;
	[SerializeField] private MenuMgr menuMgr;
	[SerializeField] private bool reverseCamera_x = false;
	[SerializeField] private bool reverseCamera_y = false;



	Camera mainCamera;
	Vector3 mainCameraOrigPos;
	float mainCameraOrigZoom;
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		mainCamera = Camera.main;
		mainCameraOrigPos = mainCamera.transform.position;
		mainCameraOrigZoom = mainCamera.orthographicSize;
	}

	// Update is called once per frame
	void Update()
	{
		if (!menuMgr.IsIngame()) return;

		float scrollInput = Input.GetAxis("Mouse ScrollWheel");
		// If there's scroll input, perform the zoom
		if (scrollInput != 0)
		{
			PerformZoom(scrollInput);
		}
		if (Input.GetMouseButtonDown(2))
		{
			mainCamera.transform.position = mainCameraOrigPos;
			mainCamera.orthographicSize = mainCameraOrigZoom;
		}
		if (Input.GetMouseButton(1))
		{
			var off = Input.mousePositionDelta;
			if (reverseCamera_x)
				off.x = -off.x;
			if (reverseCamera_y)
				off.y = -off.y;
			var newPos = transform.position - off.normalized * (moveSpeed * mainCamera.orthographicSize / 4);
			newPos.z = transform.position.z;
			transform.position = newPos;
		}
	}


	/// <summary>
	/// Tkanks to ChatGpt for providing some guidelines
	/// </summary>
	/// <param name="scrollInput"></param>
	void PerformZoom(float scrollInput)
	{
		// Get the mouse position in world space
		Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCamera.nearClipPlane));

		// Calculate the direction from the camera to the mouse position
		Vector3 zoomDirection = (mouseWorldPosition - transform.position).normalized;

		// Calculate the target zoom amount
		float targetZoom = mainCamera.orthographic ? mainCamera.orthographicSize - scrollInput * zoomSpeed : Vector3.Distance(transform.position, mouseWorldPosition) - scrollInput * zoomSpeed;

		// Clamp the zoom level
		if (mainCamera.orthographic)
		{
			float clampedDistance = Mathf.Clamp(targetZoom, minZoom, maxZoom);
			mainCamera.orthographicSize = clampedDistance;
			if (mainCamera.orthographicSize >= maxZoom || mainCamera.orthographicSize <= minZoom) return;
			//float clampedDistance = Mathf.Clamp(targetZoom, minZoom, maxZoom);
			var newPos = transform.position + (scrollInput > 0f ? zoomDirection : -zoomDirection) * (moveSpeed * clampedDistance);
			newPos.z = transform.position.z;
			transform.position = newPos;
		}
		else
		{
			float clampedDistance = Mathf.Clamp(targetZoom, minZoom, maxZoom);
			transform.position = mouseWorldPosition - zoomDirection * clampedDistance;
		}
	}
}
