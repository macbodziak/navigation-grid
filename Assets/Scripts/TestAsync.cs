using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Navigation;
using UnityEngine;

public class TestAsync : MonoBehaviour
{
    [SerializeField] int start_x;
    [SerializeField] int start_y;
    [SerializeField] int goal_x;
    [SerializeField] int goal_y;
    [SerializeField] NavGrid grid;
    Path path;
    float time_start;
    float time_start_async;
    float time_finish;
    float time_finish_async;
    Task<Path> pathTask;


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            time_start = Time.realtimeSinceStartup;

            path = Pathfinder.FindPath(grid, grid.IndexAt(start_x, start_y), grid.IndexAt(goal_x, goal_y), false);

            time_finish = Time.realtimeSinceStartup;

            // Get the elapsed time as a TimeSpan value
            Debug.Log($"path finding took <color=orange>{(time_finish - time_start) * 1000} ms </color>");
            Debug.Log("path.cost = " + path.cost);
            Pathfinder.DebugDrawPath(path, Color.red, 3f);
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            time_start_async = Time.realtimeSinceStartup;

            pathTask = Task.Run<Path>(() => Pathfinder.FindPath(grid, grid.IndexAt(start_x, start_y), grid.IndexAt(goal_x, goal_y), false));


        }

        if (pathTask != null && pathTask.Status == TaskStatus.RanToCompletion)
        {
            time_finish_async = Time.realtimeSinceStartup;
            path = pathTask.GetAwaiter().GetResult();
            Debug.Log($"async path finding took <color=#ffef8b>{(time_finish_async - time_start_async) * 1000} ms </color>");
            Debug.Log("path.cost = " + path.cost);
            Pathfinder.DebugDrawPath(path, Color.yellow, 3f);
            pathTask = null;
        }
    }
}
