using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    public AudioClip notEffective, normal, superEffective;

    // Use this for initialization
    void Start()
    {
        SoundManager.Instance = this;
    }
	
    // Update is called once per frame
    void Update()
    {
	
    }
}
