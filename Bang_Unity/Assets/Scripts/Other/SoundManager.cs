using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance = null;

    public AudioSource backGroundMusic;
    public AudioSource effectSound;

    public Image musicStateImage;
    public Sprite[] speakerSprite;

    private Coroutine backGroundMusicCoroutine;
    private bool coroutineDoing;
    private bool musicOn;

    private void Start()
    {
        instance = this;
    }

    public void SetBackGroundMusic(bool value)
    {
        musicStateImage.transform.parent.gameObject.SetActive(value);

        if (value)
        {
            if (coroutineDoing)
            {
                StopCoroutine(backGroundMusicCoroutine);
            }

            backGroundMusic.volume = 1.0f;
            backGroundMusic.Play();
            musicStateImage.sprite = speakerSprite[0];
            musicOn = true;
        }
        else
        {
            backGroundMusicCoroutine = StartCoroutine(MakeSmallMusic());
        }
    }

    private IEnumerator MakeSmallMusic ()
    {
        coroutineDoing = true;
        float volume = 1.0f;
        while (volume > 0.0f)
        {
            yield return new WaitForSeconds(0.01f);
            volume -= 0.075f;
            backGroundMusic.volume = volume;
        }

        backGroundMusic.Stop();
        coroutineDoing = false;
    }

    public void PlayDynamiteSound ()
    {
        if (musicOn)
        {
            effectSound.Play();
        }
    }

    public void OnMusicOptionButtonDown ()
    {
        if (musicOn)
        {
            musicStateImage.sprite = speakerSprite[1];
            backGroundMusic.Stop();
        }
        else
        {
            musicStateImage.sprite = speakerSprite[0];
            backGroundMusic.Play();
        }

        musicOn = !musicOn;
    }
}
