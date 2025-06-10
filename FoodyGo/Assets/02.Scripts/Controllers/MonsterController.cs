using UnityEngine;
using FoodyGo.Mapping;
using FoodyGo.Services;
using FoodyGo.Database;

namespace FoodyGo.Controllers
{
    public class MonsterController : MonoBehaviour
    {
        public float hp {
            get => _hp;
            set
            {
                _hp = value;
                animatorSpeed = _hp / maxHp;

                if(_hp <= 0)
                {
                    // TODO : 포획 성공로직
                    Destroy(gameObject);
                }
            }
        }
        public float maxHp { get; } = 100f;

        private float _hp;

        public MapLocation location;
        public MonsterService monsterService;
        public Monster monsterDataObject;
        public Animator animator;
        public float animatorSpeed;
        public float monsterWarmRate = .0001f;

        public void Damage(float amount)
        {
            hp -= amount;
        }

        private void Awake()
        {
            _hp = maxHp;
        }

        // Use this for initialization
        void Start()
        {
            animator = GetComponent<Animator>();
            animator.speed = animatorSpeed;
        }

        // Update is called once per frame
        void Update()
        {
            if (animatorSpeed == 0)
            {
                //monster is frozen solid
                animatorSpeed = 0;
                return;
            }
            //if monster is moving they will warm up a bit
            animatorSpeed = Mathf.Clamp01(animatorSpeed + monsterWarmRate);
            animator.speed = animatorSpeed;
        }


    }
}