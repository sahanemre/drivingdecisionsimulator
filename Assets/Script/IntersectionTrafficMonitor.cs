using UnityEngine;
using System.Collections.Generic;
using VehiclePhysics.UI;

public class IntersectionTrafficMonitor : MonoBehaviour
{
    [Header("Ayarlar")]
    [Tooltip("Bu Kavşak Trigger'ının kendi etiketi. (Örn: IntersectionTrigger)")]
    public string selfTag = "IntersectionTrigger"; // Bu GameObject'in kendi etiketi
    [Tooltip("Sadece belirli bir etikete sahip objelerin sayılmasını istiyorsanız etiketi buraya girin. Boş bırakırsanız tüm tetiklenen objeler sayılır.")]
    public string targetVehicleTag = "Vehicle"; // Sayılacak araçların etiketi. Genellikle aracınızın "Vehicle" veya "Car" gibi bir etiketi olmalı.

    [Header("İstatistikler")]
    public int totalCarsPassed = 0; // Bu kavşaktan toplam geçen araç sayısı (yarış boyunca)
    private List<float> lastPassTimes = new List<float>(); // Geçen araçların son X saniyedeki geçiş zamanları
    public float carsPerSecond = 0f; // Saniyede ortalama geçen araç sayısı (son X saniye üzerinden)
    private List<float> passedCarSpeeds = new List<float>(); // Geçen araçların hızlarını kaydetmek için
    public float averageSpeedOfPassedCars = 0f; // Ortalama geçen araç hızı

    [Tooltip("Kavşak yoğunluğu hesaplaması için kullanılacak zaman penceresi (saniye).")]
    public float timeWindowForCPS = 10f; // Cars Per Second için zaman penceresi

    private bool isMonitoringActive = false; // Yarış durumu kontrolü

    void OnEnable()
    {
        // Yarış başlangıcı ve bitiş olaylarına abone ol
        SimulationInfoCollector.OnRaceStarted += OnRaceStarted;
        SimulationInfoCollector.OnRaceFinished += OnRaceFinished;
    }

    void OnDisable()
    {
        // Script devre dışı bırakıldığında abonelikleri kaldır
        SimulationInfoCollector.OnRaceStarted -= OnRaceStarted;
        SimulationInfoCollector.OnRaceFinished -= OnRaceFinished;
    }

    void Start()
    {
        // Script'in bağlı olduğu GameObject'in bir Collider'ı ve Is Trigger özelliği açık mı kontrol et
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError("IntersectionTrafficMonitor script'i bir Collider'a sahip GameObject üzerinde olmalı!");
            enabled = false; // Scripti devre dışı bırak
            return;
        }
        if (!col.isTrigger)
        {
            Debug.LogWarning("IntersectionTrafficMonitor script'inin bağlı olduğu Collider'ın 'Is Trigger' özelliği açık olmalı!");
        }
        // Kendi etiketimizin doğru ayarlandığından emin olalım
        if (!gameObject.CompareTag(selfTag))
        {
             Debug.LogWarning($"IntersectionTrafficMonitor: GameObject'in etiketi '{selfTag}' olarak ayarlanmalı. Şu anki etiket: '{gameObject.tag}'");
        }
    }

    void Update()
    {
        if (!isMonitoringActive) return; // Yarış başlamadıysa veya bittiyse güncelleme yapma

        // Eski girişleri temizle
        CleanUpOldPassTimes(timeWindowForCPS);

        // Saniyede geçen araç sayısını hesapla
        CalculateCarsPerSecond();

        // Ortalama hızı hesapla
        CalculateAverageSpeed();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isMonitoringActive) return; // Yarış aktif değilse sayma

        // Eğer hedef etiket belirlenmişse ve çarpan objenin etiketi uyuşmuyorsa, çık
        if (!string.IsNullOrEmpty(targetVehicleTag) && !other.CompareTag(targetVehicleTag))
        {
            return;
        }

        totalCarsPassed++;
        lastPassTimes.Add(Time.time); // Geçiş zamanını kaydet
        // Debug.Log($"{other.gameObject.name} kavşaktan geçti! Toplam Geçen: {totalCarsPassed}"); // Gürültüyü azaltmak için kapatıldı

        // Geçen aracın hızını almaya çalış
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            float currentSpeed = rb.velocity.magnitude; // Hızı metre/saniye cinsinden al
            passedCarSpeeds.Add(currentSpeed);
            // Debug.Log($"Geçen aracın hızı: {currentSpeed:F2} m/s"); // Gürültüyü azaltmak için kapatıldı
        }
    }

    // --- Olay Dinleyicileri ---
    private void OnRaceStarted()
    {
        isMonitoringActive = true;
        // Yarış başladığında istatistikleri sıfırla
        totalCarsPassed = 0;
        lastPassTimes.Clear();
        passedCarSpeeds.Clear();
        carsPerSecond = 0f;
        averageSpeedOfPassedCars = 0f;
        Debug.Log("Intersection Monitoring Started!");
    }

    private void OnRaceFinished()
    {
        isMonitoringActive = false;
        // Yarış bittiğinde son anlık istatistikleri güncelle ve raporla
        CalculateCarsPerSecond();
        CalculateAverageSpeed();

        string resultText = $"Kavşak İzleme Sonuçları:\n" +
                            $"Toplam Geçen Araç: {totalCarsPassed}\n" +
                            $"Saniyede Ort. Araç Geçişi (son anlık): {carsPerSecond:F2}\n" +
                            $"Kavşaktan Geçen Araçların Ort. Hızı (son anlık): {averageSpeedOfPassedCars:F2} m/s";
        Debug.Log(resultText);

        Debug.Log("Intersection Monitoring Finished!");
    }

    // --- Yardımcı Hesaplama Metodları ---
    private void CleanUpOldPassTimes(float windowInSeconds)
    {
        float currentTime = Time.time;
        for (int i = lastPassTimes.Count - 1; i >= 0; i--)
        {
            if (currentTime - lastPassTimes[i] > windowInSeconds)
            {
                lastPassTimes.RemoveAt(i);
                if (passedCarSpeeds.Count > i)
                {
                    passedCarSpeeds.RemoveAt(i);
                }
            }
        }
    }

    private void CalculateCarsPerSecond()
    {
        if (lastPassTimes.Count > 0)
        {
            float oldestTime = lastPassTimes[0];
            float newestTime = Time.time;
            float timeWindow = newestTime - oldestTime;

            if (timeWindow > 0)
            {
                carsPerSecond = lastPassTimes.Count / timeWindow;
            }
            else
            {
                carsPerSecond = lastPassTimes.Count;
            }
        }
        else
        {
            carsPerSecond = 0f;
        }
    }

    private void CalculateAverageSpeed()
    {
        if (passedCarSpeeds.Count > 0)
        {
            float totalSpeed = 0f;
            foreach (float speed in passedCarSpeeds)
            {
                totalSpeed += speed;
            }
            averageSpeedOfPassedCars = totalSpeed / passedCarSpeeds.Count;
        }
        else
        {
            averageSpeedOfPassedCars = 0f;
        }
    }

    // Dışarıdan erişim için Public Metodlar
    public int GetTotalCarsPassed() { return totalCarsPassed; }
    public float GetCarsPerSecond() { return carsPerSecond; }
    public float GetAverageSpeedOfPassedCars() { return averageSpeedOfPassedCars; }
}