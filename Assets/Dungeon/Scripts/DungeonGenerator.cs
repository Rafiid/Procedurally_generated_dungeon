using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Room
{
    public Vector2 coord;
    public Vector2 size;
    public string name;
    public GameObject physicalRoom;
    public int roomType;
    
    public Room(Vector2 coord, Vector2 size, string name, GameObject physicalRoom, int roomType)
    {
        this.coord = coord;
        this.name = name;
        this.physicalRoom = physicalRoom;
        this.size = size;
        this.roomType = roomType;
    }

}


public class DungeonGenerator : MonoBehaviour
{
    public static DungeonGenerator instance;

    [Header("Properties")]
    public int rooms;
    public int loopChance;
    public GameObject[] roomPrefabs;
    public GameObject[] pathPrefabs;
    public Vector2[] doorLocation;

    private List<Room> roomList = new List<Room>();
    private List<Point> pointList = new List<Point>{};
    private List<Edge> edgeList = new List<Edge>{};
    private List<Edge> MST = new List<Edge>();
    private List<Spot> fullPath = new List<Spot>();

    private int RandomInt(int minRange, int maxRange)
    {
        return Random.Range(minRange, maxRange);
    }

    public List<Room> getRoomList()
    {
        return roomList;
    }

    private void SpawnRooms()
    {   
        int index = 0;

        if(rooms < 3) 
        {
            rooms = 3; 
            Debug.LogWarning("number of rooms cant be less than 3");
        }

        for(int i = 0; i < rooms; i++)
        {
            //create room and set location
            int randomRoom = RandomInt(0, roomPrefabs.Length);
            GameObject newRoom = Instantiate(roomPrefabs[randomRoom], Vector3.zero, Quaternion.identity);
            newRoom.name = "Room" + index;
            newRoom.tag = "Dungeon";
            newRoom.transform.parent = GameObject.Find("Rooms").transform;
            Transform floorTransform = newRoom.transform.Find("FloorTIle");

            //all rooms are on the small area because latter they will be spread out and smaller start area will cause relatively good corridor lengths
            int Xcoord = RandomInt(0, 10);
            int Ycoord = RandomInt(0, 10);

            float RoomWidth = floorTransform.localScale.x;
            //z==y
            float RoomLength = floorTransform.localScale.z;


            //calculate coords to be multiple of 6 so it fits to the net
            Xcoord -= Xcoord%6;
            Ycoord -= Ycoord%6;
        
            newRoom.transform.position = new Vector3(Xcoord, 0, Ycoord);


            roomList.Add(new Room(new Vector2(Xcoord, Ycoord), new Vector2(RoomWidth*4, RoomLength*4), newRoom.name, newRoom, randomRoom));
            index++;
        }
        
    }



    private void MoveRoom(Room room, Vector2 distance)
    {
        int direction = RandomInt(0, 4);

        //move room in one of four directions based on random number and change room coord

        if(direction == 0)
        {
            room.physicalRoom.transform.position += new Vector3((int)distance.x , 0, (int)distance.y);
            room.coord.x += (int)distance.x;
            room.coord.y += (int)distance.y;
        }
        else if(direction == 1)
        {
            room.physicalRoom.transform.position -= new Vector3((int)distance.x, 0, (int)distance.y);
            room.coord.x -= (int)distance.x;
            room.coord.y -= (int)distance.y;
        }
        else if(direction == 2)
        {
            room.physicalRoom.transform.position += new Vector3((int)-distance.x, 0, (int)distance.y);
            room.coord.x -= (int)distance.x;
            room.coord.y += (int)distance.y;
        }
        else
        {
            room.physicalRoom.transform.position += new Vector3((int)distance.x, 0, (int)-distance.y);
            room.coord.x += (int)distance.x;
            room.coord.y += (int)-distance.y;
        }
        
        
    }

