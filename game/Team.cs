using System.Collections.Generic;
using System.Linq;
using System;
using Blitz2021;
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
            return AdjacentPositions().Where(pos => MapManager.isWalkable(position, pos) && !MapManager.isPlayerOnPosition(pos)).ToList();
        }


        public List<Position> DropablePositions()
        {
            List<Position> ajdPos = new List<Position>();

            foreach (Position pos in WalkableAdjacentPositions())
            {
                bool valid = true;
                foreach (Unit unit in UnitManager.enemies)
                {
                    if (unit.position != null && unit.position.Equals(pos))
                    {
                        valid = false;
                    }
                }

                if (valid)
                {
                    ajdPos.Add(pos);
                }
            }

            return ajdPos;
        }

        public List<Position> AdjacentAllies()
        {
            var alliedPos = UnitManager.allies.Select(ally => ally.position);
            return AdjacentPositions().Where(pos => alliedPos.Contains(pos)).ToList();
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
            else if (vineValue > moveValue && vineValue > killValue)
            {
                return new Action(UnitActionType.VINE, id, targetVinePos);
            }

            return new Action(UnitActionType.MOVE, id, targetMovePos);
        }

        public int KillValue()
        {
            if (position.tileType() == TileType.SPAWN)
                return -1;

            var adjacentEnemy = UnitManager.AdjacentEnemies(position).Where(unit => unit.position.tileType() != TileType.SPAWN);
            if (adjacentEnemy.Any())
            {
                targetKillPos = adjacentEnemy.First().position;
                return 10000;
            }

            return -1;
        }

        public int VineValue()
        {
            List<Unit> vineableUnits = MapManager.vinableFrom(this.position);

            foreach (Unit unit in vineableUnits)
            {
                if (unit.teamId != MapManager.message.teamId && unit.hasDiamond)
                {
                    //Si on joue avant, on le vine
                    if (UnitManager.MyTeamPlaysBefore(unit.teamId))
                    {
                        targetVinePos = unit.position;
                        return 5000;
                    }
                }
            }

            return -1;
        }

        public double MoveValue()
        {
            var bestDiamond = MapManager.GetBestDiamond(position);
            if (bestDiamond != null)
            {
                if (bestDiamond.isEnemyOwned())
                {
                    var firstAvailablePos = MapManager.FollowPathExludingSpawn(position, bestDiamond.position);
                    targetMovePos = firstAvailablePos ?? bestDiamond.position;
                    return bestDiamond.ValueFromPosition(position);
                }

                targetMovePos = MapManager.FollowPath(position, bestDiamond.position);
                bestDiamond.setUnavailable();
                return bestDiamond.Value();
            }

            var closestFriendlyDiamond = MapManager.DiamondsByValue(this.position).FirstOrDefault();
            if (closestFriendlyDiamond == null) return 0; // TODO Fix this better
            targetMovePos = MapManager.FollowPath(position, closestFriendlyDiamond.position) ?? closestFriendlyDiamond.position;
            return closestFriendlyDiamond.Value() * 0.5;
        }
    }


    public class UnitWithDiamond : Unit
    {
        public UnitWithDiamond(Unit baseUnit) : base(baseUnit)
        {
        }

        private bool killViner;
        private Map.Position killTarget;

        public override Action NextAction()
        {
            double drop = DropValue();
            double move = MoveValue();
            double upgrade = UpgradeValue();

            if (drop > move && drop > upgrade)
            {
                return DropAction();
            }
            /*  if (killViner)
              {
                  killViner = false;
                  return new Action(UnitActionType.ATTACK, id, killTarget);
              }*/
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
            int tickLeft = MapManager.message.remainingTicks();
            Diamond diamond = getDiamond();

            if (tickLeft <= MapManager.message.teams.Count + 2 && !DropablePositions().Any())
            {
                return int.MaxValue;
            }

            if (tickLeft <= MapManager.message.teams.Count)
            {
                return int.MaxValue;
            }

            var (dist, enemy) = MapManager.MinimumDistanceFromEnemy(position);
            if (enemy == null)
            {
                return diamond.points;
            }

            var otherTeamPlaysFirst = UnitManager.otherTeamWillPlayFirstNextTurn(enemy.teamId);
            var minimumDistFromEnemy = 2 + (otherTeamPlaysFirst ? 1 : 0);

            if (dist <= minimumDistFromEnemy)
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
            int tickLeft = MapManager.message.remainingTicks();
            Diamond diamond = getDiamond();

            if (MapManager.isVinableByOtherTeams(position, teamId))
            {
                //try to find escape route
                var positions = WalkableAdjacentPositions();
                foreach (Map.Position pos in positions)
                {
                    if (MapManager.isVinableByOtherTeams(position, teamId))
                    {
                        return (diamond.points + diamond.summonLevel) * 2;
                    }
                }

                // no escape possible
                Unit viner = MapManager.vinableFrom(position).First();
                if (!UnitManager.MyTeamPlaysBefore(viner.teamId))
                {
                    killViner = true;
                    killTarget = viner.position;
                }

                return 0;
            }

            return diamond.points + diamond.summonLevel;
        }


        public double UpgradeValue()
        {
            int tickLeft = MapManager.message.remainingTicks();
            Diamond diamond = getDiamond();

            var (dist, enemy) = MapManager.MinimumDistanceFromEnemy(position);

            var minDistanceToSummon =
                enemy != null ? diamond.summonLevel + 1 + (UnitManager.otherTeamWillPlayFirstNextTurn(enemy.teamId) ? 1 : 0) : int.MaxValue;
            if (diamond.summonLevel < 5 && !MapManager.isVinableByOtherTeams(position, teamId) && dist > minDistanceToSummon)
            {
                return tickLeft * (diamond.summonLevel + 1) - diamond.summonLevel;
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
            var dropPosition = WalkableAdjacentPositions();
            if (dropPosition.Count > 0)
            {
                return new Action(UnitActionType.DROP, id, dropPosition[0]);
            }

            var alliedPos = AdjacentAllies();
            if (alliedPos.Any())
            {
                return new Action(UnitActionType.DROP, id, alliedPos.First());
            }
            else
            {
                return new Action(UnitActionType.NONE, id, position);
            }
        }

        public Action MoveAction()
        {
            var positions = WalkableAdjacentPositions();
            positions.Add(position);
            var positionFarthestFromEnemies = positions.OrderBy(pos => MapManager.MinimumDistanceFromEnemy(pos).Item1);

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
            var optimalSpawnPosition = MapManager.spawnPositions.MaxBy(position => SpawnValue(position));
            //var optimalSpawnPosition = MapManager.getBestSpawnPosition();
            MapManager.GetBestDiamond(optimalSpawnPosition)?.setUnavailable();
            return new Action(UnitActionType.SPAWN, this.id, optimalSpawnPosition);
        }

        public static int SpawnValue(Map.Position spawnFrom)
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