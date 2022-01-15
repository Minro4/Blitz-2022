using System;
using System.Linq;

namespace Blitz2022
{
    public class UnitManager
    {
        public static void ConvertTeamUnits(GameMessage message)
        {
            foreach (var team in message.teams)
            {
                Console.WriteLine(team.units);
                team.units = team.units.Select(Unit.Factory).ToList();
            }
        }
        public UnitManager(GameMessage message)
        {
             
        }
    }
}