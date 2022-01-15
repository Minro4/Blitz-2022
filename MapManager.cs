using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Blitz2021;
using Roy_T.AStar.Paths;
using Roy_T.AStar.Primitives;
using System.Linq;

namespace Blitz2022
{
    public static class MapManager
    {
        public static GameMessage message;
        public static List<Map.Position> allPositions;
        public static List<Map.Position> spawnPositions;
        public static List<Map.Position> wallPositions;
        public static List<Map.Position> emptyPositions;


        public static void Initialize(GameMessage messageParam)
        {
            //TODO
            message = messageParam;
            allPositions = new List<Map.Position>();
            for (int x = 0; x < message.map.horizontalSize(); x++){
                for (int y = 0; y < message.map.verticalSize(); y++){
                    allPositions.Add(new Map.Position(x,y));
                }
            }
            spawnPositions = allPositions.Where(position => message.map.getTileTypeAt(position) == Map.TileType.SPAWN).ToList();
            wallPositions = allPositions.Where(position => message.map.getTileTypeAt(position) == Map.TileType.WALL).ToList();
            emptyPositions = allPositions.Where(position => message.map.getTileTypeAt(position) == Map.TileType.EMPTY).ToList();
        }

        public static int Distance(Map.Position from, Map.Position to)
        {
            var path = Pathfinding.Path(message, from, to);
            if (path.Type == PathType.Complete)
            {
                return (int) path.Distance.Meters;
            }

            return int.MaxValue;
        }

        public static List<Map.Diamond> DiamondsByDistance(Map.Position from)
        {
            //TODO
            return new List<Map.Diamond>();
        }
        public static Map.Diamond ClosestDiamond(Map.Position from)
        {
            DiamondsByDistance(from).First();
            return null;
        }

        public static bool isVinable(Map.Position from, string teamId)
        {
            //TODO
            return false;
        }


        public static int WhenIsVinable(Map.Position from, string teamId)
        {
            //TODO
            //In how many turns will this position be vinable if the player is from teamId
            return 1000;
        }

        /*
        public static bool IsTheClosestUnitToPosition(Map.Position from, Map.Position to)
        {
            
        }*/
    }
}