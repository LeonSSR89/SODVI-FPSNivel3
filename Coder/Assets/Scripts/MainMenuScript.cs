using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuScript : MonoBehaviour
{
    //[SerializeField] Button exitButton, startButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Exit ()
    {
        Application.Quit();
    }

    // Update is called once per frame
    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }
}
