using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Navigation;
using System;

public class PathfindingAsyncExample : MonoBehaviour
{
    Actor selectedActor;
    int goalIndex = -1;
    bool inputBlocked = false;
    List<GameObject> pathFlags;
    Path path;
    PathRequest pathRequest;

    [SerializeField] GameObject pathFlagPrefab;
    [SerializeField] HexGrid grid;



    public void Start()
    {
        pathFlags = new();
        Debug.Log("<color=#ffa08b>IMPORTANT </color>: see <color=#8bc5ff>HowToRunExample.txt</color> guide on how to set up layers correctly to make this example work!");
    }

    private void Update()
    {
        HandleInput();
        if (pathRequest != null && pathRequest.Valid && pathRequest.IsComplete)
        {
            path = pathRequest.Complete();
            inputBlocked = false;
            PreviewPath(path);
        }
    }

    private void HandleInput()
    {
        if (inputBlocked)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClearPathPreview();
            selectedActor = null;
            goalIndex = -1;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;

            LayerMask layerMask = LayerMask.GetMask(new string[] { "Actor", "Grid" });
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {

                Actor clickedActor = hit.collider.gameObject.GetComponent<Actor>();
                if (clickedActor != null)
                {
                    path = null;
                    ClearPathPreview();
                    selectedActor = clickedActor;
                    return;
                }

                if (selectedActor == null)
                {
                    return;
                }

                int clickedIndex;
                clickedIndex = grid.IndexAt(hit.point);

                if (goalIndex == clickedIndex && path != null)
                {
                    ClearPathPreview();
                    inputBlocked = true;
                    selectedActor.MovementFinishedEvent += OnActorFinishedMovement;
                    selectedActor.MoveAlongPath(path);
                    path = null;
                }
                else
                {
                    ClearPathPreview();
                    goalIndex = grid.IndexAt(hit.point);
                    int startIndex = selectedActor.NodeIndex;
                    pathRequest = Pathfinder.SchedulePath(grid, startIndex, goalIndex);
                    inputBlocked = true;
                }

            }
        }
    }


    private void PreviewPath(Path _path)
    {
        if (_path == null)
        {
            return;
        }
        pathFlags = new List<GameObject>(_path.Count);

        foreach (var el in _path)
        {
            GameObject flag = Instantiate(pathFlagPrefab, el.worldPosition, Quaternion.identity);
            pathFlags.Add(flag);
        }
    }

    private void ClearPathPreview()
    {
        foreach (var el in pathFlags)
        {
            Destroy(el.gameObject);
        }

        pathFlags.Clear();
    }

    private void OnActorFinishedMovement(ActorFinishedMovementEventArgs e)
    {
        Actor actor = e.Actor;
        Debug.Log(actor.gameObject.name + " has finished movement at " + grid.GridCoordinatesAt(e.GoalIndex));
        inputBlocked = false;
        selectedActor.MovementFinishedEvent -= OnActorFinishedMovement;
    }

}
