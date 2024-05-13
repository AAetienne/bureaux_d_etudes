using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using Mapbox.Unity.Map;
using Mapbox.Utils;

public class userMoove : MonoBehaviour
{
    public AbstractMap map;
    private Vector3 previousPosition;
    private Vector2d previousPositionLatLong;
    private string jsonContent;
    private Vector3 direction;
    private Vector3 newDirection;

    private WeatherData weatherData;

    private float heightAboveMap = 10f; // Hauteur initiale au-dessus de la carte

    private void Start()
    {
        bool response = true;

        while(response)
        {
            previousPosition = transform.position;
            previousPositionLatLong = map.WorldToGeoPosition(previousPosition);
            Debug.Log(previousPositionLatLong);

            //ici je remplace le point se trouvant dans les coordonnées par une virgule
            string latitudeCurrentPoint = previousPositionLatLong.y.ToString();
            string longitudeCurrentPoint = previousPositionLatLong.x.ToString();

            string latitudePoint = latitudeCurrentPoint.Replace(",", ".");
            string longitudePoint = longitudeCurrentPoint.Replace(",", ".");

            Debug.Log("Appel de l'api");

            // Appeler l'API pour obtenir les données météorologiques
            string apiUrl = "https://api.open-meteo.com/v1/forecast?latitude=" + latitudePoint +
                            "&longitude=" + longitudePoint +
                            "&current=wind_speed_10m,wind_direction_10m" +
                            "&hourly=wind_speed_10m,wind_speed_80m,wind_speed_120m,wind_speed_180m," +
                            "wind_direction_10m,wind_direction_80m,wind_direction_120m,wind_direction_180m" +
                            "&timezone=GMT&forecast_days=1";

            string apiResponse = callApi(apiUrl);

            if (!string.IsNullOrEmpty(apiResponse))
            {
                // Traiter les données météorologiques
                weatherData = JsonConvert.DeserializeObject<WeatherData>(apiResponse);

                // Calculer la nouvelle direction en fonction des données météorologiques
                direction = CalculateDirectionFromWeather(weatherData);
                Debug.Log("Start direction: " + direction);
                response = false;
            }
            else
            {
                response = true;
                Debug.LogError("Impossible de récupérer les données météorologiques de l'API.");
            }
        }
    }

    private void Update()
    {
        Vector3 currentPosition = transform.position;
        Vector2d currentPositionLatLong = map.WorldToGeoPosition(currentPosition);
        Debug.Log(currentPositionLatLong);

        // Calculer la distance entre les positions actuelle et précédente en km
        double distanceTravelled = distanceHaversine(previousPosition, currentPosition);
        Debug.Log("Distance égale :" + distanceTravelled);


        // Vérifier si la distance parcourue dépasse 8 km
        if (distanceTravelled >= (double)0.5)
        {
            //ici je remplace le point se trouvant dans les coordonnées par une virgule
            string latitudeCurrentPoint = currentPositionLatLong.y.ToString();
            string longitudeCurrentPoint = currentPositionLatLong.x.ToString();

            string latitudePoint = latitudeCurrentPoint.Replace(",", ".");
            string longitudePoint = longitudeCurrentPoint.Replace(",", ".");

            Debug.Log("Appel de l'api");

            // Appeler l'API pour obtenir les données météorologiques
            string apiUrl = "https://api.open-meteo.com/v1/forecast?latitude=" + latitudePoint +
                            "&longitude=" + longitudePoint +
                            "&current=wind_speed_10m,wind_direction_10m" +
                            "&hourly=wind_speed_10m,wind_speed_80m,wind_speed_120m,wind_speed_180m," +
                            "wind_direction_10m,wind_direction_80m,wind_direction_120m,wind_direction_180m" +
                            "&timezone=GMT&forecast_days=1";

            string apiResponse = callApi(apiUrl);

            if (!string.IsNullOrEmpty(apiResponse))
            {
                // Traiter les données météorologiques
                weatherData = JsonConvert.DeserializeObject<WeatherData>(apiResponse);

                // Calculer la nouvelle direction en fonction des données météorologiques
                direction = CalculateDirectionFromWeather(weatherData);

                // Mettre à jour la position et les coordonnées lat/long précédentes
                previousPosition = currentPosition;
                previousPositionLatLong = currentPositionLatLong;
            }
            else
            {
                Debug.LogError("Impossible de récupérer les données météorologiques de l'API.");
            }

            // Appliquer la nouvelle direction au mouvement du joueur
            transform.Translate(direction * Time.deltaTime * 2f, Space.Self);

            currentPositionLatLong = new Vector2d(0, 0);
        }
        else
        {
            transform.Translate(direction * Time.deltaTime * 2f, Space.Self);

            // Déplacement horizontal
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            Vector3 moveDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;
            Vector3 newPosition = transform.position + moveDirection * Time.deltaTime * 2f;

            if (Input.GetKey(KeyCode.UpArrow))
            {
                heightAboveMap += Time.deltaTime * 2f; // Augmente la hauteur
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                heightAboveMap -= Time.deltaTime * 2f; // Diminue la hauteur
                heightAboveMap = Mathf.Max(heightAboveMap, 10f); // Limite la hauteur minimale
            }

            newPosition.y = heightAboveMap;

            // Déplace le GameObject
            transform.position = newPosition;

            // Met à jour la position et les coordonnées lat/long précédentes
            //previousPosition = newPosition;
            //previousPositionLatLong = map.WorldToGeoPosition(newPosition);
        }
    }


    private double distanceHaversine(Vector3 point1, Vector3 point2)
    {
        Vector2d point1_2d = map.WorldToGeoPosition(point1);
        Vector2d point2_2d = map.WorldToGeoPosition(point2);

        double dLat = Mathf.Deg2Rad * (point2_2d.y - point1_2d.y);
        double dLon = Mathf.Deg2Rad * (point2_2d.x - point1_2d.x);

        double a = Mathf.Sin((float)(dLon / 2)) * Mathf.Sin((float)(dLon / 2)) +
                   Mathf.Cos((float)Mathf.Deg2Rad * (float)point1_2d.y) * Mathf.Cos((float)Mathf.Deg2Rad * (float)point2_2d.y) *
                   Mathf.Sin((float)(dLat / 2)) * Mathf.Sin((float)(dLat / 2));

        double c = 2 * Mathf.Atan2(Mathf.Sqrt((float)a), Mathf.Sqrt((float)(1 - a)));

        double distance = 6371 * c; // Rayon de la terre en km
        return distance;
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

    private Vector3 CalculateDirectionFromWeather(WeatherData weatherData)
    {
        // Calculer la direction en fonction des données météorologiques
        // À implémenter selon vos besoins spécifiques
        // Cet exemple suppose que la direction du vent est utilisée comme nouvelle direction
        float windDirectionDegrees = weatherData.current.wind_direction_10m;
        Vector3 direction = new Vector3(Mathf.Cos(Mathf.Deg2Rad * windDirectionDegrees), 0f, Mathf.Sin(Mathf.Deg2Rad * windDirectionDegrees));
        return direction;
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
}