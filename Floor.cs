﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Aiv.Engine;
using System.Collections;

namespace StupidAivGame
{
	public class Floor : GameObject
	{
		// next: real map floor, so with directions
		public Room currentRoom;
		public Room [,] rooms;
		public List<Room> roomsList;

		public Room firstRoom = null;

		public int floorIndex;

		public int mapWidth;
		public int mapHeight;

		public int floorBackgroundType;

		public Floor (int floorIndex)
		{
			this.name = "floor" + floorIndex;
			this.floorIndex = floorIndex;

			//rooms = new Room[500, 500]; // worst-case linear floor
			roomsList = new List<Room>();
		}

		public Floor (Room [,] rooms, int floorIndex) : this(floorIndex)
		{
			this.rooms = rooms;
		}

		private void CalcolateMapSize ()
		{
			//int[] rowsWidth = new int[rooms.GetLength (0)];
			int maxWidth = 0;
			int maxHeight = 0;
			int startsFromX = -1;
			int startsFromY = -1;
			int[] rowsHeight = new int[rooms.GetLength (1)];
			for (int x = 0; x < rooms.GetLength (0); x++) {
				int xL = 0;
				for (int y = 0; y < rooms.GetLength (1); y++) {
					if (rooms [x, y] != null) {
						if (startsFromX > x || startsFromX == -1)
							startsFromX = x;
						if (startsFromY > y || startsFromY == -1)
							startsFromY = y;
						if (rowsHeight [y] < x)
							rowsHeight [y] = x;
						if (xL < y)
							xL = y;
					}
				}
				if (xL > maxHeight)
					maxHeight = xL;
				//rowsWidth [x] = xL;
			}
			foreach (int yL in rowsHeight)
				if (yL > maxWidth)
					maxWidth = yL;
			mapWidth = maxWidth - startsFromX + 1;
			mapHeight = maxHeight - startsFromY + 1;

			Room[,] newRooms = new Room[mapWidth, mapHeight];
			if (startsFromX < 0)
				startsFromX = 0;
			if (startsFromY < 0)
				startsFromY = 0;
			for (int bx = startsFromX; bx < rooms.GetLength(0); bx++) {
				for (int by = startsFromY; by < rooms.GetLength(1); by++) {
					if (rooms [bx, by] != null)
						newRooms [bx - startsFromX, by - startsFromY] = rooms [bx, by];
				}
			}
			rooms = newRooms;
		}

		public void RandomizeFloor (int minRoom, int maxRoom)
		{
			Random rnd = ((Game)engine.objects["game"]).random.GetRandom("randomizeFloor_" + floorIndex);

			floorBackgroundType = rnd.Next (0, 3);

			int numberOfRooms = rnd.Next (minRoom, maxRoom);
			int minEnemies = (int)(2 * ((floorIndex + 1) / 2.0));
			int maxEnemies = (int)(5 * ((floorIndex + 1) / 2.0));

			Console.WriteLine ("Randomizing floor, number of rooms: {0} ; background type: {1}", numberOfRooms, floorBackgroundType);
			rooms = new Room[numberOfRooms, numberOfRooms]; // worst-case linear floor

			RandomRooms (numberOfRooms, minEnemies, maxEnemies, rnd);
			foreach (Room room in roomsList)
				CheckRoomParents(room.roomIndex);

			CalcolateMapSize ();
		}

		private void CheckRoomParents (Tuple<int, int> roomIndex)
		{
			Room room = rooms [roomIndex.Item1, roomIndex.Item2];
			// left
			if (roomIndex.Item1 > 0 && rooms [roomIndex.Item1 - 1, roomIndex.Item2] != null) {
				rooms [roomIndex.Item1 - 1, roomIndex.Item2].right = room;
				room.left = rooms [roomIndex.Item1 - 1, roomIndex.Item2];
			}
			// right
			if ((roomIndex.Item1 + 1) < rooms.GetLength(0) && rooms [roomIndex.Item1 + 1, roomIndex.Item2] != null) {
				rooms [roomIndex.Item1 + 1, roomIndex.Item2].left = room;
				room.right = rooms [roomIndex.Item1 + 1, roomIndex.Item2];
			}
			// bottom
			if ((roomIndex.Item2 + 1) < rooms.GetLength(1) && rooms [roomIndex.Item1, roomIndex.Item2 + 1] != null) {
				rooms [roomIndex.Item1, roomIndex.Item2 + 1].top = room;
				room.bottom = rooms [roomIndex.Item1, roomIndex.Item2 + 1];
			}
			// top
			if (roomIndex.Item2 > 0 && rooms [roomIndex.Item1, roomIndex.Item2 - 1] != null) {
				rooms [roomIndex.Item1, roomIndex.Item2 - 1].bottom = room;
				room.top = rooms [roomIndex.Item1, roomIndex.Item2 - 1];
			}
		}

