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

    public int _currentHp;

    private Color c = Color.black;
    private PokemonStats _stats;
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
        Reset();
    }
    public void Reset()
    {
        target = gameObject;

        if (_instantiated != null)
            DestroyImmediate(_instantiated);

        _instantiated = Instantiate(HP_PREFAB);
        _instantiated.transform.SetParent(GameObject.Find("Canvas").transform.Find("HP_Container").transform, false);
        _rect = _instantiated.GetComponent<RectTransform>();

        _stats = GetComponent<PokemonStats>();
        _currentHp = _stats.CurrentHealth;

        SetColor(c);
        SetHP();
    }

    void Update()
    {
        if (_rect == null) Reset();

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
    void EnabledBackground(bool enabled)
    {
        if (_instantiated != null)
            _instantiated.GetComponent<HP_Tracker_UI>().EnabledBackground(enabled);
    }

    public void HP_Activated()
    {
        SetColorHP(c);
        EnabledBackground(true);
    }
    public void HP_Deactivated()
    {
        var r = c.r - c.r * 0.5f;
        var g = c.g - c.g * 0.5f;
        var b = c.b - c.b * 0.5f;

        SetColorHP(new Color(r, g, b));
        EnabledBackground(false);
    }

    public void SetHP()
    {
        if (_instantiated != null)
            _instantiated.GetComponent<HP_Tracker_UI>().SetHP(_stats.CurrentHealth, _stats.MaxHealth);
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