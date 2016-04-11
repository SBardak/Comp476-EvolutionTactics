using UnityEngine;
using System.Collections;

public enum Effectiveness
{
    NOTEFFECTIVE,
    NORMAL,
    SUPEREFFECTIVE
}

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

    private Effectiveness effect;
    private SoundManager sm;

    void Awake()
    {
        sm = GameObject.Find("SoundManager").GetComponent<SoundManager>();
    }

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

        if (modifier == 1.5)
        {
            effect = Effectiveness.SUPEREFFECTIVE;
        }
        else if (modifier == 1.0)
        {
            effect = Effectiveness.NORMAL;
        }
        else if (modifier == 0.5)
        {
            effect = Effectiveness.NOTEFFECTIVE;
        }

        return modifier;
    }

    public int GetDamage(Character target, bool initialAttack = true)
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

        if (initialAttack)
        {
            float distance = Vector3.Distance(target.transform.position, transform.position);
            bool isInRangeToCounter = distance <= target.GetComponent<PokemonStats>().AttackRange;

            var n = target.name.Split("("[0]);

            UIManager.Instance.AddAction("It would do " + damage + " damage to " + n[0] + ".");
            if (isInRangeToCounter)
            {
                int receivedDamage = target.GetComponent<AttackAlgorithm>().GetDamage(this.GetComponent<Character>(), false);

                UIManager.Instance.AddAction("And you would receive " + receivedDamage + " damage.");
            }
            else
            {
                UIManager.Instance.AddAction(" And you would receive no damage.");
            }
        }
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
        var pokeStats = target.GetComponent<PokemonStats>();
        float rand = Random.Range(0, 100);
        //if (rand <= Accuracy)
        //{
        var n = target.name.Split("("[0]);
        var nn = name.Split("("[0]);
        UIManager.Instance.AddAction(nn[0] + " attacks " + n[0] + " and do " + damage + " damage.");
        pokeStats.CurrentHealth = pokeStats.CurrentHealth - damage;

        UIManager.Instance.CreateNewDamageLabel(damage, target.transform.position);
        StartCoroutine(AttackAnimation());


        var exp = transform.GetComponent<Experience>();
        if (exp != null)
        {
            int xpGain = pokeStats.XP_on_Death;
            if (pokeStats.CurrentHealth > 0)
                xpGain = (int)(xpGain * 0.2f);
        
            exp.gainXP(xpGain);
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
        PlayAttackSound();
        yield return new WaitForSeconds(0.1f);
        transform.position -= (transform.forward * 0.5f);
    }

    private void PlayAttackSound()
    {
        if (effect == Effectiveness.NOTEFFECTIVE)
        {
            AudioSource.PlayClipAtPoint(sm.notEffective, Vector3.zero);
        }
        else if (effect == Effectiveness.NORMAL)
        {
            AudioSource.PlayClipAtPoint(sm.normal, Vector3.zero);
        }
        else if (effect == Effectiveness.SUPEREFFECTIVE)
        {
            AudioSource.PlayClipAtPoint(sm.superEffective, Vector3.zero);
        }
    }
}
