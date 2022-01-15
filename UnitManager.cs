﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Blitz2022
{
    public static class UnitManager
    {
        public static List<Unit> units;
        public static List<Unit> allies;
        public static List<Unit> enemies;

        private static void ConvertTeamUnits(GameMessage message)
        {
            foreach (var team in message.teams)
            {
                team.units = team.units.Select(Unit.Factory).ToList();
            }
        }
        public static void Initialize(GameMessage message)
        {
            ConvertTeamUnits(message);
            allies = message.teams.FindAll(teams => teams.id == message.teamId).SelectMany(team => team.units).ToList();
            enemies = message.teams.FindAll(teams => teams.id != message.teamId).SelectMany(team => team.units).ToList();
            units = message.teams.SelectMany(team => team.units).ToList();
        }

        public static List<Unit> AdjacentEnemies(Map.Position from)
        {
            List<Map.Position> adjacentPos = new List<Map.Position>();
            adjacentPos.Add(new Map.Position(from.x+1, from.y));
            adjacentPos.Add(new Map.Position(from.x, from.y+1));
            adjacentPos.Add(new Map.Position(from.x-1, from.y));
            adjacentPos.Add(new Map.Position(from.x, from.y-1));

            List<Unit> AdjacentEnemies = new List<Unit>();
            foreach(Unit unit in UnitManager.units)
            {
                if (adjacentPos.Contains(unit.position) && unit.teamId != MapManager.message.teamId)
                    AdjacentEnemies.Add(unit);
            }
            return AdjacentEnemies;
        }
    }
}