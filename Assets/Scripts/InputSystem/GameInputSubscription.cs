using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInputSubscription : MonoBehaviour
{
	public Vector2 MoveInput {  get; private set; }
	public float MenuEnter { get; private set; }
	public float BoardCenter { get; private set; }
	public Vector2 ZoomInput { get; private set; }
	public float Click { get; private set; }
	public Vector2 Touch0position { get; private set; }
	public Vector2 Touch1position { get; private set; }
	[SerializeField]
	float MMBpressHoldDuration = 0.5f;
	public bool MMBtap { get; private set; }
	public bool MMBhold { get; private set; }
	bool MMBclick;
	Coroutine MMBclickRutine;



	InputActions inputActions;

	private void OnDisable()
	{
		inputActions.InputGame.Disable();

		inputActions.InputGame.MoveInput.performed -= MoveInput_performed;
		inputActions.InputGame.MoveInput.canceled -= MoveInput_performed;

		inputActions.InputGame.Zoom.performed -= Zoom_performed;
		inputActions.InputGame.Zoom.canceled -= Zoom_performed;

		inputActions.InputGame.EnterMenu.performed -= EnterMenu_performed;
		inputActions.InputGame.EnterMenu.canceled -= EnterMenu_performed;

		inputActions.InputGame.Click.performed -= Click_performed;
		inputActions.InputGame.Click.canceled -= Click_performed;

		inputActions.InputGame.BoardCenter.performed -= BoardCenter_performed;
		inputActions.InputGame.BoardCenter.canceled -= BoardCenter_performed;

		inputActions.InputGame.Touch0.performed -= Touch0_performed;
		inputActions.InputGame.Touch0.canceled -= Touch0_performed;
		inputActions.InputGame.Touch1.performed -= Touch1_performed;
		inputActions.InputGame.Touch1.canceled -= Touch1_performed;

		inputActions.InputGame.MMBPress.started -= MMBPress_performed;
		inputActions.InputGame.MMBPress.canceled -= MMBPress_performed;
	}
	private void OnEnable()
	{
		inputActions = new InputActions();
		inputActions.InputGame.Enable();

		inputActions.InputGame.MoveInput.performed += MoveInput_performed;
		inputActions.InputGame.MoveInput.canceled += MoveInput_performed;

		inputActions.InputGame.Zoom.performed += Zoom_performed;
		inputActions.InputGame.Zoom.canceled += Zoom_performed;

		inputActions.InputGame.EnterMenu.performed += EnterMenu_performed;
		inputActions.InputGame.EnterMenu.canceled += EnterMenu_performed;

		inputActions.InputGame.Click.performed += Click_performed;
		inputActions.InputGame.Click.canceled += Click_performed;

		inputActions.InputGame.BoardCenter.performed += BoardCenter_performed;
		inputActions.InputGame.BoardCenter.canceled += BoardCenter_performed;

		inputActions.InputGame.Touch0.performed += Touch0_performed;
		inputActions.InputGame.Touch0.canceled += Touch0_performed;
		inputActions.InputGame.Touch1.performed += Touch1_performed;
		inputActions.InputGame.Touch1.canceled += Touch1_performed;

		inputActions.InputGame.MMBPress.started += MMBPress_performed;
		inputActions.InputGame.MMBPress.canceled += MMBPress_performed;

	}

	private void MMBPress_performed(InputAction.CallbackContext ctx)
	{
		if (ctx.phase == InputActionPhase.Started)
		{
			MMBclick = true;
			MMBclickRutine = StartCoroutine(MMBclickTimeoutRutine());
			MMBhold = true;
		}
		else
		{
			StopCoroutine(MMBclickRutine);
			if (MMBclick) MMBtap = true;
			StartCoroutine(MMBclickUnpressRutine());
		}
		if (ctx.phase == InputActionPhase.Canceled)
			MMBhold = false;
		Debug.Log(ctx);
	}
	IEnumerator MMBclickTimeoutRutine()
	{
		yield return new WaitForSeconds(MMBpressHoldDuration);
		MMBclick = false;
	}
	IEnumerator MMBclickUnpressRutine()
	{
		yield return new WaitForFixedUpdate();
		MMBtap = false;
	}


	private void Touch0_performed(InputAction.CallbackContext ctx)
	{
		Touch0position = ctx.ReadValue<Vector2>();
	}
	private void Touch1_performed(InputAction.CallbackContext ctx)
	{
		Touch1position = ctx.ReadValue<Vector2>();
	}

	private void BoardCenter_performed(InputAction.CallbackContext ctx)
	{
		BoardCenter = ctx.ReadValue<float>();
	}

	private void Click_performed(InputAction.CallbackContext ctx)
	{
		Click = ctx.ReadValue<float>();
	}

	private void EnterMenu_performed(InputAction.CallbackContext ctx)
	{
		MenuEnter = ctx.ReadValue<float>();
	}

	private void Zoom_performed(InputAction.CallbackContext ctx)
	{
		ZoomInput = ctx.ReadValue<Vector2>();
	}

	private void MoveInput_performed(InputAction.CallbackContext ctx)
	{
		MoveInput = ctx.ReadValue<Vector2>();
	}


}
