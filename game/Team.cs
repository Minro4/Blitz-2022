using System.Collections.Generic;
using static Blitz2022.Map;

namespace Blitz2022
{
    public class Team
    {
        public string id;
        public string name;
        public int score;
        public List<Unit> units;
        public List<string> errors;
    }

    public class Unit
    {
        public string id;
        public string teamId;
        public Position position;
        public List<Position> path;
        public bool hasDiamond;
        public string diamondId;
        public bool hasSpawned;
        public bool isSummoning;
        UnitState lastState;

        public virtual Action NextAction()
        {
            return new Ac
        }


        public bool isVinable()
        {
            return MapManager.isVinable(position, teamId);
        }
    }

    public class UnitWithoutDiamond : Unit
    {
        public override Action NextAction()
        {
            //TODO
            return null;
        }

        public int KillValue()
        {
            //TODO
            return 0;
        }

        public int MoveValue()
        {
            //TODO
            return 0;
        }
    }

    public class UnitWithDiamond : Unit
    {
        public override Action NextAction()
        {
            //TODO
            return null;
        }

        public int DropValue()
        {
            //TODO
            return 0;
        }

        public int UpgradeValue()
        {
            //TODO
            return 0;
        }
    }

    public class UnitDead : Unit
    {
        public override Action NextAction()
        {
            //Toute essayer les positions de spawn possible
            return null;
        }

        public int SpawnValue(Map.Position spawnFrom)
        {
            //TODO
            return 0;
        }
    }

    public class UnitState
    {
        public Position positionBefore;
        public string wasVinedBy;
        public string wasAttackedBy;
    }
}