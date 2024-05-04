using System.Collections.Generic;
using UnityEngine;

public class SetUp : MonoBehaviour
{ 
    void Start()
    {
        List<Room> roomList = DungeonGenerator.instance.getRoomList();

        transform.position = new Vector3(roomList[0].coord.x, 0.5f, roomList[0].coord.y);
    }

}
