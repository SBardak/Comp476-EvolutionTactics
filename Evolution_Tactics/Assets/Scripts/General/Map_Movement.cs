using UnityEngine;
using System.Collections;

public class Map_Movement : MonoBehaviour
{

    public static Map_Movement Instance;

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

    Vector3 syncPosition = Vector3.zero;
    [SerializeField]
    float _damp = 1.5f;

    [SerializeField]
    float _zoomDamp = 10f;

    void Start()
    {
        Instance = this;

        _bounds.min.x = _bounds.min.y = 0;
        _bounds.max.x = TileGenerator.Instance.mapWidth;
        _bounds.max.y = TileGenerator.Instance.mapHeight;

        _bounds.min.y -= 10;
        _bounds.max.y -= 10;
        //Cursor.SetCursor(_cursor, _hotSpot, _cursorMode);
    }

    // Update is called once per frame
    void Update()
    {
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

            if (pos != transform.position)
                syncPosition = Vector3.zero;

            pos.x = Mathf.Clamp(pos.x, _bounds.min.x, _bounds.max.x);
            pos.z = Mathf.Clamp(pos.z, _bounds.min.y, _bounds.max.y);

            transform.position = pos;

            if (input(KeyCode.KeypadPlus) || input("r"))
            {
                Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - Time.deltaTime * _zoomDamp, _bounds.Zoom.x, _bounds.Zoom.y);
            }
            else if (input(KeyCode.KeypadMinus) || input("f"))
            {
                Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize + Time.deltaTime * _zoomDamp, _bounds.Zoom.x, _bounds.Zoom.y);
            }
        }

        if (syncPosition != Vector3.zero)
        {
            var pos = transform.position;

            pos = Vector3.Lerp(transform.position, syncPosition, Time.deltaTime * _damp);

            pos.x = Mathf.Clamp(pos.x, _bounds.min.x, _bounds.max.x);
            pos.z = Mathf.Clamp(pos.z, _bounds.min.y, _bounds.max.y);

            transform.position = pos;

            //if (p.x == _bounds.min.x || p.x == _bounds.max.x)
            //    syncPosition.x = p.x;

            //if (p.z == _bounds.min.y || p.z == _bounds.max.y)
            //    syncPosition.z = p.z;

            if ((transform.position - syncPosition).sqrMagnitude < 0.02f)
                syncPosition = Vector3.zero;
        }
    }

    public void CenterOn(GameObject o)
    {
        syncPosition = o.transform.position;
        syncPosition.y = transform.position.y;

        syncPosition.z -= 10; // If I did my math right (z - tan(45) [which is 1] * 10)
    }

    bool input(KeyCode k)
    {
        return Input.GetKey(k);
    }

    bool input(string s)
    {
        return Input.GetKey(s);
    }
}
