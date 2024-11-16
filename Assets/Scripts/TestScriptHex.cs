using System.Collections;
using System.Collections.Generic;
using Navigation;
using UnityEngine;
using UnityEngine.Rendering;

public class TestScriptHex : MonoBehaviour
{
    [SerializeField] NavGrid grid;
    [SerializeField] Vector2Int start;
    [SerializeField] Vector2Int goal;
    [SerializeField] bool excludeGoal;
    [SerializeField] Vector2Int startArea;
    [SerializeField] int budget;
    [SerializeField] Actor actor_1;
    [SerializeField] Actor actor_2;
    System.Diagnostics.Stopwatch stopwatch;
    float time_start;
    float time_finish;
    PathRequest pathQuery;
    Path path;

    private void Start()
    {
        stopwatch = new();
        stopwatch.Start();
        grid.ScanForActors(100f);
        // grid.InstallActor(actor_1, 0);
        // grid.InstallActor(actor_2, 1, 3);
        HexGrid hexGrid = grid as HexGrid;
        if (hexGrid != null)
        {
            path = Pathfinder.FindPath(hexGrid, start.x, start.y, goal.x, goal.y, excludeGoal);
        }
        SquareGrid squareGrid = grid as SquareGrid;
        if (squareGrid != null)
        {
            path = Pathfinder.FindPath(squareGrid, start.x, start.y, goal.x, goal.y, excludeGoal);
        }
        ShowDebugPath(path);
        actor_1.MovementFinishedEvent += OnMovementFinished;
        actor_1.MoveAlongPath(path);
        // mc.FaceTowards(hexGrid.NodeWorldPositionAt(1, 1));
        // hexGrid.SetMovementModifier(9, 9, 6f);
        // hexGrid.SetMovementModifier(9, 10, 6f);
        // hexGrid.SetMovementModifier(9, 11, 6f);
        // hexGrid.SetMovementModifier(9, 8, 6f);
        // hexGrid.SetMovementModifier(2, 0, 6f);
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
            HexGrid hexGrid = grid as HexGrid;
            if (hexGrid != null)
            {
                path = Pathfinder.FindPath(hexGrid, start.x, start.y, goal.x, goal.y, excludeGoal);
            }
            SquareGrid squareGrid = grid as SquareGrid;
            if (squareGrid != null)
            {
                path = Pathfinder.FindPath(squareGrid, start.x, start.y, goal.x, goal.y, excludeGoal);
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

            WalkableArea area = null;
            time_start = Time.realtimeSinceStartup;
            stopwatch.Reset();
            stopwatch.Start();
            // for (int i = 0; i < 10; i++)

            HexGrid hexGrid = grid as HexGrid;
            if (hexGrid != null)
            {

                area = Pathfinder.FindWalkableArea(hexGrid, startArea.x, startArea.y, budget);
            }
            SquareGrid squareGrid = grid as SquareGrid;
            if (squareGrid != null)
            {
                area = Pathfinder.FindWalkableArea(squareGrid, startArea.x, startArea.y, budget);
            }


            stopwatch.Stop();
            time_finish = Time.realtimeSinceStartup;
            System.TimeSpan ts = stopwatch.Elapsed;
            Debug.Log($"area finding took {ts.TotalMilliseconds} ms");
            Debug.Log($"path finding took {time_finish - time_start} s");
            ShowDebugArea(area);
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            actor_1.Pause();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            actor_1.Continue();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            actor_1.Cancel();
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
        if (area == null)
        {
            return;
        }

        foreach (var element in area)
        {

            Debug.DrawLine(element.worldPosition, grid.NodeWorldPositionAt(element.originIndex), Color.yellow, 10.0f);
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
                Node node = grid.NodeAt(hit.point);

                Debug.Log("gridPos: " + node.gridPosition + " , node id: " + node.id + " , walkable: " + node.walkable);

                // Do something with the hit object, e.g.:
                // hit.collider.gameObject.GetComponent<Renderer>().material.color = Color.red;
            }
        }
    }

    private void OnMovementFinished(object sender, ActorFinishedMovementEventArgs args)
    {
        Actor actor = sender as Actor;
        // grid.UninstallActor(args.GoalIndex);
        Debug.Log(actor.gameObject.name + " finished movement at " + grid.GridPositionAt(args.GoalIndex));
    }
}
