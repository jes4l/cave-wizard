using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

// Manages sounds, button delays and scenes to go to the correct one.
public class LevelLink : MonoBehaviour {
    public GameObject soundFX;

    // There is a delay to let the sound effects play.
    // Created so that there is time to let the music play before moving on to new scene.
    // Acts as a manager for buttons to move to scenes.
    // Level Number remians 0 upon clicking the start button.
    public void OnClick(int index) {
        switch (index) {
            case 0:
                // start
                StartCoroutine(LoadSceneAfterDelay(2));
                GridManager.levelNumber = 0;
                break;
            case 1:
                // instructions
                StartCoroutine(LoadSceneAfterDelay(1));
                break;
            case 2:
                // quit on editor / build
                sfx(0);
                #if UNITY_EDITOR
                EditorApplication.isPlaying = false;
                #else
                Application.Quit();
                #endif
                break;
            case 3:
                // back on instructions / home on win screen
                StartCoroutine(LoadSceneAfterDelay(0));
                break;
        }
    }
    // Plays sound effects.
    private void sfx(int i) =>
        soundFX.transform.GetChild(i).GetComponent<AudioSource>().Play();

    // Delay before loading scene to play sound effects.
    private IEnumerator LoadSceneAfterDelay(int sceneIndex) {
        sfx(0);
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene(sceneIndex);
    }
}
