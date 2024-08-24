using System.Collections;
using System.Collections.Generic;
using Navigation;
using UnityEngine;
using UnityEngine.Rendering;

public class TestScript : MonoBehaviour
{
    [SerializeField] NavGrid navGrid;
    [SerializeField] Vector2Int start;
    [SerializeField] Vector2Int goal;

    [SerializeField] Vector2Int startArea;
    [SerializeField] int budget;
    System.Diagnostics.Stopwatch stopwatch;
    float time_start;
    float time_finish;
    PathQuery pathQuery;
    Path path;

    private void Start()
    {
        stopwatch = new();
        stopwatch.Start();


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
        Debug.Log("deltaTime " + Time.deltaTime);

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
            // for (int i = 0; i < 10; i++)
            {
                path = Pathfinder.FindPath(navGrid, start.x, start.y, goal.x, goal.y);
            }
            stopwatch.Stop();

            // Get the elapsed time as a TimeSpan value
            System.TimeSpan ts = stopwatch.Elapsed;
            Debug.Log($"path finding took {ts.TotalMilliseconds} ms");
            ShowDebugPath(path);
        }



        // if (Input.GetKeyDown(KeyCode.Space))
        // {

        //     List<WalkableAreaElement> area = new();
        //     time_start = Time.realtimeSinceStartup;
        //     stopwatch.Reset();
        //     stopwatch.Start();
        //     // for (int i = 0; i < 10; i++)
        //     {
        //         area = Pathfinder.FindWalkableArea(navGrid, startArea.x, startArea.y, budget);
        //     }
        //     stopwatch.Stop();
        //     time_finish = Time.realtimeSinceStartup;
        //     System.TimeSpan ts = stopwatch.Elapsed;
        //     Debug.Log($"area finding took {ts.TotalMilliseconds} ms");
        //     Debug.Log($"path finding took {time_finish - time_start} s");
        //     ShowDebugArea(area);
        // }

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

    private void ShowDebugArea(List<WalkableAreaElement> area)
    {
        if (area == null)
        {
            return;
        }

        foreach (var element in area)
        {

            Debug.DrawLine(element.worldPosition, navGrid.GetNodeWorldPosition(element.originIndex), Color.yellow, 10.0f);
        }

    }
}
