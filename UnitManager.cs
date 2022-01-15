using System;
using System.Collections.Generic;
using System.Linq;

namespace Blitz2022
{
    public static class UnitManager
    {
        public static List<Unit> units;
        public static List<Unit> allies;
        public static List<Unit> enemies;

        public static GameMessage gameMessage;
        public static string teamId;

        private static void ConvertTeamUnits(GameMessage message)
        {
            foreach (var team in message.teams)
            {
                team.units = team.units.Select(Unit.Factory).ToList();
            }
        }

        public static void Initialize(GameMessage message)
        {
            gameMessage = message;
            ConvertTeamUnits(message);
            allies = message.teams.FindAll(teams => teams.id == message.teamId).SelectMany(team => team.units).ToList();
            enemies = message.teams.FindAll(teams => teams.id != message.teamId).SelectMany(team => team.units).ToList();
            units = message.teams.SelectMany(team => team.units).ToList();

            teamId = message.teamId;

            MapManager.Initialize(message);

            if (allies.Where(unit => !unit.hasSpawned).ToList().Count > 0)
            {
                MapManager.updateSpawnCostMap();
            }
        }

        public static List<Unit> AdjacentEnemies(Map.Position from)
        {
            List<Map.Position> adjacentPos = new List<Map.Position>();
            adjacentPos.Add(new Map.Position(from.x + 1, from.y));
            adjacentPos.Add(new Map.Position(from.x, from.y + 1));
            adjacentPos.Add(new Map.Position(from.x - 1, from.y));
            adjacentPos.Add(new Map.Position(from.x, from.y - 1));

            List<Unit> AdjacentEnemies = new List<Unit>();
            foreach (Unit unit in UnitManager.units)
            {
                if (adjacentPos.Contains(unit.position) && unit.teamId != MapManager.message.teamId)
                    AdjacentEnemies.Add(unit);
            }

            return AdjacentEnemies;
        }

        public static bool MyTeamPlaysBefore(string teamId)
        {
            //position de l'ennemi dans l'ordre du tour
            int enemyTeamIndex = MapManager.message.teamPlayOrderings[MapManager.message.tick].Select((s, i) => new { teamId = s, index = i })
                .FirstOrDefault(x => x.teamId.Equals(teamId)).index;
            //position de notre �quipe dans l'ordre du tour
            int myTeamIndex = MapManager.message.teamPlayOrderings[MapManager.message.tick].Select((s, i) => new { teamId = s, index = i })
                .FirstOrDefault(x => x.teamId.Equals(MapManager.message.teamId)).index;

            //Si on jour avant, on le vine
            if (myTeamIndex < enemyTeamIndex)
                return true;

            return false;
        }

        public static bool otherTeamWillPlayFirstNextTurn(string otherTeamId)
        {
            if (gameMessage.remainingTicks() == 0)
            {
                return false;
            }

            var orderNextTick = gameMessage.teamPlayOrderings[gameMessage.tick + 1];
            foreach (var team in orderNextTick)
            {
                if (team == gameMessage.teamId)
                {
                    return true;
                }

                if (team == otherTeamId)
                {
                    return false;
                }
            }

            return false;
        }
    }
}