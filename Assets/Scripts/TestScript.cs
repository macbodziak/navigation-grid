using System.Collections;
using System.Collections.Generic;
using Navigation;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    [SerializeField] NavGrid navGrid;
    [SerializeField] Vector2Int start;
    [SerializeField] Vector2Int goal;



    private void Start()
    {
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        stopwatch.Start();

        Path path = Pathfinder.FindPath(navGrid, start.x, start.y, goal.x, goal.y);
        stopwatch.Stop();

        // Get the elapsed time as a TimeSpan value
        System.TimeSpan ts = stopwatch.Elapsed;
        Debug.Log($"path finding took {ts.TotalMilliseconds} ms");
        ShowDebugPath(path);

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
}
