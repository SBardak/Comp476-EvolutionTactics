using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Started by: Lukas
// Modified by:

/*
  This script is for the moment only for testing purpose
  */

public class Tile : MonoBehaviour
{
    public Character player;

    public List<Tile> neighbours;
    public float costSoFar, heuristicValue, totalEstimatedValue;
    public Tile prevNode;

    void Start()
    {
        neighbours = new List<Tile>();
        GetNeighbours();
    }

    void Update()
    {
	
    }

    // Get its neighbours
    // TODO Will need to add more checkers, for instance to see if neighbour can be visited
    private void GetNeighbours()
    {
        Tile[,] tiles = GameObject.Find("TileGenerator").GetComponent<TileGenerator>().Tiles;
        int x = (int)transform.position.x;
        int z = (int)transform.position.z;

        if (x - 1 >= 0)
        {
            neighbours.Add(tiles[x - 1, z]);
        }
        if (x + 1 < tiles.GetLength(0))
        {
            neighbours.Add(tiles[x + 1, z]);
        }
        if (z - 1 >= 0)
        {
            neighbours.Add(tiles[x, z - 1]);
        }
        if (z + 1 < tiles.GetLength(1))
        {
            neighbours.Add(tiles[x, z + 1]);
        }
    }
}
