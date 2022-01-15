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
        public static List<costMapPoint> spawnCostMap;

       public class costMapPoint
        {

            public costMapPoint(Map.Position position, double cost)
            {
                this.cost = cost;
                this.position = position;
            }

            public double cost;
            public Map.Position position;
        }

        public static void updateSpawnCostMap()
        {
            spawnCostMap = new List<costMapPoint>();
            foreach (Map.Position spawnPosition in spawnPositions) 
            {
                spawnCostMap.Add(new costMapPoint(spawnPosition, UnitDead.SpawnValue(spawnPosition)));
            }
        }

        public static  Map.Position getBestSpawnPosition() 
        {
            if (spawnCostMap != null && spawnCostMap.Count > 0) 
            {
                double maxValue = spawnCostMap[0].cost;
                int maxIndex = 0;
                for (int x = 0; x < spawnCostMap.Count; x++)
                {
                    if (maxValue < spawnCostMap[x].cost)
                    {
                        maxValue = spawnCostMap[x].cost;
                        maxIndex = x;
                    }
                }

                Map.Position bestSpawn = spawnCostMap[maxIndex].position;
                spawnCostMap.RemoveAt(maxIndex);
                return bestSpawn;
            }
            return new Map.Position(0, 0);

        }

        public static void Initialize(GameMessage messageParam)
        {
            //TODO
            message = messageParam;
            allPositions = new List<Map.Position>();
            for (int x = 0; x < message.map.horizontalSize(); x++)
            {
                for (int y = 0; y < message.map.verticalSize(); y++)
                {
                    allPositions.Add(new Map.Position(x, y));
                }
            }

            spawnPositions = allPositions.Where(position => message.map.getTileTypeAt(position) == Map.TileType.SPAWN && !MapManager.isPlayerOnPosition(position)).ToList();
            wallPositions = allPositions.Where(position => message.map.getTileTypeAt(position) == Map.TileType.WALL).ToList();
            emptyPositions = allPositions.Where(position => message.map.getTileTypeAt(position) == Map.TileType.EMPTY).ToList();

            
            var alliesWithDiamonds = UnitManager.allies.Where(ally => ally.diamondId != null).ToList();
            if (alliesWithDiamonds.Count() != 0){
                foreach (var ally in alliesWithDiamonds){
                    message.map.diamonds.Where(d => d.id == ally.diamondId).FirstOrDefault().setUnavailable();
                }
            }
        }

        public static int Distance(Map.Position from, Map.Position to)
        {
            //return Math.Abs(from.x - to.x) + Math.Abs(from.y - to.y);
            var path = Path(from, to);

            bool invalidPath =  path.Edges.Count() == 0 || path.Edges[path.Edges.Count()-1].End.Position.X != to.x || path.Edges[path.Edges.Count()-1].End.Position.Y != to.y;
            return invalidPath ? int.MaxValue : (int)path.Distance.Meters ;
        }

        public static Path Path(Map.Position from, Map.Position to)
        {
            return Pathfinding.Path(message, from, to);
        }

        public static int MinimumDistanceFromEnemy(Map.Position pos)
        {
            try
            {
                var closestEnemy = UnitManager.enemies.Where(enemie => enemie.hasSpawned).OrderBy(enemy => Distance(pos, enemy.position)).First();
                return Distance(pos, closestEnemy.position);
            }
            catch (InvalidOperationException)
            {
                return int.MaxValue;
            }
        }

        public static List<Map.Diamond> DiamondsByValue(Map.Position from)
        {
            var viableDiamonds = message.map.diamonds.Where(diamond => diamond.ValueFromPosition(from) != 0).ToList();
            return viableDiamonds != null ? viableDiamonds.OrderBy(diamond => diamond.ValueFromPosition(from)).ToList() : null;
        }

        public static List<Map.Diamond> AvailableDiamondsByValue(Map.Position from)
        {
            List<Map.Diamond> diamondsByDistance = DiamondsByValue(from);
            return diamondsByDistance != null ? diamondsByDistance.Where(x => x.isAvailable).ToList() : null;
        }

        public static Map.Diamond GetBestDiamond(Map.Position from)
        {
            var bestDiamond = AvailableDiamondsByValue(from).LastOrDefault();
            return bestDiamond != null ? bestDiamond : null;
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
            List<Unit> vinableUnit = new List<Unit>();

            int wallMaxX = getWallMaxX(from);
            int wallMinX = getWallMinX(from);
            int wallMaxY = getWallMaxY(from);
            int wallMinY = getWallMinY(from);

            foreach (Team team in message.teams)
            {
                foreach (Unit unit in team.units)
                {
                    if (unit.position != null)
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

        public static bool isVinableByOtherTeams(Map.Position from, string teamId)
        {
            List<Unit> unites = vinableFrom(from);

            foreach (Unit unite in unites)
            {
                if (unite.teamId != teamId)
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

        public static bool isWalkable(Map.Position from, Map.Position to)
        {
            return isEmpty(to) && !(from.tileType() == Map.TileType.EMPTY && to.tileType() == Map.TileType.SPAWN);
        }

        public static bool isEmpty(Map.Position position)
        {
            return message.map.getTileTypeAt(position) != Map.TileType.WALL && !isPlayerOnPosition(position);
        }

        public static bool isPlayerOnPosition(Map.Position position)
        {
            return UnitManager.units.Find(unit => unit.position == position) != null;
        }

        public static bool IsTheClosestUnitToPosition(Map.Position from, Map.Position to)
        {
            return true;
        }
        
        public static Map.Position FirstAvailablePositionToGoTo(Map.Position from, Map.Position to)
        {
            var path = Path(from, to).Edges;
            foreach (var edge in path.Reverse())
            {
                var pos = new Map.Position((int)edge.End.Position.X, (int)edge.End.Position.Y);
                if (isEmpty(pos))
                {
                    return pos;
                }
            }
            
            return null;
        }

        public static List<Map.Position> AdjacentPositions(Map.Position from)
        {
            return new List<Map.Position>()
            {
                new Map.Position(from.x,from.y + 1),
                new Map.Position(from.x, from.y - 1),
                new Map.Position(from.x + 1, from.y),
                new Map.Position(from.x - 1, from.y),
            }.Where(pos => pos.isValid(MapManager.message) && (message.map.getTileTypeAt(pos) != Map.TileType.WALL) && (message.map.getTileTypeAt(pos) != Map.TileType.SPAWN)).ToList();

        }

        public static Map.Position FirstAvailablePositionToGoToExludingSpawn(Map.Position from, Map.Position to)
        {

            foreach (Map.Position pos in AdjacentPositions(to))
            {
                var path = Path(from, to).Edges;
                foreach (var edge in path.Reverse())
                {
                    var currentStep = new Map.Position((int)edge.End.Position.X, (int)edge.End.Position.Y);
                    if (isEmpty(currentStep))
                    {
                        return currentStep;
                    }
                }
            }

            return null;
        }

    }
}