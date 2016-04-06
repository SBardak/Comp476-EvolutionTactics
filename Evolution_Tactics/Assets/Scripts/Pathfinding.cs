﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Started by: Lukas
// Modified by:

public class Pathfinding : MonoBehaviour
{
    public delegate void PathFindingHandler();

    public event PathFindingHandler OnReachEnd;

    public Tile _startNode, _endNode;

    public List<Tile> pathList = new List<Tile>();
    public List<Tile> openList = new List<Tile>();
    public List<Tile> closedList = new List<Tile>();

    private Character player;
    public int counter = 0;
    public bool _goalAttained = false;
    public bool _middleOfTheNodeAttained = false;
    public bool _hasPath = false;

    private TileStats.type myType;
    private int movementRange;
    private PokemonStats stats;

    void Start()
    {
        Graph();
        stats = GetComponent<PokemonStats>();
        myType = stats.MyType;
        movementRange = stats.MovementRange;
    }

    void Graph()
    {
        player = GetComponentInParent<Character>();
    }

    void Update()
    {
        // if path has been calculated
        if (_hasPath)
        {
            // if end node attained
            if (!_goalAttained)
            {
                if (counter <= movementRange || pathList.Count > counter && _endNode == pathList[pathList.Count - 1])
                {
                    bool tileCollision = false;

                    player.ArriveAndLookWhereYoureGoing(pathList[counter].transform.position);

                    //Check for tile collision
                    Collider[] collisionArray = Physics.OverlapSphere(player.transform.position, 0.05f);
                    for (int i = 0; i < collisionArray.Length; i++)
                    {          
                        // Check if arrived
                        if ((counter > movementRange && !pathList[counter].HasPlayer) || collisionArray[i].GetComponent(typeof(Tile)) == _endNode)
                        {
                            GoalAttained = true;
                            _endNode = pathList[counter];
                            if (_endNode._hCollectible != null)
                            {
                                stats.GiveHealth(_endNode._hCollectible.healthGiven);
                                TileGenerator.Instance.SetNewCollectibleTile();
                            }
                        }
                        else if (collisionArray[i].GetComponent(typeof(Tile)) == pathList[counter])
                        {
                            tileCollision = true;
                        }
                        player.SetCurrentTile(pathList[counter]);
                    }

                    if (_goalAttained || tileCollision)
                    {
                        counter++;
                    }
                }
                else
                {
                    CalculateNewPath();
                }
            }
            // if not in the middle of the end node
            else if (!_middleOfTheNodeAttained)
            {
                if (GoToMiddleOfTile())
                {
                    _middleOfTheNodeAttained = true;
                    _hasPath = false;
                    if (OnReachEnd != null)
                        OnReachEnd();
                }
            }
        }
    }

    private bool GoToMiddleOfTile()
    {
        return player.MoveTo(_endNode.transform.position, pathList[pathList.Count - 2].transform.position);
    }

    public void SetPath(Tile end)
    {
        StartNode = player._currentTile;
        _endNode = end;

        CalculateNewPath();
    }

    // Calculate a new path with dijkstra algorithm
    public void CalculateNewPath()
    {
        counter = 0;
        StartNode = player._currentTile;
        _startNode.costSoFar = 0;

        
        // If player select current node
        if (_startNode == _endNode)
        {
            // might add more behaviour
            Debug.Log("SAME TILE");   
        }
        // if player select a non-empty tile
        if (_endNode.IsOccupied)
        {
            Debug.Log("Need to select empty tile");
            return;
        }
        
        ResetPath();

        DijkstraPathfinding();
    }

    // Dijsktra pathfinding algorithm
    private void DijkstraPathfinding()
    {
        openList.Add(_startNode);

        //while open list is open or closed list does not include all nodes
        while (openList.Count > 0)// || closedList.Count != TileGenerator.Instance.Tiles.Length)
        {
            Tile currentNode = openList[0];

            // Bunch of cost calculations and logic
            foreach (Tile candidateNode in openList)
            {
                if (candidateNode.totalEstimatedValue < currentNode.totalEstimatedValue)
                {
                    currentNode = candidateNode;
                }
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (Tile neighbour in currentNode.neighbours)
            {
                if (neighbour == null)
                {
                    continue;
                }

                bool inOpenList = false;
                bool inClosedList = false;

                if (closedList.Contains(neighbour))
                    inClosedList = true;
                else if (openList.Contains(neighbour))
                    inOpenList = true;


                float newCost = (currentNode.costSoFar + Cost(currentNode.transform.position, neighbour.transform.position));

                if (closedList.Contains(neighbour) && newCost < neighbour.costSoFar)
                {
                    if (!neighbour.IsOccupied || (neighbour.HasPlayer && (neighbour._character.tag == gameObject.tag || myType == TileStats.type.Flying)))
                    {
                        neighbour.costSoFar = newCost;
                        neighbour.totalEstimatedValue = neighbour.costSoFar + neighbour.heuristicValue;
                        neighbour.prevNode = currentNode;
                        closedList.Remove(neighbour);
                        openList.Add(neighbour);
                    }
                    else
                    {
                        closedList.Add(neighbour);
                    }

                }
                else if (inOpenList && newCost < neighbour.costSoFar)
                {
                    neighbour.costSoFar = newCost;
                    neighbour.totalEstimatedValue = neighbour.costSoFar + neighbour.heuristicValue;
                    neighbour.prevNode = currentNode;
                }
                else if (!inClosedList && !inOpenList)
                {
                    if (!neighbour.IsOccupied || (neighbour.HasPlayer && (neighbour._character.tag == gameObject.tag || myType == TileStats.type.Flying)))
                    {
                        neighbour.costSoFar = newCost;
                        neighbour.totalEstimatedValue = neighbour.costSoFar + neighbour.heuristicValue;
                        neighbour.prevNode = currentNode;
                        openList.Add(neighbour);
                    }
                    else
                    {
                        closedList.Add(neighbour);
                    }
                }
            }

        }

        pathList.Add(_endNode);
        while (true)
        {

            if (pathList[pathList.Count - 1].prevNode == _startNode)
            {
                pathList.Add(pathList[pathList.Count - 1].prevNode);
                pathList.Reverse();
                return;
            }
            else
            {
                pathList.Add(pathList[pathList.Count - 1].prevNode);
            }
        }

    }

    // Calculate the cost
    private float Cost(Vector3 currentNode, Vector3 neighbor)
    {
        return (currentNode - neighbor).magnitude;
    }

    // Clear all lists and tile's variables
    private void ResetPath()
    {
        openList.Clear();
        closedList.Clear();
        pathList.Clear();
        GoalAttained = false;
        _middleOfTheNodeAttained = false;
        _hasPath = true;

        foreach (Tile node in TileGenerator.Instance.Tiles)
        {
            node.costSoFar = 0;
            node.totalEstimatedValue = 0;
            node.heuristicValue = 0;
        }
    }

    public bool GoalAttained
    {
        set
        {
            if (transform.tag == "Human")
            {
                if (value)
                    UIManager.Instance.CreateHumanPlayerActionUI(player);
                else
                    UIManager.Instance.DeleteHumanPlayerActionUI();
            }

            _goalAttained = value;
        }
        get
        {
            return _goalAttained;
        }
    }

    public Tile StartNode
    {
        set
        {
            player.SetCurrentTile(value);
            _startNode = value;
        }
    }
}
