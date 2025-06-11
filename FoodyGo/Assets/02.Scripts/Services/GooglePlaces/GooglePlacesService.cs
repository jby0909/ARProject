using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace FoodyGo.Services
{
    public class GooglePlacesService : MonoBehaviour
    {
        private const string BASE_URL = "https://places.googleapis.com/v1/places:searchNearby";
        private const string API_KEY = "AIzaSyBw1Z13PQ5FxJ3slO01PBZhvoPiRp3bEHc";

        #region SearchNearby ¿äÃ» ¸ðµ¨
        
        [Flags]
        public enum PlacesFields
        {
            None                = 0 << 0,
            DisplayName         = 1 << 0,
            FormattedAddress    = 1 << 1,
            Types               = 1 << 2,
            Location            = 1 << 3
        }

        [Serializable]
        class NearbyRequest
        {
            public List<string> includedTypes;
            public List<string> excludedTypes;
            public List<string> includedPrimaryTypes;
            public List<string> excludedPrimaryTypes;
            public string languageCode;
            public string regionCode;
            public int maxResultCount;
            public string rankPreference;
            public LocationRestriction locationRestriction;
        }

        [Serializable]
        class LocationRestriction
        {
            public Circle circle;
        }

        [Serializable]
        class Circle
        {
            public Center center;
            public float radius;
        }

        [Serializable]
        class Center
        {
            public double latitude;
            public double longitude;
        }
        #endregion

        #region SearchNearby ÀÀ´ä ¸ðµ¨
        [Serializable]
        class NearByResponse
        {
            public List<Place> places;
        }

        [Serializable]
        class Place
        {
            public List<string> types;
            public string formattedAddress;
            public Location location;
            public string websiteUri;
            public DisplayName displayName;
        }

        [Serializable]
        class DisplayName
        {
            public string text;
            public string languageCode;
        }

        [Serializable]
        class Location
        {
            public double latitude;
            public double longitude;
        }

        #endregion


        public void SerachNearbyRequest(double latitude, 
                                        double longitude, 
                                        float radius,
                                        List<string> includedTypes, 
                                        List<string> excludedTypes, 
                                        List<string> includedPrimaryTypes, 
                                        List<string> excludedPrimaryTypes,
                                        int maxResultCount, 
                                        string rankPreference, 
                                        string languageCode, 
                                        string regionCode, 
                                        PlacesFields fieldMask,
                                        Action<IEnumerable<(string name, double latitude, double longitude)>> completed)
        {
            StartCoroutine(C_SearchNearbyRequest(latitude, longitude, radius, includedTypes, excludedTypes, includedPrimaryTypes, excludedPrimaryTypes,
                                                maxResultCount, rankPreference, languageCode, regionCode, fieldMask, completed));
        }

        IEnumerator C_SearchNearbyRequest(double latitude, 
                                          double longitude, 
                                          float radius, 
                                          List<string> includedTypes, 
                                          List<string> excludedTypes, 
                                          List<string> includedPrimaryTypes, 
                                          List<string> excludedPrimaryTypes, 
                                          int maxResultCount, 
                                          string rankPreference, 
                                          string languageCode, 
                                          string regionCode, 
                                          PlacesFields fieldMask,
                                          Action<IEnumerable<(string name, double latitude, double longitude)>> completed)
        {
            NearbyRequest request = new NearbyRequest
            {
                includedTypes = includedTypes,
                excludedTypes = excludedTypes,
                includedPrimaryTypes = includedPrimaryTypes,
                excludedPrimaryTypes = excludedPrimaryTypes,
                maxResultCount = maxResultCount,
                rankPreference = rankPreference,
                languageCode = languageCode,
                regionCode = regionCode,
                locationRestriction = new LocationRestriction
                {
                    circle = new Circle
                    {
                        center = new Center
                        {
                            latitude = latitude,
                            longitude = longitude,
                        },
                        radius = radius,
                    }
                },
            };

            string jsonBody = JsonUtility.ToJson(request);
            UnityWebRequest webRequest = new UnityWebRequest(BASE_URL, UnityWebRequest.kHttpVerbPOST)
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody)),
                downloadHandler = new DownloadHandlerBuffer()
            };

            string fieldMaskHeader =
            string.Join(",",
            Enum.GetValues(typeof(PlacesFields))
                .Cast<PlacesFields>()
                .Where(f => f != PlacesFields.None && fieldMask.HasFlag(f))
                .Select(f =>
                {
                    string name = f.ToString();
                    string camel = char.ToLower(name[0]) + name.Substring(1);
                    return $"places.{camel}";
                })
            );

            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("X-Goog-Api-Key", API_KEY);
            webRequest.SetRequestHeader("X-Goog-FieldMask", fieldMaskHeader);

            yield return webRequest.SendWebRequest();

            string responseJson = webRequest.downloadHandler.text;
            Debug.Log($"{nameof(GooglePlacesService)} : SerachNearby responded {responseJson}");
            NearByResponse response = JsonUtility.FromJson<NearByResponse>(responseJson);
            
            
            
            completed?.Invoke(response.places
                .Select(place => (place.displayName.text, place.location.latitude, place.location.longitude)));

        }
    }
}

