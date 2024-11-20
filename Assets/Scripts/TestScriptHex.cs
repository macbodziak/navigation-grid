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
    [SerializeField] Vector2Int teleportGoal;
    [SerializeField] Vector2Int startArea;
    [SerializeField] int budget;
    [SerializeField] Actor actor_1;
    [SerializeField] Actor actor_2;
    System.Diagnostics.Stopwatch stopwatch;
    float time_start;
    float time_finish;
    PathRequest pathQuery;
    WalkableAreaRequest areaRequest;
    Path path;
    WalkableArea area;

    private void Start()
    {
        stopwatch = new();
        stopwatch.Start();

        actor_2.NodeExitedEvent += OnActorExitedNode;
        actor_2.NodeEnteredEvent += OnActorEnteredNode;
        actor_1.NodeExitedEvent += OnActorExitedNode;
        actor_1.NodeEnteredEvent += OnActorEnteredNode;
        // grid.ScanForActors(100f);


        // actorList = grid.AdjacentActors(13);
        // Debug.Log("ACTORS   ---  ");
        // if (actorList != null)
        // {

        //     foreach (var act in actorList)
        //     {
        //         Debug.Log("ACTOR  ---  " + act.gameObject.name);
        //     }
        // }
        // grid.InstallActor(actor_1, 0);
        // grid.InstallActor(actor_2, 1, 3);






        // HexGrid hexGrid = grid as HexGrid;
        // if (hexGrid != null)
        // {
        //     path = Pathfinder.FindPath(hexGrid, start.x, start.y, goal.x, goal.y, excludeGoal);
        // }
        // SquareGrid squareGrid = grid as SquareGrid;
        // if (squareGrid != null)
        // {
        //     path = Pathfinder.FindPath(squareGrid, start.x, start.y, goal.x, goal.y, excludeGoal);
        // }
        // ShowDebugPath(path);
        // actor_1.MovementFinishedEvent += OnMovementFinished;
        // actor_1.MoveAlongPath(path);





        // mc.FaceTowards(hexGrid.NodeWorldPositionAt(1, 1));
        grid.SetMovementCostModifierAt(11, 10, 2);
        grid.SetMovementCostModifierAt(12, 10, 2);
        grid.SetMovementCostModifierAt(11, 9, 2);
        grid.SetMovementCostModifierAt(12, 9, 2f);
        // grid.SetMovementCostModifierAt(19, 11, 2f);
    }


    void Update()
    {
        MousePositionToGripPositionDebug();

        if (Input.GetKeyDown(KeyCode.A))
        {
            stopwatch.Reset();
            stopwatch.Start();
            time_start = Time.realtimeSinceStartup;

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
            Debug.Log("path.cost = " + path.cost);
            Pathfinder.DebugDrawPath(path, Color.red, 3f);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            time_start = Time.realtimeSinceStartup;
            stopwatch.Reset();
            stopwatch.Start();
            HexGrid hexGrid = grid as HexGrid;
            if (hexGrid != null)
            {
                pathQuery = Pathfinder.SchedulePath(hexGrid, new Vector2Int(start.x, start.y), new Vector2Int(goal.x, goal.y), excludeGoal);
            }
            SquareGrid squareGrid = grid as SquareGrid;
            if (squareGrid != null)
            {
                pathQuery = Pathfinder.SchedulePath(squareGrid, new Vector2Int(start.x, start.y), new Vector2Int(goal.x, goal.y), excludeGoal);
            }
        }


        if (pathQuery != null)
        {
            if (pathQuery.IsComplete)
            {
                path = pathQuery.Complete();
                pathQuery = null;
                stopwatch.Stop();
                time_finish = Time.realtimeSinceStartup;
                System.TimeSpan ts = stopwatch.Elapsed;
                Debug.Log($"path finding took {ts.TotalMilliseconds} ms");
                Debug.Log($"path finding took <color=orange>{time_finish - time_start} s </color>");
                Debug.Log("path.cost = " + path.cost);
                Pathfinder.DebugDrawPath(path, new Color(1f, 0.2f, 0.2f), 3f);
                pathQuery = null;
            }
            else
            {
                Debug.Log("...");
            }
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
            Pathfinder.DebugDrawArea(grid, area, Color.yellow, 2.5f);
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            time_start = Time.realtimeSinceStartup;
            stopwatch.Reset();
            stopwatch.Start();
            HexGrid hexGrid = grid as HexGrid;
            if (hexGrid != null)
            {
                areaRequest = Pathfinder.ScheduleWalkableArea(hexGrid, hexGrid.IndexAt(startArea.x, startArea.y), budget);
            }
            SquareGrid squareGrid = grid as SquareGrid;
            if (squareGrid != null)
            {
                areaRequest = Pathfinder.ScheduleWalkableArea(squareGrid, squareGrid.IndexAt(startArea.x, startArea.y), budget);
            }
        }


        if (areaRequest != null)
        {
            if (areaRequest.IsComplete)
            {
                var time_start_ = Time.realtimeSinceStartup;
                area = areaRequest.Complete();
                var time_finish_ = Time.realtimeSinceStartup;

                Debug.Log($"           ----- stage complete() <color=cyan>{(time_finish_ - time_start_) * 1000} ms </color>");

                areaRequest = null;
                stopwatch.Stop();
                time_finish = Time.realtimeSinceStartup;
                System.TimeSpan ts = stopwatch.Elapsed;
                Debug.Log($"area finding took <color=orange>{ts.TotalMilliseconds} ms </color>");
                Debug.Log($"area finding took <color=orange>{time_finish - time_start} s </color>");
                Pathfinder.DebugDrawArea(grid, area, new Color(0.5f, 0.5f, 0f), 3.75f);
            }
            else
            {
                Debug.Log("...");
            }
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

        if (Input.GetKeyDown(KeyCode.V))
        {
            actor_2.Teleport(grid.IndexAt(teleportGoal.x, teleportGoal.y));
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
                // Node node = grid.NodeAt(hit.point);

                // Debug.Log("gridPos: " + node.gridCoordinates + " , node id: " + node.id + " , walkable: " + node.walkable);
                Debug.Log(grid.NodeDebugString(grid.IndexAt(hit.point)));

                // Do something with the hit object, e.g.:
                // hit.collider.gameObject.GetComponent<Renderer>().material.color = Color.red;
            }
        }
    }

    private void OnMovementFinished(object sender, ActorFinishedMovementEventArgs args)
    {
        Actor actor = sender as Actor;
        // grid.UninstallActor(args.GoalIndex);
        Debug.Log(actor.gameObject.name + " finished movement at " + grid.GridCoordinatesAt(args.GoalIndex));
        Debug.Break();

        int nodeIndex = actor.NodeIndex;
        Vector2Int start = grid.GridCoordinatesAt(nodeIndex);
        HexGrid hexGrid = grid as HexGrid;
        if (hexGrid != null)
        {
            path = Pathfinder.FindPath(hexGrid, start.x, start.y, 0, 0, excludeGoal);
        }
        SquareGrid squareGrid = grid as SquareGrid;
        if (squareGrid != null)
        {
            path = Pathfinder.FindPath(squareGrid, start.x, start.y, 0, 0, excludeGoal);
        }

        actor.MoveAlongPath(path);

    }

    private void OnActorExitedNode(object sender, ActorExitedNodeEventArgs args)
    {
        Actor actor = sender as Actor;
        Debug.Log(actor.gameObject.name + " exited from " + grid.GridCoordinatesAt(args.FromIndex));
    }

    private void OnActorEnteredNode(object sender, ActorEnteredNodeEventArgs args)
    {
        Actor actor = sender as Actor;
        Debug.Log(actor.gameObject.name + " entered at " + grid.GridCoordinatesAt(args.ToIndex));
    }
}
