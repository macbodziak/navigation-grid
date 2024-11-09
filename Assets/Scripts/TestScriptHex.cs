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
    }

    private void ShowDebugPath(Path path)
    {

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
                Vector2Int gridPos = hexGrid.WorldPositionToGridPosition(hit.point);
                Node node = hexGrid.NodeAt(hit.point);

                Debug.Log("gridPos: " + gridPos + " , node id: " + node.id + " , walkable: " + node.walkable);

                // Do something with the hit object, e.g.:
                // hit.collider.gameObject.GetComponent<Renderer>().material.color = Color.red;
            }
        }
    }
}
