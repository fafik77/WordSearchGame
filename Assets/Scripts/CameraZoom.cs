using System.Text.RegularExpressions;
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
	[SerializeField] private Vector2 sizesCenterNoMoveOnZoom = new(64, 64);


	Camera mainCamera;
	Vector3 mainCameraOrigPos;
	float mainCameraOrigZoom;
	Vector2 boardSize;


	void Start()
	{
		boardSize = new(50, 14);
		mainCamera = Camera.main;
		mainCameraOrigPos = mainCamera.transform.position;
		mainCameraOrigZoom = mainCamera.orthographicSize;
	}

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
			ResetCamera();
		}

		if (Input.GetMouseButton(1))
		{
			var off = Input.mousePositionDelta;
			if (reverseCamera_x)
				off.x = -off.x;
			if (reverseCamera_y)
				off.y = -off.y;
			var newPos = transform.position - off.normalized * (moveSpeed * mainCamera.orthographicSize / 4);
			SetCameraPos(newPos);
		}
	}
	public void ResetCamera()
	{
		mainCamera.transform.position = mainCameraOrigPos;
		mainCamera.orthographicSize = mainCameraOrigZoom;
	}
	public void SetCameraPos(Vector3 pos)
	{
		pos.z = transform.position.z;
		pos.x = Mathf.Clamp(pos.x, -1, boardSize.x + 1);
		pos.y = Mathf.Clamp(pos.y, -(boardSize.y + 1), 1);
		transform.position = pos;
	}
	public void SetCameraZoom(float zoom)
	{
		zoom = Mathf.Round(zoom += 0.5f) - 0.5f;

		float clampedDistance = Mathf.Clamp(zoom, minZoom, maxZoom);
		mainCamera.orthographicSize = clampedDistance;
	}
	public void SetCamera(Vector3 pos, float zoom)
	{
		SetCameraPos(pos);
		SetCameraZoom(zoom);
	}
	public void SetCameraDefaults(Vector3 pos, float zoom, Vector2 boardSize)
	{
		this.boardSize = boardSize;
		SetCamera(pos, zoom);
		mainCameraOrigPos = mainCamera.transform.position;
		mainCameraOrigZoom = mainCamera.orthographicSize;
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
			if (!IsInDeadCenter(sizesCenterNoMoveOnZoom, Input.mousePosition)) //do not move the camera if in the dead zone
			{
				var newPos = transform.position + (scrollInput > 0f ? zoomDirection : -zoomDirection) * (moveSpeed * clampedDistance);
				SetCameraPos(newPos);
			}
		}
		else
		{
			float clampedDistance = Mathf.Clamp(targetZoom, minZoom, maxZoom);
			transform.position = mouseWorldPosition - zoomDirection * clampedDistance;
		}
	}
	
	/// <param name="sizes">size of the middle region</param>
	/// <returns>A pair of positions: (From, To)</returns>
	(Vector2, Vector2) GetScreenCenter(Vector2 sizes)
	{
		Vector2 screen = new(Screen.width, Screen.height);
		screen /= 2; //get in the middle
		sizes /= 2; //get in the middle
		var posFrom = screen - sizes;
		var posTo = screen + sizes;
		return (posFrom, posTo);
	}
	bool IsInDeadCenter(Vector2 sizes, Vector2 pointToCheck)
	{
		var poss = GetScreenCenter(sizes);
		if (pointToCheck.x >= poss.Item1.x &&
			pointToCheck.x <= poss.Item2.x &&
			pointToCheck.y >= poss.Item1.y &&
			pointToCheck.y <= poss.Item2.y
			) return true;
		return false;
	}
}
