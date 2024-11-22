using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Navigation;
using System;

public class PathfindingExample : MonoBehaviour
{
    Actor selectedActor;
    int goalIndex = -1;
    bool inputBlocked = false;
    List<GameObject> pathFlags;
    Path path;

    [SerializeField] GameObject pathFlagPrefab;
    [SerializeField] HexGrid grid;


    public void Start()
    {
        pathFlags = new();
    }

    private void Update()
    {
        HandleInput();
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

                if (goalIndex == clickedIndex)
                {
                    ClearPathPreview();
                    inputBlocked = true;
                    selectedActor.MovementFinishedEvent += OnActorFinishedMovement;
                    selectedActor.MoveAlongPath(path);
                }
                else
                {
                    ClearPathPreview();
                    goalIndex = grid.IndexAt(hit.point);
                    int startIndex = selectedActor.NodeIndex;
                    path = Pathfinder.FindPath(grid, startIndex, goalIndex);
                    PreviewPath(path);
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

    private void OnActorFinishedMovement(object sender, ActorFinishedMovementEventArgs e)
    {
        Actor actor = sender as Actor;
        Debug.Log(actor.gameObject.name + " has finished movement at " + grid.GridCoordinatesAt(e.GoalIndex));
        inputBlocked = false;
        selectedActor.MovementFinishedEvent -= OnActorFinishedMovement;
    }

}
