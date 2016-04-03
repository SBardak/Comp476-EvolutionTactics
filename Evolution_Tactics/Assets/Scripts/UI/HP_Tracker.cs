using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HP_Tracker : MonoBehaviour
{
    #region Fields
    [SerializeField]
    GameObject target;
    public float xOffset;
    public float yOffset;
    public Image HP_PREFAB;

    private Image _instantiated;
    private RectTransform _rect;

    public int _maxHp, _currentHp;

    Color c = Color.black;
    #endregion Fields
    //=========================================================================
    #region Methods

    void Start()
    {
        if (HP_PREFAB == null)
        {
            Debug.LogError("HP_Tracker requires HP prefab");
            Destroy(this);
            return;
        }
       target = gameObject;
        _instantiated = Instantiate(HP_PREFAB);
        _instantiated.transform.SetParent(GameObject.Find("Canvas").transform.Find("HP_Container").transform, false);
        _rect = _instantiated.GetComponent<RectTransform>();

        PokemonStats stats = GetComponent<PokemonStats>();
        _maxHp = stats.MaxHealth;
        _currentHp = stats.CurrentHealth;

        SetColor(c);
        SetHP();
    }

    void Update()
    {
        var screenPos = Camera.main.WorldToScreenPoint(target.transform.position);

        screenPos.x -= xOffset - 9;
        screenPos.y -= yOffset;

        _rect.position = screenPos;
    }
    //-------------------------------------------------------------------------
    #region Setters & Activators

    public void SetColor(Color color)
    {
        c = color;
        SetColorHP(c);
    }
    void SetColorHP(Color c)
    {
        if (_instantiated != null)
            _instantiated.GetComponent<HP_Tracker_UI>().SetColor(c);
    }

    public void HP_Activated()
    {
        SetColorHP(c);
    }
    public void HP_Deactivated()
    {
        var r = c.r - c.r * 0.5f;
        var g = c.g - c.g * 0.5f;
        var b = c.b - c.b * 0.5f;

        SetColorHP(new Color(r, g, b));
    }

    private void SetHP()
    {
        _instantiated.GetComponent<HP_Tracker_UI>().SetHP(_currentHp, _maxHp);
    }
    public void SetHp(int currentHp)
    {
        _currentHp = currentHp;
        SetHP();
    }

    #endregion Setters & Activators
    //-------------------------------------------------------------------------
    #region Destroy
    void HandleDeath()
    {
        Destroy(_instantiated.gameObject);
    }
    void OnDestroy()
    {
        if (_instantiated != null && _instantiated.gameObject != null)
            Destroy(_instantiated.gameObject);
    }
    #endregion Destroy

    #endregion Methods
}