using TMPro;
using UnityEngine;

namespace FoodyGo.Controllers
{
    public class PlaceMarkerController : MonoBehaviour
    {
        [SerializeField] TMP_Text[] _placeNames;

        /// <summary>
        /// �߰� ������ �ʿ��ϸ� �Ķ���ͷ� �ٸ� ���� �� �޾Ƽ� ����
        /// </summary>
        public void RefreshPlace(string placeName)
        {
            for (int i = 0; i < _placeNames.Length; i++)
            {
                _placeNames[i].text = placeName;
            }
        }
    }
}

