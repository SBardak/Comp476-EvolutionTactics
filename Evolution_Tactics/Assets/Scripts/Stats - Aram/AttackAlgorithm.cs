using UnityEngine;
using System.Collections;

public class AttackAlgorithm : MonoBehaviour
{
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

    void getStats(Character target)
    {

        myType = GetComponent<PokemonStats>().MyType;

        MaxHealth = GetComponent<PokemonStats>().MaxHealth;
        CurrentHealth = GetComponent<PokemonStats>().CurrentHealth;
        Attack = GetComponent<PokemonStats>().Attack;
        Defense = GetComponent<PokemonStats>().Defense;
        Accuracy = GetComponent<PokemonStats>().Accuracy;

        Enemy_MaxHealth = target.GetComponent<PokemonStats>().MaxHealth;
        Enemy_CurrentHealth = target.GetComponent<PokemonStats>().CurrentHealth;
        Enemy_Attack = target.GetComponent<PokemonStats>().Attack;
        Enemy_Defense = target.GetComponent<PokemonStats>().Defense;
        Enemy_Accuracy = target.GetComponent<PokemonStats>().Accuracy;

    }

    float typeAdvantage(Character target)
    {
        float modifier = 1.0f;
        TileStats.type Enemy_Type = target.GetComponent<PokemonStats>().MyType;

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
                modifier += 0.5f;
            }
            if (Enemy_Type == TileStats.type.Grass)
            {
                modifier -= 0.5f;
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
                //modifier += 0.5f;
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
                //modifier += 0.5f;
            }
        }
        else if (myType == TileStats.type.Flying)
        {
            if (Enemy_Type == TileStats.type.Electric)
            {
                modifier += 0;
            }
            if (Enemy_Type == TileStats.type.Ground)
            {
                //modifier += 0.5f;
            }
        }



        return modifier;
    }

    public int GetDamage(Character target)
    {
        getStats(target);

        damage = Attack - Enemy_Defense;

        if (GetComponent<PokemonStats>().AttackBonus() == true)
        {
            damage = (int)((float)damage * 1.2f);
        }
        if (damage <= 0)
        {
            damage = 0;
        }
        damage = (int)((float)damage * typeAdvantage(target));

        Debug.LogWarning("It would do " + damage + " damages to " + target.name);

        return damage;
    }

    public int GetDamage(Character target, Tile tile)
    {
        getStats(target);

        damage = Attack - Enemy_Defense;

        if (GetComponent<PokemonStats>().AttackBonus(tile) == true)
        {
            damage = (int)((float)damage * 1.2f);
        }
        if (damage <= 0)
        {
            damage = 0;
        }
        damage = (int)((float)damage * typeAdvantage(target));

        return damage;
    }

    public void DoDamage(Character target)
    {

        float rand = Random.Range(0, 100);
        //if (rand <= Accuracy)
        //{
        Debug.LogWarning(gameObject.name + " attacks " + target.name + " and do " + damage + " damages.");
        target.GetComponent<PokemonStats>().CurrentHealth -= damage;

        StartCoroutine(UIManager.Instance.CreateNewDamageLabel(damage));
        StartCoroutine(AttackAnimation());

        if (target.GetComponent<PokemonStats>().CurrentHealth <= 0)
        {
            // transform.GetComponent<Experience>().gainXP(target.GetComponent<PokemonStats>().XP_on_Death);
        }
        /* }
        else
        {
            Debug.LogWarning("MISS");
        }*/
    }

    private IEnumerator AttackAnimation()
    {
        transform.position += (transform.forward * 0.5f);
        yield return new WaitForSeconds(0.1f);
        transform.position -= (transform.forward * 0.5f);
    }
}
