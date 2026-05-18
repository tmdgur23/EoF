using LoadingMenu;
using OptionMenu;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using AudioSettings = OptionMenu.AudioSettings;

#pragma warning disable 0649
public class MainMenu : MonoBehaviour
{
	[SerializeField] private AudioSettings m_audioSettings;

	private void Start()
	{
	}

	public void StartBattle()
	{
		if (Options.LoadConfigData().BattleCount > 0)
		{
			OnContinue();
		}
		else
		{
			OnStart();
		}
	}

	private void OnStart()
	{
		LoadingScreen.LoadScene(2);
	}

	private void OnContinue()
	{
		LoadingScreen.LoadScene(2);
	}

	public void QuitApplication()
	{
		Application.Quit();
	}
}
#pragma warning restore 0649