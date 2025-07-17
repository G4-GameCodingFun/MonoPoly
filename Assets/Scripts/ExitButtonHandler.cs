using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitButtonHandler : MonoBehaviour
{
    public void OnExitButtonClick()
    {
        SceneManager.LoadScene("MainMenu");
    }
}