using UnityEngine;
using System.Collections;

public class CharacterVoice : MonoBehaviour
{

    public void PlayVoice()
    {
        if (GetComponent<AudioSource>().clip != null)
            GetComponent<AudioSource>().Play();
    }
}
