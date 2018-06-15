using UnityEngine;

public class CarController : MonoBehaviour
{

    public float speed;

    private NodeController nodeController;
    private Node origin;
    private Node destination;
    private GameObject car;
    private bool driving;

    public void SetOrigin(Node origin)
    {
        this.origin = origin;
    }

    public void SetDestination(Node destination)
    {
        this.destination = destination;
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
        origin = destination;
        destination = nodeController.GetRandomNeighbor(destination);
        MoveToOriginAndPointAtDestination();
    }
}