    private void SpreadRooms()
    {
        bool roomsSpread;

        do
        {
            roomsSpread = true;

            foreach(Room room in roomList)
            {
                foreach(Room otherRoom in roomList)
                {
                    if(room.physicalRoom != otherRoom.physicalRoom && IsOverlapping(room, otherRoom))
                    {
                        //if rooms are overlapping spread them out. Calculate vector of distance beetween two rooms. Check what is longer, vector, width or length
                        // (check width and length of both rooms and choose longer one)

                        Vector2 distanceVector = otherRoom.coord - room.coord;
                        roomsSpread = false; 

                        if(distanceVector.magnitude > room.size.x && distanceVector.magnitude > room.size.y)
                        {
                            MoveRoom(otherRoom, distanceVector);  
                        }
                        else if(room.size.x > room.size.y && room.size.x > otherRoom.size.x) 
                        {
                            MoveRoom(otherRoom, new Vector2(room.size.x, room.size.x)); 
                        }
                        else if(room.size.y > room.size.x && room.size.y > otherRoom.size.y) 
                        {
                            MoveRoom(otherRoom, new Vector2(room.size.y, room.size.y));
                        }
                        else if(otherRoom.size.x > otherRoom.size.y && otherRoom.size.x > room.size.x) 
                        {
                            MoveRoom(otherRoom, new Vector2(otherRoom.size.x, otherRoom.size.x)); 
                        }
                        else
                        {
                            MoveRoom(otherRoom, new Vector2(otherRoom.size.y, otherRoom.size.y));
                        }
                    }
                }
            }
        } while (!roomsSpread);
    }


    private bool IsOverlapping(Room room, Room otherRoom)
    {
        //callculate vertices of two room and then use them to check if they overlapping
        Vector2[] roomVertices = new Vector2[4];
        roomVertices[0] = new Vector2(room.coord.x - room.size.x / 2, room.coord.y - room.size.y / 2); // left down vertice 
        roomVertices[1] = new Vector2(room.coord.x + room.size.x / 2, room.coord.y - room.size.y / 2); // right down vertice
        roomVertices[2] = new Vector2(room.coord.x + room.size.x / 2, room.coord.y + room.size.y / 2); // right up vertice
        roomVertices[3] = new Vector2(room.coord.x - room.size.x / 2, room.coord.y + room.size.y / 2); // left up vertice

        Vector2[] otherRoomVertices = new Vector2[4];
        otherRoomVertices[0] = new Vector2(otherRoom.coord.x - otherRoom.size.x / 2, otherRoom.coord.y - otherRoom.size.y / 2); // left down vertice 
        otherRoomVertices[1] = new Vector2(otherRoom.coord.x + otherRoom.size.x / 2, otherRoom.coord.y - otherRoom.size.y / 2); // right down vertice
        otherRoomVertices[2] = new Vector2(otherRoom.coord.x + otherRoom.size.x / 2, otherRoom.coord.y + otherRoom.size.y / 2); // right up vertice
        otherRoomVertices[3] = new Vector2(otherRoom.coord.x - otherRoom.size.x / 2, otherRoom.coord.y + otherRoom.size.y / 2); // left up vertice

        if(roomVertices[0].x > otherRoomVertices[1].x || roomVertices[1].x < otherRoomVertices[0].x)
            return false; // No Over Lapping on x axis
        if(roomVertices[0].y > otherRoomVertices[2].y || roomVertices[2].y < otherRoomVertices[0].y)
            return false; // No Over Lapping on y axis

        return true;
    }

    
    public void CreateDelaunayTriangulation()
    {
        int smallX = 0;
        int bigX = 0;
        int smallY = 0;
        int bigY = 0;

        foreach(Room room in roomList)
        {   

            //Calculate coord for vertex of big Triangle
            if(room.coord.x > bigX) bigX = (int)room.coord.x;
            if(room.coord.x < smallX) smallX = (int)room.coord.x;
            if(room.coord.y > bigY) bigY = (int)room.coord.y;
            if(room.coord.y < smallY) smallY = (int)room.coord.y;


            pointList.Add(new Point((int)room.coord.x, (int)room.coord.y));
            
        }

        
        //change point coord from the middle of the room to the square next to doors
        foreach(Room room in roomList)
        {
            foreach(Point point in pointList)
            {
                if(room.coord.x == point.x && room.coord.y == point.y || room.coord.x == point.x && room.coord.y == point.y)
                {
                    point.x += doorLocation[room.roomType].x;
                    point.y += doorLocation[room.roomType].y;
                }
            }
            
        }
        



        //Create Triangulation
        DelaunayTriangulation delaunayTriangulation = new DelaunayTriangulation(smallX, bigX, smallY, bigY);

        foreach(Point point in pointList)
        {
            delaunayTriangulation.AddPoint(point);
        }

        delaunayTriangulation.RemoveSuperTriangle();



        //prapare edge list for graph
        foreach(Triangle triangle in delaunayTriangulation.triangulation)
        {

            // Debug.DrawLine(new Vector3(triangle.points[0].x, 0, triangle.points[0].y), new Vector3(triangle.points[1].x, 0, triangle.points[1].y), Color.red, 55);
            // Debug.DrawLine(new Vector3(triangle.points[1].x, 0, triangle.points[1].y), new Vector3(triangle.points[2].x, 0, triangle.points[2].y), Color.red, 55);
            // Debug.DrawLine(new Vector3(triangle.points[2].x, 0, triangle.points[2].y), new Vector3(triangle.points[0].x, 0, triangle.points[0].y), Color.red, 55);

            foreach(Edge edge in triangle.edges)
            {
                edgeList.Add(edge);
            }


        }

        //delete repeating edges. If there are two adge A->B and B->A delete one of them cuz its the same edge. 
        for (int i = 0; i < edgeList.Count; i++)
        {
            for (int j = edgeList.Count - 1; j > i; j--)
            {
                if (edgeList[i] != edgeList[j] &&
                    edgeList[i].pointA.x == edgeList[j].pointB.x &&
                    edgeList[i].pointA.y == edgeList[j].pointB.y &&
                    edgeList[i].pointB.x == edgeList[j].pointA.x &&
                    edgeList[i].pointB.y == edgeList[j].pointA.y)
                {
                    edgeList.RemoveAt(i);
                    i--;
                    break;
                }
            }
        }
    }


