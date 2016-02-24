﻿using UnityEngine;
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

    void Start()
    {
        tiles = new Tile[mapWidth, mapHeight];
        CreateMap();
    }

    void Update()
    {

    }

    private void CreateMap()
    {
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                tiles[i, j] = Instantiate(tilePrefab, new Vector3(i, 0f, j), Quaternion.identity) as Tile;
            }
        }
        int x = Random.Range(0, mapWidth);
        int z = Random.Range(0, mapHeight);

        player.transform.position = new Vector3(x, 0f, z);
    }

    public Tile[,] Tiles
    {
        get { return tiles; }
    }
}
