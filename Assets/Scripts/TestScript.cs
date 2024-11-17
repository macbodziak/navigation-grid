using System;
using System.Collections;
using System.Collections.Generic;
using Navigation;
using UnityEngine;
using UnityEngine.Rendering;

public class TestScript : MonoBehaviour
{
    [SerializeField] SquareGrid navGrid;
    [SerializeField] Vector2Int start;
    [SerializeField] Vector2Int goal;
    [SerializeField] bool excludeGoal;
    [SerializeField] Actor actor_1;
    [SerializeField] Actor actor_2;
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


        path = Pathfinder.FindPath(navGrid, start.x, start.y, goal.x, goal.y, excludeGoal);
        actor_1.MoveAlongPath(path);

        navGrid.SetMovementCostModifierAt(9, 12, 4f);
        navGrid.SetMovementCostModifierAt(10, 12, 4f);
        navGrid.SetMovementCostModifierAt(11, 12, 4f);
        // navGrid.SetMovementModifier(0, 4, 3f);
        navGrid.SetMovementCostModifierAt(9, 10, 4f);
        // navGrid.SetMovementModifier(1, 5, 3f);
        // navGrid.SetMovementModifier(2, 5, 3f);

        // // for (int i = 0; i < 10; i++)
        // {
        //     path = Pathfinder.FindPath(navGrid, start.x, start.y, goal.x, goal.y);
        // }
        // stopwatch.Stop();

        // // Get the elapsed time as a TimeSpan value
        // System.TimeSpan ts = stopwatch.Elapsed;
        // Debug.Log($"path finding took {ts.TotalMilliseconds} ms");
        // ShowDebugPath(path);


        // //-------------------
        // List<WalkableAreaElement> area = new();
        // stopwatch.Reset();
        // stopwatch.Start();
        // // for (int i = 0; i < 10; i++)
        // {
        //     area = Pathfinder.FindWalkableArea(navGrid, startArea.x, startArea.y, budget);
        // }
        // stopwatch.Stop();
        // ts = stopwatch.Elapsed;
        // Debug.Log($"area finding took {ts.TotalMilliseconds} ms");
        // ShowDebugArea(area);


        //--------------------
        // stopwatch.Reset();
        // time_start = Time.realtimeSinceStartup;
        // stopwatch.Start();
        // pathQuery = Pathfinder.SchedulePath(navGrid, new Vector2Int(start.x, start.y), new Vector2Int(goal.x, goal.y));
    }


    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            time_start = Time.realtimeSinceStartup;
            stopwatch.Reset();
            stopwatch.Start();
            pathQuery = Pathfinder.SchedulePath(navGrid, new Vector2Int(start.x, start.y), new Vector2Int(goal.x, goal.y));
        }
        if (pathQuery != null)
        {
            if (pathQuery.IsComplete)
            {
                Path path = pathQuery.Complete();
                pathQuery = null;
                stopwatch.Stop();
                time_finish = Time.realtimeSinceStartup;
                System.TimeSpan ts = stopwatch.Elapsed;
                Debug.Log($"path finding took {ts.TotalMilliseconds} ms");
                Debug.Log($"path finding took <color=orange>{time_finish - time_start} s </color>");
                ShowDebugPath(path);
            }
            else
            {
                Debug.Log("...");
            }
        }



        if (Input.GetKeyDown(KeyCode.A))
        {
            stopwatch.Reset();
            stopwatch.Start();
            time_start = Time.realtimeSinceStartup;
            // for (int i = 0; i < 10; i++)
            {
                path = Pathfinder.FindPath(navGrid, start.x, start.y, goal.x, goal.y, excludeGoal);
            }
            stopwatch.Stop();
            time_finish = Time.realtimeSinceStartup;

            // Get the elapsed time as a TimeSpan value
            System.TimeSpan ts = stopwatch.Elapsed;
            Debug.Log($"path finding took {ts.TotalMilliseconds} ms");
            Debug.Log($"path finding took <color=orange>{time_finish - time_start} s </color>");
            ShowDebugPath(path);
        }



        if (Input.GetKeyDown(KeyCode.Q))
        {

            WalkableArea area;
            time_start = Time.realtimeSinceStartup;
            stopwatch.Reset();
            stopwatch.Start();
            // for (int i = 0; i < 10; i++)
            {
                area = Pathfinder.FindWalkableArea(navGrid, startArea.x, startArea.y, budget);
            }
            stopwatch.Stop();
            time_finish = Time.realtimeSinceStartup;
            System.TimeSpan ts = stopwatch.Elapsed;
            Debug.Log($"area finding took {ts.TotalMilliseconds} ms");
            Debug.Log($"path finding took {time_finish - time_start} s");
            ShowDebugArea(area);
        }

        MousePositionToGripPositionDebug();

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
            Debug.DrawLine(path.elements[i - 1].worldPosition, path.elements[i].worldPosition, Color.red, 5.0f);
        }
    }

    private void ShowDebugArea(WalkableArea area)
    {
        if (area == null)
        {
            return;
        }

        foreach (var element in area)
        {

            Debug.DrawLine(element.worldPosition, navGrid.WorldPositionAt(element.originIndex), Color.yellow, 10.0f);
        }

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
                Vector2Int gridPos = navGrid.GridCoordinatesAt(hit.point);
                Node node = navGrid.NodeAt(hit.point);

                Debug.Log("gridPos: " + gridPos + " , node id: " + node.id + " , walkable: " + node.walkable);

                // Do something with the hit object, e.g.:
                // hit.collider.gameObject.GetComponent<Renderer>().material.color = Color.red;
            }
        }
    }
}