    private void FindPath()
    {
        //prapare list for visited vertex with start vertex included
        Point startVertex = pointList[0];
        List<Point> visitedVertex = new List<Point>();
        visitedVertex.Add(startVertex);
        

        //loop while MST didnt reach to all vertex yet
        while(visitedVertex.Count < pointList.Count)
        {
            Edge selectedEdge = null;

            //if edge has one point in MST and one apart then add it to mst
            //normally algorithm should check weight of edges here and choose the lowest but in my case its enough to add first one found
            foreach(Edge edge in edgeList)
            {
                if(visitedVertex.Contains(edge.pointA) && !visitedVertex.Contains(edge.pointB) || visitedVertex.Contains(edge.pointB) && !visitedVertex.Contains(edge.pointA))
                {
                    selectedEdge = edge;
                    break;
                }
            }

            //add edge to the mst. Add its points to visited vertexs
            if(selectedEdge != null)
            {
                MST.Add(selectedEdge);
                if(!visitedVertex.Contains(selectedEdge.pointA))
                {
                    visitedVertex.Add(selectedEdge.pointA);
                }
                if(!visitedVertex.Contains(selectedEdge.pointB))
                {
                    visitedVertex.Add(selectedEdge.pointB);
                }
            }
        }

        // foreach(Edge edge in MST)
        // {   
        //     Debug.DrawLine(new Vector3(edge.pointA.x, 0, edge.pointA.y), new Vector3(edge.pointB.x, 0, edge.pointB.y), Color.green, 55);
        // }

        
        //we already have mst. Now set rate and chose edges that we will additionally add to the path to make some loops in dungeon
        foreach(Edge edge in edgeList)
        {
            int chance = RandomInt(0, 100);
            if(!MST.Contains(edge) && chance < loopChance)
            {
                MST.Add(edge);
                // Debug.DrawLine(new Vector3(edge.pointA.x, 0, edge.pointA.y), new Vector3(edge.pointB.x, 0, edge.pointB.y), Color.blue, 55);
            }
        }  
    }


