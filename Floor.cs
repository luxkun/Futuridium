using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace StupidAivGame
{
	public class Floor
	{
		// next: real map floor, so with directions
		public int currentRoomIndex;
		public Room currentRoom;
		public List<Room> rooms;
		public Game game;

		public Floor (Game game, List<Room> rooms)
		{
			this.game = game;
			this.rooms = rooms;
		}

		public static Floor randomFloor (Game game, int minRoom, int maxRoom)
		{
			Random rnd = new Random((int) DateTime.Now.Ticks);
			int numberOfRooms = rnd.Next (minRoom, maxRoom);

			// different name? static scope?
			List<Room> randomRooms = new List<Room>();
			for (int i = 0; i < numberOfRooms; i++) {
				Room newRoom = Room.randomRoom (i, game, 2, 5, 1);
				randomRooms.Add (newRoom);
			}
			return new Floor (game, randomRooms);
		}

		public void OpenRoom (int roomIndex) 
		{
			Debug.Assert (roomIndex < rooms.Count);
			currentRoom = rooms [roomIndex];
			currentRoom.SpawnEnemies ();
			this.currentRoomIndex = roomIndex;
		}
	}
}

