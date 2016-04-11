using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;
using System.Linq;

#region Inspector class
/// <summary>
/// </summary>
[System.Serializable]
public class TileObstacleType
{
    public string Name;
    public TileStats.type Type;
    public Material Material;
}
#endregion Inspector class

public class TileObstacle : MonoBehaviour {
    [SerializeField]
    TileObstacleType[] Types;

    public void SetType(TileStats.type type)
    {
        var t = Types.First(a => a.Type == type);
        if (t == null)
            return;

        if (t.Material == null)
            return;

        var r = GetComponent<Renderer>();
        if (r == null)
            r = GetComponentInChildren<Renderer>();

        if (r == null)
            return;

        var rotation = UnityEngine.Random.Range(0, 361);
        this.transform.rotation = Quaternion.Euler(0, rotation, 0);

        r.material = t.Material;
    }

    public List<TileStats.type> GetTypes()
    {
        return Types.Select(t => t.Type).ToList();
    }
}

[System.Serializable]
public class TileObstacleList {
    [SerializeField]
    TileObstacle[] _obstacles;
    bool isInitialized = false;

    Dictionary<TileStats.type, List<TileObstacle>> _list = new Dictionary<TileStats.type, List<TileObstacle>>();

    public List<TileObstacle> GetObstacles(TileStats.type t)
    {
        if (!_list.ContainsKey(t))
            InitializeType(t);

        return _list[t];
    }

    void InitializeType(TileStats.type t)
    {
        var list = _obstacles.Where(o => o.GetTypes().Contains(t)).ToList();
        _list.Add(t, list);
    }
    
}
