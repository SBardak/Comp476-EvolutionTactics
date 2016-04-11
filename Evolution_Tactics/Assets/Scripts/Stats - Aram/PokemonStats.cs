using UnityEngine;
using System.Collections;

public class PokemonStats : MonoBehaviour
{
    #region Fields

    public int Level = 1;
    public int XP_on_Death = 10;

    public int MaxHealth { get { return (int)_maxHealth; } }

    public int _currentHealth = 100;

    public int Attack { get { return (int)_attack; } }

    public int Defense { get { return (int)_defense; } }

    public int Accuracy = 100;

    public int MovementRange { get { return (int)_movementRange; } }

    public int AttackRange { get { return (int)_attackRange; } set { _attackRange = value; } }

    [SerializeField]
    float _maxHealth = 100,
        _attack = 10,
        _defense = 10,
        _movementRange = 3,
        _attackRange = 1;

    [SerializeField]
    float _UP_HP = 10,
        _UP_Atk = 1,
        _UP_Def = 1,
        _UP_Mov = 0.2f,
        _UP_AtkR = 0;

    TileStats.type TileType;
    public TileStats.type MyType = TileStats.type.Fire;

    #endregion Fields


    public void SetLevel(int level)
    {
        while (Level < level)
            LevelUpAttributes();

        int xp = 100;
        for (int i = 1; i < level; ++i)
            xp = (int)(1.2f * (float)xp);

        XP_on_Death = (int)(xp * 0.8);
    }

    public void LevelUp()
    {
        UIManager.Instance.CreateNewLevelUpLabel("Level up!", transform.position);
        LevelUpAttributes();

        //transform.GetComponent<Evolve>().EvolveCheck();
    }

    private void LevelUpAttributes()
    {
        Level += 1;

        _attack += _UP_Atk;
        _defense += _UP_Def;
        _movementRange += _UP_Mov;
        _maxHealth += _UP_HP;
        _attackRange += _UP_AtkR;
        CurrentHealth = MaxHealth;
    }

    public void GiveHealth(int health)
    {
        // TODO Small healing sound
        int newHealth = _currentHealth + health;
        if (newHealth > MaxHealth)
        {
            CurrentHealth = MaxHealth;
        }
        else
        {
            CurrentHealth = newHealth;
        }
    }

    public int CurrentHealth
    {
        set
        {
            _currentHealth = value;
            if (GetComponent<HP_Tracker>() != null)
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

    public bool AttackBonus()
    {
        var t = TileGenerator.Instance.Tiles[(int)transform.position.x, (int)transform.position.z];
        var ts = t.gameObject.GetComponent<TileStats>();
        if (ts == null)
            return false;

        return ts.MyType == MyType;
    }

    public bool AttackBonus(Tile tile)
    {
        var ts = tile.gameObject.GetComponent<TileStats>();
        if (ts == null)
            return false;

        return ts.MyType == MyType;
    }
}
