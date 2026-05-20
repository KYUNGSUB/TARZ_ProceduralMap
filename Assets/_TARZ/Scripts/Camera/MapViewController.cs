using UnityEngine;

public class MapViewController : MonoBehaviour
{
    [Header("Cameras")]
    public Camera mainCamera;
    public Camera mapCamera;

    [Header("Map Settings")]
    public string mapRootName = "MapRoot";
    public float mapCameraHeight = 100f;
    public float mapPadding = 1.2f;

    [Header("Zoom")]
    public float zoomSpeed = 10f;
    public float minZoomSize = 10f;
    public float maxZoomSize = 200f;

    private bool isMapOpen = false;
    private float fittedSize = 50f;

    void Start()
    {
        if (mapCamera != null)
        {
            mapCamera.gameObject.SetActive(false);
            mapCamera.orthographic = true;
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMapView();
        }

        if (isMapOpen)
        {
            HandleZoom();
        }
    }

    void ToggleMapView()
    {
        isMapOpen = !isMapOpen;

        if (isMapOpen)
        {
            FitMapCameraToMap();
        }

        if (mapCamera != null)
            mapCamera.gameObject.SetActive(isMapOpen);

        if (mainCamera != null)
            mainCamera.gameObject.SetActive(!isMapOpen);

        Time.timeScale = isMapOpen ? 0f : 1f;

        Cursor.visible = isMapOpen;
        Cursor.lockState = isMapOpen
            ? CursorLockMode.None
            : CursorLockMode.Locked;
    }

    void FitMapCameraToMap()
    {
        if (mapCamera == null)
            return;

        GameObject mapRoot = GameObject.Find(mapRootName);

        if (mapRoot == null)
        {
            Debug.LogWarning($"MapRoot not found: {mapRootName}");
            return;
        }

        Renderer[] renderers = mapRoot.GetComponentsInChildren<Renderer>();

        if (renderers == null || renderers.Length == 0)
        {
            Debug.LogWarning("No renderers found in MapRoot.");
            return;
        }

        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        Vector3 center = bounds.center;

        mapCamera.transform.position = new Vector3(
            center.x,
            mapCameraHeight,
            center.z
        );

        mapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        mapCamera.orthographic = true;

        float mapWidth = bounds.size.x;
        float mapDepth = bounds.size.z;

        float aspect = (float)Screen.width / Screen.height;

        float sizeByDepth = mapDepth * 0.5f;
        float sizeByWidth = (mapWidth * 0.5f) / aspect;

        fittedSize = Mathf.Max(sizeByDepth, sizeByWidth) * mapPadding;

        fittedSize = Mathf.Clamp(fittedSize, minZoomSize, maxZoomSize);

        mapCamera.orthographicSize = fittedSize;

        Debug.Log($"Full Map Camera Fit: Center={center}, BoundsSize={bounds.size}, OrthoSize={fittedSize}");
    }

    void HandleZoom()
    {
        if (mapCamera == null)
            return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) < 0.001f)
            return;

        mapCamera.orthographicSize -= scroll * zoomSpeed;

        mapCamera.orthographicSize = Mathf.Clamp(
            mapCamera.orthographicSize,
            minZoomSize,
            maxZoomSize
        );
    }
}