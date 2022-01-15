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
                //TODO
                return points;
            }

            public void setUnavailable()
            {
                isAvailable = false;
            }

            public bool IsClosest(Map.Position from)
            {
                var distanceToDiamond = MapManager.Distance(from, position);
                if (distanceToDiamond == int.MaxValue)
                {
                    return false;
                }

                var distances = UnitManager.units.Where(unit => !unit.hasDiamond).Select(unit => MapManager.Distance(unit.position, position));
                var minDist = distances.Min();
                
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
        }

        public enum TileType
        {
            EMPTY, WALL, SPAWN
        }

        public class PointOutOfMapException : Exception
        {
            public PointOutOfMapException(Position position, int horizontalSize, int verticalSize)
            : base(String.Format("Point {0} is out of map, x and y must be greater than 0 and x less than {1} and y less than {2}.", position, horizontalSize, verticalSize))
            { }
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