using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Started by: Lukas
// Modified by:

public class Pathfinding : MonoBehaviour
{
    public delegate void PathFindingHandler();

    public event PathFindingHandler OnReachEnd;

    public Tile _startNode, _endNode;

    public List<Tile> nodeList = new List<Tile>();
    public List<Tile> pathList = new List<Tile>();
    public List<Tile> openList = new List<Tile>();
    public List<Tile> closedList = new List<Tile>();

    private Character player;
    public int counter = 0;
    public bool _goalAttained = false;
    public bool _middleOfTheNodeAttained = false;
    public bool _hasPath = false;

    void Start()
    {
        Graph();
    }

    void Graph()
    {
        // Find all tiles and add them to the global list
        GameObject[] node = GameObject.FindGameObjectsWithTag("Tile");
        player = GetComponentInParent<Character>();

        foreach (GameObject n in node)
        {
            nodeList.Add(n.GetComponent<Tile>());
        }

        // TODO REMOVED
        //Randomize start and end nodes and place player at start
        StartNode = nodeList[Random.Range(0, nodeList.Count - 1)];
        //_endNode = nodeList[Random.Range(0, nodeList.Count - 1)];

        player.transform.position = new Vector3(_startNode.transform.position.x, player.transform.position.y, _startNode.transform.position.z);
        // _startNode.GetComponent<Renderer>().material.color = Color.green;
        //_endNode.GetComponent<Renderer>().material.color = Color.red;

        //CalculateNewPath();
    }

    void Update()
    {
        // TODO: Change
        if (nodeList.Count == 0)
        {
            Debug.LogError("REMOVE ME. PROBLEM: Start comes too soon, tiles not generated");
            Graph();
        }

        // if path has been calculated
        if (_hasPath)
        {
            // if end node attained
            if (!_goalAttained)
            {
                if (pathList.Count > counter && _endNode == pathList[pathList.Count - 1])
                {
                    bool tileCollision = false;

                    player.ArriveAndLookWhereYoureGoing(pathList[counter].transform.position);

                    //Check for tile collision
                    Collider[] collisionArray = Physics.OverlapSphere(player.transform.position, 0.05f);
                    for (int i = 0; i < collisionArray.Length; i++)
                    {          
                        // Check if arrived
                        if (collisionArray[i].GetComponent(typeof(Tile)) == _endNode)
                        {
                            // TODO: Walk towards center of tile

                            GoalAttained = true;

                            if (OnReachEnd != null)
                                OnReachEnd();
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
                }
            }
        }
    }

    private bool GoToMiddleOfTile()
    {
        return player.MoveTo(_endNode.transform.position, pathList[pathList.Count - 2].transform.position);
    }


    public void RandomPath()
    {
        StartNode = player._currentTile;
        _endNode = nodeList[Random.Range(0, nodeList.Count - 1)];

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
            RandomPath();
            return;
        }
        // if player select a non-empty tile
        if (_endNode._player != null)
        {
            Debug.Log("Need to select empty tile");
            RandomPath();
            return;
        }
        
        ResetPath();

        //foreach (Tile n in nodeList)
        //{
        //    if (Cost(player.transform.position, n.transform.position) < Cost(player.transform.position, _startNode.transform.position))
        //    {
        //        StartNode = n;
        //    }
        //}

        DijkstraPathfinding();
    }

    // Dijsktra pathfinding algorithm
    private void DijkstraPathfinding()
    {
        openList.Add(_startNode);

        //while open list is open or closed list does not include all nodes
        while (openList.Count > 0 || closedList.Count != nodeList.Count)
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
                    if (neighbour._player == null || (neighbour._player != null && neighbour._player.tag == gameObject.tag))
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
                    if (neighbour._player == null || (neighbour._player != null && neighbour._player.tag == gameObject.tag))
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

        foreach (Tile node in nodeList)
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
