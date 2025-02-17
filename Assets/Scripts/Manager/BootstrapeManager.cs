using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BootstrapeManager : MonoBehaviour
{
    public void CreateHost() { SceneManager.LoadScene(1); }
}