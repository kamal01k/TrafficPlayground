using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Handles click events on the grid:
// - Starting a new road
// - Finishing a road
// - Cancelling a road that's be drawn
// - Toggling between intersections and origin/destinations
public class NodeController : MonoBehaviour
{
    public GameObject nodePrefab;
    public GameObject intersectionPrefab;
    public GameObject roadPrefab;
    public GameObject carPrefab;
    public GameObject selectionIndicator;
    public GameObject originDestinationPrefab;

    private List<Node> nodes = new List<Node>();
    private bool drawingRoad;
    private bool drawingFromExistingNode;
    private Node roadStart;
    private GameObject newRoad;
    private GameObject carsParent;
    private bool leftMouseButtonPressed;
    private Vector3 positionWhereLeftMouseButtonPressed;
    private bool nodeExistsWhereLeftMouseButtonPressed;
    private Node nodeWhereLeftMouseButtonPressed;
    private bool clickedOnExistingNode;

    public void Start()
    {
        carsParent = GameObject.Find("Cars");
    }

    public void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            GameObject newCar = Instantiate(carPrefab, carsParent.transform);
            CarController carController = newCar.GetComponent<CarController>();

            Node origin = GetRandomOriginDestination();
            Node destination = GetOriginDestinationOtherThan(origin);
            List<Route> routes = GetRoutes(origin, destination);
            PrintRoutes(routes);
            if (routes.Count < 1)
            {
                Debug.Log("No routes to set for car.");
                return;
            }
            int indexOfRouteToUse = Random.Range(0, routes.Count);
            carController.SetRoute(routes[indexOfRouteToUse]);
        }
    }

    private void PrintRoutes(List<Route> routes)
    {
        if (routes.Count == 0)
        {
            Debug.Log("No routes found.");
            return;
        }

        int routeNumber = 1;
        foreach (Route route in routes)
        {
            string routeString = "Route " + routeNumber + ":";
            foreach (Node node in route.path)
            {
                routeString += " " + node + ";";
            }
            Debug.Log(routeString);
            routeNumber++;
        }
    }

    public void HoveringOnNewGridPoint(Vector3 currentPosition)
    {
        // Update the node showing what grid point the user has moved over
        selectionIndicator.transform.position = currentPosition;

        // If mouse button is held down and current mouse position is different
        // than clicked position, draw a road.
        if (leftMouseButtonPressed && (currentPosition != positionWhereLeftMouseButtonPressed))
        {
            DrawRoad(positionWhereLeftMouseButtonPressed, currentPosition);
        }
    }

    private void DrawRoad(Vector3 start, Vector3 end)
    {
        // Find midpoint on plane betweent start and end position,
        // put the road there, and rotate it so that it goes from start to end
        Vector3 midpoint = Vector3.Lerp(start, end, 0.5f);
        Quaternion rotation = Quaternion.identity;
        rotation.SetLookRotation(start - end);

        float roadLength = Vector3.Distance(start, end);
        if (newRoad == null)
        {
            newRoad = Instantiate(roadPrefab, midpoint, rotation);
            newRoad.transform.localScale = new Vector3(0.2f, 0.1f, roadLength);
        }
        else
        {
            newRoad.transform.localScale = new Vector3(0.2f, 0.1f, roadLength);
            newRoad.transform.position = midpoint;
            newRoad.transform.rotation = rotation;
        }
    }

    public void LeftMousePressAtGridPoint(Vector3 position)
    {
        nodeWhereLeftMouseButtonPressed = AddOrLocateNode(position, out nodeExistsWhereLeftMouseButtonPressed);
        leftMouseButtonPressed = true;
        positionWhereLeftMouseButtonPressed = position;
    }

    public void LeftMouseReleasedAtGridPoint(Vector3 currentPosition)
    {
        leftMouseButtonPressed = false;

        // If button was released somewhere other than where it was clicked,
        // we need leave the road we drew and to create an intersection if
        // one doesn't already exist.
        if (currentPosition != positionWhereLeftMouseButtonPressed)
        {
            // TODO: What if the road already exists?  Don't draw another.. maybe delete it?
            newRoad = null;
            Node node = AddOrLocateNode(currentPosition);
            nodeWhereLeftMouseButtonPressed.AddNeighbor(node);
            node.AddNeighbor(nodeWhereLeftMouseButtonPressed);
        }

        // If the button was released on the same place that it was clicked
        // and a node was already there, cycle the node to the next type
        // (intersection > origin/destination, origin/destination > intersection)
        if ((currentPosition == positionWhereLeftMouseButtonPressed)
                && nodeExistsWhereLeftMouseButtonPressed)
        {
            nodeWhereLeftMouseButtonPressed.SelectNextNodeState();
        }
    }

    public void RightClickAtGridPoint(Vector3 position)
    {
        if (drawingRoad)
        {
            if (newRoad != null)
            {
                Destroy(newRoad);
                drawingRoad = false;
            }

            if (!drawingFromExistingNode)
            {
                RemoveNode(roadStart);
            }
        }
    }

    private Node AddNode(Vector3 location)
    {
        Node node = new Node(location, intersectionPrefab, originDestinationPrefab);
        nodes.Add(node);
        return node;
    }

    // Add or find an existing node.  This will be to start drawing a road, so
    // no information about connections is required.
    public Node AddOrLocateNode(Vector3 gridCoords, out bool nodeExists)
    {
        // Find out if a ndoe for this coord already exists
        Node foundNode = nodes.FirstOrDefault(n => n.location == gridCoords);

        if (foundNode != null)
        {
            nodeExists = true;
            return foundNode;
        }

        nodeExists = false;
        return AddNode(gridCoords);
    }

    public Node AddOrLocateNode(Vector3 gridCoords)
    {
        bool throwAway;
        return AddOrLocateNode(gridCoords, out throwAway);
    }

    // Add or locate an existing node and add connection information.
    // This is done when we're clicking to finish drawing a road.
    public Node AddOrUpdateNodeWithConnection(Vector3 gridCoords, Node fromNode)
    {
        Node node = AddOrLocateNode(gridCoords);
        fromNode.AddNeighbor(node);
        node.AddNeighbor(fromNode);
        return node;
    }

    public void RemoveNode(Node node)
    {
        Node foundNode = nodes.FirstOrDefault(n => n.location == node.location);
        if (foundNode != null)
        {
            Destroy(foundNode.nodeGameObject);
        }
    }

    public Node GetRandomNode()
    {
        if (nodes.Count <= 1)
        {
            return null;
        }

        int i = UnityEngine.Random.Range(0, nodes.Count);
        return nodes[i];
    }

    // Return a node which is marked as being an origin/destination
    public Node GetRandomOriginDestination()
    {
        var originDestinationNodes = from node in nodes
                                     where node.nodeState == Node.NodeState.OriginDestination
                                     select node;

        int originDestinationCount = originDestinationNodes.Count<Node>();
        if (originDestinationCount == 0)
        {
            return null;
        }

        return originDestinationNodes.ToList<Node>()[UnityEngine.Random.Range(0, originDestinationCount)];
    }

    // Return an origin/destination node other than the one we've specified
    public Node GetOriginDestinationOtherThan(Node specified)
    {
        var originDestinationNodes = from node in nodes
                                     where ((node.nodeState == Node.NodeState.OriginDestination) && node != specified)
                                     select node;

        int otherOriginDestinationCount = originDestinationNodes.Count<Node>();
        if (otherOriginDestinationCount == 0)
        {
            return null;
        }

        return originDestinationNodes.ToList<Node>()[Random.Range(0, otherOriginDestinationCount)];
    }

    // Get a list of all routs from start to end
    public List<Route> GetRoutes(Node start, Node end, Node cameFrom = null, List<Node> visitedNodes = null)
    {
        List<Route> routes = new List<Route>();

        if (visitedNodes == null)
        {
            visitedNodes = new List<Node>();
        }

        visitedNodes.Add(start);

        // Get all the neighbors that start connects to
        foreach (Node neighbor in start.neighbors)
        {
            // We want to ignore nodes that we've already visited.
            if (visitedNodes.Contains(neighbor))
            {
                continue;
            }

            // If we find the neighbor that is the destination,
            // return a list containing just that neighbor.
            if (neighbor == end)
            {
                Route route = new Route();
                route.Add(end);
                routes.Add(route);
                continue;
            }

            // Otherwise, keep looking
            List<Route> deeperRoutes = GetRoutes(neighbor, end, start, visitedNodes);
            if (deeperRoutes != null)
            {
                routes.AddRange(deeperRoutes);
            }
        }

        // If any routes were found, now we need to add ourselves to the front
        // of each before we return them.
        foreach (Route route in routes)
        {
            route.AddToFrontOfList(start);
        }

        // We also need to remove this node from the list of visited nodes,
        // as there may be another path to this node that we haven't yet found
        visitedNodes.Remove(start);

        return routes;
    }

    public Node GetRandomNeighbor(Node node)
    {
        int i = UnityEngine.Random.Range(0, node.neighbors.Count);
        return node.neighbors[i];
    }
}