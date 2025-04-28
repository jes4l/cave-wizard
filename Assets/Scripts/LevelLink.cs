using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class LevelLink : MonoBehaviour
{
    public void OnClick(int i)
    {
        switch (i)
        {
            case 0: // start
                SceneManager.LoadScene(2);
                break;
            case 1: // instructions
                SceneManager.LoadScene(1);
                break;
            case 2: // exit
                #if UNITY_EDITOR
                EditorApplication.isPlaying = false;
                #else
                Application.Quit();
                #endif
                break;
            case 3: // back
                SceneManager.LoadScene(0);
                break;
        }
    }
}
