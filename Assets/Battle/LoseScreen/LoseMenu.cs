using OptionMenu;
using UnityEngine;

namespace Battle.LoseScreen
{
	public class LoseMenu : MonoBehaviour
	{
		public void Restart() 
		{
			Options.ResetConfigData();
			PlayerPrefs.SetInt("MainScene_Rewards", 0);
			UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
		}
	}
}