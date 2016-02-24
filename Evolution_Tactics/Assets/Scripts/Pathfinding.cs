using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pathfinding : MonoBehaviour
{
    public Tile startNode, endNode;

    public List<Tile> nodeList = new List<Tile>();
    public List<Tile> pathList = new List<Tile>();
    public List<Tile> openList = new List<Tile>();
    public List<Tile> closedList = new List<Tile>();

    public Player player;
    public int pathCounter = 0;

    void Start()
    {
        GameObject[] povs = GameObject.FindGameObjectsWithTag("Tile");

        foreach (GameObject o in povs)
        {
            nodeList.Add(o.GetComponent<Tile>());
        }

        startNode = nodeList[Random.Range(0, nodeList.Count - 1)];
        endNode = nodeList[Random.Range(0, nodeList.Count - 1)];

        player.transform.position = new Vector3(startNode.transform.position.x, player.transform.position.y, startNode.transform.position.z);

        startNode.GetComponent<Renderer>().material.color = Color.green;
        endNode.GetComponent<Renderer>().material.color = Color.red;

        ClearPov();
        CalculateNewPovPath();
    }

    void Update()
    {
        //If the NPC is in the middle of a pathfinding journey, let it continue
        if (pathList.Count > pathCounter && endNode == pathList[pathList.Count - 1])
        {
            if (Vector3.Angle(player.transform.forward, (pathList[pathCounter].transform.position - player.transform.position)) > 35)
            {
                player.ArriveAndLookWhereYoureGoing(pathList[pathCounter].transform.position);
            }
            else
            {
                player.ArriveAndLookWhereYoureGoing(pathList[pathCounter].transform.position);
            }
            bool nodeAttained = false;
            Collider[] collisionArray = Physics.OverlapSphere(player.transform.position, 0.2f);
            for (int i = 0; i < collisionArray.Length; i++)
            {
                if (collisionArray[i].GetComponent(typeof(Tile)) == pathList[pathCounter])
                {
                    nodeAttained = true;
                }
            }

            if (nodeAttained)
            {
                pathCounter++;
            }
        }
        else
        {
            CalculateNewPovPath();
        }
    }

    private void CalculateNewPovPath()
    {
        openList.Clear();
        closedList.Clear();
        pathList.Clear();

        pathCounter = 0;
        startNode = nodeList[0];
        startNode.costSoFar = 0;

        foreach (Tile n in nodeList)
        {
            if (Cost(player.transform, n.transform) < Cost(player.transform, startNode.transform))
            {
                startNode = n;
            }
        }

        /*switch (mode)
        {
            case Modes.CLUSTER:
                calculateCluster_Pov();
                break;
            case Modes.DIJKSTRA:
                calculateDijkstra_Pov();
                break;
            case Modes.EUCLIDEAN:
                calculateEuclidean_Pov();
                break;
        }*/
        calculateDijkstra_Pov();
    }

    void calculateDijkstra_Pov()
    {
        openList.Add(startNode);

        while (openList.Count > 0 || closedList.Count != nodeList.Count)
        {
            Tile currentNode = openList[0];

            foreach (Tile candidateNode in openList)
            {
                if (candidateNode.totalEstimatedValue < currentNode.totalEstimatedValue)
                {
                    currentNode = candidateNode;
                }
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (Tile neighbor in currentNode.neighbours)
            {
                if (neighbor == null)
                {
                    continue;
                }

                bool inOpenList = false;
                bool inClosedList = false;

                if (closedList.Contains(neighbor))
                    inClosedList = true;
                else if (openList.Contains(neighbor))
                    inOpenList = true;


                float newCost = (currentNode.costSoFar + Cost(currentNode.transform, neighbor.transform));

                if (closedList.Contains(neighbor) && newCost < neighbor.costSoFar)
                {
                    neighbor.costSoFar = newCost;
                    neighbor.totalEstimatedValue = neighbor.costSoFar + neighbor.heuristicValue;
                    neighbor.prevNode = currentNode;
                    closedList.Remove(neighbor);
                    openList.Add(neighbor);

                }
                else if (inOpenList && newCost < neighbor.costSoFar)
                {
                    neighbor.costSoFar = newCost;
                    neighbor.totalEstimatedValue = neighbor.costSoFar + neighbor.heuristicValue;
                    neighbor.prevNode = currentNode;
                }
                else if (!inClosedList && !inOpenList)
                {
                    neighbor.costSoFar = newCost;
                    neighbor.totalEstimatedValue = neighbor.costSoFar + neighbor.heuristicValue;
                    neighbor.prevNode = currentNode;
                    openList.Add(neighbor);
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

    private float Cost(Transform currentNode, Transform neighbor)
    {
        return (currentNode.position - neighbor.position).magnitude;
    }

    private void ClearPov()
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
