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
    }
}