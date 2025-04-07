using Assets.Scripts.Internal;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;

public class MenuMgr : MonoBehaviour
{
	[SerializeField] private InGameUI ingameUI;
	[SerializeField] private PauseMenuUI pauseMenuUI;
	[SerializeField] private ChooseBoardUI chooseBoardUI;
	[SerializeField] private SettingsUi settingsUi;

	Stack<MonoBehaviour> menusStack = new Stack<MonoBehaviour>();

	private void Start()
	{
		if (ingameUI)
			ingameUI.gameObject.SetActive(true);
		if (pauseMenuUI)
			pauseMenuUI.gameObject.SetActive(true);
		if (chooseBoardUI)
			chooseBoardUI.gameObject.SetActive(true);
		if (settingsUi != null)
			settingsUi.gameObject.SetActive(true);
	}
	public void MenuEscKey()
	{
		if (menusStack.Count == 0)
		{
			ingameUI.Hide();
			menusStack.Push(pauseMenuUI);
			pauseMenuUI.Show();
		}
		else
		{
			var last = menusStack.Pop() as ICameraView;
			last.Hide();
			if (menusStack.Count != 0)
			{
				var prev = menusStack.Peek() as ICameraView;
				prev.Show();
			}
			else
				ingameUI.Show();
		}
	}
}
