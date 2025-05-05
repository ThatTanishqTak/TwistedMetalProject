using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class MenuMusicManager : MonoBehaviour
{
    private static MenuMusicManager _instance;
    private AudioSource _audio;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            _audio = GetComponent<AudioSource>();
            _audio.loop = true;

            if (IsMenuScene(SceneManager.GetActiveScene().name))
                _audio.Play();

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (IsMenuScene(scene.name))
        {
            if (!_audio.isPlaying)
                _audio.Play();
        }
        else
        {
            if (_audio.isPlaying)
                _audio.Stop();

            SceneManager.sceneLoaded -= OnSceneLoaded;
            Destroy(gameObject);
        }
    }

    private bool IsMenuScene(string name)
    {
        return name == "MainMenu" || name == "CharacterSelect";
    }
}
