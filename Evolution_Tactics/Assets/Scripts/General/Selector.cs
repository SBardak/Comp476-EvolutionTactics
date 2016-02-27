using UnityEngine;
using System.Collections;

public class Selector : MonoBehaviour
{
    Color _initialColor;

    //public delegate void SelectorEventHandler(object sender);
    //public event SelectorEventHandler OnClick;
    //public event SelectorEventHandler OnHover;

    void Start()
    {
        _initialColor = GetComponent<Renderer>().material.color;
    }

    public virtual void RayHit(int number)
    {
        Debug.LogWarning("Original");
    }

    protected virtual void OnMouseOver()
    {
        SetColor(Color.red);
    }

    protected void OnMouseExit()
    {
        SetColor(_initialColor);
    }

    protected void SetColor(Color c)
    {
        GetComponent<Renderer>().material.color = c;
    }
}
