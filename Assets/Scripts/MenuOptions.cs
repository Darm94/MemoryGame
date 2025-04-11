using UnityEngine;
using UnityEngine.SceneManagement;


public class MenuOptions : MonoBehaviour
{
    [SerializeField] private Scene GameScene ;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void StartGame()
    {
        SceneManager.LoadScene(GameScene.name);
    }


    void QuitGame()
    {
        Application.Quit();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
