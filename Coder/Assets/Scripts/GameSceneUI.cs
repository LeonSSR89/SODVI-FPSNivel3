using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneUI : MonoBehaviour
{
    public void Reload()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(1);
    }
    public void GoMenu ()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }
}
