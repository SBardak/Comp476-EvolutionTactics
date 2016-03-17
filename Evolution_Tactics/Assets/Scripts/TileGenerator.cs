using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Started by: Lukas
// Modified by:

/*
  This script is only for testing purpose
  */

[System.Serializable]
public class TileGeneratorInspector
{
    public string Name;
    public string charCode;

    public Tile TilePrefab;
}

public class TileGenerator : MonoBehaviour
{
    public GameObject player;

    public Tile tilePrefab, tileB;
    public TileGeneratorInspector[] _tilePrefabs;
    public int mapWidth, mapHeight;

    private Tile[,] tiles;

    private GameObject map;

    [SerializeField]
    TextAsset TestMap;

    Dictionary<string, Tile> dict = new Dictionary<string, Tile>();

    void Awake()
    {
        foreach (var t in _tilePrefabs)
            dict.Add(t.charCode, t.TilePrefab);

        if (TestMap != null && !ReadMap())
        {
            tiles = new Tile[mapWidth, mapHeight];
            CreateMap();
        }
    }

    /// <summary>
    /// Attempts to read a map file
    /// </summary>
    /// <returns></returns>
    bool ReadMap()
    {
        // File 'header'
        var s = TestMap.text;
        int i, a;
        GetIndex(s, 0, out i, out a);
        if (i == 0) return false;

        var fl = s.Substring(0, i);
        if (fl != "Evo_Map")
            return false;

        int i2, a2;
        GetIndex(s, i + a, out i2, out a2);

        int w, h;
        GetDimensions(s.Substring(i + a, i2 - (i + a)), out w, out h);
        s = s.Substring(i2 + a2);

        var col = 0;
        var row = 0;
        var count = 0;
        map = new GameObject();
        map.name = "Map";

        var t = 'a';

        tiles = new Tile[w, h];

        // If we use two lists and generate the neighbour list here,
        // we could potentially create none square maps.
        // lc Would be currently read tiles, last would be previous row.
        // Each col could check last char/tile and check if it exists, if so, connect to it
        //List<char> lc;
        //List<char> last

        foreach (var c in s)
        {
            //// Newline stuff
            if (c == '\r' || c == '\n')
            {
                if (t == '\r')
                {
                    t = 'a';
                    continue;
                }
                t = c;
                col = 0;
                ++row;
                continue;
            }

            tiles[col, row] = Instantiate(GetTile(c), new Vector3(col, 0f, row), Quaternion.identity) as Tile;
            tiles[col, row].transform.parent = map.transform;
            tiles[col, row].name = "TILE " + count;

            ++col;
            ++count;
        }
        return true;
    }
    /// <summary>
    /// Get dimensions from file
    /// </summary>
    /// <param name="s"></param>
    /// <param name="w"></param>
    /// <param name="h"></param>
    void GetDimensions(string s, out int w, out int h)
    {
        w = h = 0;

        var ss = s.Split(';');

        foreach (var sss in ss)
        {
            var split = sss.Split(':');
            Debug.Log(split[1]);
            int t = int.Parse(split[1]);
            if (split[0] == "w")
                w = t;
            else
                h = t;
        }
    }
    /// <summary>
    /// Helper method
    /// </summary>
    /// <param name="s"></param>
    /// <param name="start"></param>
    /// <param name="index"></param>
    /// <param name="extra"></param>
    void GetIndex(string s, int start, out int index, out int extra)
    {
        index = -1;
        extra = 0;

        var i = s.IndexOf("\r\n", start);
        var a = 2;

        // Not windows
        if (i == 0)
        {
            a = 1;
            char[] chars = new char[2];
            chars[0] = '\r';
            chars[1] = '\n';

            i = s.IndexOfAny(chars, start);
            if (i == 0)
                return;
        }

        index = i;
        extra = a;
    }
    /// <summary>
    /// Get tile indicated by char
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    Tile GetTile(char t)
    {
        if (dict.ContainsKey(t.ToString()))
            return dict[t.ToString()];
        else
            return tilePrefab;
    }

    void Start()
    {
    }

    void Update()
    {

    }

    private void CreateMap()
    {
        map = new GameObject();
        map.name = "Map";

        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                tiles[i, j] = Instantiate(tilePrefab, new Vector3(i, 0f, j), Quaternion.identity) as Tile;
                tiles[i, j].transform.parent = map.transform;
                tiles[i, j].name = "TILE " + (i * mapWidth + j);
            }
        }
    }

    public Tile[,] Tiles
    {
        get { return tiles; }
    }
}
