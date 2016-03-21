using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Spooky : MonoBehaviour {

    MovieTexture test;
    RawImage img;
    AudioSource audio;

    int i = 0;

    void Start()
    {
        img = GetComponent<RawImage>();
        test = (MovieTexture)img.texture;

        test.loop = true;
        //GetComponent<Renderer>().material.mainTexture = test;
        //img.material.mainTexture = test;

        audio = GetComponent<AudioSource>();
        audio.clip = test.audioClip;

        SetPos();
    }

	// Update is called once per frame
	void Update () {
        if (Input.GetKeyUp(KeyCode.P))
        {
            if (!test.isPlaying)
            {
                foreach (var t in GameObject.FindObjectOfType<Canvas>().GetComponentsInChildren<Image>())
                {
                    t.sprite = null;
                }
                foreach (var t in GameObject.FindObjectsOfType<Tile>())
                {
                    t.GetComponent<Renderer>().material.mainTexture = test;
                }

                img.enabled = true;
                test.Play();
                audio.Play();
            }
            else
            {
                i = 0;

                img.enabled = false;
                test.Stop();
                audio.Stop();
            }
        }
        if (test.isPlaying)
        {
            SetPos();
            ++i;
        }
    }

    void SetPos()
    {
        var pos = img.rectTransform.position;

        if (i < 60)
        {
            pos.x = 100;
            pos.y = Screen.height - 100;
        }
        else if (i < 120)
        {
            pos.x = Screen.width - 100;
            pos.y = Screen.height - 100;
        }
        else if (i < 180)
        {
            pos.x = Screen.width - 100;
            pos.y = 100;
        }
        else if (i < 240)
        {
            pos.x = 100;
            pos.y = 100;
        }
        else {
            i = 0;
        }

        img.rectTransform.position = pos;
    }


}
