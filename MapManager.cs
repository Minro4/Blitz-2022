﻿using System;
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

        private static int getWallMaxX(Map.Position from) 
        {
            int maxX = message.map.horizontalSize();
            for (int x = from.x; x >= 0 && x < maxX; x++)
            {
                Map.Position currentPosition = new Map.Position(x, from.y);

                if (isWallInGame(message.map.getTileTypeAt(currentPosition)))
                {
                    return x;
                }
            }

            return message.map.horizontalSize();
        }

        private static int getWallMinX(Map.Position from)
        {
            int maxX = message.map.horizontalSize();
            for (int x = from.x; x >= 0 && x < maxX; x--)
            {
                Map.Position currentPosition = new Map.Position(x, from.y);

                if (isWallInGame(message.map.getTileTypeAt(currentPosition)))
                {
                    return x;
                }
            }

            return 0;
        }

        private static int getWallMaxY(Map.Position from)
        {
            int maxY = message.map.verticalSize();
            for (int y = from.y; y >= 0 && y < maxY; y++)
            {
                Map.Position currentPosition = new Map.Position(from.x, y);

                if (isWallInGame(message.map.getTileTypeAt(currentPosition)))
                {
                    return y;
                }
            }

            return message.map.verticalSize();
        }

        private static int getWallMinY(Map.Position from)
        {
            int maxY = message.map.verticalSize();
            for (int y = from.y; y >= 0 && y < maxY; y--)
            {
                Map.Position currentPosition = new Map.Position(from.x, y);

                if (isWallInGame(message.map.getTileTypeAt(currentPosition)))
                {
                    return y;
                }
            }

            return 0;
        }

        public static List<Unit> vinableFrom(Map.Position from) 
        {
            List<Unit> vinableUnit =new List<Unit>();

            int wallMaxX = getWallMaxX(from);
            int wallMinX = getWallMinX(from);
            int wallMaxY = getWallMaxY(from);
            int wallMinY = getWallMinY(from);

            foreach (Team team in message.teams)
            {
                    foreach (Unit unit in team.units)
                    {
                        if (unit.position.y == from.y)
                        {
                            if (unit.position.x > wallMinX && unit.position.x < wallMaxX)
                            {
                                vinableUnit.Add(unit);
                            }
                        }

                        if (unit.position.x == from.x)
                        {
                            if (unit.position.y > wallMinY && unit.position.y < wallMaxY)
                            {
                                vinableUnit.Add(unit);
                            }
                        }

                    }
            }

            return vinableUnit;
        }

        public static bool isVinable(Map.Position from, string teamId)
        {
            List<Unit> unites = vinableFrom(from);

            foreach (Unit unite in unites) 
            {
                if (unite.teamId == teamId) 
                {
                    return true;
                }
                    
            }

            return false;
        }

        public static bool isWallInGame(Map.TileType tile) 
        {
            return tile == Map.TileType.SPAWN || tile == Map.TileType.WALL;
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