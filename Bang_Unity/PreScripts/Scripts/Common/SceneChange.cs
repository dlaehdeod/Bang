using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    public static SceneChange instance = null;

    private Image blackScreen;

    private void Start ()
    {
        if (instance == null)
        {
            instance = this;
            blackScreen = transform.GetChild(0).GetComponent<Image>();
            DontDestroyOnLoad(gameObject);
            return;
        }

        Destroy(this);
    }

    public void NextScene (string sceneName)
    {
        StartCoroutine(FadeOut_LoadScene_FadeIn(sceneName));
    }

    private IEnumerator FadeOut_LoadScene_FadeIn (string sceneName)
    {
        Color alpha = blackScreen.color;
        blackScreen.raycastTarget = true;

        while (alpha.a < 1.0f)
        {
            yield return null;
            alpha.a += 0.025f;
            blackScreen.color = alpha;
        }

        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(sceneName);
        yield return new WaitForSeconds(0.5f);

        while (alpha.a > 0.0f)
        {
            yield return null;
            alpha.a -= 0.025f;
            blackScreen.color = alpha;
        }

        blackScreen.raycastTarget = false;
    }
}
