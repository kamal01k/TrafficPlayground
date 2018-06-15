using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Vector3 location;

    public List<Node> neighbors = new List<Node>();
    public GameObject nodeGameObject;
    public bool isOriginDestination;
    public int OriginDestinationNumber;
    public GameObject intersectionPrefab;
    public GameObject originDestinationPrefab;

    public enum NodeState
    {
        Intersection,
        OriginDestination,
        ToBeDestroyed
    }

    public NodeState nodeState;

    // If a node is clicked on, it should move to the next state.
    public NodeState GetNextNodeState()
    {
        switch (nodeState)
        {
            case NodeState.Intersection:
                return NodeState.OriginDestination;
            case NodeState.OriginDestination:
                return NodeState.Intersection;
            default:
                Debug.LogError("Invalid nodestate, returning Intersection.");
                return NodeState.Intersection;
        }
    }

    public Node (Vector3 location, GameObject nodeGameObject)
    {
        this.location = location;
        this.nodeGameObject = nodeGameObject;
        this.nodeState = NodeState.Intersection;
        Debug.Log("Adding node at " + location);
    }

    public void SelectNextNodeState()
    {
        //Destroy(nodeGameObject);

        nodeState = GetNextNodeState();
        if (nodeState == NodeState.OriginDestination)
        {
            nodeGameObject = originDestinationPrefab;
        }
        else
        {
            nodeGameObject = intersectionPrefab;
        }
    }

    public void AddNeighbor(Node neighbor)
    {
        neighbors.Add(neighbor);
        Debug.Log("Adding neighbor from " + location + " to " + neighbor.location);
    }

    public void RemoveNeighbor(Node neighbor)
    {
        neighbors.Remove(neighbor);
        Debug.Log("Removing neighbor at " + neighbor.location + " from " + location);
    }

}
