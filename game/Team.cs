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
            }.Where(pos => pos.isValid(MapManager.message)).ToList();
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

        private Map.Position targetKillPos;
        private Map.Position targetMovePos;
        private Map.Position targetVinePos;

        public override Action NextAction()
        {
            var moveValue = MoveValue();
            int killValue = KillValue();
            int vineValue = VineValue();

            if (moveValue > killValue && moveValue > vineValue)
            {
                return new Action(UnitActionType.MOVE, id, targetMovePos);
            }
            else if (killValue > moveValue && killValue > vineValue)
            {
                return new Action(UnitActionType.ATTACK, id, targetKillPos);
            }
            else if(vineValue > moveValue && vineValue > killValue)
            {
                return new Action(UnitActionType.VINE, id, targetVinePos);
            }

            return new Action(UnitActionType.MOVE, id, targetMovePos);
        }

        public int KillValue()
        {
            List<Unit> adjacentEnemy = UnitManager.AdjacentEnemies(this.position);
            if (adjacentEnemy.Any())
            {
                targetKillPos = adjacentEnemy[0].position;
                return 10000;
            }

            return -1;
        }

        public int VineValue()
        {
            List<Unit> vineableUnits = MapManager.vinableFrom(this.position);

            foreach(Unit unit in vineableUnits)
            {
                if (unit.teamId != MapManager.message.teamId && unit.hasDiamond)
                {
                    //position de l'ennemi dans l'ordre du tour
                    int enemyTeamIndex = MapManager.message.teamPlayOrderings[0].Select((s, i) => new { teamId = s, index = i }).FirstOrDefault(x => x.teamId.Equals(unit.teamId)).index;
                    //position de notre �quipe dans l'ordre du tour
                    int myTeamIndex = MapManager.message.teamPlayOrderings[0].Select((s, i) => new { teamId = s, index = i }).FirstOrDefault(x => x.teamId.Equals(MapManager.message.teamId)).index;
                    
                    //Si on jour avant, on le vine
                    if(myTeamIndex< enemyTeamIndex)
                    {
                        targetVinePos = unit.position;
                        return 5000;
                    }
                }
            }
            return -1;
        }

        public int MoveValue()
        {
            List<Diamond> diamondsByValue = MapManager.AvailableDiamondsByValue(this.position);
            
            var closestFreeDiamonds = diamondsByValue.Where(diamond => diamond.IsClosest(position) && diamond.isFree()).ToList();
            if (closestFreeDiamonds.Any())
            {
                var bestDiamond = closestFreeDiamonds.First();
                targetMovePos = bestDiamond.position;
                return bestDiamond.Value();
            }

            foreach (Diamond diamond in diamondsByDistance)
            var enemyDiamonds = diamondsByValue.Where(diamond => diamond.isEnemyOwned() || diamond.isFree()).ToList();
            if (enemyDiamonds.Any())
            {
                var closest = enemyDiamonds.First();
                targetMovePos = closest.position;
                return closest.Value() * 1.5;
            }

            var closestFriendlyDiamond = diamondsByValue.First();
            targetMovePos = closestFriendlyDiamond.position;
            return closestFriendlyDiamond.Value() * 0.5;
        }

        
    }

    public class UnitWithDiamond : Unit
    {
        public UnitWithDiamond(Unit baseUnit) : base(baseUnit)
        {
        }

        public override Action NextAction()
        {
            
            double drop = DropValue();
            double move = MoveValue();
            double upgrade = UpgradeValue();

            if (drop > move && drop > upgrade)
            {
                return DropAction();
            }
            else if (move > drop && move > upgrade)
            {
                return MoveAction();
            }
            else if (upgrade > drop && upgrade > move)
            {
                return new Action(UnitActionType.SUMMON, id, position);
            }
            else 
            {
                return new Action(UnitActionType.NONE, id, position);
            }

        }

        public double DropValue()
        {
            //TODO si ennemie proche
            int tickLeft = MapManager.message.tick - MapManager.message.totalTick;
            Diamond diamond = getDiamond();

            if (tickLeft < 5)
            {
                return 1000000;
            }
            else if (2 > MapManager.MinimumDistanceFromEnemy(position)) 
            {
                return int.MaxValue;
            }
            else
            {
                 return diamond.points;
            }
           
        }
        

        
        public double MoveValue()
        {
            //TODO
            int tickLeft = MapManager.message.tick - MapManager.message.totalTick;
            Diamond diamond = getDiamond();

            if (MapManager.isVinableByOtherTeams(position, teamId)) 
            {
                var positions = WalkableAdjacentPositions();
                foreach (Map.Position pos in positions) 
                {
                    if (MapManager.isVinableByOtherTeams(position, teamId)) 
                    {
                        return (diamond.points + diamond.summonLevel) * 2;
                    }
                }
            }

            return diamond.points + diamond.summonLevel;
        }


        public double UpgradeValue()
        {
            
            int tickLeft = MapManager.message.tick - MapManager.message.totalTick;
            Diamond diamond = getDiamond();

            //TODO minus si ennemie trop proche
            if (diamond.summonLevel < 5 && !MapManager.isVinableByOtherTeams(position, teamId) && 2 < MapManager.MinimumDistanceFromEnemy(position))
            {
                return tickLeft*(diamond.summonLevel+1) - diamond.summonLevel;
            }

            return 0;
        }

        public Diamond getDiamond() 
        {
            foreach (Diamond diamond in MapManager.message.map.diamonds) 
            {
                if (diamond.position.Equals(position)) 
                {
                    return diamond;
                }      
            }

            return null;
        }

        public Action DropAction() 
        {
            var dropPosition = DropablePositions();
            if (dropPosition.Count > 0)
            {
                return new Action(UnitActionType.DROP, id, dropPosition[0]);
            }
            else
            {
                return new Action(UnitActionType.NONE, id, position);
            }
        }

        public Action MoveAction()
        {
            var positions = WalkableAdjacentPositions();
            var positionFarthestFromEnemies = positions.OrderBy(pos => MapManager.MinimumDistanceFromEnemy(pos));

            foreach (Position pos in positionFarthestFromEnemies.Reverse<Position>()) 
            {
                if (MapManager.isVinableByOtherTeams(position, teamId))
                {
                   return new Action(UnitActionType.MOVE, id, pos);
                }
            }
            return new Action(UnitActionType.MOVE, id, positionFarthestFromEnemies.Last());
        }


    }

    public class UnitDead : Unit
    {
        public UnitDead(Unit baseUnit) : base(baseUnit)
        {
        }

        public override Action NextAction()
        {
            var optimalSpawnPosition = MapManager.spawnPositions.MinBy(position => SpawnValue(position));
            MapManager.GetBestDiamond(optimalSpawnPosition)?.setUnavailable();
            return new Action(UnitActionType.SPAWN, this.id, optimalSpawnPosition);
        }

        public int SpawnValue(Map.Position spawnFrom)
        {
            var bestDiamond = MapManager.GetBestDiamond(spawnFrom);
            return bestDiamond != null ? bestDiamond.ValueFromPosition(spawnFrom) : 0;
        }
    }

    public class UnitState
    {
        public Position positionBefore;
        public string wasVinedBy;
        public string wasAttackedBy;
    }
}
