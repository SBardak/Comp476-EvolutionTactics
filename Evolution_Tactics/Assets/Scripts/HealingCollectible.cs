using UnityEngine;
using System.Collections;

public class HealingCollectible : MonoBehaviour
{
    #region Fields
    public Tile _currentTile;
    public int healthGiven = 20;

    public AudioClip healingSound;
    private bool isQuit = false;
    #endregion Fields

    #region Methods
    void Update()
    {
        transform.Rotate(new Vector3(15, 30, 45) * Time.deltaTime);
    }

    void OnApplicationQuit()
    {
        isQuit = true;
    }

    void OnDestroy()
    {
        if (!isQuit)
            AudioSource.PlayClipAtPoint(healingSound, Vector3.zero);
    }
    #endregion Methods
}
