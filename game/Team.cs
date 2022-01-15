using System.Collections.Generic;
using System.Linq;
using System;
using static Blitz2022.Action;
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
            return null;
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
            Map.Position optimalSpawnPosition;
            optimalSpawnPosition = MapManager.spawnPositions.MaxBy(position => SpawnValue(position));
            Array.Find(MapManager.message.map.diamonds,element => element == MapManager.DiamondsByDistance(optimalSpawnPosition).First()).isAvailable = false;
            return new Action(UnitActionType.SPAWN, this.id, optimalSpawnPosition) ;
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