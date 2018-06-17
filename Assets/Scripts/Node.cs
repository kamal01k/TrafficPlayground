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
    public NodeState nodeState;

    public enum NodeState
    {
        Intersection,
        OriginDestination,
        ToBeDestroyed
    }

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

    public Node(Vector3 location, GameObject intersectionPrefab, GameObject originDestinationPrefab)
    {
        this.location = location;

        // Nodes always start as intersections.  Unfortunately I don't yet know how
        // to get a reference to a prefab from a class that doesn't
        // inherit MonoBehaviour, so we have to use this references that were passed
        // in from NodeController.
        this.intersectionPrefab = intersectionPrefab;
        this.originDestinationPrefab = originDestinationPrefab;
        this.nodeGameObject = GameObject.Instantiate(intersectionPrefab, location, Quaternion.identity);
        this.nodeState = NodeState.Intersection;
    }

    public override string ToString()
    {
        return location.x + ", " + location.z;
    }

    public void SelectNextNodeState()
    {
        nodeState = GetNextNodeState();

        GameObject prefabToInstantiate;
        if (nodeState == NodeState.OriginDestination)
        {
            prefabToInstantiate = originDestinationPrefab;
        }
        else
        {
            prefabToInstantiate = intersectionPrefab;
        }

        GameObject.Destroy(nodeGameObject);
        nodeGameObject = GameObject.Instantiate(originDestinationPrefab, location, Quaternion.identity);
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