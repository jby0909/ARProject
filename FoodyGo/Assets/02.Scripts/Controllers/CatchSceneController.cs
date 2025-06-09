using UnityEngine;
using FoodyGo.Database;

namespace FoodyGo.Controllers
{
    public class CatchSceneController : MonoBehaviour
    {
        public Transform frozenParticlePrefab;
        public MonsterController monster;
        public GameObject[] frozenDisableList;
        public GameObject[] frozenEnableList;

        public void OnMonsterHit(GameObject go, Collision collision)
        {
            monster = go.GetComponent<MonsterController>();
            if (monster != null)
            {
                print("Monster hit");
                var animSpeedReduction = Mathf.Sqrt(collision.relativeVelocity.magnitude) / 10;
                monster.animatorSpeed = Mathf.Clamp01(monster.animatorSpeed - animSpeedReduction);
                if (monster.animatorSpeed == 0)
                {
                    print("Monster FROZEN");

                    Instantiate(frozenParticlePrefab);

                    foreach (var g in frozenDisableList)
                    {
                        g.SetActive(false);
                    }
                    foreach (var g in frozenEnableList)
                    {
                        g.SetActive(true);
                    }
                }
            }
        }
    }
}