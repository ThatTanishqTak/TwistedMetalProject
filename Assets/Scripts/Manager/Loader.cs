using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/* --MAKE SURE WE REMOVE ALL THE DEBUG STATMENTS BEFORE WE BUILD THE FINAL VERSION-- */

public static class Loader
{
    public enum Scene
    {
        Lobby,
        CharacterSelect
    }

    private static Scene targetScene;

    public static void LoadNetwork(Scene targetScene)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(targetScene.ToString(), LoadSceneMode.Single);
    }

    public static void LoaderCallback()
    {
        SceneManager.LoadScene(targetScene.ToString());
    }
}