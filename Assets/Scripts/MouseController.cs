using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Tells us where on the grid we've clicked
public class MouseController : MonoBehaviour {

    
    public GameObject roadPrefab;

    

    private GridLineCreator grid;
    private NodeController nodeController;
    private Camera mainCamera;


    private Vector3 lastGridPosition;


	void Start () {
        mainCamera = Camera.main;
        nodeController = FindObjectOfType<NodeController>();
        grid = FindObjectOfType<GridLineCreator>();
    }
	
	void Update () {

        // Shoot a ray from the camera to the mouse so we can see what it hits
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);



        RaycastHit hitInfo;

        // The only thing with a collider is the ground, so we don't need
        // to test what we're hitting.
        if (Physics.Raycast(ray, out hitInfo))
        {
            Vector3 gridSnapPosition = getClosestGridpoint(hitInfo.point);

            // Move the cursor to the new node we're pointing at
            if (gridSnapPosition != lastGridPosition)
            {
                nodeController.HoveringOnNewGridPoint(gridSnapPosition);
                lastGridPosition = gridSnapPosition;
            }

            // Left mouse button went down this frame
            if (Input.GetMouseButtonDown(0))
            {
                nodeController.LeftMousePressAtGridPoint(gridSnapPosition);
            }

            // Left mouse button released this frame
            if (Input.GetMouseButtonUp(0))
            {
                nodeController.LeftMouseReleasedAtGridPoint(gridSnapPosition);
            }

            // Right click 
            else if (Input.GetMouseButtonDown(1))
            {
                nodeController.RightClickAtGridPoint(gridSnapPosition);

            }
        }
	}

    private Vector3 getClosestGridpoint(Vector3 position)
    {
        float distanceFromLeftEdgeOfGround = position.x - grid.GetLeftEdgeX();
        float gridUnitsFromLeftEdgeOfGround = distanceFromLeftEdgeOfGround / grid.GetGridSizeX();
        float closestGridX = grid.GetLeftEdgeX() +
            Mathf.Round(gridUnitsFromLeftEdgeOfGround) * grid.GetGridSizeX();

        float distanceFromTopEdgeOfGround = grid.GetTopEdgeZ() - position.z; 
        float gridUnitsFromTopEdgeOfGround = distanceFromTopEdgeOfGround / grid.GetGridSizeZ();
        float closestGridZ = grid.GetTopEdgeZ() -
            Mathf.Round(gridUnitsFromTopEdgeOfGround) * grid.GetGridSizeZ();

        return new Vector3(closestGridX, 0, closestGridZ);
    }
}
