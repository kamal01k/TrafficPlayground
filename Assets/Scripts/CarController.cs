using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarController : MonoBehaviour
{

    public float speed;

    private NodeController nodeController;
    private Node origin;
    private Node destination;
    private GameObject car;
    private bool driving;
    private Route currentRoute;
    private int currentLeg;
    private Text destinationLabel;
    private CarState carState;
    private float brakingAcceleration = 0;

    private static float LABEL_OFFSET_X = 80;
    private static float LABEL_OFFSET_Y = 25;
    private static float CAR_OFFSET_FROM_CENTER_OF_ROAD = 0.055f;
    private static float CAR_STOP_DISTANCE = 0.2f;

    private List<Vector3> turningPath;
    private int turningPathIndex;

    public enum CarState
    {
        Spawned,
        Cruising,
        Braking,
        Stopped,
        Turning,
        Accelerating
    }

    public void SetOrigin(Node origin)
    {
        this.origin = origin;
    }

    public void SetDestination(Node destination)
    {
        this.destination = destination;
    }

    public void SetRoute(Route route)
    {
        currentRoute = route;
        currentLeg = 1;

        // Set the destination label to be the destination number of the last node in the path
        destinationLabel = GetComponentInChildren<Text>();
        destinationLabel.text = route.path[route.path.Count - 1].originDestinationNumber.ToString();

        SetOrigin(route.path[0]);
        SetDestination(route.path[currentLeg]);
    }

    // Use this for initialization
    void Start()
    {
        nodeController = FindObjectOfType<NodeController>();
        carState = CarState.Spawned;
        MoveToOriginAndPointAtDestination();
        StartDriving();
    }

    private void MoveToOriginAndPointAtDestination()
    {
        transform.position = origin.location;
        Quaternion rotation = Quaternion.identity;
        rotation.SetLookRotation(destination.location - origin.location);
        transform.rotation = rotation;
        transform.position += transform.right * CAR_OFFSET_FROM_CENTER_OF_ROAD;
    }

    public void OnTriggerEnter(Collider other)
    {
        switch (carState)
        {
            case CarState.Spawned:
                carState = CarState.Cruising;
                break;
            case CarState.Cruising:
                carState = CarState.Braking;
                break;
            default:
                Debug.Log("Car was in state " + carState + ", which OnTriggerEnter() didn't handle");
                break;
        }
        Debug.Log("I ran into " + other.name + " located at " + other.transform.root.position);
    }

    private void StartDriving()
    {
        driving = true;
    }

    private void StopDriving()
    {
        driving = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (driving)
        {
            switch (carState)
            {
                case CarState.Braking:
                    if (speed > 0)
                    {
                        // We want to be completely stopped when
                        // the car is CAR_STOP_DISTANCE from its current
                        // destination.
                        speed -= GetBrakingStrength() * Time.deltaTime;
                        transform.position += transform.forward * Time.deltaTime * speed;
                    }
                    else
                    {
                        carState = CarState.Stopped;
                        Debug.Log("Car is stopped.");
                    }
                    break;
                case CarState.Stopped:
                    turningPath = GetTurningPathCubic(
                        currentRoute.path[currentLeg],
                        currentRoute.path[currentLeg + 1]);
                    turningPathIndex = 0;
                    carState = CarState.Turning;
                    break;
                case CarState.Turning:
                    if (turningPathIndex == turningPath.Count)
                    {
                        carState = CarState.Cruising;
                    }
                    else
                    {
                        MoveCarThroughTurn();
                    }
                    break;
                case CarState.Cruising:
                    transform.position += transform.forward * Time.deltaTime * speed;
                    break;
                default:
                    break;
            }

            destinationLabel.transform.position =
                Camera.main.WorldToScreenPoint(transform.position) + new Vector3(LABEL_OFFSET_X, LABEL_OFFSET_Y);

            if (ArrivedAtDestination())
            {
                SetNewDestination();
            }
        }
    }

    private void MoveCarThroughTurn2()
    {
        Vector3 targetDir = turningPath[turningPathIndex] - transform.position;
        transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, targetDir, 1, 1));
        transform.position = Vector3.MoveTowards(transform.position, turningPath[turningPathIndex], Time.deltaTime * 0.1f);
        if (transform.position == turningPath[turningPathIndex])
        {
            turningPathIndex++;
        }
    }

    private void MoveCarThroughTurn()
    {
        Vector3 targetDir = turningPath[turningPathIndex] - transform.position;
        transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, targetDir, 1, 1));
        transform.position = Vector3.MoveTowards(transform.position, turningPath[turningPathIndex], Time.deltaTime * 0.1f);
        if (transform.position == turningPath[turningPathIndex])
        {
            turningPathIndex++;
        }
    }

    private float GetBrakingStrength()
    {
        if (brakingAcceleration != 0)
        {
            return brakingAcceleration;
        }

        float distanceFromDestination = Vector3.Distance(transform.position, destination.location)
            - CAR_STOP_DISTANCE;
        Debug.Log("Distance to stop in: " + distanceFromDestination);
        float newBrakingAcceleration = (speed * speed) / (2 * distanceFromDestination);
        brakingAcceleration = newBrakingAcceleration;
        Debug.Log("Braking acceleration: " + brakingAcceleration);
        return newBrakingAcceleration;
    }

    private bool ArrivedAtDestination()
    {
        // TODO: Come up with a less hacky / more reliable way to do this
        if (Vector3.Distance(transform.position, destination.location) <= speed * Time.deltaTime * 4)
        {
            return true;
        }

        return false;
    }

    // Given the current position and rotation of a car and the node that it should turn towards,
    // calculate a list of locations for it to move through to turn towards that destination.
    private List<Vector3> GetTurningPath(Vector3 currentPosition, Node intersection, Node destination)
    {
        List<Vector3> turningPath = new List<Vector3>();

        Debug.Log("Finding turn from intersection at " + intersection.location + " to node at " + destination.location);

        Vector3 fromIntersectionToDestination = destination.location - intersection.location;
        Debug.Log("fromIntersectionToDestination = " + fromIntersectionToDestination);

        Vector2 fromIntToDest2D = new Vector2(destination.location.x, destination.location.z)
            - new Vector2(intersection.location.x, intersection.location.z);
        Debug.Log("fromIntToDest2D = " + fromIntToDest2D);
        Vector2 offsetFromCenterOfDestinationRoad = -Vector2.Perpendicular(fromIntToDest2D).normalized * CAR_OFFSET_FROM_CENTER_OF_ROAD;
        Debug.Log("perpindicular = " + offsetFromCenterOfDestinationRoad);
        Vector3 offsetFromCenterOfDestinationRoad3D = new Vector3(offsetFromCenterOfDestinationRoad.x, 0, offsetFromCenterOfDestinationRoad.y);

        Vector2 currentHeading2D = new Vector2(transform.forward.x, transform.forward.z);
        Vector2 currentOffsetFromRoadCenter = -Vector2.Perpendicular(currentHeading2D).normalized * CAR_OFFSET_FROM_CENTER_OF_ROAD;

        Vector3 currentOffsetFromRoadCenter3D = new Vector3(currentOffsetFromRoadCenter.x, 0, currentOffsetFromRoadCenter.y);
        Vector3 turnCenter = intersection.location + offsetFromCenterOfDestinationRoad3D + currentOffsetFromRoadCenter3D;

        Vector3 turnEnd = intersection.location + new Vector3(offsetFromCenterOfDestinationRoad.x, 0, offsetFromCenterOfDestinationRoad.y);
        turnEnd += fromIntersectionToDestination.normalized * CAR_STOP_DISTANCE;

        Debug.Log("turnEnd = " + turnEnd);


        int NUM_POINTS = 20;
        Vector3[] points = new Vector3[NUM_POINTS];
        Vector3 lastPoint = currentPosition;
        for (int i = 1; i < NUM_POINTS + 1; i++)
        {
            float t = (float)i / (float)NUM_POINTS;
            Vector3 newPoint = CalcualteQuadraticBezierPoint(t, transform.position, turnCenter, turnEnd);
            turningPath.Add(newPoint);
            Debug.DrawLine(lastPoint + Vector3.up * 0.2f, newPoint + Vector3.up * 0.2f, Color.white, 10f);
            lastPoint = newPoint;
        }
        Debug.DrawLine(lastPoint + Vector3.up * 0.2f, turnEnd + Vector3.up * 0.2f, Color.white, 10f);


        return turningPath;
    }

    //                                        start       startDir    end         endDir
    Vector3 CalcuateCubicBezierPoint(float t, Vector3 start, Vector3 startDir, Vector3 end, Vector3 endDir)
    {
        float u = 1 - t;
        float uu = u * u;
        float uuu = uu * u;

        float tt = t * t;
        float ttt = tt * t;

        return uuu * start + 3 * uu * t * startDir + 3 * u * tt * endDir + ttt * end;
    }

    // Get the end-point and the "handle" (direction) for the end of a cubic bezier
    void GetEndPositionAndDirectionForTurn(Node intersection, Node destination, out Vector3 position, out Vector3 direction)
    {
        Vector3 fromIntersectionToDestination = destination.location - intersection.location;
        Vector2 fromIntToDest2D = new Vector2(destination.location.x, destination.location.z)
            - new Vector2(intersection.location.x, intersection.location.z);
        Vector2 offsetFromCenterOfDestinationRoad =
            -Vector2.Perpendicular(fromIntToDest2D).normalized * CAR_OFFSET_FROM_CENTER_OF_ROAD;
        Vector3 offsetFromCenterOfDestinationRoad3D =
            new Vector3(offsetFromCenterOfDestinationRoad.x, 0, offsetFromCenterOfDestinationRoad.y);

        position = intersection.location
            + new Vector3(offsetFromCenterOfDestinationRoad.x, 0, offsetFromCenterOfDestinationRoad.y)
            + fromIntersectionToDestination.normalized * CAR_STOP_DISTANCE;

        direction = position - fromIntersectionToDestination.normalized * 0.15f;
    }

    List<Vector3> GetTurningPathCubic(Node intersection, Node destination)
    {
        List<Vector3> path = new List<Vector3>();

        Vector3 start = transform.position;
        Vector3 startDir = start + transform.forward * 0.15f;
        Vector3 end;
        Vector3 endDir;

        GetEndPositionAndDirectionForTurn(intersection, destination, out end, out endDir);

        int NUM_POINTS = 20;
        Vector3 lastPoint = start;
        Vector3 upOffset = new Vector3(0, 0.2f, 0);
        for (int i = 1; i < NUM_POINTS + 1; i++)
        {
            float t = (float)i / (float)NUM_POINTS;
            Vector3 newPoint = CalcuateCubicBezierPoint(t, start, startDir, end, endDir);
            Debug.DrawLine(lastPoint + upOffset, newPoint + upOffset, Color.white, 10);
            lastPoint = newPoint;
        }
        Debug.DrawLine(lastPoint + upOffset, end + upOffset, Color.white, 10);

        return path;
    }



    Vector3 CalcualteQuadraticBezierPoint(float t, Vector3 start, Vector3 corner, Vector3 end)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        Vector3 p = uu * start;
        p += 2 * u * t * corner;
        p += tt * end;
        return p;
    }

    private void SetNewDestination()
    {
        // If we're at the end of our route, find a random route to another
        // destination
        if (destination == currentRoute.path[currentRoute.path.Count - 1])
        {
            Node newDestination = nodeController.GetOriginDestinationOtherThan(destination);
            List<Route> newRoutes = nodeController.GetRoutes(destination, newDestination);
            int indexOfRouteToUse = Random.Range(0, newRoutes.Count);
            SetRoute(newRoutes[indexOfRouteToUse]);
        }

        // Otherwise, move to the next leg
        else
        {
            origin = destination;
            currentLeg++;
            destination = currentRoute.path[currentLeg];
        }

        MoveToOriginAndPointAtDestination();
    }
}