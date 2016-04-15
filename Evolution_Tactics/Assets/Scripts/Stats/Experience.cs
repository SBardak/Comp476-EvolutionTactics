using UnityEngine;
using System.Collections;

public class Experience : MonoBehaviour {

    public int experience;
    public int NextLevelXP;
    int CarryOverXP;


	void Start () {
        experience = 0;
        NextLevelXP = 100;
        CarryOverXP = 0;
	}

    // Update is called once per frame

    public void gainXP(int XP)
    {
        experience += XP;
        if (experience >= NextLevelXP)
        {

            transform.GetComponent<PokemonStats>().LevelUp();  //call Level up from PokemonStats and increase stats.

            CarryOverXP = experience - NextLevelXP;
            NextLevelXP =(int) (1.2f * (float)NextLevelXP);   //make sure no XP is wasted, and increase XP needed for future levels.
            experience = 0;
            gainXP(CarryOverXP);
        }

    }

    
    void Update () {
	
	}
}
