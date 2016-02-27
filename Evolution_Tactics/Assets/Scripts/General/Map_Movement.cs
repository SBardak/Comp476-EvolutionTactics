using UnityEngine;
using System.Collections;

public class Map_Movement : MonoBehaviour {

    [SerializeField]
    int _boundary = 50;
    [SerializeField]
    int _speed = 5;

    [SerializeField]
    Texture2D _cursor;
    [SerializeField]
    Vector2 _hotSpot = Vector2.zero;
    [SerializeField]
    CursorMode _cursorMode = CursorMode.Auto;

    [SerializeField]
    Boundary _bounds;

    [SerializeField]
    bool _enabled = true;

    void Start()
    {
        //Cursor.SetCursor(_cursor, _hotSpot, _cursorMode);
    }

	// Update is called once per frame
	void Update () {
        if (_enabled)
        {
            Vector3 pos = transform.position;
            if (Input.mousePosition.x > Screen.width - _boundary || input("d"))
                pos.x += _speed * Time.deltaTime;
            if (Input.mousePosition.x < _boundary || input("a"))
                pos.x -= _speed * Time.deltaTime;

            if (Input.mousePosition.y > Screen.height - _boundary || input("w"))
                pos.z += _speed * Time.deltaTime;
            if (Input.mousePosition.y < _boundary || input("s"))
                pos.z -= _speed * Time.deltaTime;

            pos.x = Mathf.Clamp(pos.x, _bounds.min.x, _bounds.max.x);
            pos.z = Mathf.Clamp(pos.z, _bounds.min.y, _bounds.max.y);

            transform.position = pos;
        }
    }

    bool input(string s)
    {
        return Input.GetKey(s);
    }
}
