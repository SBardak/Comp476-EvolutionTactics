using UnityEngine;
using System.Collections;

public class HealingCollectible : MonoBehaviour
{
    public Tile _currentTile;
    public int healthGiven = 20;

    void Update()
    {
        transform.Rotate(new Vector3(15, 30, 45) * Time.deltaTime);
    }
}
