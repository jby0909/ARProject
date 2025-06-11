using TMPro;
using UnityEngine;

namespace FoodyGo.Controllers
{
    public class PlaceMarkerController : MonoBehaviour
    {
        [SerializeField] TMP_Text[] _placeNames;

        /// <summary>
        /// 추가 컨텐츠 필요하면 파라미터로 다른 정보 다 받아서 띄우기
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

