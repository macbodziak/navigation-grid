using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bodziak.Spline
{
    public class SplineUtility
    {
        //Creates a Cutmull-Rom Spline, includign the start and end control oints
        public static List<Vector3> MakeSpline(List<Vector3> controlPoints, int subdivide)
        {
            if (controlPoints.Count < 3)
            {
                return null;
            }
            int count = controlPoints.Count;

            if (subdivide < 1)
            {
                subdivide = 1;
            }

            //initilize the list with the exact size needed for the spline points
            List<Vector3> spline = new List<Vector3>(subdivide * (count - 1) + 1);
            float t = 1.0f / subdivide;

            //calculate points for first control point (use it twice)
            for (int i = 0; i < subdivide; i++)
            {
                spline.Add(GetPoint(controlPoints[0], controlPoints[0], controlPoints[1], controlPoints[2], t * i));
            }

            for (int j = 1; j < count - 2; j++)
            {
                for (int i = 0; i < subdivide; i++)
                {
                    spline.Add(GetPoint(controlPoints[j - 1], controlPoints[j], controlPoints[j + 1], controlPoints[j + 2], t * i));
                }
            }

            //calculate points for last control point (use it twice)
            for (int i = 0; i < subdivide; i++)
            {
                spline.Add(GetPoint(controlPoints[count - 3], controlPoints[count - 2], controlPoints[count - 1], controlPoints[count - 1], t * i));
            }

            spline.Add(controlPoints[count - 1]);
            return spline;
        }

        static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            Vector3 returnPoint = new Vector3();

            returnPoint = 0.5f * (2.0f * p1 + (p2 - p0) * t + (2.0f * p0 - 5.0f * p1 + 4.0f * p2 - p3) * t * t + (3.0f * p1 - p0 - 3.0f * p2 + p3) * t * t * t);
            return returnPoint;
        }


        public static void DebugDrawSpline(List<Vector3> points, Color color, float duration = 1.0f)
        {
            for (int i = 0; i < points.Count - 1; i++)
            {
                Debug.DrawLine(points[i], points[i + 1], color, duration);
            }
        }
    }

}