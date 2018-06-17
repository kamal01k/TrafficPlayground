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

    private static float LABEL_OFFSET_X = 80;
    private static float LABEL_OFFSET_Y = 25;

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
        MoveToOriginAndPointAtDestination();
        StartDriving();
    }

    private void MoveToOriginAndPointAtDestination()
    {
        transform.position = origin.location;
        Quaternion rotation = Quaternion.identity;
        rotation.SetLookRotation(destination.location - origin.location);
        transform.rotation = rotation;
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
            transform.position += transform.forward * Time.deltaTime * speed;
            destinationLabel.transform.position =
                Camera.main.WorldToScreenPoint(transform.position) + new Vector3(LABEL_OFFSET_X, LABEL_OFFSET_Y);

            if (ArrivedAtDestination())
            {
                SetNewDestination();
            }
        }
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