/*using System.Collections;
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
    public AbstractMap map; // Reference à la carte Mapbox
    private Vector3 previousPosition;
    private Vector3 actualPosition;
    private float distance;
    private Vector2d latLong;
    private Vector2d latLong1;

    private Vector2d actualPosition_latlong;

    private string jsonContent;
    private string jsonContent2;
    private WeatherData weatherData = new WeatherData();
    private WeatherData weatherData1 = new WeatherData();

    // fonction du calcul de la distance en km entre deux
    // avec les coordonnees longitude et latitude
    private double distanceHaversine(Vector2d point1, Vector2d point2)
    {
        double dLat = Deg2Rad(point2.x - point1.x);
        double dLon = Deg2Rad(point2.y - point1.y);

        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(Deg2Rad(point1.x)) * Math.Cos(Deg2Rad(point2.x)) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        double distance = 6371 * c; // Rayon de la terre en km
        return distance;
    }

    private double Deg2Rad(double deg)
    {
        return deg * (Math.PI / 180);
    }

    private void Start()
    {
        previousPosition = transform.position;
        while (previousPosition == null)
        {
            //on obtient la position initiale
            previousPosition = transform.position;
        }*/
        /*latLong1 = map.WorldToGeoPosition(previousPosition);

        string latitude1 = latLong1.y.ToString();
        string longitude1 = latLong1.x.ToString();
            
        string latitudePoint1 = latitude1.Replace(",", ".");
        string longitudePoint1 = longitude1.Replace(",", ".");

        string Url = "https://api.open-meteo.com/v1/forecast?latitude="+latitudePoint1+"&longitude="+longitudePoint1+"&current=wind_speed_10m,wind_direction_10m&hourly=wind_speed_10m,wind_speed_80m,wind_speed_120m,wind_speed_180m,wind_direction_10m,wind_direction_80m,wind_direction_120m,wind_direction_180m&timezone=GMT&forecast_days=1";
        string apiResponse = callApi(Url);

        //deserialisation du resultat de l'api
        if (apiResponse != null)
        {
            // Enregistrement des données dans le fichier data.json
            File.WriteAllText("data.json", apiResponse);
            Debug.Log("Les données ont été enregistrées dans data.json");

            // Extraction des données dans une variable
            
            
        }
        else
        {
            Debug.LogError("Impossible de récupérer les données de l'API.");
        }*/
    //}

    /*private void Update()
    {
        // on s'assure que la carte est initialisee
        if (map == null)
        {
            Debug.LogError("La reference a la carte Mapbox n'est pas definie.");
            return;
        }
        else
        {
            actualPosition = transform.position;
            while (latLong == null)
            {
                // position actuelle du GameObject
                Vector3 actualPosition = transform.position;
            }

            // Conversion de la position en coordonnees de latitude et longitude
            latLong = map.WorldToGeoPosition(actualPosition);

            // affichage des coordonnees 
            //Debug.Log("Position du GameObject: " + latLong.x + ", " + latLong.y);


            jsonContent = File.ReadAllText("data.json");
            WeatherData weatherData = JsonConvert.DeserializeObject<WeatherData>(jsonContent);
            //calcul de la direction en fonction de l'angle fournit par l'Api

            while (distanceHaversine(latLong, actualPosition_latlong) < 8)
            {

                //latLong = map.WorldToGeoPosition(actualPosition);
                Vector3 dir = new Vector3(Mathf.Cos(Mathf.Deg2Rad * weatherData.current.wind_direction_10m), 0f, Mathf.Sin(Mathf.Deg2Rad * weatherData.current.wind_direction_10m));
                Debug.Log("Nouvelle direction 1: " + weatherData.current.wind_direction_10m);
                transform.Translate(dir * 0.005f * Time.deltaTime, Space.Self);
                actualPosition_latlong = map.WorldToGeoPosition(transform.position);
            }

        }*/

        /*if (distanceHaversine(previousPosition, transform.position) > 1000)
        {
            actualPosition = transform.position;
            Vector2d latLong2 = map.WorldToGeoPosition(actualPosition);

            string latitude2 = latLong2.y.ToString();
            string longitude2 = latLong2.x.ToString();

            string latitudePoint2 = latitude2.Replace(",", ".");
            string longitudePoint2 = longitude2.Replace(",", ".");

            string Url = "https://api.open-meteo.com/v1/forecast?latitude="+latitudePoint2+"&longitude="+longitudePoint2+"&current=wind_speed_10m,wind_direction_10m&hourly=wind_speed_10m,wind_speed_80m,wind_speed_120m,wind_speed_180m,wind_direction_10m,wind_direction_80m,wind_direction_120m,wind_direction_180m&timezone=GMT&forecast_days=1";
            string apiResponse = callApi(Url);
            if (apiResponse != null)
            {
                // Enregistrement des données dans le fichier data.json
                File.WriteAllText("data1.json", apiResponse);
                Debug.Log("Les données ont été enregistrées dans data1.json");

                // Extraction des données dans une variable
                jsonContent = File.ReadAllText("data1.json");
                Debug.Log("Données extraites avec succès :"+ jsonContent);
            }
            else
            {
                Debug.LogError("Impossible de récupérer les données de l'API.");
            }       
        }*/
       /* if (distanceHaversine(latLong, map.WorldToGeoPosition(transform.position)) > 8)
        {
            jsonContent2 = File.ReadAllText("data1.json");
            WeatherData weatherData1 = JsonConvert.DeserializeObject<WeatherData>(jsonContent2);
            //calcul de la direction en fonction de l'angle fournit par l'Api
            previousPosition = actualPosition;
            actualPosition = new Vector3(0f, 0f, 0f);

            while (distanceHaversine(latLong, actualPosition_latlong) < 8)
            {

                Vector3 dir1 = new Vector3(Mathf.Cos(Mathf.Deg2Rad * weatherData1.current.wind_direction_10m), 0f, Mathf.Sin(Mathf.Deg2Rad * weatherData1.current.wind_direction_10m));
                Debug.Log("Nouvelle direction 2: " + weatherData1.current.wind_direction_10m);
                transform.Translate(dir1 * 0.005f * Time.deltaTime, Space.Self);
                actualPosition_latlong = map.WorldToGeoPosition(transform.position);
            }

            latLong = new Vector2d(0, 0);
        }
        else
        {
            Debug.Log("NONE");    
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
    public float elevation;
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
}*/