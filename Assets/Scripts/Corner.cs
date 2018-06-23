using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Corners built from Bezier curves.
public class Corner {

    public Vector3[] points;
    private float[] distances;
    private int numPoints;
    private Vector3 start;
    private Vector3 startHandle;
    private Vector3 end;
    private Vector3 endHandle;

    public float totalDistance
    {
        get
        {
            return distances[numPoints - 1];
        }
    }

    public Corner (Vector3[] testPoints)
    {
        points = testPoints;
        numPoints = testPoints.Length;
        distances = new float[numPoints];
        distances[0] = 0;
        Vector3 lastPoint = testPoints[0];
        for (int i = 1; i < numPoints; i++)
        {
            distances[i] = distances[i - 1] + Vector3.Distance(points[i], points[i - 1]);
        }
    }

    public Corner (Vector3 start, Vector3 startHandle, Vector3 end, Vector3 endHandle, int numPoints)
    {
        this.start = start;
        this.startHandle = startHandle;
        this.end = end;
        this.endHandle = endHandle;
        this.numPoints = numPoints;

        points = new Vector3[numPoints];
        distances = new float[numPoints];

        points[0] = start;
        distances[0] = 0;

        for (int i = 1; i < numPoints; i++)
        {
            float t = (float)i / (float)numPoints;
            points[i] = CalcuateCubicBezierPoint(t, start, startHandle, end, endHandle);
            distances[i] = distances[i - 1] + Vector3.Distance(points[i], points[i - 1]);
            Debug.Log("distance " + i + ": " + distances[i]);
            Debug.DrawLine(points[i - 1] + Vector3.up, points[i] + Vector3.up, Color.white, 10f);
        }
    }

    private Vector3 CalcuateCubicBezierPoint(float t, Vector3 start, Vector3 startHandle, Vector3 end, Vector3 endHandle)
    {
        float u = 1 - t;
        float uu = u * u;
        float uuu = uu * u;

        float tt = t * t;
        float ttt = tt * t;

        return uuu * start + 3 * uu * t * startHandle + 3 * u * tt * endHandle + ttt * end;
    }

    // Get a point along the curve at distance f
    public Vector3 GetPositionAtDistance(float distance, out bool pastEnd)
    {
        if (distance > distances[numPoints - 1]) {
            pastEnd = true;
            return Vector3.negativeInfinity;
        }

        pastEnd = false;

        // Find the float index of the closest distance.
        // We can use this to lerp between the values that we have in the table.
        float index = FindInterpolatedIndexOfClosestDistance(distance, 0, numPoints - 1);
        Debug.Log("For distance " + distance + ", got index " + index);

        int integerPart = (int)index;
        float decimalPart = index - integerPart;

        // Close enough - use what we have in the table.
        if (Mathf.Approximately(decimalPart, 0))
        {
            return points[(int)index];
        }

        // Lerp to interpolate between the two closest values we have.
        return Vector3.Lerp(points[integerPart], points[integerPart + 1], decimalPart);
    }

    private float FindInterpolatedIndexOfClosestDistance(float targetDistance, int startIndex, int endIndex)
    {
        int midPoint = 0;
        int left = startIndex;
        int right = endIndex;
        float currentDistance = 0;

        while (left < right)
        {
            midPoint = (left + right) / 2;
            currentDistance = distances[midPoint];
            if (currentDistance == targetDistance)
            {
                return midPoint;
            }

            if (currentDistance < targetDistance)
            {
                left = midPoint + 1;
            }
            else
            {
                right = midPoint;
            }
        }

        float currentError = currentDistance - targetDistance;

        // If the value we found is too big, find the value below it
        // and see which is closer
        if (currentError > 0)
        {
            // Find the error from value below it.  We know that it will 
            // be too small.
            float lowerError = targetDistance - distances[midPoint - 1];
            //float partialIndexValue;

            // Figure out where to choose between these values for an
            // interpolated match.
            return (float)(midPoint) - (currentError / (lowerError + currentError));
        }

        // The value we found is too small, so find the value above it.
        if (currentError < 0)
        {
            float higherError = targetDistance - distances[midPoint + 1];
            float partialIndexValue;

            return (float)midPoint + currentError / (currentError + higherError);
        }

        // Something has gone terribly wrong...
        return -1;
    }
}
