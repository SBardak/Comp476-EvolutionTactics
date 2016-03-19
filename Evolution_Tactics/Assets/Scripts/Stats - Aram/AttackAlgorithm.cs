using UnityEngine;
using System.Collections;

public class AttackAlgorithm : MonoBehaviour
{
    GameObject Target;

    int MaxHealth;
    int CurrentHealth;
    int Attack;
    int Defense;
    int Accuracy;

    int Enemy_MaxHealth;
    int Enemy_CurrentHealth;
    int Enemy_Attack;
    int Enemy_Defense;
    int Enemy_Accuracy;


    TileStats.type myType;
    int damage;

    void getStats(GameObject Target)
    {

        myType = GetComponent<PokemonStats>().MyType;

        MaxHealth = GetComponent<PokemonStats>().MaxHealth;
        CurrentHealth = GetComponent<PokemonStats>().CurrentHealth;
        Attack = GetComponent<PokemonStats>().Attack;
        Defense = GetComponent<PokemonStats>().Defense;
        Accuracy = GetComponent<PokemonStats>().Accuracy;

        Enemy_MaxHealth = Target.GetComponent<PokemonStats>().MaxHealth;
        Enemy_CurrentHealth = Target.GetComponent<PokemonStats>().CurrentHealth;
        Enemy_Attack = Target.GetComponent<PokemonStats>().Attack;
        Enemy_Defense = Target.GetComponent<PokemonStats>().Defense;
        Enemy_Accuracy = Target.GetComponent<PokemonStats>().Accuracy;

    }

    float typeAdvantage(GameObject Target)
    {
        float modifier = 1.0f;
        TileStats.type Enemy_Type = Target.GetComponent<PokemonStats>().MyType;

        if (myType == TileStats.type.Fire)
        {
            if (Enemy_Type == TileStats.type.Grass)
            {
                modifier += 0.5f;  
            }
            if (Enemy_Type == TileStats.type.Water)
            {
                modifier -= 0.5f;
            }
        }
        else if (myType == TileStats.type.Water)
        {
            if (Enemy_Type == TileStats.type.Fire)
            {
                modifier -= 0.5f;
            }
            if (Enemy_Type == TileStats.type.Grass)
            {
                modifier += 0.5f;
            }
        }
        else if (myType == TileStats.type.Grass)
        {
            if (Enemy_Type == TileStats.type.Water)
            {
                modifier += 0.5f;
            }
            if (Enemy_Type == TileStats.type.Fire)
            {
                modifier -= 0.5f;
            }
        }
        else if (myType == TileStats.type.Electric)
        {
            if (Enemy_Type == TileStats.type.Ground)
            {
                modifier = 0;
            }
            else if (Enemy_Type == TileStats.type.Flying)
            {
                modifier += 0.5f;
            }
        }
        else if (myType == TileStats.type.Ground)
        {
            if (Enemy_Type == TileStats.type.Flying)
            {
                modifier = 0;
            }
            else if (Enemy_Type == TileStats.type.Electric)
            {
                modifier += 0.5f;
            }
        }
        else if (myType == TileStats.type.Flying)
        {
            if (Enemy_Type == TileStats.type.Electric)
            {
                modifier += 0.5f;
            }
            if (Enemy_Type == TileStats.type.Ground)
            {
                modifier += 0.5f;
            }
        }



        return modifier;
    }

    public int GetDamage(GameObject target)
    {
        getStats(Target);

        damage = Attack - Enemy_Defense;

        if (GetComponent<PokemonStats>().AttackBonus() == true)
        {
            damage = (int)((float)damage * 1.2f);
        }
        if (damage <= 0)
        {
            damage = 0;
        }
        damage = (int)((float)damage * typeAdvantage(Target));
    }

    public void DoDamage(GameObject target)
    {
        float rand = Random.Range(0, 100);
        if (rand <= Accuracy)
        {
            Enemy_CurrentHealth -= damage;
        }
    }
}
