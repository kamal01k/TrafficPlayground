using UnityEngine;

// Corners built from Bezier curves.
public class Corner
{

    private Vector3[] points;
    private float[] distances;
    private int numPoints;

    public float totalDistance
    {
        get
        {
            return distances[numPoints - 1];
        }
    }

    // To come up with a corner, we need the four points that define the cubic bezier
    // and a count of how many points we should store.
    public Corner(Vector3 start, Vector3 startHandle, Vector3 end, Vector3 endHandle, int numPoints)
    {
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

    // Get a point along the curve at a given distance into the curve
    public Vector3 GetPositionAtDistance(float distance, out bool pastEnd)
    {
        if (distance > distances[numPoints - 1])
        {
            pastEnd = true;
            return Vector3.negativeInfinity;
        }

        pastEnd = false;

        // Find the float index of the closest distance.
        // We can use this to lerp between the values that we have in the table.
        float index = FindInterpolatedIndexOfClosestDistance(distance, 0, numPoints - 1);

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

        // First a regular binary search,
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

        // Then a check of where we should be assuming a linear scale
        // between the closest points we have.
        float currentError = currentDistance - targetDistance;

        // If the value we found is too big
        if (currentError > 0)
        {
            float lowerError = targetDistance - distances[midPoint - 1];

            // Figure out where to choose between these values for an
            // interpolated match.
            return (float)(midPoint) - (currentError / (lowerError + currentError));
        }

        // The value we found is too small, so find the value above it.
        if (currentError < 0)
        {
            float higherError = targetDistance - distances[midPoint + 1];

            return (float)midPoint + currentError / (currentError + higherError);
        }

        // Something has gone terribly wrong...
        return -1;
    }
}