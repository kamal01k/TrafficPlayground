using System.Collections.Generic;

public class Route
{
    public List<Node> path;
    public float length;

    public Route()
    {
        path = new List<Node>();
    }

    public void Add(Node node)
    {
        path.Add(node);
    }

    public void AddToFrontOfList(Node node)
    {
        path.Insert(0, node);
    }
}
