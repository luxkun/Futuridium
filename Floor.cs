using System;
using System.Collections.Generic;
using System.Diagnostics;
using Aiv.Engine;

namespace Futuridium
{
    public class Floor : GameObject
    {
        public Floor(int floorIndex)
        {
            name = "floor" + floorIndex;
            FloorIndex = floorIndex;

            RoomsList = new List<Room>();
        }

        public Floor(Room[,] rooms, int floorIndex) : this(floorIndex)
        {
            Rooms = rooms;
        }

        public Room CurrentRoom { get; private set; }

        public Room FirstRoom { get; private set; }

        public int FloorBackgroundType { get; set; }

        public int FloorIndex { get; }

        public int MapHeight { get; private set; }

        public int MapWidth { get; private set; }

        public Room[,] Rooms { get; private set; }

        public List<Room> RoomsList { get; }

        private void CalcolateMapSize()
        {
            var maxWidth = 0;
            var maxHeight = 0;
            var startsFromX = -1;
            var startsFromY = -1;
            var rowsHeight = new int[Rooms.GetLength(1)];
            for (var x = 0; x < Rooms.GetLength(0); x++)
            {
                var xL = 0;
                for (var y = 0; y < Rooms.GetLength(1); y++)
                {
                    if (Rooms[x, y] != null)
                    {
                        if (startsFromX > x || startsFromX == -1)
                            startsFromX = x;
                        if (startsFromY > y || startsFromY == -1)
                            startsFromY = y;
                        if (rowsHeight[y] < x)
                            rowsHeight[y] = x;
                        if (xL < y)
                            xL = y;
                    }
                }
                if (xL > maxHeight)
                    maxHeight = xL;
            }
            foreach (var yL in rowsHeight)
                if (yL > maxWidth)
                    maxWidth = yL;
            MapWidth = maxWidth - startsFromX + 1;
            MapHeight = maxHeight - startsFromY + 1;

            var newRooms = new Room[MapWidth, MapHeight];
            if (startsFromX < 0)
                startsFromX = 0;
            if (startsFromY < 0)
                startsFromY = 0;
            for (var bx = startsFromX; bx < Rooms.GetLength(0); bx++)
            {
                for (var by = startsFromY; by < Rooms.GetLength(1); by++)
                {
                    if (Rooms[bx, by] != null)
                        newRooms[bx - startsFromX, by - startsFromY] = Rooms[bx, by];
                }
            }
            Rooms = newRooms;
        }

        public void RandomizeFloor(int minRoom, int maxRoom)
        {
            var rnd = ((Game) engine.objects["game"]).Random.GetRandom("randomizeFloor_" + FloorIndex);

            FloorBackgroundType = rnd.Next(0, GameBackground.AvailableBackgrounds);

            var numberOfRooms = rnd.Next(minRoom, maxRoom);
            var minEnemies = (int) (1*((FloorIndex + 2)/2.0));
            var maxEnemies = (int) (5*((FloorIndex + 2)/2.0));

            Debug.WriteLine("Randomizing floor, number of rooms: {0} ; background type: {1}", numberOfRooms,
                FloorBackgroundType);
            Rooms = new Room[numberOfRooms, numberOfRooms]; // worst-case linear floor

            RandomRooms(numberOfRooms, minEnemies, maxEnemies, rnd);
            foreach (var room in RoomsList)
                CheckRoomParents(room.RoomIndex);

            CalcolateMapSize();
        }

        private void CheckRoomParents(Tuple<int, int> roomIndex)
        {
            var room = Rooms[roomIndex.Item1, roomIndex.Item2];
            // left
            if (roomIndex.Item1 > 0 && Rooms[roomIndex.Item1 - 1, roomIndex.Item2] != null)
            {
                Rooms[roomIndex.Item1 - 1, roomIndex.Item2].Right = room;
                room.Left = Rooms[roomIndex.Item1 - 1, roomIndex.Item2];
            }
            // right
            if (roomIndex.Item1 + 1 < Rooms.GetLength(0) && Rooms[roomIndex.Item1 + 1, roomIndex.Item2] != null)
            {
                Rooms[roomIndex.Item1 + 1, roomIndex.Item2].Left = room;
                room.Right = Rooms[roomIndex.Item1 + 1, roomIndex.Item2];
            }
            // bottom
            if (roomIndex.Item2 + 1 < Rooms.GetLength(1) && Rooms[roomIndex.Item1, roomIndex.Item2 + 1] != null)
            {
                Rooms[roomIndex.Item1, roomIndex.Item2 + 1].Top = room;
                room.Bottom = Rooms[roomIndex.Item1, roomIndex.Item2 + 1];
            }
            // top
            if (roomIndex.Item2 > 0 && Rooms[roomIndex.Item1, roomIndex.Item2 - 1] != null)
            {
                Rooms[roomIndex.Item1, roomIndex.Item2 - 1].Bottom = room;
                room.Top = Rooms[roomIndex.Item1, roomIndex.Item2 - 1];
            }
        }

