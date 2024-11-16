using System.Collections;
using System.Collections.Generic;
using Navigation;
using UnityEngine;

public class TestScriptClickPath : MonoBehaviour
{
    Node start;
    Node goal;
    Node temp;

    [SerializeField] GameObject startPrefab;
    [SerializeField] GameObject goalPrefab;
    [SerializeField] GameObject pathPrefab;

    GameObject startFlag;
    GameObject goalFlag;
    List<GameObject> pathFlags;


    [SerializeField] SquareGrid navGrid;

    // Start is called before the first frame update
    void Start()
    {
        start = Node.NullNode();
        goal = Node.NullNode();
        pathFlags = new();
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            if (start.id == -1)
            {
                if (GetNodeAtMousePosition(out temp))
                {
                    if (temp.walkable)
                    {
                        start = temp;
                        startFlag = Instantiate<GameObject>(startPrefab, navGrid.WorldPositionAt(start.id), Quaternion.identity);
                    }
                }
            }
            else if (goal.id == -1)
            {
                if (GetNodeAtMousePosition(out temp))
                {
                    if (temp.id != start.id)
                    {
                        goal = temp;
                        goalFlag = Instantiate<GameObject>(goalPrefab, navGrid.WorldPositionAt(goal.id), Quaternion.identity);
                    }
                }
            }
            else
            {
                Path path = Pathfinder.FindPath(navGrid, start.id, goal.id);
                if (path == null)
                {
                    Debug.Log("<color=orange>No Path Found</color>");
                    return;
                }

                if (path.Count < 2)
                {
                    return;
                }


                for (int i = 1; i < path.Count - 1; i++)
                {
                    pathFlags.Add(Instantiate<GameObject>(pathPrefab, path[i].worldPosition, Quaternion.identity));
                }

            }

        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            start = Node.NullNode();
            Destroy(startFlag);
            goal = Node.NullNode();
            Destroy(goalFlag);

            foreach (var item in pathFlags)
            {
                Destroy(item);
            }
            pathFlags.Clear();
        }
    }


    private bool GetNodeAtMousePosition(out Node node)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Grid")))
        {
            Debug.Log("Hit object: " + hit.collider.gameObject.name + ", point: " + hit.point);
            Vector2Int gridPos = navGrid.GridCoordinatesAt(hit.point);
            node = navGrid.NodeAt(hit.point);
            Debug.Log("gridPos: " + gridPos + " , node id: " + node.id + " , walkable: " + node.walkable);
            if (node.id == -1)
            {
                return false;
            }
            return true;
        }
        node = Node.NullNode();
        return false;
    }
}
