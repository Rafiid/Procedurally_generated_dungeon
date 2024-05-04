using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class Spot
{
    //location in the world
    public float x;
    public float y;

    //location in the map
    public int i;
    public int j;

    public float h = 0;
    public int g = 0;
    public float f = 0;
    public List<Spot> neighbors = new();
    public Spot previous = null;
    public bool wall = false;

    public Spot(int x, int y, int i, int j)
    {
        //weried numbers to make cube be in the middle of  grid
        this.x = x - 1;
        this.y = y - 1;

        this.i = i;
        this.j = j;
    }
  
}


public class AStarPathfinding
{

    //coords of map corners
    public int smallX;
    public int bigX;
    public int smallY;
    public int bigY;

    public int columns;
    public int rows;
    public Spot[,] map;
    public List<Spot> closeSet = new();
    public List<Spot> openSet = new();
    public Spot start = null;
    public Spot end = null;
    public List<Spot> path = new();
    

    public AStarPathfinding(int smallX, int bigX, int smallY, int bigY)
    {
        this.smallX = smallX;
        this.bigX = bigX;
        this.smallY = smallY;
        this.bigY = bigY;

        //create map that will be a little bit larger than dungeon
        // /6 beacause one corridor prefab is 6x6
        columns = (bigX-smallX)/6;
        rows = (bigY-smallY)/6;
        map = new Spot[columns, rows];

        for(int i=0; i<columns; i++)
        {   
            for(int j=0; j<rows; j++)
            {
                //6*i, 6*i bacause we have to move next position of the cube by size of last one (in my case corridor is 6x6)
                map[i,j] = new Spot(smallX + 6*i, smallY + 6*j, i, j);
            }
        }


        //calculete neighbors of the cube. Check if cube is not f.e on the edge of the map (if it is then it wont have 4 neighbors)
        foreach(Spot spot in map)
        {
            if(spot.i < columns - 1)
            spot.neighbors.Add(map[spot.i + 1, spot.j]);
            if(spot.i > 0 )
            spot.neighbors.Add(map[spot.i - 1, spot.j]);
            if(spot.j < rows - 1)
            spot.neighbors.Add(map[spot.i, spot.j + 1]);
            if(spot.j > 0 )
            spot.neighbors.Add(map[spot.i, spot.j - 1]);
            
        }

        
    }

    //calculate the distance between point and the end of path
    public float Heuristic(Spot neighbor)
    {
        return MathF.Abs(neighbor.i - end.i) + MathF.Abs(neighbor.j - end.j);
    }

    public void SetStart(float startX, float startY, float endX, float endY)
    {
        //set start and end point of the path
        start = map[(int)((startX-smallX)/6) + 1, (int)((startY-smallY)/6) + 1];
        end = map[(int)((endX-smallX)/6) + 1, (int)((endY-smallY)/6) + 1];
    }


    public List<Spot> FindCorridor()
    {
        openSet.Add(start);

        //check if the start or the end is not a wall
        if(start.wall == true || end.wall == true)
        {
            return path;
        }


        while(openSet.Count > 0)
        {
            Spot current = null;
            float maxF = Int32.MaxValue;
            foreach(Spot spot in openSet)
            {
                if(spot.f < maxF)
                {
                    current = spot;
                    maxF = spot.f;
                }
            }

            if(current == end)
            {
                Spot temp = current;
                path.Add(temp);
                while(temp.previous != null)
                {
                    path.Add(temp.previous);
                    temp = temp.previous;
                }

                return path;
            }

            openSet.Remove(current);
            closeSet.Add(current);

            foreach(Spot neighbor in current.neighbors)
            {
                if(!closeSet.Contains(neighbor) && !neighbor.wall)
                {
                    int tempG = current.g + 1;
                    bool newPath = false;

                    if(openSet.Contains(neighbor))
                    {
                        if(tempG < neighbor.g)
                        {
                            neighbor.g = tempG;
                            newPath = true;
                        }
                    }
                    else
                    {
                        newPath = true;
                        neighbor.g = tempG;
                        openSet.Add(neighbor);
                    }

                    if(newPath)
                    {
                        neighbor.h = Heuristic(neighbor);
                        neighbor.f = neighbor.g + neighbor.h;
                        neighbor.previous = current;
                    }
                    
                }
            }
        }

        return path;
    }
}
