using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using System.Linq;

#region Inspector class
/// <summary>
/// Used to: Assign a charcode to a type as well as chance of occuring in random map generation
/// </summary>
[System.Serializable]
public class TileGeneratorInspector
{
    public string Name;
    public string charCode;
    public int occurence = 1;

    public TileStats.type Type;
}
#endregion Inspector class

public class TileGenerator : MonoBehaviour
{
    #region Fields

    public static TileGenerator Instance;

    public GameObject player;
    public HealingCollectible healingCollectiblePrefab;
    private HealingCollectible _collectible;

    public Tile tilePrefab;
    public TileGeneratorInspector[] _tilePrefabs;
    public int mapWidth, mapHeight;

    private Tile[,] tiles;

    private GameObject map;

    [SerializeField]
    TextAsset TestMap;

    Dictionary<string, TileStats.type> dict = new Dictionary<string, TileStats.type>();

    float yPos = 0;

    [SerializeField]
    TileObstacleList _obstacles;

    #endregion Fields

    //=========================================================================

    #region Properties

    /// <summary>
    /// Gets 2d array map tiles
    /// </summary>
    public Tile[,] Tiles
    {
        get { return tiles; }
    }

    #endregion Properties

    //=========================================================================

    #region Methods

    //-------------------------------------------------------------------------

    #region Map file reading

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

                var pieces = c.Split(':');
                var tileChar = pieces[0];
                bool tileObj = pieces.Length > 1;                    

                var tile = Instantiate(tilePrefab, new Vector3(col, yPos, row), Quaternion.identity) as Tile;
                SetTileType(tileChar, tile.gameObject);
                if (tileObj)
                    SetTileObstacle(tile);

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

    #endregion Map file reading

    //-------------------------------------------------------------------------

    #region Random map generation

    /// <summary>
    /// Generates a random map
    /// </summary>
    void CreateMap()
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
                SetRandomTileObstacle(tile);

                tiles[i, j] = tile;
                tiles[i, j].transform.parent = map.transform;
                tiles[i, j].name = "TILE " + (i * mapWidth + j);
            }
        }
    }

    void SetRandomTileObstacle(Tile t)
    {
        if (Random.Range(0, 100) < 90)
            return;

        SetTileObstacle(t);
    }

    void SetTileObstacle(Tile t)
    {
        var type = t.GetComponent<TileStats>().MyType;
        var obs = _obstacles.GetObstacles(type);

        if (obs.Count == 0)
            return;

        var rand = Random.Range(0, obs.Count);
        var obj = (TileObstacle)Instantiate(obs[rand], t.transform.position, Quaternion.identity);

        obj.SetType(type);
        obj.transform.parent = t.transform;
        t._obstacle = obj;
    }

    /// <summary>
    /// Creates the probability table using values from array
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Gets the tile type in the probability table
    /// </summary>
    /// <param name="table">Probability table</param>
    /// <param name="value">Probability index</param>
    /// <returns></returns>
    private string GetTileType(List<KeyValuePair<int, TileGeneratorInspector>> table, int value)
    {
        foreach (var t in table)
            if (t.Key > value)
                return t.Value.charCode;
        return table[0].Value.charCode;
    }

    #endregion Random map generation

    //-------------------------------------------------------------------------

    #region Boundaries

    /// <summary>
    /// Within map (width)
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public bool InXRange(int t)
    {
        return t >= 0 && t < mapWidth;
    }

    /// <summary>
    /// Within map (height)
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public bool InZRange(int t)
    {
        return t >= 0 && t < mapHeight;
    }

    /// <summary>
    /// Combines width/height together
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public bool InMap(int x, int z)
    {
        return InXRange(x) && InZRange(z);
    }

    #endregion Boundaries

    //-------------------------------------------------------------------------

    #region Collectible (health for now)

    /// <summary>
    /// Sets a new collectible onto a tile
    /// </summary>
    public void SetNewCollectibleTile()
    {
        if (_collectible != null)
        {
            _collectible._currentTile._hCollectible = null;
            Destroy(_collectible.gameObject);
        }
        
        int height = Random.Range(0, mapHeight - 1), width = Random.Range(0, mapWidth - 1);
        Debug.LogWarning(mapHeight + " " + height + " " + mapWidth + " " + width);
        Tile t = Tiles[width, height];
        while (t.IsOccupied)
        {
            height = Random.Range(0, mapHeight - 1);
            width = Random.Range(0, mapWidth - 1);
            t = Tiles[width, height];
        }

        Vector3 newPos = new Vector3(t.transform.position.x, healingCollectiblePrefab.transform.position.y, t.transform.position.z);
        _collectible = Instantiate(healingCollectiblePrefab, newPos, Quaternion.identity) as HealingCollectible;
        _collectible._currentTile = t;
        t._hCollectible = _collectible;
    }

    #endregion Collectible (health for now)

    //-------------------------------------------------------------------------

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

    public Tile GetSurroundingAvailableTile(Tile t, int range)
    {
        var tiles = t.GetTilesAny(range);
        return tiles.First(kvp => !kvp.Key.IsOccupied).Key;
    }

    #endregion Methods
}
