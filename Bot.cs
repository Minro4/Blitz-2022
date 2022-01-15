using System;
using System.Collections.Generic;
using System.Linq;
using Blitz2021;
using static Blitz2022.Action;
using static Blitz2022.Map;

namespace Blitz2022
{
    public class Bot
    {
        public static string NAME = "GUY";

        public Bot()
        {
            // initialize some variables you will need throughout the game here
            Console.WriteLine("Initializing your super mega bot!");
        }

        /*
        * Here is where the magic happens, for now the moves are random. I bet you can do better ;)
        */
        public GameCommand nextMove(GameMessage gameMessage)
        {
            List<Action> actions = new List<Action>();

            Pathfinding.Initialize(gameMessage);
            UnitManager.Initialize(gameMessage);
            MapManager.Initialize(gameMessage);

            actions = UnitManager.allies.Select(unit => unit.NextAction()).ToList();
            return new GameCommand(actions);
        }

        private Position findRandomSpawn(Map map)
        {
            List<Position> spawns = new List<Position>();
            int x = 0;
            foreach (string[] tileX in map.tiles)
            {
                int y = 0;
                foreach (string tileY in tileX)
                {
                    var position = new Position(x, y);
                    if (map.getTileTypeAt(position) == TileType.SPAWN)
                    {
                        spawns.Add(position);
                    }

                    y++;
                }

                x++;
            }

            return spawns[new Random().Next(spawns.Count)];
        }

        private Position getRandomPosition(int horizontalSize, int verticalSize)
        {
            Random rand = new Random();
            return new Position(rand.Next(horizontalSize), rand.Next(verticalSize));
        }
    }
}