using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarController : MonoBehaviour
{

    private float speed = 10;
    private float acceleration = 2.5f;

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
    private static float CAR_OFFSET_FROM_CENTER_OF_ROAD = 0.55f;
    private static float CAR_STOP_DISTANCE = 2.0f;

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
                    // If we're stopped at a dstination, just
                    // disappear.
                    if (currentRoute.path[currentLeg].nodeState == Node.NodeState.OriginDestination)
                    {
                        Destroy(gameObject);
                        break;
                    }

                    SetupTurn();
                    carState = CarState.Turning;
                    break;
                case CarState.Turning:
                    if (pastEndOfTurn)
                    {
                        // Moving through the points of the turn won't
                        // quite get us oriented propertly, so do that
                        // here.
                        AlignCarWithRoad();
                        carState = CarState.Cruising;
                        Debug.Log("Car is done turning");
                        currentLeg++;

                    }
                    else
                    {
                        MoveCarThroughTurnUsingDistanceFunction();
                    }
                    break;
                case CarState.Cruising:
                    if (speed < 10)
                    {
                        speed += Time.deltaTime * acceleration;
                    }
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

    private void AlignCarWithRoad()
    {
        Vector3 targetDir = currentRoute.path[currentLeg + 1].location - currentRoute.path[currentLeg].location;
        transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, targetDir, 1, 1));
    }

    // We'll use these to draw a cubic bezier, with "pX" values typical
    // of what you'd see in a diagram of such
    // (https://en.wikipedia.org/wiki/B%C3%A9zier_curve#/media/File:Bezier_curve.svg)
    private Vector3 turnStartPosition; // p0
    private Vector3 turnStartHandle;   // p1
    private Vector3 turnEndHandle;     // p2
    private Vector3 turnEndPosition;   // p3
    private const float TIME_TO_COMPLETE_TURN = 1f;
    private const float BEZIER_HANDLE_SCALING = 1.5f;
    private bool pastEndOfTurn;

    private Corner corner;
    private float currentCornerDistance;

    // Get everything set so we can call the function to get the location
    // of the car at any time.  We need to get the "points" for our bezier
    // curve.
    private void SetupTurn()
    {
        pastEndOfTurn = false;
        currentCornerDistance = 0;
        speed = 0;

        turnStartPosition = transform.position;
        turnStartHandle = transform.position + (transform.forward * BEZIER_HANDLE_SCALING);
        GetEndPositionAndEndHandleForTurn(
            currentRoute.path[currentLeg],
            currentRoute.path[currentLeg + 1],
            out turnEndPosition,
            out turnEndHandle);

        corner = new global::Corner(turnStartPosition, turnStartHandle, turnEndPosition, turnEndHandle, 20);
    }

    private void MoveCarThroughTurnUsingDistanceFunction()
    {
        speed += acceleration * Time.deltaTime;
        currentCornerDistance += Time.deltaTime * speed;

        Vector3 newPosition = corner.GetPositionAtDistance(currentCornerDistance, out pastEndOfTurn);
        Debug.Log("Got turn position " + newPosition + " for distance " + currentCornerDistance);

        if (!pastEndOfTurn)
        {
            Vector3 targetDir = newPosition - transform.position;
            transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, targetDir, 1, 1));
            transform.position = newPosition;
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

    // Get the end-point and the "handle" (direction) for the end of a cubic bezier
    void GetEndPositionAndEndHandleForTurn(Node intersection, Node destination, out Vector3 position, out Vector3 direction)
    {
        Vector3 fromIntersectionToDestination = destination.location - intersection.location;
        Vector2 fromIntToDest2D = new Vector2(destination.location.x, destination.location.z)
            - new Vector2(intersection.location.x, intersection.location.z);
        Vector2 offsetFromCenterOfDestinationRoad =
            -Vector2.Perpendicular(fromIntToDest2D).normalized * CAR_OFFSET_FROM_CENTER_OF_ROAD;

        position = intersection.location
            + new Vector3(offsetFromCenterOfDestinationRoad.x, 0, offsetFromCenterOfDestinationRoad.y)
            + fromIntersectionToDestination.normalized * CAR_STOP_DISTANCE;

        direction = position - fromIntersectionToDestination.normalized * 1.5f;
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