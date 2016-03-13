using UnityEngine;
using System.Collections;

// Started by: Lukas
// Modified by:

/*
  This script is only for testing purpose
  */

public class TileGenerator : MonoBehaviour
{
    public GameObject player;

    public Tile tilePrefab;
    public int mapWidth, mapHeight;

    private Tile[,] tiles;

    private GameObject map;

    void Awake()
    {
        tiles = new Tile[mapWidth, mapHeight];
        CreateMap();
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
            }
        }
    }

    public Tile[,] Tiles
    {
        get { return tiles; }
    }
}
