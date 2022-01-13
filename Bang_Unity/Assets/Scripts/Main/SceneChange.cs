using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    public static SceneChange instance = null;

    private Image blackScreen;
    private float changeWaiting;

    private void Start ()
    {
        if (instance == null)
        {
            instance = this;
            blackScreen = transform.GetChild(0).GetComponent<Image>();
            DontDestroyOnLoad(gameObject);
            changeWaiting = 0.5f;

            changeWaiting = 0.025f;
            return;
        }

        Destroy(gameObject);
    }

    public void NextScene (string sceneName)
    {
        StartCoroutine(FadeOut_LoadScene_FadeIn(sceneName));
    }

    private IEnumerator FadeOut_LoadScene_FadeIn (string sceneName)
    {
        Color alpha = blackScreen.color;
        float fadeSpeed = 0.025f;
        
        blackScreen.raycastTarget = true;

        while (alpha.a < 1.0f)
        {
            yield return null;
            alpha.a += fadeSpeed;
            blackScreen.color = alpha;
        }

        yield return new WaitForSeconds(changeWaiting);
        SceneManager.LoadScene(sceneName);
        yield return new WaitForSeconds(changeWaiting);

        while (alpha.a > 0.0f)
        {
            yield return null;
            alpha.a -= fadeSpeed;
            blackScreen.color = alpha;
        }

        blackScreen.raycastTarget = false;
    }
}
