using UnityEngine;
using UnityEngine.UIElements;

public class CongratulationsYouWon : MonoBehaviour
{
	public UIDocument ui { get; private set; }
	Label timeLabel;
	string timeText;
	private void Awake()
	{
		ui = GetComponent<UIDocument>();
	}


	private void OnDisable()
	{
		timeLabel = null;
		Singleton.boardUiEvents.CreateBoardEvent -= BoardUiEvents_CreateBoardEvent;
	}
	private void OnEnable()
	{
		timeLabel = ui.rootVisualElement.Q<Label>("time");
		timeLabel.text = timeText;
		Singleton.boardUiEvents.CreateBoardEvent += BoardUiEvents_CreateBoardEvent;
	}

	private void BoardUiEvents_CreateBoardEvent(bool obj)
	{
		SetActive(false);
	}


	public void SetActive(bool active)
	{
		this.gameObject.SetActive(active);
	}
	public void SetTimeText(string timeText)
	{
		this.timeText = timeText;
		if (timeLabel != null)
		{
			timeLabel.text = timeText;
		}
	}
}
