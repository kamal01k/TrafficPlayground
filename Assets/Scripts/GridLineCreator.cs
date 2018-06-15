using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridLineCreator : MonoBehaviour {

    // How many grid lines (edges of ground not included) do we want?
    public int GridCount;
    public GameObject ground;
    public GameObject gridLinePrefab;


    private float LeftEdgeX;
    private float TopEdgeZ;
    private float GridSizeX;
    private float GridSizeZ;

    public float GetLeftEdgeX()
    {
        return LeftEdgeX;
    }

    public float GetTopEdgeZ()
    {
        return TopEdgeZ;
    }

    public float GetGridSizeX()
    {
        return GridSizeX;
    }

    public float GetGridSizeZ()
    {
        return GridSizeZ;
    }

	// Use this for initialization
	void Start () {

        // Find out how big the ground is.
        Mesh mesh = ground.gameObject.GetComponent<MeshFilter>().mesh;
        Bounds bounds = mesh.bounds;
        float groundHeight = ground.transform.localScale.z * bounds.size.z;
        float groundWidth = ground.transform.localScale.x * bounds.size.x;

        // Setup values that the mouse controller will use
        LeftEdgeX = ground.transform.position.x - groundWidth / 2.0f;
        TopEdgeZ = ground.transform.position.z + groundHeight / 2.0f;

        //
        // Set up grid lines
        //
        float totalGridLines = 2 + GridCount;

        // Because we have gridlines on each edge, if we have
        // n gridlines, we have n-1 sections dividing them.
        float deltaX = groundWidth / (totalGridLines - 1);
        GridSizeX = deltaX;
        float deltaY = groundHeight / (totalGridLines - 1);
        GridSizeZ = deltaY;

        float groundEdgeX = ground.transform.position.x - (groundWidth / 2);
        float groundEdgeY = ground.transform.position.y - (groundHeight / 2);

        // Draw veritcal grid lines
        for (int i = 0; i < totalGridLines; i++)
        {
            float xPosition = groundEdgeX + deltaX * i;
            Vector3 position = new Vector3(xPosition, 0, 0);
            Instantiate(gridLinePrefab, position, Quaternion.identity, gameObject.transform);
        }

        // Draw horizontal grid lines
        for (int i = 0; i < totalGridLines; i++)
        {
            float yPosition = groundEdgeY + deltaY * i;
            Vector3 position = new Vector3(0, 0, yPosition);
            Quaternion quaternion = Quaternion.Euler(0, 90, 0);
            Instantiate(gridLinePrefab, position, quaternion, gameObject.transform);
        }
    }
}
