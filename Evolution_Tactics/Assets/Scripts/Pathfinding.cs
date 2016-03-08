using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Started by: Lukas
// Modified by:

public class Pathfinding : MonoBehaviour
{
    public Tile startNode, endNode;

    public List<Tile> nodeList = new List<Tile>();
    public List<Tile> pathList = new List<Tile>();
    public List<Tile> openList = new List<Tile>();
    public List<Tile> closedList = new List<Tile>();

    public Character player;
    public int counter = 0;
    public bool goalAttained = false;

    void Start()
    {
        // Find all tiles and add them to the global list
        GameObject[] node = GameObject.FindGameObjectsWithTag("Tile");
        foreach (GameObject n in node)
        {
            nodeList.Add(n.GetComponent<Tile>());
        }

        // TODO REMOVED
        //Randomize start and end nodes and place player at start
        startNode = nodeList[Random.Range(0, nodeList.Count - 1)];
        endNode = nodeList[Random.Range(0, nodeList.Count - 1)];
        player.transform.position = new Vector3(startNode.transform.position.x, player.transform.position.y, startNode.transform.position.z);
        startNode.GetComponent<Renderer>().material.color = Color.green;
        endNode.GetComponent<Renderer>().material.color = Color.red;

        ClearLists();
        CalculateNewPath();
    }

    public void PathFind()
    {
        ClearLists();
        CalculateNewPath();
    }

    void Update()
    {
        // if path has been calculated
        if (!goalAttained && pathList.Count > counter && endNode == pathList[pathList.Count - 1])
        {
            bool tileCollision = false;

            //player.KinematicMovement(pathList[counter].transform.position);
            player.ArriveAndLookWhereYoureGoing(pathList[counter].transform.position);

            //Check for tile collision
            Collider[] collisionArray = Physics.OverlapSphere(player.transform.position, 0.3f);
            for (int i = 0; i < collisionArray.Length; i++)
            {          
                // Check if arrived
                if (collisionArray[i].GetComponent(typeof(Tile)) == endNode)
                {
                    goalAttained = true;
                }
                else if (collisionArray[i].GetComponent(typeof(Tile)) == pathList[counter])
                {
                    tileCollision = true;
                }
            }

            if (goalAttained || tileCollision)
            {
                counter++;
            }
        }
        else
        {
            CalculateNewPath();
        }
    }

    // Calculate a new path with dijkstra algorithm
    private void CalculateNewPath()
    {
        openList.Clear();
        closedList.Clear();
        pathList.Clear();

        counter = 0;
        startNode = nodeList[0];
        startNode.costSoFar = 0;

        foreach (Tile n in nodeList)
        {
            if (Cost(player.transform.position, n.transform.position) < Cost(player.transform.position, startNode.transform.position))
            {
                startNode = n;
            }
        }

        DijkstraPathfinding();
    }

    // Dijsktra pathfinding algorithm
    private void DijkstraPathfinding()
    {
        openList.Add(startNode);

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
                    neighbour.costSoFar = newCost;
                    neighbour.totalEstimatedValue = neighbour.costSoFar + neighbour.heuristicValue;
                    neighbour.prevNode = currentNode;
                    closedList.Remove(neighbour);
                    openList.Add(neighbour);

                }
                else if (inOpenList && newCost < neighbour.costSoFar)
                {
                    neighbour.costSoFar = newCost;
                    neighbour.totalEstimatedValue = neighbour.costSoFar + neighbour.heuristicValue;
                    neighbour.prevNode = currentNode;
                }
                else if (!inClosedList && !inOpenList)
                {
                    neighbour.costSoFar = newCost;
                    neighbour.totalEstimatedValue = neighbour.costSoFar + neighbour.heuristicValue;
                    neighbour.prevNode = currentNode;
                    openList.Add(neighbour);
                }

            }

        }

        pathList.Add(endNode);
        while (true)
        {

            if (pathList[pathList.Count - 1].prevNode == startNode)
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
    private void ClearLists()
    {
        openList.Clear();
        closedList.Clear();
        pathList.Clear();

        foreach (Tile node in nodeList)
        {
            node.costSoFar = 0;
            node.totalEstimatedValue = 0;
            node.heuristicValue = 0;
        }
    }
}