		// 
		// 1 if couldnt create
		/* Depth
		private int RandomRooms (Tuple<int, int> lastRoomIndex, int maxRooms, int minEnemies, int maxEnemies, Random rnd)
		{
			if (roomsList.Count >= maxRooms || lastRoomIndex.Item1 < 0 || lastRoomIndex.Item2 < 0 || 
				lastRoomIndex.Item1 >= rooms.GetLength(0) || lastRoomIndex.Item2 >= rooms.GetLength(1) || rooms [lastRoomIndex.Item1, lastRoomIndex.Item2] != null)
				return 1;
			// spawn boss room, should be one of the farthest
			if (roomsList.Count == (maxRooms - 1)) {
			}
			rooms [lastRoomIndex.Item1, lastRoomIndex.Item2] = Room.RandomRoom (roomsList.Count, minEnemies, maxEnemies, this, floorIndex, lastRoomIndex);
			roomsList.Add (rooms [lastRoomIndex.Item1, lastRoomIndex.Item2]);
			if (firstRoom == null)
				firstRoom = rooms [lastRoomIndex.Item1, lastRoomIndex.Item2];
			int startingRooms = roomsList.Count;
			int errorRooms = 0;
			while (startingRooms == roomsList.Count && errorRooms < 4) {
				for (int i = 0; i < 4; i++) { // randomize visit
					int rndX = rnd.Next(-1, 2);
					int rndY = rnd.Next(-1, 2);
					if (rndX != 0 && rndY != 0) {
						if (rnd.Next (0, 2) == 1)
							rndX = 0;
						else
							rndY = 0;
					}
					if (rnd.Next (0, 2) == 1) {
						errorRooms += RandomRooms (Tuple.Create (lastRoomIndex.Item1 + rndX, lastRoomIndex.Item2 + rndY), maxRooms, minEnemies, maxEnemies, rnd);
					}
				}
			}
			return 0;
		}
		*/
		// breadth
		// TODO: boss room
		private int RandomRooms (int maxRooms, int minEnemies, int maxEnemies, Random rnd)
		{
			if (firstRoom != null) {
				throw new Exception ("Floor.RandomRooms can be called only once.");
			}
			Tuple<int, int> newRoomIndex = Tuple.Create (rooms.GetLength (0) / 2, rooms.GetLength (1) / 2);
			firstRoom = Room.RandomRoom (roomsList.Count, minEnemies, maxEnemies, this, floorIndex, newRoomIndex, rnd);
			rooms [newRoomIndex.Item1, newRoomIndex.Item2] = firstRoom;
			roomsList.Add (firstRoom);

			Queue<Room> queue = new Queue<Room> (maxRooms);
			queue.Enqueue (firstRoom);
			while (roomsList.Count < maxRooms) {
				Room currentRoom = queue.Dequeue();
				int addedRooms = 0;
				while (addedRooms == 0) { // randomize visit
					int rndX, rndY;
					do {
						rndX = rnd.Next (currentRoom.roomIndex.Item1 > 0 ? -1 : 0, currentRoom.roomIndex.Item1 < (maxRooms - 1) ? 2 : 1);
						rndY = rnd.Next (currentRoom.roomIndex.Item2 > 0 ? -1 : 0, currentRoom.roomIndex.Item2 < (maxRooms - 1) ? 2 : 1);
						if (rndX != 0 && rndY != 0) {
							if (rnd.Next (0, 2) == 1)
								rndX = 0;
							else
								rndY = 0;
						}
						newRoomIndex = Tuple.Create (currentRoom.roomIndex.Item1 + rndX, currentRoom.roomIndex.Item2 + rndY);
					} while ((rndX == 0 && rndY == 0) || rooms [newRoomIndex.Item1, newRoomIndex.Item2] != null);
					if (rnd.Next (0, 2) == 1) {
						Room newRoom = Room.RandomRoom (roomsList.Count, minEnemies, maxEnemies, this, floorIndex, newRoomIndex, rnd);
						rooms [newRoomIndex.Item1, newRoomIndex.Item2] = newRoom;
						roomsList.Add (newRoom);
						queue.Enqueue (newRoom);
						addedRooms++;
					}
				}
			}
			return 0;
		}

		public bool OpenRoom (Room room) 
		{
			if (room != null && (currentRoom == null || currentRoom.enemies.Count == 0)) {
				Debug.Assert (roomsList.Contains (room) && (currentRoom == null || currentRoom.left == room || currentRoom.right == room || currentRoom.top == room || currentRoom.bottom == room));

				Game game = (Game)engine.objects ["game"];
				if (currentRoom != null) {
					if (currentRoom.left == room) {
						game.player.x = game.engine.width - 32 - game.player.width;
						game.player.y = game.engine.height / 2;
					} else if (currentRoom.right == room) {
						game.player.x = 32;
						game.player.y = game.engine.height / 2;
					} else if (currentRoom.top == room) {
						game.player.x = game.engine.width / 2;
						game.player.y = game.engine.height - 32 - game.player.height;
					} else if (currentRoom.bottom == room) {
						game.player.x = game.engine.width / 2;
						game.player.y = 32;
					} else {
						return false;
					}
				}
				if (currentRoom != null)
					Game.OnDestroyHelper (currentRoom);

				currentRoom = room;
				engine.SpawnObject (currentRoom.name, currentRoom);
				currentRoom.SpawnEnemies ();
				currentRoom.gameBackground.SetupDoorsForRoom (room);

				return true;
			}
			return false;
		}
	}
}

