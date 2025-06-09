using UnityEngine;
using System.Collections;
using FoodyGo.Mapping;
using System.Collections.Generic;
using FoodyGo.Database;
using FoodyGo.Utils;
using FoodyGo.Services.GPS;

namespace FoodyGo.Services
{
    public class MonsterService : MonoBehaviour
    {
        public GPSLocationService gpsLocationService;
        public GameObject monsterPrefab;
        private double lastTimestamp;

        [Header("Monster Spawn Parameters")]
        public float monsterSpawnRate = .75f;
        public float latitudeSpawnOffset = .001f;
        public float longitudeSpawnOffset = .001f;

        [Header("Monster Visibility")]
        public float monsterHearDistance = 200f;
        public float monsterSeeDistance = 100f;
        public float monsterLifetimeSeconds = 30;

        [Header("Monster Foot Step Range")]
        public float oneStepRange = 125f;
        public float twoStepRange = 150f;
        public float threeStepRange = 200f;

        public List<Monster> monsters;

        // Use this for initialization
        void Start()
        {
            monsters = new List<Monster>();
            StartCoroutine(CleanupMonsters());
            gpsLocationService.onMapRedraw += GpsLocationService_OnMapRedraw;
        }

        private void GpsLocationService_OnMapRedraw()
        {
            //map is recentered, recenter all monsters
            foreach (Monster m in monsters)
            {
                if (m.gameObject != null)
                {
                    var newPosition = ConvertToWorldSpace((float)m.location.longitude, (float)m.location.latitude);
                    m.gameObject.transform.position = newPosition;
                }
            }
        }

        private IEnumerator CleanupMonsters()
        {
            while (true)
            {
                var now = Epoch.Now;
                var list = monsters.ToArray();
                for (int i = 0; i < list.Length; i++)
                {
                    if (list[i].spawnTimestamp + monsterLifetimeSeconds < now)
                    {
                        var monster = list[i];
                        print("Cleaning up monster");
                        if (monster.gameObject != null)
                        {
                            Destroy(monster.gameObject);
                        }
                        monsters.Remove(monster);
                    }
                }
                yield return new WaitForSeconds(5);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (gpsLocationService != null &&
                gpsLocationService.isReady &&
                gpsLocationService.timeStamp > lastTimestamp)
            {
                lastTimestamp = gpsLocationService.timeStamp;

                //update the monsters around the player
                CheckMonsters();
            }
        }

        private void CheckMonsters()
        {
            if (Random.value > monsterSpawnRate)
            {
                var mlat = gpsLocationService.latitude + Random.Range(-latitudeSpawnOffset, latitudeSpawnOffset);
                var mlon = gpsLocationService.longitude + Random.Range(-longitudeSpawnOffset, longitudeSpawnOffset);
                var monster = new Monster
                {
                    location = new MapLocation(mlon, mlat),
                    spawnTimestamp = gpsLocationService.timeStamp
                };
                monsters.Add(monster);
            }

            //store players location for easy access in distance calculations
            var playerLocation = new MapLocation(gpsLocationService.longitude, gpsLocationService.latitude);
            //get the current Epoch time in seconds
            var now = Epoch.Now;

            foreach (Monster m in monsters)
            {
                var d = MathG.Distance(m.location, playerLocation);
                if (MathG.Distance(m.location, playerLocation) < monsterSeeDistance)
                {
                    m.lastSeenTimestamp = now;
                    m.footstepRange = 4;
                    if (m.gameObject == null)
                    {
                        print("Monster seen, distance " + d + " started at " + m.spawnTimestamp);
                        SpawnMonster(m);
                    }
                    else
                    {
                        m.gameObject.SetActive(true);  //make sure the monster is visible
                    }
                    continue;
                }

                if (MathG.Distance(m.location, playerLocation) < monsterHearDistance)
                {
                    m.lastHeardTimestamp = now;
                    var footsteps = CalculateFootstepRange(d);
                    m.footstepRange = footsteps;
                    print("Monster heard, footsteps " + footsteps);
                }

                //hide monsters that can't be seen 
                if (m.gameObject != null)
                {
                    m.gameObject.SetActive(false);
                }
            }
        }

        private int CalculateFootstepRange(float distance)
        {
            if (distance < oneStepRange) return 1;
            if (distance < twoStepRange) return 2;
            if (distance < threeStepRange) return 3;
            return 4;
        }

        private Vector3 ConvertToWorldSpace(float longitude, float latitude)
        {
            //convert GPS lat/long to world x/y 
            var x = ((GoogleMapUtils.LonToX(longitude)
                - gpsLocationService.mapWorldCenter.x) * gpsLocationService.mapScale.x);
            var y = (GoogleMapUtils.LatToY(latitude)
                - gpsLocationService.mapWorldCenter.y) * gpsLocationService.mapScale.y;
            return new Vector3(-x, 0, y);
        }

        private void SpawnMonster(Monster monster)
        {
            var lon = monster.location.longitude;
            var lat = monster.location.latitude;
            var position = ConvertToWorldSpace((float)lon, (float)lat);
            var rotation = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up);
            monster.gameObject = (GameObject)Instantiate(monsterPrefab, position, rotation);
        }
    }
}