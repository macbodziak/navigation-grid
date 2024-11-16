using System.Collections;
using System.Collections.Generic;
using Navigation;
using UnityEngine;

public class TestSpline : MonoBehaviour
{
    List<Vector3> spline;
    Path path;
    [SerializeField] Vector2Int start;
    [SerializeField] Vector2Int goal;
    [SerializeField] int subdivide;

    [SerializeField] NavGrid navGrid;
    // Start is called before the first frame update
    void Start()
    {
        HexGrid hexGrid = navGrid as HexGrid;
        if (hexGrid != null)
        {
            path = Pathfinder.FindPath(hexGrid, start.x, start.y, goal.x, goal.y);
        }
        SquareGrid squareGrid = navGrid as SquareGrid;
        if (squareGrid != null)
        {
            path = Pathfinder.FindPath(squareGrid, start.x, start.y, goal.x, goal.y);
        }

    }

    void Update()
    {
        if (path != null)
        {
            spline = Utilities.SplineBuilder.MakeSpline(path.WorldPositions(), subdivide);
            DebugDrawSpline(spline);
        }
    }


    public void DebugDrawSpline(List<Vector3> spline)
    {
        Vector3 a = spline[0];

        for (int i = 1; i < spline.Count; i++)
        {
            Debug.DrawLine(spline[i - 1], spline[i], Color.cyan);
            Debug.DrawLine(spline[i], spline[i] + Vector3.up * 0.25f, Color.blue);
        }
    }

}
