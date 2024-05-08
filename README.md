The project aims to generate a procedural dungeon in two dimensions, consisting of pre-made room prefabs and corridors 
connecting these rooms. The project was developed in C# within the Unity environment. It utilizes
algorithms such as Delaunay triangulation, finding the minimum spanning tree, and the A* search algorithm.
The project concept was inspired by the first part of this video:
[https://www.youtube.com/watch?v=rBY2Dzej03A&t=257s&ab_channel=Vazgriz].
In the following sections, I will briefly describe the subsequent steps presented
in the video in my implementation of these algorithms, avoiding diving into details and simply outlining how the mentioned 
algorithms work.

The first step is to place rooms at random points on a surface with predetermined dimensions. The size of the initial 
surface determines factors such as the length and winding of individual corridors. To achieve relatively
short and winding corridors, I set the size of the initial space to be relatively small, so that most rooms overlap.


![image](https://github.com/Rafiid/Procedurally_generated_dungeon/assets/79717572/10214b0c-983d-4efe-9e6c-53b7710b61c7)

                                            [Ten rooms in small area]

The next step is to spread the rooms apart. This involves comparing two consecutive rooms. If they overlap, a length is 
chosen to determine the distance by which the room will be moved. The distance between the room
centers and the length and width of each room are checked. The largest value is chosen as the displacement size, and then 
the room is randomly moved in one of four directions by this value. If the aforementioned initial
surface on which the rooms are scattered is large, the distance between the rooms will always be the highest value, 
resulting in long and boring corridors.

![image](https://github.com/Rafiid/Procedurally_generated_dungeon/assets/79717572/21db1b9f-7bae-4f2e-b5dd-f4a38c22dbf7)

                                            [Ten rooms spread apart]

Now, Delaunay Triangulation can be performed [https://en.wikipedia.org/wiki/Delaunay_triangulation]. Thanks to this 
algorithm, optimal connections between rooms are found. The set of points used to construct the graph
consists of coordinates for each room, indicating where thedoor is located and thus where the corridor should lead. These 
coordinates are calculated based on the local door coordinates for each room and its center
coordinates in the general space.

![image](https://github.com/Rafiid/Procedurally_generated_dungeon/assets/79717572/d8e650bf-b60a-400d-9fef-215feeada64e)



                                    [Door location for room type with index 0]

The goal of the algorithm is to create a list of points called a triangulation, i.e., a set of triangles characterized by 
the property that no vertex lies inside the circumcircle of any triangle in the set. The first step
is to create the so-called super triangle, which is large enough to contain all points inside it. Then, all points are 
iterated through, added to the triangulation, forming triangles, and checking if they satisfy the
aforementioned condition. After the algorithm is executed, a set of edges belonging to triangles is obtained, which often 
repeat, so duplicates are removed. If there are two edges between the same points in the set
(i.e., from point A to B and from point B to A), one of these edges is also deleted.

[zdiecie triangulacji]

The next step is to find the main path in the dungeon. This should be the optimal route through all rooms. This is 
facilitated by the Minimum Spanning Tree [https://en.wikipedia.org/wiki/Minimum_spanning_tree]. 
It is a minimal subtree containing all points in the graph connected by edges with the smallest possible weights, so as not 
to form cycles. In my case, the graph is unweighted, so when selecting the appropriate edges, the
first possible one is always chosen. The method offinding the MST involves creating a list of visited points, starting with 
only one randomly selected point. Then, as long as the list of visited vertices does not contain
all vertices, an edge that meets the condition ofconnecting a visited point with an unvisited one is searched for. When such 
an edge is found, it is added to the MST, and the new vertex is added to the list of visited ones.

[zdiecie mst]

Such a tree creates a dungeon with straight corridors. The next step is to add certain unused edges, which will create 
cycles in the graph and thus make the dungeon more winding. I iterate through all the edges outside
the MST and with a certain probability add them to the MST (this probability can be adjusted from the Unity inspector, 
usually around 30%).

[zdiecie nowego mst]

Now, nothing stands in the way of laying out the appropriate paths between the rooms. This was accomplished using the A* 
search algorithm. For this purpose, a suitable grid was created to facilitate pathfinding.
Each square represented a "piece" of the corridor, which would later be represented as a prefab. To make this technique 
work, care was taken to ensure that each room prefab was of the appropriate size and fit into the grid.
Before the search for corridors begins, all rooms are iterated through to mark on the grid where the rooms are located and 
to designate them as walls, indicating that a corridor cannot be placed there. Now, the algorithm
can start searching for optimal paths between the vertices of all edges in the MST (enriched with edges introducing cycles). 
In the meantime, vertices between which a corridor is created (i.e., squares next to the rooms
just below the doors) are collected in a separate list, so that they can later be properly prepared for interaction with the 
room doors.

[tutaj screen siatki z pokojami ]

The algorithm divides all available points (squares in the grid) into two groups: OpenSet and CloseSet. OpenSet stores 
points that are iterated through, while CloseSet stores points that have been fully explored.
In the main loop, a point with the smallest total cost is selected, estimated as the sum of the cost of reaching that point 
from the start and the distance from the end point. Now it is removed from OpenSet, and then
all its neighbors are examined, setting parameters for them, depending on, among other things, the previously selected 
point. An important moment occurs when a point achieves a new smallest cost of arrival. Then, besides
calculating all parameters, a variable, previous, is also initialized, pointing to the previous point. Its purpose is to 
help create the path from the back, checking subsequent previous variables for each point, starting
from the end. If the examined neighbor is not yet in OpenSet, it is now added to it, ensuring the continuation of the main 
loop. Finding the path can end in two ways. The first is when the selected point eventually
becomes our endpoint, and the loop ends, returning a list of consecutive points leading from start to finish. The second is 
when all points are explored and OpenSet is empty without finding a point that is our
endpoint, then an empty path is returned, indicating that such a path does not exist.



After this process, two ready lists are obtained. One with all the squares in the grid where corridor prefabs should be 
placed, and the other list containing all the squares in the grid representing corridor prefabs
that need to be adapted to neighbor the corridor doors. The only additional task is to remove duplicates, because in the 
process of creating corridors, these freshly generated ones are not set as walls, causing corridors
to intersect. This intentional feature leads to the creation of interesting combinations, such as intersections, double, or 
even triple corridors, and something resembling squares. Each square on the grid is replaced by
a corridor prefab, which is simply a small space enclosed by walls on all sides.

[zdj prefabu korytarza]

Now, by comparing all the points in the corridor list, it is checked whether a given point is adjacent to another point that 
is also in this list. If so, based on the x, y coordinates on the map, its position is
checked (whether it is on top, bottom, left, or right), and the appropriate wall is removed. This ultimately leads to the 
creation of a complete corridor. The principle is similar for the corridor fragment at the
doors of the rooms. The room where the given end point of the corridor is located is found, and the appropriate wall is 
removed based on the local door location relative to the center of the room. For example,
if the door location meets the condition: door.x > 0 && Mathf.Abs(door.x) > room.size.x/2, it means that the door is to the 
right of the room, i.e., to the left of the corridor.

Such a conducted algorithm results in the creation of a fully functional, unique dungeon.

[gotowy loch]
