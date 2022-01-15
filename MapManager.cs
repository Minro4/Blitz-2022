using System.Collections.Generic;

namespace Blitz2022
{
    public static class MapManager
    {
        public static GameMessage message;

        public static void Initialize(GameMessage messageParam)
        {
            //TODO
            message = messageParam;
        }

        public static int Distance(Map.Position from, Map.Position to)
        {
            //TODO
            return 0;
        }

        public static List<Map.Diamond> DiamondsByDistance(Map.Position from)
        {
            //TODO
            return new List<Map.Diamond>();
        }

        public static bool isVinable(Map.Position from, string teamId)
        {
            //TODO
            return false;
        }


        public static int WhenIsVinable(Map.Position from, string teamId)
        {
            //TODO
            //In how many turns will this position be vinable if the player is from teamId
            return 1000;
        }

        /*
        public static bool IsTheClosestUnitToPosition(Map.Position from, Map.Position to)
        {
            
        }*/
    }
}