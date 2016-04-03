using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HP_Tracker_UI : MonoBehaviour
{
    #region Fields
    [SerializeField]
    Text _text;
    [SerializeField]
    Image _inner;

    float initialW;
    RectTransform _rect;
    #endregion Fields
    //=========================================================================
    #region Methods

    void Awake()
    {

        _rect = _inner.GetComponent<RectTransform>();

        initialW = _rect.sizeDelta.x; 
    }

    public void SetColor(Color c)
    {
        _inner.color = c;
    }

    public void SetHP(int current, int max)
    {
        if (max == 0)
            return;

        _text.text = current + " / " + max;

        float per = (max - current) / (float)max;
        var d = _rect.sizeDelta;
        d.x = initialW - (per * initialW);
        _rect.sizeDelta = d;

        /* Movement variant */
        //var w = rect.sizeDelta.x;

        //var p = rect.localPosition;

        //float per = (max - current) / (float)max;
        //p.x = (per * -w);

        //rect.localPosition = p;
    }
    
    #endregion Methods
}
