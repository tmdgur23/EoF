using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using AudioSettings = OptionMenu.AudioSettings;

#pragma warning disable 0649
[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
	private static MusicPlayer m_instance;

	public static MusicPlayer Instance
	{
		get
		{
			if (m_instance == null)
			{
				var t = FindObjectOfType<MusicPlayer>();
				if (t == null)
				{
					t = Instantiate(Resources.Load<MusicPlayer>("MusicPlayer"));
				}

				m_instance = t;
			}

			return m_instance;
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void InitializeMusicPlayer()
	{
		var dummy = Instance;
	}

	[SerializeField] private AudioSource m_audioSource;
	[SerializeField] private AudioMixer AudioMixer = null;

	private void Awake()
	{
		m_audioSource = GetComponent<AudioSource>();
		LoadSettings();
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
		}
		else
		{
			m_instance = this;
		}

		DontDestroyOnLoad(gameObject);
		
		// Clean up AudioListener on this GameObject if it exists
		AudioListener localListener = GetComponent<AudioListener>();
		if (localListener != null)
		{
			DestroyImmediate(localListener);
		}
	}

	private void Start()
	{
		OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		// Enforce exactly one AudioListener in the scene
		AudioListener[] listeners = FindObjectsOfType<AudioListener>();
		if (listeners.Length == 0)
		{
			Debug.LogWarning("[AI] No AudioListener found in the scene! Adding one.");
			Camera mainCam = Camera.main;
			if (mainCam != null)
			{
				mainCam.gameObject.AddComponent<AudioListener>();
			}
			else
			{
				gameObject.AddComponent<AudioListener>();
			}
		}
		else if (listeners.Length > 1)
		{
			Debug.LogWarning($"[AI] Found {listeners.Length} AudioListeners. Cleaning up extra ones.");
			for (int i = 1; i < listeners.Length; i++)
			{
				Destroy(listeners[i]);
			}
		}

		// Change music based on scene (Prevent restarting same clip)
		if (scene.name == "MainMenu")
		{
			AudioClip clip = Resources.Load<AudioClip>("MusicMenu");
			if (clip != null && (m_audioSource == null || m_audioSource.clip != clip)) Play(clip);
		}
		else if (scene.name == "Main")
		{
			AudioClip clip = Resources.Load<AudioClip>("Ambience");
			if (clip != null && (m_audioSource == null || m_audioSource.clip != clip)) Play(clip);
		}
		else if (scene.name == "Battle")
		{
			AudioClip clip = Resources.Load<AudioClip>("MusicBattle");
			if (clip != null && (m_audioSource == null || m_audioSource.clip != clip)) Play(clip);
		}

		// Enforce Cursor State based on scene
		if (scene.name == "Battle" || scene.name == "MainMenu")
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
		else if (scene.name == "Main")
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
	}

	/// <summary>
	/// Load and set Audio volumes. 
	/// </summary>
	private void LoadSettings()
	{
		var masterValue = PlayerPrefs.GetFloat(AudioSettings.MasterPref, 0.75f);
		var sfxValue = PlayerPrefs.GetFloat(AudioSettings.SFXPref, 0.75f);
		var musicValue = PlayerPrefs.GetFloat(AudioSettings.MusicPref, 0.75f);

		AudioMixer.SetFloat("Master", 20f * Mathf.Log10(masterValue));
		AudioMixer.SetFloat("Music", 20f * Mathf.Log10(musicValue));
		AudioMixer.SetFloat("SFX", 20f * Mathf.Log10(sfxValue));
	}

	public void Play(AudioClip clip)
	{
		if (m_audioSource.clip != null && (m_audioSource.clip == clip || m_audioSource.clip.name == clip.name)) return;
		m_audioSource.clip = clip;
		m_audioSource.Play();
	}

	public void Stop()
	{
		m_audioSource.Stop();
	}
}
#pragma warning restore 0649