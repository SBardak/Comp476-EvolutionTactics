using UnityEngine;
using System.Collections;

public class Selector : MonoBehaviour
{
    //public delegate void SelectorEventHandler(object sender);
    //public event SelectorEventHandler OnClick;
    //public event SelectorEventHandler OnHover;

    void Start()
    {
        // _initialColor isn't set because Start is never called
    }

    public virtual void RayHit(int number)
    {
        Debug.LogWarning("Original");
    }

    protected virtual void OnMouseOver()
    {
    }

    protected virtual void OnMouseExit()
    {
    }

    protected void SetColor(Color c)
    {
        //GetComponent<Renderer>().material.color = c;
    }
}
