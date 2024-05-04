using System.Collections.Generic;

public class Point
{
    public float x;
    public float y;
    public float z;

    public Point(float x, float y)
    {
        this.x = x;
        this.z = 0;
        this.y = y;
    }


    public bool IsInCircumcircleOf(Triangle triangle)
    {
        float a_x = triangle.points[0].x;
        float a_y = triangle.points[0].y;

        float b_x = triangle.points[1].x;
        float b_y = triangle.points[1].y;

        float c_x = triangle.points[2].x;
        float c_y = triangle.points[2].y;

        float d_x = x;
        float d_y = y;

        float[,] incircle = new float[3, 3] { {a_x - d_x, a_y - d_y, (a_x - d_x) * (a_x - d_x) + (a_y - d_y) * (a_y - d_y)},
                                              {b_x - d_x, b_y - d_y, (b_x - d_x) * (b_x - d_x) + (b_y - d_y) * (b_y - d_y)},
                                              {c_x - d_x, c_y - d_y, (c_x - d_x) * (c_x - d_x) + (c_y - d_y) * (c_y - d_y)} };
    
        if (Determinant(incircle) < 0)
            return true;
        else
            return false;
    }

    public float Determinant(float[,] matrix)
    {
        float det = 0;
        det += matrix[0, 0] * (matrix[1, 1] * matrix[2, 2] - matrix[1, 2] * matrix[2, 1]);
        det -= matrix[0, 1] * (matrix[1, 0] * matrix[2, 2] - matrix[1, 2] * matrix[2, 0]);
        det += matrix[0, 2] * (matrix[1, 0] * matrix[2, 1] - matrix[1, 1] * matrix[2, 0]);
        return det;
    }
}

public class Edge
{
    public Point pointA;
    public Point pointB;


    public Edge(Point A, Point B)
    {
       pointA = A;
       pointB = B;
    }
}

public class Triangle
{
    public Point[] points = new Point[3];
    public Edge[] edges = new Edge[3];
    

    public Triangle(Point A, Point B, Point C)
    {
        points[0] = A;
        points[1] = B;
        points[2] = C;

        edges[0] = new Edge(A, B);
        edges[1] = new Edge(B, C);
        edges[2] = new Edge(C, A);
    }

    public bool HasVertex(Point point)
    {
        if(points[0] == point || points[1] == point || points[2] == point)
        {
            return true;
        }

        return false;
    }
}

public class DelaunayTriangulation
{
    public List<Triangle> triangulation = new List<Triangle>();
    public Point SuperPointA;
    public Point SuperPointB;
    public Point SuperPointC;
    public Triangle superTriangle;

    public DelaunayTriangulation(int smallX, int bigX, int smallY, int bigY)
    {   
        int width = bigX - smallX;
        int height = bigY - smallY;
        SuperPointA = new Point(-2*width, -2*height);
        SuperPointB = new Point(bigX - width/2, 2*height);
        SuperPointC = new Point(2*width, -2*height);
        superTriangle = new Triangle(SuperPointA, SuperPointB, SuperPointC);
        triangulation.Add(superTriangle);

    }


    public bool SharedEdge(Edge edgeA, Edge edgeB)
    {
        if((edgeA.pointA == edgeB.pointA && edgeA.pointB == edgeB.pointB) || (edgeA.pointA == edgeB.pointB && edgeA.pointB == edgeB.pointA))
        {
            return true;
        }

        return false;
    }
    

    public void AddPoint(Point point)
    {
        List<Triangle> badTriangles = new List<Triangle>();

        foreach(Triangle triangle in triangulation)
        {
            if(point.IsInCircumcircleOf(triangle))
            {
                badTriangles.Add(triangle);
            }
        }

        List<Edge> polygon = new List<Edge>();
        
        foreach(Triangle triangle in badTriangles)
        {
            foreach(Edge edge in triangle.edges)
            {   
                bool isNeighbour = false;
                foreach(Triangle otherTriangle in badTriangles)
                {
                    if(triangle == otherTriangle)
                    {
                        continue;
                    }

                    foreach(Edge otherEdge in otherTriangle.edges)
                    {
                        if(SharedEdge(edge, otherEdge))
                        {
                            isNeighbour = true;
                        }
                        
                    }
                }

                if(!isNeighbour)
                {
                    polygon.Add(edge);
                }
                    
            }
        }

        foreach(Triangle triangle in badTriangles)
        {
            triangulation.Remove(triangle);
        }

        foreach(Edge edge in polygon)
        {
            Triangle newTriangle = new Triangle(edge.pointA, edge.pointB, point);
            triangulation.Add(newTriangle);
        }
            
    }


    public void RemoveSuperTriangle()
    {
        for(int i = 0; i < triangulation.Count; i++)
        {
            if(triangulation[i].HasVertex(SuperPointA) || triangulation[i].HasVertex(SuperPointB) || triangulation[i].HasVertex(SuperPointC))
            {
                triangulation.Remove(triangulation[i]);
                i--;
            }
        }
    }
}
