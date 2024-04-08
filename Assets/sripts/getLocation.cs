using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks; 
using Mapbox.Unity.Map;
using Mapbox.Utils;


public class getLocation : MonoBehaviour
{
    public AbstractMap map; // Reference a la carte Mapbox
    private Vector3 previousPosition;
    private Vector3 actualPosition;
    private float distance;

    // fonction du calcul de la distance en km entre deux
    // avec les coordonnees longitude et latitude
    private float distanceHaversine(Vector3 point1, Vector3 point2)
    {
        float dLat = Mathf.Deg2Rad * (point2.x - point1.x);
        float dLon = Mathf.Deg2Rad * (point2.y - point1.y);

        float a = Mathf.Sin(dLat / 2) * Mathf.Sin(dLat / 2) +
                  Mathf.Cos(Mathf.Deg2Rad * point1.x) * Mathf.Cos(Mathf.Deg2Rad * point2.x) *
                  Mathf.Sin(dLon / 2) * Mathf.Sin(dLon / 2);

        float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));

        float distance = 6371 * c; //on multiplie par la rayon de la terre en km
        return distance;
    }

    private void Start()
    {
        while (previousPosition == null)
        {
            //on obtient la position initiale
            previousPosition = transform.position;
        }
    }

    private void Update()
    {
        // on s'assure que la carte est initialisee
        if (map == null)
        {
            Debug.LogError("La reference a la carte Mapbox n'est pas definie.");
            return;
        }
        else
        {
            // position actuelle du GameObject
            Vector3 actualPosition = transform.position;

            // Conversion de la position en coordonnees de latitude et longitude
            Vector2d latLong = map.WorldToGeoPosition(actualPosition);

            // affichage des coordonnees 
            Debug.Log("Position du GameObject: " + latLong.x + ", " + latLong.y);

            if(distanceHaversine(previousPosition, actualPosition) >= 10)
            {
                previousPosition = actualPosition;
                string latitude = latLong.y.ToString();
                string longitude = latLong.x.ToString();
            
                string latitudePoint = latitude.Replace(",", ".");
                string longitudePoint = longitude.Replace(",", ".");

                string Url = "https://api.open-meteo.com/v1/forecast?latitude="+latitudePoint+"&longitude="+longitudePoint+"&current=wind_speed_10m,wind_direction_10m&hourly=wind_speed_10m,wind_speed_80m,wind_speed_120m,wind_speed_180m,wind_direction_10m,wind_direction_80m,wind_direction_120m,wind_direction_180m&timezone=GMT&forecast_days=1";
                string apiResponse = callApi(Url);

                //deserialisation du resultat de l'api
                if (apiResponse != null)
                {
                    // Enregistrement des données dans le fichier data.json
                    File.WriteAllText("data.json", apiResponse);
                    Debug.Log("Les données ont été enregistrées dans data.json");

                    // Extraction des données dans une variable
                    dynamic extractedData = JsonConvert.DeserializeObject(apiResponse);
                    var data = extractedData.results;
                    Debug.Log("Données extraites avec succès :"+ data);
                }
                else
                {
                    Debug.LogError("Impossible de récupérer les données de l'API.");
                }
            }
        }
    }


    private string callApi(string URL)
    {
        try
        {
            WebClient client = new WebClient();
            return client.DownloadString(URL);
        }
        catch (WebException e)
        {
            Debug.Log("Erreur appel Api:" + e.Message);
            return null;
        }
    }
}

public class WeatherData
{
    public double latitude;
    public double longitude;
    public float generationtime_ms;
    public int utc_offset_seconds;
    public string timezone;
    public string timezone_abbreviation;
    public int elevation;
    public Units current_units;
    public Current current;
    public Units hourly_units;
    public Hourly hourly;
}

public class Units
{
    public string time;
    public string interval;
    public string wind_speed_10m;
    public string wind_speed_80m;
    public string wind_speed_120m;
    public string wind_speed_180m;
    public string wind_direction_10m;
    public string wind_direction_80m;
    public string wind_direction_120m;
    public string wind_direction_180m;
}

public class Current
{
    public string time;
    public int interval;
    public float wind_speed_10m;
    public int wind_direction_10m;
}

public class Hourly
{
    public List<string> time;
    public List<float> wind_speed_10m;
    public List<float> wind_speed_80m;
    public List<float> wind_speed_120m;
    public List<float> wind_speed_180m;
    public List<int> wind_direction_10m;
    public List<int> wind_direction_80m;
    public List<int> wind_direction_120m;
    public List<int> wind_direction_180m;
}