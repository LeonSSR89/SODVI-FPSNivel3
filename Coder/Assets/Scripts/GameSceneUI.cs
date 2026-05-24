using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneUI : MonoBehaviour
{
    public void Reload()
    {
        SceneManager.LoadScene(1);
    }
    public void GoMenu ()
    {
        SceneManager.LoadScene(0);
    }
}
