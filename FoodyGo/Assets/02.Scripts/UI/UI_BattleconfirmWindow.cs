using FoodyGo.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace FoodyGo.UI
{
    public class UI_BattleconfirmWindow : UI_Base
    {
        [SerializeField] Button _confirm;
        [SerializeField] Button _cancel;

        private void Start()
        {
            _confirm.onClick.AddListener(() =>
            {
                GameManager.instance.ActiveAdditiveScene("Catch");
                Hide();
            });

            _cancel.onClick.AddListener(() =>
            {
                Hide();
            });
        }
    }
}
