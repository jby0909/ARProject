using FoodyGo.Database;
using FoodyGo.Services;
using UnityEngine;
using UnityEngine.UI;

namespace FoodyGo.UI
{
    public class UI_MonsterTracker : MonoBehaviour
    {
        [SerializeField] MonsterService _monsterService;
        [SerializeField] Sprite[] _pawIcons;
        [SerializeField] Image _paw;

        private void Update()
        {
            RefreshPaw();
        }

        void RefreshPaw()
        {
            int minFootStep = 4;

            foreach(Monster monster in _monsterService.monsters)
            {
                minFootStep = Mathf.Min(monster.footstepRange, minFootStep);
            }

            switch(minFootStep)
            {
                case 1: 
                    _paw.sprite = _pawIcons[0];
                    break;
                case 2:
                    _paw.sprite = _pawIcons[1];
                    break;
                case 3:
                    _paw.sprite = _pawIcons[2];
                    break;
                default:
                    _paw.sprite = null;
                    break;
            }

            _paw.enabled = minFootStep >= 1 && minFootStep <= 3;
        }
    }
}

