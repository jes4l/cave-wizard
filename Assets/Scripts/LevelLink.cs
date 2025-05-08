using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class LevelLink : MonoBehaviour
{
    public GameObject soundFX;

    public void OnClick(int index)
    {
        switch (index)
        {
            case 0:
                StartCoroutine(LoadSceneAfterDelay(2));
                GridManager.levelNumber = 0;
                break;
            case 1:
                StartCoroutine(LoadSceneAfterDelay(1));
                break;
            case 2:
                sfx(0);
                #if UNITY_EDITOR
                EditorApplication.isPlaying = false;
                #else
                Application.Quit();
                #endif
                break;
            case 3:
                StartCoroutine(LoadSceneAfterDelay(0));
                break;
        }
    }
    
    private void sfx(int i) =>
        soundFX.transform.GetChild(i).GetComponent<AudioSource>().Play();

    private IEnumerator LoadSceneAfterDelay(int sceneIndex)
    {
        sfx(0);
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene(sceneIndex);
    }
}