    private void CreateCorridors()
    {
        List<Vector4> startEnd = new List<Vector4>();
        //corners coords of the map
        int smallX = 0;
        int bigX = 0;
        int smallY = 0;
        int bigY = 0;

        foreach(Room room in roomList)
        {      
            if(room.coord.x > bigX) bigX = (int)room.coord.x;
            if(room.coord.x < smallX) smallX = (int)room.coord.x;
            if(room.coord.y > bigY) bigY = (int)room.coord.y;
            if(room.coord.y < smallY) smallY = (int)room.coord.y;
        }

        //make prepared field a little bigger
        smallX-=50;
        bigX+=50;
        smallY-=50;
        bigY+=50;

        Transform corridorsTransform = transform.Find("Corridors");

        
        foreach(Edge edge in MST)
        {
            AStarPathfinding aStarPathfinding = new AStarPathfinding(smallX, bigX, smallY, bigY);

            //add rooms as a wall to te pathfinder map
            foreach(Room room in roomList)
            {
                for(int i=0; i<room.size.x; i+=6)
                {
                    for(int j=0; j<room.size.y; j+=6)
                    {
                        foreach(Spot spot in aStarPathfinding.map)
                        {
                            if(spot.x == room.coord.x - (room.size.x/2) + 3 + i && spot.y == room.coord.y - (room.size.y/2) + 3 + j)
                            {
                                spot.wall = true;   
                            }
                        }
                    }
                }
            }

            //set start and endpoint of the path
            aStarPathfinding.SetStart(edge.pointA.x, edge.pointA.y, edge.pointB.x, edge.pointB.y);

            //add map localization and world localization
            startEnd.Add(new Vector4((int)((edge.pointA.x-smallX)/6) + 1, (int)((edge.pointA.y-smallY)/6) + 1, edge.pointA.x, edge.pointA.y));
            startEnd.Add(new Vector4((int)((edge.pointB.x-smallX)/6) + 1, (int)((edge.pointB.y-smallY)/6) + 1, edge.pointB.x, edge.pointB.y));
            List<Spot> path = aStarPathfinding.FindCorridor();


            if(path.Count > 0)
            {
                fullPath.AddRange(path);
            }
            else
            {
                Debug.Log("No solution");
            }
        }

        //remove duplicates
        fullPath = fullPath.GroupBy(spot => new { spot.x, spot.y })
                                    .Select(group => group.First())
                                    .ToList();

        startEnd = startEnd.GroupBy(v => new { v.x, v.y })
                                    .Select(group => group.First())
                                    .ToList();

        //create paths
        int floorIndex = 0 ;
        foreach(Spot spot in fullPath)
        {
            int randomPath = RandomInt(0, pathPrefabs.Length);
            GameObject newPath = Instantiate(pathPrefabs[randomPath], Vector3.zero, Quaternion.identity);
            newPath.transform.parent = corridorsTransform;
            newPath.name = "path" + floorIndex;
            floorIndex++;
            newPath.transform.position = new Vector3(spot.x, 0, spot.y);

            //remove ussles walls
            foreach(Spot otherSpot in fullPath)
            {
                if(spot.i + 1 == otherSpot.i && spot.j == otherSpot.j)
                {
                    Destroy(newPath.transform.Find("right").gameObject);

                }

                if(spot.i - 1 == otherSpot.i && spot.j == otherSpot.j)
                {
                    Destroy(newPath.transform.Find("left").gameObject);

                }
                

                if(spot.i == otherSpot.i && spot.j + 1 == otherSpot.j)
                {
                    Destroy(newPath.transform.Find("top").gameObject);

                }

                if(spot.i == otherSpot.i && spot.j - 1 == otherSpot.j)
                {
                    Destroy(newPath.transform.Find("down").gameObject);

                }
            }
            
            //remove walls in front off doors
            foreach(Vector4 doorMapLocation in startEnd)
            {
                if(spot.i == doorMapLocation.x && spot.j == doorMapLocation.y)
                {
                    foreach(Room room in roomList)
                    {
                        if(room.coord.x + doorLocation[room.roomType].x == doorMapLocation.z && room.coord.y + doorLocation[room.roomType].y == doorMapLocation.w)
                        {
                            if(doorLocation[room.roomType].x > 0 && Mathf.Abs(doorLocation[room.roomType].x) > room.size.x/2)
                            {
                                Destroy(newPath.transform.Find("left").gameObject);
 
                            }
                            else if(doorLocation[room.roomType].x < 0 && Mathf.Abs(doorLocation[room.roomType].x) > room.size.x/2)
                            {
                                Destroy(newPath.transform.Find("right").gameObject);
  
                            }
                            else if(doorLocation[room.roomType].y < 0 && Mathf.Abs(doorLocation[room.roomType].y) > room.size.y/2)
                            {
                                Destroy(newPath.transform.Find("top").gameObject);

                            }
                            else if(doorLocation[room.roomType].y > 0 && Mathf.Abs(doorLocation[room.roomType].y) > room.size.y/2)
                            {
                                Destroy(newPath.transform.Find("down").gameObject);

                            }
                        }
                    } 
                }
            }
        }
    }


    private void Start()
    {
        SpawnRooms();
        SpreadRooms();
        CreateDelaunayTriangulation();
        FindPath();
        CreateCorridors();

        instance = this;
    }
}
