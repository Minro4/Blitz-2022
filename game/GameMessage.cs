using System.Collections.Generic;
using System.Linq;
using Blitz2022;

namespace Blitz2022
{
    public class GameConfig
    {
        public int pointsPerDiamond;
        public int maximumDiamondSummonLevel;
        public int initialDiamondSummonLevel;
    }

    public class GameMessage
    {
        public int tick;

        public int totalTick;

        public string teamId;
        public List<Team> teams;

        public Map map;

        public GameConfig gameConfig;

        public Dictionary<int, string[]> teamPlayOrderings;

        public Dictionary<string, Team> getTeamsMapById
        {
            get { return this.teams.ToDictionary(team => team.id, team => team); }
        }
    }
}