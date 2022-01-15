using System;
using System.Linq;

namespace Blitz2022
{
    public class Map
    {
        public string[][] tiles;
        public Diamond[] diamonds;

        public class Diamond
        {
            public Position position;
            public string id;
            public int summonLevel;
            public int points;
            public string ownerId;
            public bool isAvailable = true;

            public int Value()
            {
                //TODO fix pour vrai
                return points + 5 * MapManager.message.remainingTicks();
            }

            public int ValueFromPosition(Position from)
            {
                var distance = MapManager.Distance(from, position);
                var remainingTicks = MapManager.message.remainingTicks();
                var holdingTicks = remainingTicks - distance;
                if (holdingTicks <= 0)
                {
                    return 0;
                }

                return points + Math.Max(0, holdingTicks * 5 - valueLostSummoningUpToMax());
            }

            public int valueLostSummoningUpToMax()
            {
                int max = 5;
                int sum = 0;
                for (int i = summonLevel; i < max; i++)
                {
                    sum += (max - i) * (i + 1);
                }

                return sum;
            }

            public int timeToSummonDiamondTo(int level)
            {
                int sum = 0;
                for (int i = summonLevel + 1; i <= level; i++)
                {
                    sum += i;
                }

                return sum;
            }

            public void setUnavailable()
            {
                isAvailable = false;
            }

            public bool isFree()
            {
                return isAvailable && ownerId == null;
            }

            public bool isEnemyOwned()
            {
                return UnitManager.allies.All(ally => ally.id != ownerId);
            }

            public bool IsClosest(Map.Position from)
            {
                var distanceToDiamond = MapManager.Distance(from, position);
                if (distanceToDiamond == int.MaxValue)
                {
                    return false;
                }

                var spawnedUnitsWithoutSpawned = UnitManager.units.Where(unit => !unit.hasDiamond && unit.hasSpawned);
                var distances = spawnedUnitsWithoutSpawned.Select(unit => MapManager.Distance(unit.position, position)).ToList();
                var minDist = distances.Count > 0 ? distances.Min() : int.MaxValue;

                return distanceToDiamond <= minDist;
            }
        }

        public class Position
        {
            public Position(int x, int y)
            {
                this.x = x;
                this.y = y;
            }

            public int x;
            public int y;


            public bool isValid(GameMessage gameMessage)
            {
                return x >= 0 && y >= 0 && x < gameMessage.map.horizontalSize() && y < gameMessage.map.verticalSize();
            }

            public override string ToString()
            {
                return string.Format("P({0},{1})", this.x, this.y);
            }

            public bool isVinable(string teamId)
            {
                //TODO
                // return mapManager.isvinable
                return false;
            }

            public TileType tileType()
            {
                return MapManager.message.map.getTileTypeAt(this);
            }

            public override bool Equals(object obj)
            {
                if (obj is Position)
                {
                    return (obj as Position).x == this.x && (obj as Position).y == this.y;
                }

                return base.Equals(obj);
            }
            
            public static bool operator ==(Position a, Position b)
            {
                if (a is null)
                {
                    if (b is null)
                    {
                        return true;
                    }

                    // Only the left side is null.
                    return false;
                }
                // Equals handles case of null on right side.
                return a.Equals(b);
            }
            
            public static bool operator !=(Position a, Position b) => !(a == b);
        }

        public enum TileType
        {
            EMPTY,
            WALL,
            SPAWN
        }

        public class PointOutOfMapException : Exception
        {
            public PointOutOfMapException(Position position, int horizontalSize, int verticalSize)
                : base(String.Format("Point {0} is out of map, x and y must be greater than 0 and x less than {1} and y less than {2}.", position,
                    horizontalSize, verticalSize))
            {
            }
        }

        public int horizontalSize()
        {
            return this.tiles.Length;
        }

        public int verticalSize()
        {
            return this.tiles[0].Length;
        }

        public TileType getTileTypeAt(Position position)
        {
            string rawTile = this.getRawTileValueAt(position);

            switch (rawTile)
            {
                case "EMPTY":
                    return TileType.EMPTY;
                case "WALL":
                    return TileType.WALL;
                case "SPAWN":
                    return TileType.SPAWN;
                default:
                    throw new ArgumentException(String.Format("'{0}' is not a valid tile", rawTile));
            }
        }

        public String getRawTileValueAt(Position position)
        {
            this.validateTileExists(position);
            return this.tiles[position.x][position.y];
        }

        public void validateTileExists(Position position)
        {
            if (position.x < 0 || position.y < 0 || position.x >= this.horizontalSize() || position.y >= this.verticalSize())
            {
                throw new PointOutOfMapException(position, this.horizontalSize(), this.verticalSize());
            }
        }
    }
}