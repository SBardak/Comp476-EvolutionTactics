using UnityEngine;
using System.Collections;

public class MovementRange : MonoBehaviour
{
    [SerializeField]
    private int _range;

    public int Range { get { return _range; } }
}
