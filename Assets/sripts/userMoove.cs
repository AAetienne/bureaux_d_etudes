using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Mapbox.Unity.Map;
using Mapbox.Unity.Location;
using System.Globalization;

public class BalloonController : MonoBehaviour
{
    public AbstractMap map;
    public float altitude = 0f;
    public float altitudeStep = 10f;
    public float altitudeChangeSpeed = 2f;
    private Vector3 targetWindDirection;
    private float targetWindSpeed;
    private Vector3 currentWindDirection;
    private float currentWindSpeed;
    private string weatherApiKey = "YOUR_API_KEY";
    private LocationProviderFactory _locationProviderFactory;
    private ILocationProvider _locationProvider;

    private List<float> altitudes = new List<float> { 10f, 80f, 120f, 180f };
    private List<float> windSpeeds = new List<float>();
    private List<int> windDirections = new List<int>();

    // Camera variables
    public Transform cameraTransform;
    public Vector3 cameraOffset;
    public float cameraSmoothSpeed = 0.125f;

    // Balloon size
    private float objectHeight;

    // Target altitude
    private float targetAltitude;

    // Start and end points
    private Vector3 startPoint;
    private Vector3 endPoint;
    private bool startPointSelected = false;
    private bool endPointSelected = false;

    public GameObject startPointMarker;
    public GameObject endPointMarker;

    void Start()
    {
        //initialisation du centre de la carte à Brussels
        map.Initialize(new Mapbox.Utils.Vector2d(50.8503, 4.3517), 8);

        _locationProviderFactory = LocationProviderFactory.Instance;
        _locationProvider = _locationProviderFactory.DefaultLocationProvider;

        objectHeight = GetComponent<Renderer>().bounds.size.y;

        StartCoroutine(GetWindData());

        currentWindDirection = Vector3.forward;
        currentWindSpeed = 0f;

        targetAltitude = altitude;
    }

