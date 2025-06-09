using UnityEngine;
using FoodyGo.Mapping;

namespace FoodyGo.Database
{
    public class Monster
    {
        public MapLocation location;
        public Vector3 position;
        public double spawnTimestamp;
        public double lastHeardTimestamp;
        public double lastSeenTimestamp;
        public GameObject gameObject;
        public int footstepRange;
    }
}