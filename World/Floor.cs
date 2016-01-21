using System;
using System.Collections.Generic;
using System.Diagnostics;
using Aiv.Engine;
using Futuridium.Characters;

namespace Futuridium.World
{
    public class Floor : GameObject
    {
        public Floor(int floorIndex)
        {
            Name = "floor" + floorIndex;
            FloorIndex = floorIndex;

            RoomsList = new List<Room>();

            OnDestroy += DestroyEvent;
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
            var charactersInfo = (CharactersInfo) Engine.Objects["charactersInfo"];
            var newRoomIndex = Tuple.Create(Rooms.GetLength(0)/2, Rooms.GetLength(1)/2);
            FirstRoom = new Room(null, newRoomIndex, this)
            {
                RoomType = 0,
                FirstRoom = true
            };
            FirstRoom.RandomizeRoom(0, 0, (int) (FloorIndex*1.99f), rnd, charactersInfo);
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

        private void DestroyEvent(object sender)
        {
            AudioSource.Pause();
            foreach (var room in RoomsList)
                Game.Game.OnDestroyHelper(room);
        }

        public bool OpenRoom(Room room)
        {
            if (room != null && (CurrentRoom == null || CurrentRoom.Enemies.Count == 0))
            {
                Game.Game.Instance.StartLoading();
                Debug.Assert(RoomsList.Contains(room) &&
                             (CurrentRoom == null || CurrentRoom.Left == room || CurrentRoom.Right == room ||
                              CurrentRoom.Top == room || CurrentRoom.Bottom == room));
                Room lastRoom = null;
                if (CurrentRoom != null)
                {
                    CurrentRoom.CloseRoom();
                    Game.Game.OnDestroyHelper(CurrentRoom);
                    CurrentRoom.Enabled = false;
                    lastRoom = CurrentRoom;
                }

                CurrentRoom = room;
                if (Engine.Objects.ContainsKey(CurrentRoom.Name))
                    CurrentRoom.Enabled = true;
                else
                    Engine.SpawnObject(CurrentRoom);
                CurrentRoom.OpenRoom();
                // empty room
                if (CurrentRoom.Enemies.Count == 0)
                {
                    CurrentRoom.GameBackground.OpenDoors();
                }
                if (lastRoom != null)
                {
                    var playerWidth = Player.Instance.Width;
                    var playerHeight = Player.Instance.Height;
                    if (lastRoom.Left == room)
                    {
                        Player.Instance.X = room.Width - playerWidth -
                                            CurrentRoom.GameBackground.RightDoorAsset.Width
                                            - CurrentRoom.GameBackground.SpawnOnDoorPadding;
                        //Player.Instance.y = Game.Instance.engine.height/2 - playerWidth/2;
                    }
                    else if (lastRoom.Right == room)
                    {
                        Player.Instance.X = CurrentRoom.GameBackground.LeftDoorAsset.Width
                                            + CurrentRoom.GameBackground.SpawnOnDoorPadding;
                        Player.Instance.Y = room.Height/2 - playerHeight/2;
                    }
                    else if (lastRoom.Top == room)
                    {
                        Player.Instance.X = room.Width/2 - playerWidth/2;
                        Player.Instance.Y = room.Height - playerHeight -
                                            CurrentRoom.GameBackground.BottomDoorAsset.Height
                                            - CurrentRoom.GameBackground.SpawnOnDoorPadding;
                    }
                    else if (lastRoom.Bottom == room)
                    {
                        Player.Instance.X = room.Width/2 - playerWidth/2;
                        Player.Instance.Y = CurrentRoom.GameBackground.TopDoorAsset.Height
                                            + CurrentRoom.GameBackground.SpawnOnDoorPadding;
                    }
                }

                Player.Instance.RoomChanged();

                CurrentRoom.SpawnRoomObjects();

                Game.Game.Instance.StopLoading();

                AudioSource.Play(((AudioAsset)Engine.GetAsset("sound_door_close")).Clip);

                //Engine.PlaySound("sound_door_close");

                return true;
            }
            return false;
        }

        public void RandomizeFloor(int minRoom, int maxRoom)
        {
            var rnd = Game.Game.Instance.Random.GetRandom("randomizeFloor_" + FloorIndex);

            FloorBackgroundType = rnd.Next(0, GameBackground.AvailableBackgrounds);

            var numberOfRooms = rnd.Next(minRoom, maxRoom);
            var minEnemies = (int) (4*((FloorIndex + 5)/10f));
            var maxEnemies = (int) (8*((FloorIndex + 5)/9f));

            Debug.WriteLine("Randomizing floor, number of rooms: {0} ; background type: {1}", numberOfRooms,
                FloorBackgroundType);
            Rooms = new Room[numberOfRooms, numberOfRooms]; // worst-case linear floor

            RandomRooms(numberOfRooms, minEnemies, maxEnemies, rnd);
            foreach (var room in RoomsList)
                CheckRoomParents(room.RoomIndex);

            CalcolateMapSize();
        }
    }
}