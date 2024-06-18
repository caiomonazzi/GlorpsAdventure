using UnityEngine;
using UnityEngine.SceneManagement;


public class LoadingSceneManager : MonoBehaviour
{
    private void Start()
    {
        LoadScene();
    }

    void LoadScene()
    {
        SceneManager.LoadScene(ScenesManager.Instance.sceneToLoad);
    }
}