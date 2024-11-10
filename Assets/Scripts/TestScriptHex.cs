using System.Collections;
using System.Collections.Generic;
using Navigation;
using UnityEngine;
using UnityEngine.Rendering;

public class TestScriptHex : MonoBehaviour
{
    [SerializeField] HexGrid hexGrid;
    [SerializeField] Vector2Int start;
    [SerializeField] Vector2Int goal;

    [SerializeField] Vector2Int startArea;
    [SerializeField] int budget;
    System.Diagnostics.Stopwatch stopwatch;
    float time_start;
    float time_finish;
    PathRequest pathQuery;
    Path path;

    private void Start()
    {
        stopwatch = new();
        stopwatch.Start();


    }


    void Update()
    {
        MousePositionToGripPositionDebug();

        if (Input.GetKeyDown(KeyCode.A))
        {
            stopwatch.Reset();
            stopwatch.Start();
            time_start = Time.realtimeSinceStartup;
            // for (int i = 0; i < 10; i++)
            {
                path = Pathfinder.FindPath(hexGrid, start.x, start.y, goal.x, goal.y);
            }
            stopwatch.Stop();
            time_finish = Time.realtimeSinceStartup;

            // Get the elapsed time as a TimeSpan value
            System.TimeSpan ts = stopwatch.Elapsed;
            Debug.Log($"path finding took {ts.TotalMilliseconds} ms");
            Debug.Log($"path finding took <color=orange>{time_finish - time_start} s </color>");
            ShowDebugPath(path);
        }
    }

    private void ShowDebugPath(Path path)
    {
        if (path == null)
        {
            return;
        }

        Debug.Log("path.cost = " + path.cost);

        for (int i = 1; i < path.Count; i++)
        {
            Debug.DrawLine(path.elements[i - 1].worldPosition, path.elements[i].worldPosition, Color.red, 15.0f);
        }
    }

    private void ShowDebugArea(WalkableArea area)
    {
    }


    private void MousePositionToGripPositionDebug()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Create a RaycastHit object to store information about what we hit
            RaycastHit hit;

            // Perform the raycast
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Grid")))
            // if (Physics.Raycast(ray, out hit))
            {
                // Log the name of the object that was hit
                Debug.Log("Hit object: " + hit.collider.gameObject.name + ", point: " + hit.point);
                Node node = hexGrid.NodeAt(hit.point);

                Debug.Log("gridPos: " + node.gridPosition + " , node id: " + node.id + " , walkable: " + node.walkable);

                // Do something with the hit object, e.g.:
                // hit.collider.gameObject.GetComponent<Renderer>().material.color = Color.red;
            }
        }
    }
}
