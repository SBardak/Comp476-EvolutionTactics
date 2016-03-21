using UnityEngine;
using System.Collections;

public class PokemonStats : MonoBehaviour
{

    public int MaxHealth = 100;
    public int _currentHealth = 100;
    public int Attack = 10;
    public int Defense = 10;
    public int Accuracy = 100;
    public int MovementRange = 3;

    public int DEBUG_MAX_ATTACK_RANGE = 1;

    TileStats.type TileType;
    public TileStats.type MyType = TileStats.type.Fire;

    public int CurrentHealth
    {
        set
        {
            _currentHealth = value;
            GetComponent<HP_Tracker>().SetHp(_currentHealth);
            healthCheck();
        }
        get
        {
            return _currentHealth;
        }
    }

    void healthCheck()
    {
        if (CurrentHealth > MaxHealth)
        {
            CurrentHealth = MaxHealth;
        }
        else if (CurrentHealth <= 0)
        {
            gameObject.SendMessage("HandleDeath");
        }
    }
    // Update is called once per frame
    public bool AttackBonus()
    {
        var t = TileGenerator.Instance.Tiles[(int)transform.position.x, (int)transform.position.z];
        var ts = t.gameObject.GetComponent<TileStats>();
        if (ts == null)
            return false;

        return ts.MyType == MyType;
    }
}
