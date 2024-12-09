
using System.Collections.Generic;
using UnityEngine;
using Navigation;


public class WalkableAreaExample : MonoBehaviour
{
    Actor selectedActor;
    bool inputBlocked = false;
    List<GameObject> areaMarkers;
    WalkableArea area;

    [SerializeField] GameObject pathFlagPrefab;
    [SerializeField] NavGrid grid;
    [SerializeField] int budget = 50;


    public void Start()
    {
        areaMarkers = new();
        Debug.Log("<color=#ffa08b>IMPORTANT </color>: see <color=#8bc5ff>HowToRunExample.txt</color> guide on how to set up layers correctly to make this example work!");
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
            ClearAreaPreview();
            selectedActor = null;
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
                    selectedActor = clickedActor;
                    area = Pathfinder.FindWalkableArea(grid, selectedActor.NodeIndex, budget);
                    ClearAreaPreview();
                    PreviewArea(area);
                    return;
                }

                if (selectedActor == null)
                {
                    return;
                }

                int clickedIndex;
                clickedIndex = grid.IndexAt(hit.point);

                if (area.ContainsNode(clickedIndex))
                {
                    inputBlocked = true;
                    ClearAreaPreview();
                    Path path = area.GetPathFromNodeIndex(clickedIndex);
                    Pathfinder.DebugDrawPath(path, Color.red, 5f);
                    selectedActor.MovementFinishedEvent += OnActorFinishedMovement;
                    selectedActor.MoveAlongPath(path);
                }
            }
        }
    }


    private void PreviewArea(WalkableArea _area)
    {
        if (_area == null)
        {
            return;
        }
        areaMarkers = new List<GameObject>(_area.Count());

        foreach (var el in _area.GetWalkableAreaElements())
        {
            GameObject flag = Instantiate(pathFlagPrefab, el.worldPosition, Quaternion.identity);
            areaMarkers.Add(flag);
        }
    }

    private void ClearAreaPreview()
    {
        foreach (var el in areaMarkers)
        {
            Destroy(el.gameObject);
        }

        areaMarkers.Clear();
    }

    private void OnActorFinishedMovement(ActorFinishedMovementEventArgs e)
    {
        Debug.Log(e.Actor.gameObject.name + " has finished movement at " + grid.GridCoordinatesAt(e.GoalIndex));
        inputBlocked = false;
        area = Pathfinder.FindWalkableArea(grid, selectedActor.NodeIndex, budget);
        PreviewArea(area);
        selectedActor.MovementFinishedEvent -= OnActorFinishedMovement;
    }

}
