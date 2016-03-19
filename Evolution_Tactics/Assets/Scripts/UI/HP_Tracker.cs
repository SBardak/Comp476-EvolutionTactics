using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HP_Tracker : MonoBehaviour
{
    [SerializeField]
    GameObject target;
    public float xOffset;
    public float yOffset;
    public Image HP_PREFAB;

    private Image _instantiated;
    private RectTransform _rect;

    public int _maxHp, _currentHp;

    [SerializeField]
    Color c;
    // TODO: Change so that the color can be generated at launch

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
        _instantiated.transform.SetParent(GameObject.Find("Canvas").transform, false);
        _rect = _instantiated.GetComponent<RectTransform>();

        PokemonStats stats = GetComponent<PokemonStats>();
        _maxHp = stats.MaxHealth;
        _currentHp = stats.CurrentHealth;

        _instantiated.GetComponent<HP_Tracker_UI>().SetColor(c);

        // SetHP();
    }

    void Update()
    {
        var screenPos = Camera.main.WorldToScreenPoint(target.transform.position);

        screenPos.x -= xOffset;
        screenPos.y -= yOffset;

        _rect.position = screenPos;

        // TODO: REMOVE ME
        SetHP();
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
}