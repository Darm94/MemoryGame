using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuOptions : MonoBehaviour
{

    [SerializeField] private SceneAsset GameScene ;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void StartGame()
    {
        SceneManager.LoadScene(GameScene.name);
    }


    public void QuitGame()
    {
        #if UNITY_EDITOR
        EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
