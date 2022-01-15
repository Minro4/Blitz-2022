using Roy_T.AStar.Primitives;
using Roy_T.AStar.Grids;
using Roy_T.AStar.Paths;
using Blitz2022;


namespace Blitz2021
{
    public static class Pathfinding
    {
        private static PathFinder pathFinder;
        private static Grid gridInsideSpawn;
        private static Grid gridOutsideSpawn;

        private readonly static Velocity traversalVelocity = Velocity.FromKilometersPerHour(100);

        private static bool isInitialized = false;

        public static void Initialize(GameMessage gameMessage)
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
            pathFinder = new PathFinder();
            gridInsideSpawn = CreateGrid(gameMessage, true);
            gridOutsideSpawn = CreateGrid(gameMessage, false);
        }

        public static Grid CreateGrid(GameMessage gameMessage, bool isFromInsideSpawn)
        {
            var gridSize = new GridSize(columns: gameMessage.map.horizontalSize(), rows: gameMessage.map.verticalSize());
            var cellSize = new Size(Distance.FromMeters(1), Distance.FromMeters(1));

            var grid = Grid.CreateGridWithLateralConnections(gridSize, cellSize, traversalVelocity);

            for (int i = 0; i < gameMessage.map.horizontalSize(); i++)
            {
                for (int j = 0; j < gameMessage.map.verticalSize(); j++)
                {
                    Map.Position pos = new Map.Position(i, j);

                    if (!isWalkable(pos, gameMessage, isFromInsideSpawn))
                    {
                        grid.DisconnectNode(new GridPosition(i, j));
                    }
                }
            }

            return grid;
        }

        public static bool isWalkable(Map.Position pos, GameMessage gameMessage, bool isFromInsideSpawn)
        {
            var tileType = gameMessage.map.getTileTypeAt(pos);
            return tileType != Map.TileType.WALL && (isFromInsideSpawn || tileType != Map.TileType.SPAWN);
        }

        public static Path Path(GameMessage gameMessage, Map.Position from, Map.Position to)
        {
            if (gameMessage.map.getTileTypeAt(from) == Map.TileType.SPAWN)
            {
                return PathFromInsideSpawn(from, to);
            }

            return PathFromOutsideSpawn(from, to);
        }

        public static Path PathFromInsideSpawn(Map.Position p1, Map.Position p2)
        {
            return pathFinder.FindPath(new GridPosition(p1.x, p1.y), new GridPosition(p2.x, p2.y), gridInsideSpawn);
        }

        public static Path PathFromOutsideSpawn(Map.Position p1, Map.Position p2)
        {
            return pathFinder.FindPath(new GridPosition(p1.x, p1.y), new GridPosition(p2.x, p2.y), gridOutsideSpawn);
        }
    }
}