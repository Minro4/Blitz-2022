using System;
using System.Collections.Generic;
using System.Linq;

namespace Blitz2022
{
    public class UnitManager
    {
        public List<Unit> units;
        public static void ConvertTeamUnits(GameMessage message)
        {
            foreach (var team in message.teams)
            {
                team.units = team.units.Select(Unit.Factory).ToList();
            }
        }
        public UnitManager(GameMessage message)
        {
            units = message.teams.SelectMany(team => team.units).ToList();
        }

        public static List<Unit> AdjacentEnemy(Map.Position from)
        {
            List<Map.Position> adjacentPos = new List<Map.Position>();
            adjacentPos.Add(new Map.Position(from.x+1, from.y));
            adjacentPos.Add(new Map.Position(from.x, from.y+1));
            adjacentPos.Add(new Map.Position(from.x-1, from.y));
            adjacentPos.Add(new Map.Position(from.x, from.y-1));

            List<Unit> AdjacentEnemies = new List<Unit>();
            foreach (Map.Position pos in adjacentPos)
            {
                
            }
            return AdjacentEnemies;
        }
    }
}