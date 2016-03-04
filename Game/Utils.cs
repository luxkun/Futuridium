using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Aiv.Engine;
using OpenTK;

namespace Futuridium.Game
{
    public static class Utils
    {
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static List<string> GetAssetName(string baseName, int sX, int sY, int lenX = 1, int lenY = 1)
        {
            var result = new List<string>();
            for (var y = 0; y < lenY; y++)
            {
                for (var x = 0; x < lenX; x++)
                {
                    result.Add($"{baseName}_{y + sY}_{x + sX}");
                }
            }
            return result;
        }

        public static void LoadAnimation(Engine engine, string name, string fileName, int xLen, int yLen)
        {
            var spriteAsset = new SpriteAsset(fileName);
            var blockSizeOnWall = new Vector2(spriteAsset.Width/(float) xLen, spriteAsset.Height/(float) yLen);
            for (var posX = 0; posX < xLen; posX++)
                for (var posY = 0; posY < yLen; posY++)
                {
                    var animName = $"{name}_{posY}_{posX}";
                    Debug.WriteLine("Loaded animations: " + animName);
                    engine.LoadAsset(animName,
                        new SpriteAsset(fileName, (int) (posX*blockSizeOnWall.X), (int) (posY*blockSizeOnWall.Y),
                            (int) blockSizeOnWall.X, (int) blockSizeOnWall.Y));
                }
        }
    }
}