    void Update()
    {
        // Check for mouse click
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Vector3 point = hit.point;

                if (!startPointSelected)
                {
                    startPoint = point;
                    startPointSelected = true;
                    Instantiate(startPointMarker, point, Quaternion.identity);
                }
                else if (!endPointSelected)
                {
                    endPoint = point;
                    endPointSelected = true;
                    Instantiate(endPointMarker, point, Quaternion.identity);
                }
            }
        }

        if (startPointSelected && endPointSelected)
        {
            // Move the balloon from startPoint to endPoint
            MoveObjectTowardsEndPoint();
        }

        // Controls for ascending and descending
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            targetAltitude += altitudeStep;
            StartCoroutine(GetWindData());
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            targetAltitude = Mathf.Max(targetAltitude - altitudeStep, 0);
            StartCoroutine(GetWindData());
        }

        altitude = Mathf.Lerp(altitude, targetAltitude, Time.deltaTime * altitudeChangeSpeed);

        SmoothWindTransition();

        Vector3 displacement = currentWindDirection * currentWindSpeed * Time.deltaTime;

        Vector3 newPosition = transform.position + displacement;

        transform.Translate(displacement, Space.World);

        UpdateAltitudePosition();

        FollowObjectWithCamera();
    }

    void MoveObjectTowardsEndPoint()
    {
        Vector3 direction = (endPoint - transform.position).normalized;
        float speed = currentWindSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, endPoint, speed);

        if (Vector3.Distance(transform.position, endPoint) < 0.1f)
        {
            Debug.Log("Object has reached the destination.");
        }
    }

    void UpdateAltitudePosition()
    {
        Vector3 position = transform.position;
        position.y = Mathf.Max(altitude + objectHeight / 2, objectHeight / 2);
        transform.position = position;
    }

    IEnumerator GetWindData()
    {
        var location = _locationProvider.CurrentLocation.LatitudeLongitude;

        //permet de remplacer la virgule par un point dans les coordonnées retournées
        string latitude = location.x.ToString(CultureInfo.InvariantCulture);
        string longitude = location.y.ToString(CultureInfo.InvariantCulture);

        if (location.x < -90 || location.x > 90 || location.y < -180 || location.y > 180)
        {
            Debug.LogError("Invalid latitude or longitude values.");
            yield break;
        }

        string url = $"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&hourly=wind_speed_10m,wind_speed_80m,wind_speed_120m,wind_speed_180m,wind_direction_10m,wind_direction_80m,wind_direction_120m,wind_direction_180m&appid={weatherApiKey}&units=metric";

        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"Error: {request.error}");
        }
        else
        {
            string json = request.downloadHandler.text;

            WeatherData weatherData = JsonUtility.FromJson<WeatherData>(json);
            UpdateWindData(weatherData);

            Debug.Log($"Wind Speed: {targetWindSpeed}, Wind Direction: {targetWindDirection}");
        }
    }

    void UpdateWindData(WeatherData weatherData)
    {
        windSpeeds.Clear();
        windDirections.Clear();

        windSpeeds.Add(weatherData.current.wind_speed_10m);
        windDirections.Add(weatherData.current.wind_direction_10m);
        windSpeeds.Add(weatherData.hourly.wind_speed_80m[0]);
        windDirections.Add(weatherData.hourly.wind_direction_80m[0]);
        windSpeeds.Add(weatherData.hourly.wind_speed_120m[0]);
        windDirections.Add(weatherData.hourly.wind_direction_120m[0]);
        windSpeeds.Add(weatherData.hourly.wind_speed_180m[0]);
        windDirections.Add(weatherData.hourly.wind_direction_180m[0]);

        InterpolateWindData(targetAltitude);
    }

    void InterpolateWindData(float currentAltitude)
    {
        if (currentAltitude <= altitudes[0])
        {
            targetWindSpeed = windSpeeds[0];
            targetWindDirection = Quaternion.Euler(0, -windDirections[0], 0) * Vector3.forward;
        }
        else if (currentAltitude >= altitudes[altitudes.Count - 1])
        {
            targetWindSpeed = windSpeeds[windSpeeds.Count - 1];
            targetWindDirection = Quaternion.Euler(0, -windDirections[windDirections.Count - 1], 0) * Vector3.forward;
        }
        else
        {
            for (int i = 0; i < altitudes.Count - 1; i++)
            {
                if (currentAltitude > altitudes[i] && currentAltitude <= altitudes[i + 1])
                {
                    float t = (currentAltitude - altitudes[i]) / (altitudes[i + 1] - altitudes[i]);
                    targetWindSpeed = Mathf.Lerp(windSpeeds[i], windSpeeds[i + 1], t);
                    float windDirectionDegrees = Mathf.LerpAngle(windDirections[i], windDirections[i + 1], t);
                    targetWindDirection = Quaternion.Euler(0, -windDirectionDegrees, 0) * Vector3.forward;
                    break;
                }
            }
        }
    }

    void SmoothWindTransition()
    {
        float smoothTime = 0.5f;
        currentWindDirection = Vector3.Slerp(currentWindDirection, targetWindDirection, Time.deltaTime / smoothTime);
        currentWindSpeed = Mathf.Lerp(currentWindSpeed, targetWindSpeed, Time.deltaTime / smoothTime);
    }

    void FollowObjectWithCamera()
    {
        Vector3 desiredPosition = transform.position + cameraOffset;
        Vector3 smoothedPosition = Vector3.Lerp(cameraTransform.position, desiredPosition, cameraSmoothSpeed);
        cameraTransform.position = smoothedPosition;

        // Make the camera look in the direction the balloon is facing
        cameraTransform.rotation = Quaternion.LookRotation(transform.forward, Vector3.up);
    }
}

[System.Serializable]
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

[System.Serializable]
public class Current
{
    public string time;
    public int interval;
    public float wind_speed_10m;
    public int wind_direction_10m;
}

[System.Serializable]
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

[System.Serializable]
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
