using System.Collections.Generic;
using static Blitz2022.Map;
using static Blitz2022.Unit;

namespace Blitz2022
{
    public class GameCommand
    {
        public GameCommand(List<Action> actions)
        {
            this.actions = actions;
        }

        public List<Action> actions;
    }

    public class Action
    {
        public Position target;
        public UnitActionType action;
        public string unitId;
        public ActionType type;

        public Action(UnitActionType action, string unitId, Position target)
        {
            this.type = ActionType.UNIT;
            this.unitId = unitId;
            this.target = target;
            this.action = action;
        }
        public Action(UnitActionType action, string unitId)
        {
            this.type = ActionType.UNIT;
            this.unitId = unitId;
            this.action = action;
        }

        public enum ActionType
        {
            UNIT
        }

        public enum UnitActionType
        {
            SPAWN, MOVE, SUMMON, DROP, VINE, ATTACK, NONE
        }
    }
}