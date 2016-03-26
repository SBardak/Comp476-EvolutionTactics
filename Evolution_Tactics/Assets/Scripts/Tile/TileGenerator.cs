using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

[System.Serializable]
public class TileGeneratorInspector
{
    public string Name;
    public string charCode;
    public int occurence = 1;

    public TileStats.type Type;
}

public class TileGenerator : MonoBehaviour
{
    public static TileGenerator Instance;

    public GameObject player;

    public Tile tilePrefab;
    public TileGeneratorInspector[] _tilePrefabs;
    public int mapWidth, mapHeight;

    private Tile[,] tiles;

    private GameObject map;

    [SerializeField]
    TextAsset TestMap;

    Dictionary<string, TileStats.type> dict = new Dictionary<string, TileStats.type>();

    float yPos = 0;

    void Awake()
    {
        Instance = this;
        foreach (var t in _tilePrefabs)
            dict.Add(t.charCode, t.Type);

        if (TestMap == null || !ReadMap())
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

        // No newline
        if (i == -1)
            return false;

        // Check if our maps
        var fl = s.Substring(0, i);
        if (fl != "Evo_Map")
            return false;

        // Get second line which contains width/height
        int i2, a2;
        GetIndex(s, i + a, out i2, out a2);

        int w, h;
        GetDimensions(s.Substring(i + a, i2 - (i + a)), out w, out h);
        mapWidth = w;
        mapHeight = h;

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

        var fLines = Regex.Split(s, "\r\n|\n|\r");

        //foreach (var l in fLines)
        for (int current = fLines.Length - 1; current >= 0; current--)
        {
            var l = fLines[current];
            foreach (var c in l.Split(' '))
            {
                if (c == "")
                    continue;

                var tile = Instantiate(tilePrefab, new Vector3(col, yPos, row), Quaternion.identity) as Tile;
                SetTileType(c, tile.gameObject);
                tiles[col, row] = tile;
                tiles[col, row].transform.parent = map.transform;
                tiles[col, row].name = "TILE " + count;

                ++col;
                ++count;
            }

            col = 0;
            ++row;
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
        if (i == -1)
        {
            a = 1;
            char[] chars = new char[2];
            chars[0] = '\r';
            chars[1] = '\n';

            i = s.IndexOfAny(chars, start);
            if (i == -1)
                return;
        }

        index = i;
        extra = a;
    }

    /// <summary>
    /// Set tile type indicated by char
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    void SetTileType(string t, GameObject o)
    {
        TileStats ts;
        if (dict.ContainsKey(t) && (ts = o.GetComponent<TileStats>()) != null)
            ts.SetTile(dict[t]);
    }

    void Start()
    {
    }

    void Update()
    {

    }

    private void CreateMap()
    {
        var table = GenerateProbabilityRanges();

        map = new GameObject();
        map.name = "Map";

        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                var tileIndex = Random.Range(0, 101);

                var tile = Instantiate(tilePrefab, new Vector3(i, yPos, j), Quaternion.identity) as Tile;
                SetTileType(GetTileType(table, tileIndex), tile.gameObject);
                tiles[i, j] = tile;
                tiles[i, j].transform.parent = map.transform;
                tiles[i, j].name = "TILE " + (i * mapWidth + j);
            }
        }
    }

    private List<KeyValuePair<int, TileGeneratorInspector>> GenerateProbabilityRanges()
    {
        int max = 0;
        int totalSoFar = 0;
        List<KeyValuePair<int, TileGeneratorInspector>> table =
            new List<KeyValuePair<int, TileGeneratorInspector>>();

        foreach (var tp in _tilePrefabs)
        {
            if (tp.occurence < 0)
                tp.occurence = 0;
            max += tp.occurence;
        }

        foreach (var tp in _tilePrefabs)
        {
            if (tp.occurence == 0)
                continue;
            totalSoFar += tp.occurence * 100 / max;
            table.Add(new KeyValuePair<int, TileGeneratorInspector>(totalSoFar, tp));
        }

        return table;
    }

    private string GetTileType(List<KeyValuePair<int, TileGeneratorInspector>> table, int value)
    {
        foreach (var t in table)
            if (t.Key > value)
                return t.Value.charCode;
        return table[0].Value.charCode;
    }

    public bool InXRange(int t)
    {
        return t >= 0 && t < mapWidth;
    }
    public bool InZRange(int t)
    {
        return t >= 0 && t < mapHeight;
    }
    public bool InMap(int x, int z)
    {
        return InXRange(x) && InZRange(z);
    }

    public Tile[,] Tiles
    {
        get { return tiles; }
    }
}
