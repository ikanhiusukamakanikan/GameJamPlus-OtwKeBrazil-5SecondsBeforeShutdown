using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Dipanggil ketika tombol Play ditekan
    public void PlayGame()
    {
        // Load scene berikutnya berdasarkan urutan di Build Settings
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        // Atau bisa langsung pakai nama:
        // SceneManager.LoadScene("GameScene");
    }

    // Dipanggil ketika tombol Quit ditekan
    public void QuitGame()
    {
        Debug.Log("Quit game");
        Application.Quit();     
    }

   
    public void OpenOptions()
    {
        Debug.Log("Open options menu");
      
    }
}
