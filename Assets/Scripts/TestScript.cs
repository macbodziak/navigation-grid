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



    private void Start()
    {
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        stopwatch.Start();

        Path path = null;
        // for (int i = 0; i < 10; i++)
        {
            path = Pathfinder.FindPath(navGrid, start.x, start.y, goal.x, goal.y);
        }
        stopwatch.Stop();

        // Get the elapsed time as a TimeSpan value
        System.TimeSpan ts = stopwatch.Elapsed;
        Debug.Log($"path finding took {ts.TotalMilliseconds} ms");
        ShowDebugPath(path);


        //-------------------
        List<WalkableAreaElement> area = new();
        stopwatch.Reset();
        stopwatch.Start();
        // for (int i = 0; i < 10; i++)
        {
            area = Pathfinder.FindWalkableArea(navGrid, startArea.x, startArea.y, budget);
        }
        stopwatch.Stop();
        ts = stopwatch.Elapsed;
        Debug.Log($"area finding took {ts.TotalMilliseconds} ms");
        ShowDebugArea(area);

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
        foreach (var element in area)
        {

            Debug.DrawLine(element.worldPosition, navGrid.GetNodeWorldPosition(element.originIndex), Color.yellow, 10.0f);
        }

    }
}
