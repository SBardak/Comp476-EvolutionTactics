using UnityEngine;
using System.Collections;

public class PokemonStats : MonoBehaviour {

    public int MaxHealth=100;
    public int CurrentHealth=100;
    public int Attack=10;
    public int Defense = 10;
    public int Accuracy = 100;
    public int MovementRange = 3;




   

    TileStats.type TileType;
    public TileStats.type MyType = TileStats.type.Fire;

    // Use this for initialization
    void Start () {
        
	}

    void healthCheck()
    {
        if (CurrentHealth > MaxHealth)
        {
            CurrentHealth = MaxHealth;
        }
        else if (CurrentHealth <= 0)
        {
            //You are dead
        }
    }	
	// Update is called once per frame
    public bool AttackBonus()
    {
        if (TileStats.TileList[(int)transform.position.x, (int)transform.position.z]==MyType)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
	void Update () {

        healthCheck();
        


	}
}