        // breadth
        private void RandomRooms(int maxRooms, int minEnemies, int maxEnemies, Random rnd)
        {
            if (FirstRoom != null)
            {
                throw new Exception("Floor.RandomRooms can be called only once per floor.");
            }
            var charactersInfo = (CharactersInfo) engine.objects["charactersInfo"];
            var newRoomIndex = Tuple.Create(Rooms.GetLength(0)/2, Rooms.GetLength(1)/2);
            FirstRoom = new Room(null, newRoomIndex, this) {RoomType = 0};
            FirstRoom.RandomizeRoom(0, 0, FloorIndex, rnd, charactersInfo);
            Rooms[newRoomIndex.Item1, newRoomIndex.Item2] = FirstRoom;
            RoomsList.Add(FirstRoom);

            var queue = new Queue<Room>(maxRooms);
            queue.Enqueue(FirstRoom);
            while (RoomsList.Count < maxRooms)
            {
                var currentRoom = queue.Dequeue();
                int addedRooms = 0, i = 0;
                while (addedRooms == 0 && i < 4)
                {
                    // randomize visit
                    if (rnd.Next(0, 2) == 1)
                    {
                        int rndX, rndY;
                        do
                        {
                            rndX = rnd.Next(currentRoom.RoomIndex.Item1 > 0 ? -1 : 0,
                                currentRoom.RoomIndex.Item1 < maxRooms - 1 ? 2 : 1);
                            rndY = rnd.Next(currentRoom.RoomIndex.Item2 > 0 ? -1 : 0,
                                currentRoom.RoomIndex.Item2 < maxRooms - 1 ? 2 : 1);
                            if (rndX != 0 && rndY != 0)
                            {
                                if (rnd.Next(0, 2) == 1)
                                    rndX = 0;
                                else
                                    rndY = 0;
                            }
                            newRoomIndex = Tuple.Create(currentRoom.RoomIndex.Item1 + rndX,
                                currentRoom.RoomIndex.Item2 + rndY);
                        } while ((rndX == 0 && rndY == 0) || Rooms[newRoomIndex.Item1, newRoomIndex.Item2] != null);
                        var roomType = 0;
                        if (RoomsList.Count == maxRooms - 1)
                        {
                            Debug.WriteLine("BOSS ROOM GEN.");
                            roomType = 1;
                        }
                        var newRoom = new Room(null, newRoomIndex, this) {RoomType = roomType};
                        newRoom.RandomizeRoom(roomType == 1 ? 1 : minEnemies, roomType == 1 ? 1 : maxEnemies, FloorIndex,
                            rnd, charactersInfo);
                        Rooms[newRoomIndex.Item1, newRoomIndex.Item2] = newRoom;
                        RoomsList.Add(newRoom);
                        queue.Enqueue(newRoom);
                        addedRooms++;
                        i++;
                    }
                }
            }
        }

        public bool OpenRoom(Room room)
        {
            if (room != null && (CurrentRoom == null || CurrentRoom.Enemies.Count == 0))
            {
                var game = (Game) engine.objects["game"];
                game.StartLoading();
                Debug.Assert(RoomsList.Contains(room) &&
                             (CurrentRoom == null || CurrentRoom.Left == room || CurrentRoom.Right == room ||
                              CurrentRoom.Top == room || CurrentRoom.Bottom == room));
                if (CurrentRoom != null)
                {
                    var playerWidth = Utils.FixBoxValue(game.Player.width);
                    var playerHeight = Utils.FixBoxValue(game.Player.height);
                    if (CurrentRoom.Left == room)
                    {
                        game.Player.x = game.engine.width - playerWidth -
                                        Utils.FixBoxValue(CurrentRoom.GameBackground.RightDoorAsset.sprite.Width)
                                        - CurrentRoom.GameBackground.SpawnOnDoorPadding;
                        game.Player.y = game.engine.height/2;
                    }
                    else if (CurrentRoom.Right == room)
                    {
                        game.Player.x = Utils.FixBoxValue(CurrentRoom.GameBackground.LeftDoorAsset.sprite.Width)
                                        + CurrentRoom.GameBackground.SpawnOnDoorPadding;
                        game.Player.y = game.engine.height/2;
                    }
                    else if (CurrentRoom.Top == room)
                    {
                        game.Player.x = game.engine.width/2;
                        game.Player.y = game.engine.height - playerHeight -
                                        Utils.FixBoxValue(CurrentRoom.GameBackground.BottomDoorAsset.sprite.Height)
                                        - CurrentRoom.GameBackground.SpawnOnDoorPadding;
                    }
                    else if (CurrentRoom.Bottom == room)
                    {
                        game.Player.x = game.engine.width/2;
                        game.Player.y = Utils.FixBoxValue(CurrentRoom.GameBackground.TopDoorAsset.sprite.Height)
                                        + CurrentRoom.GameBackground.SpawnOnDoorPadding;
                    }
                    else
                    {
                        return false;
                    }
                }
                if (CurrentRoom != null)
                    Game.OnDestroyHelper(CurrentRoom);

                CurrentRoom = room;
                engine.SpawnObject(CurrentRoom.name, CurrentRoom);
                CurrentRoom.SpawnEnemies();
                // empty room
                if (CurrentRoom.Enemies.Count == 0)
                {
                    CurrentRoom.GameBackground.OpenDoors();
                }

                game.StopLoading();

                return true;
            }
            return false;
        }
    }
}