using System;
using System.Collections.Generic;
using System.Linq;

namespace Blitz2022
{
    public static class UnitManager
    {
        public static List<Unit> units;
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
            units = message.teams.SelectMany(team => team.units).ToList();
        }
    }
}