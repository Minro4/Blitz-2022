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

        public Unit()
        {
        }

        public Unit(Unit other)
        {
            id = other.id;
            teamId = other.teamId;
            position = other.position;
            path = other.path;
            hasDiamond = other.hasDiamond;
            diamondId = other.diamondId;
            hasSpawned = other.hasSpawned;
            isSummoning = other.isSummoning;
            lastState = other.lastState;
        }

        public static Unit Factory(Unit unit)
        {
            if (!unit.hasSpawned)
            {
                return new UnitDead(unit);
            }

            if (unit.hasDiamond)
            {
                return new UnitWithDiamond(unit);
            }

            return new UnitWithoutDiamond(unit);
        }

        public virtual Action NextAction()
        {
            return null;
        }


        public bool isVinable()
        {
            return MapManager.isVinable(position, teamId);
        }

        public List<Position> AdjacentPositions()
        {
            return new List<Position>()
            {
                new Position(position.x, position.y + 1),
                new Position(position.x, position.y - 1),
                new Position(position.x + 1, position.y),
                new Position(position.x - 1, position.y),
            };
        }

        public List<Position> WalkableAdjacentPositions()
        {
            return AdjacentPositions().Where(pos => MapManager.isWalkable(position, pos)).ToList();
        }
        
        public List<Position> DropablePositions()
        {
            return AdjacentPositions().Where(MapManager.isEmpty).ToList();
        }
    }

    public class UnitWithoutDiamond : Unit
    {
        public UnitWithoutDiamond(Unit baseUnit) : base(baseUnit)
        {
        }

        private Map.Position targetPos;

        public override Action NextAction()
        {
            if(MoveValue() > KillValue())
            {
                return new Action(UnitActionType.MOVE, id, targetPos);
            }
            else if (KillValue() > MoveValue())
            {
                return new Action(UnitActionType.ATTACK, id, targetPos);
            }
            return new Action(UnitActionType.NONE, id, position);
        }

        public int KillValue()
        {
            List<Unit> adjacentEnemy = UnitManager.AdjacentEnemies(this.position);
            if (adjacentEnemy.Any())
            {
                targetPos = adjacentEnemy[0].position;
                return 10000;
            }
                
            return -1;
        }

        public int MoveValue()
        {
            List<Diamond> diamondsByDistance = MapManager.AvailableDiamondsByDistance(this.position);
            int maxvalue = 0;

            foreach(Diamond diamond in diamondsByDistance)
            {
                if(diamond.IsClosest(this.position))
                {
                    //TODO
                    //Faut faire un calcul plus complexe que la soustraction pour estimer la valeur
                    int diamondValue = diamond.Value() - MapManager.Distance(this.position, diamond.position);
                    if(maxvalue <= diamondValue)
                    {
                        maxvalue = diamondValue;
                        targetPos = diamond.position;
                    }
                }
            }

            return maxvalue;
        }
    }

    public class UnitWithDiamond : Unit
    {
        public UnitWithDiamond(Unit baseUnit) : base(baseUnit)
        {
        }

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
        public UnitDead(Unit baseUnit) : base(baseUnit)
        {
        }

        public override Action NextAction()
        {
            Map.Position optimalSpawnPosition;
            optimalSpawnPosition = MapManager.spawnPositions.MaxBy(position => SpawnValue(position));
            Array.Find(MapManager.message.map.diamonds, element => element == MapManager.DiamondsByDistance(optimalSpawnPosition).First()).isAvailable = false;
            return new Action(UnitActionType.SPAWN, this.id, optimalSpawnPosition);
        }

        public int SpawnValue(Map.Position spawnFrom)
        {
            return MapManager.Distance(MapManager.GetClosestDiamond(spawnFrom).position, spawnFrom);
        }
    }

    public class UnitState
    {
        public Position positionBefore;
        public string wasVinedBy;
        public string wasAttackedBy;
    }
}
