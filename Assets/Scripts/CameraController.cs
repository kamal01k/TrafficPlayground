using UnityEngine;

public class CameraController : MonoBehaviour
{

    private const float CAMERA_PAN_SPEED = 0.01f;
    private const float ZOOM_MAGNITUDE_OFFSET = 10;

    void Update()
    {
        PanCamera();
        ZoomCamera();
    }

    private void PanCamera()
    {
        // The closer we're zoomed in, the slower we should pan.
        // ZOOM_MAGNITUDE_OFFSET exists so we don't go move at a glacial rate even if we're
        // quite close.
        float zoomFactor = Camera.main.transform.position.magnitude + ZOOM_MAGNITUDE_OFFSET;

        // zoomFactor is squared because the panning speed just "feels better" that way.
        float panSpeedCoefficient = CAMERA_PAN_SPEED * zoomFactor * zoomFactor;

        if (Input.GetKey(KeyCode.A))
        {
            transform.position -= Time.deltaTime * transform.right * panSpeedCoefficient;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            transform.position += Time.deltaTime * transform.right * panSpeedCoefficient;
        }
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += Time.deltaTime * transform.forward * panSpeedCoefficient;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            transform.position -= Time.deltaTime * transform.forward * panSpeedCoefficient;
        }
    }

    private void ZoomCamera()
    {
        float zoomDelta = Input.GetAxis("Mouse ScrollWheel");
        Camera.main.transform.localPosition -= Camera.main.transform.localPosition * zoomDelta;
    }
}
