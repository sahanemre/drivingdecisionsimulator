using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VehiclePhysics.UI; // Dashboard script'i bu namespace içinde olduğu için gerekli

namespace VehiclePhysics.UI
{
    public class SimulationInfoCollector : MonoBehaviour
    {
        // Yarış başlangıcı ve bitişi için olaylar (Events)
        public static event Action OnRaceStarted;
        public static event Action OnRaceFinished;

        [Header("Yarış Durumu")]
        public bool collisionFinished = false;
        public bool collisionStarted = false;

        [Header("Bağlantılar")]
        public Dashboard dashboard; // Dashboard script'ine referans
        public InputMonitor inputMonitor;

        [Header("Pedal Sayaçları")]
        public int gasPedalCounter = 0;
        public int brakePedalCounter = 0;

        // Pedal kontrolü için özel değişkenler
        private bool gasPressed = false;
        private bool brakePressed = false;
        private bool firstGasTouch = false;
        private bool timerStarted = false;
        private float startTime = 0f;

        // Genel yarış süresi için
        private bool raceTimerStarted = false;
        private float raceStartTime = 0f;
        private float raceEndTime = 0f;
        private float totalRaceTime = 0f;

        // Ortalama hız için pozisyonlar
        // Artık hız için pozisyon farkına ihtiyaç duymuyoruz,
        // ancak toplam mesafeyi yine de başlangıç-bitiş pozisyonlarından hesaplayabiliriz.
        private Vector3 startPosition;
        private Vector3 finishPosition;
        private float totalDistance = 0f; // Toplam mesafe

        // Anlık hızın ortalaması için yeni bir liste
        private List<float> recordedSpeedsMs = new List<float>();


        // --- Kamera Kontrolleri ve Sayaçları ---
        [Header("Kamera Kontrolleri")]
        [Tooltip("Sol kameranın GameObject'ini buraya sürükleyin.")]
        public GameObject leftCameraObject; // Kameranın GameObject'ini tutar
        [Tooltip("Sağ kameranın GameObject'ini buraya sürükleyin.")]
        public GameObject rightCameraObject; // Kameranın GameObject'ini tutar

        [Tooltip("Z tuşuna basılma sayısı (Sol Kamera)")]
        public int leftCameraActivationCount = 0;
        [Tooltip("X tuşuna basılma sayısı (Sağ Kamera)")]
        public int rightCameraActivationCount = 0;

        private bool isLeftCameraActive = false;
        private bool isRightCameraActive = false;

        // Yarış bitişinde gösterilecek sonuçlar
        [Header("Yarış Sonuçları")]
        public string raceResultText = "";
        public float finalRaceTime = 0f;
        public int finalGasPedalCount = 0;
        public int finalBrakePedalCount = 0;
        public float finalAverageSpeed = 0f; // VPP speedMs üzerinden ortalama hız
        public int finalLeftCameraActivationCount = 0;
        public int finalRightCameraActivationCount = 0;


        void Awake()
        {
            // Başlangıçta kameraların pasif olduğundan emin ol
            if (leftCameraObject != null) leftCameraObject.SetActive(false);
            if (rightCameraObject != null) rightCameraObject.SetActive(false);
        }

        void Update()
        {
            // Yalnızca yarış başladıysa ve bitmediyse pedal ve kamera kontrollerini yap
            if (!collisionStarted || collisionFinished) return;

            float throttleInput = GetThrottleInput();
            float brakeInput = GetBrakeInput();

            // GAZ pedal kontrol
            if (throttleInput > 0.1f)
            {
                if (!gasPressed)
                {
                    gasPedalCounter++;
                    gasPressed = true;
                    if (!firstGasTouch)
                    {
                        firstGasTouch = true;
                        Debug.Log("First Gas Pedal Touch detected!");
                    }
                    if (!timerStarted)
                    {
                        timerStarted = true;
                        startTime = Time.time;
                        Debug.Log("Timer started at: " + startTime + " seconds");
                    }
                }
            }
            else
            {
                if (gasPressed && timerStarted)
                {
                    float elapsedTime = Time.time - startTime;
                    Debug.Log("Gas Pedal Pressed for " + elapsedTime + " seconds");
                    timerStarted = false;
                }
                gasPressed = false;
            }

            // FREN pedal kontrol
            if (brakeInput > 0.1f)
            {
                if (!brakePressed)
                {
                    brakePedalCounter++;
                    brakePressed = true;
                }
            }
            else
            {
                brakePressed = false;
            }

            // --- Kamera Kontrolleri ---
            HandleCameraActivation();

            // --- Anlık Hızı Kaydet ---
            if (dashboard != null && dashboard.isActiveAndEnabled)
            {
                recordedSpeedsMs.Add(dashboard.speedMs * 3.6f); // Dashboard'dan anlık hızı al ve listeye ekle
            }
        }

        void OnTriggerEnter(Collider other)
        {
            // --- Başlangıç Çizgisi ---
            if (other.gameObject.CompareTag("StartLine") && !collisionStarted)
            {
                collisionStarted = true;
                Debug.Log("Collision Started");

                // Yarış zamanı başlat
                if (!raceTimerStarted)
                {
                    raceTimerStarted = true;
                    raceStartTime = Time.time;
                    startPosition = transform.position; // Başlangıç pozisyonunu kaydet (toplam mesafe için hala gerekli)
                    Debug.Log("Race Timer Started at: " + raceStartTime + " seconds. Start Position: " + startPosition);

                    // Tüm sayaçları ve sonuçları sıfırla
                    gasPedalCounter = 0;
                    brakePedalCounter = 0;
                    firstGasTouch = false;
                    timerStarted = false;
                    raceResultText = "";
                    finalRaceTime = 0f;
                    finalGasPedalCount = 0;
                    finalBrakePedalCount = 0;
                    finalAverageSpeed = 0f;
                    totalDistance = 0f;

                    // Kamera sayaçlarını sıfırla
                    leftCameraActivationCount = 0;
                    rightCameraActivationCount = 0;
                    finalLeftCameraActivationCount = 0;
                    finalRightCameraActivationCount = 0;

                    // Hız kayıtlarını sıfırla
                    recordedSpeedsMs.Clear();

                    // Yarışın başladığını bildiren olayı fırlat
                    OnRaceStarted?.Invoke();
                    Debug.Log("<color=red>SimulationInfoCollector: OnRaceStarted event Fırlatıldı!</color>"); // Debug log
                }
            }

            // --- Bitiş Çizgisi ---
            if (other.gameObject.CompareTag("FinishLine") && !collisionFinished)
            {
                collisionFinished = true;
                Debug.Log("Collision Finished");

                if (raceTimerStarted)
                {
                    raceEndTime = Time.time;
                    finishPosition = transform.position; // Bitiş pozisyonunu kaydet (toplam mesafe için hala gerekli)

                    totalRaceTime = raceEndTime - raceStartTime;
                    totalDistance = Vector3.Distance(startPosition, finishPosition); // Başlangıç-bitiş pozisyonu mesafesi

                    // Ortalama hızı hesapla (Kaydedilen anlık hızların ortalaması)
                    if (recordedSpeedsMs.Count > 0)
                    {
                        float totalSpeedSum = 0f;
                        foreach (float speed in recordedSpeedsMs)
                        {
                            totalSpeedSum += speed;
                        }
                        finalAverageSpeed = totalSpeedSum / recordedSpeedsMs.Count;
                    }
                    else
                    {
                        finalAverageSpeed = 0f;
                    }

                    Debug.Log("Race Finished! Total Race Time: " + totalRaceTime + " seconds. Finish Position: " + finishPosition);
                    Debug.Log("Total Distance (Start-Finish): " + totalDistance + " meters.");
                    Debug.Log("Average Speed (VPP Data): " + finalAverageSpeed + " m/s");


                    // Yarış sonuçlarını kaydet
                    finalRaceTime = totalRaceTime;
                    finalGasPedalCount = gasPedalCounter;
                    finalBrakePedalCount = brakePedalCounter;
                    finalLeftCameraActivationCount = leftCameraActivationCount;
                    finalRightCameraActivationCount = rightCameraActivationCount;

                    // Sonuç mesajını oluştur
                    raceResultText = $"Yarış Tamamlandı!\n" +
                                     $"Toplam Süre: {finalRaceTime:F2} saniye\n" +
                                     $"Kat Edilen Mesafe (Start-Finish): {totalDistance:F2} metre\n" +
                                     $"Ortalama Hız (VPP): {finalAverageSpeed:F2} m/s\n" +
                                     $"Gaz Pedalı Basılma Sayısı: {finalGasPedalCount}\n" +
                                     $"Fren Pedalı Basılma Sayısı: {finalBrakePedalCount}\n" +
                                     $"Sol Kamera Aktivasyon Sayısı (Z): {finalLeftCameraActivationCount}\n" +
                                     $"Sağ Kamera Aktivasyon Sayısı (X): {finalRightCameraActivationCount}";

                    Debug.Log(raceResultText);

                    raceTimerStarted = false;

                    // Yarış bitince kameraları pasif yap
                    if (leftCameraObject != null) leftCameraObject.SetActive(false);
                    if (rightCameraObject != null) rightCameraObject.SetActive(false);

                    // Yarışın bittiğini bildiren olayı fırlat
                    OnRaceFinished?.Invoke();
                    Debug.Log("<color=red>SimulationInfoCollector: OnRaceFinished event Fırlatıldı!</color>"); // Debug log
                }
            }
        }

        // --- Kamera Aktivasyonunu Yöneten Metod ---
        private void HandleCameraActivation()
        {
            // Sol Kamera (Z tuşu)
            if (leftCameraObject != null)
            {
                if (Input.GetKeyDown(KeyCode.Z))
                {
                    if (!isLeftCameraActive)
                    {
                        leftCameraObject.SetActive(true);
                        isLeftCameraActive = true;
                        leftCameraActivationCount++;
                        Debug.Log("Sol Kamera Aktif. Sayı: " + leftCameraActivationCount);

                        if (rightCameraObject != null && rightCameraObject.activeSelf)
                        {
                            rightCameraObject.SetActive(false);
                            isRightCameraActive = false;
                        }
                    }
                }
                else if (Input.GetKeyUp(KeyCode.Z))
                {
                    if (isLeftCameraActive)
                    {
                        leftCameraObject.SetActive(false);
                        isLeftCameraActive = false;
                        Debug.Log("Sol Kamera Pasif.");
                    }
                }
            }

            // Sağ Kamera (X tuşu)
            if (rightCameraObject != null)
            {
                if (Input.GetKeyDown(KeyCode.X))
                {
                    if (!isRightCameraActive)
                    {
                        rightCameraObject.SetActive(true);
                        isRightCameraActive = true;
                        rightCameraActivationCount++;
                        Debug.Log("Sağ Kamera Aktif. Sayı: " + rightCameraActivationCount);

                        if (leftCameraObject != null && leftCameraObject.activeSelf)
                        {
                            leftCameraObject.SetActive(false);
                            isLeftCameraActive = false;
                        }
                    }
                }
                else if (Input.GetKeyUp(KeyCode.X))
                {
                    if (isRightCameraActive)
                    {
                        rightCameraObject.SetActive(false);
                        isRightCameraActive = false;
                        Debug.Log("Sağ Kamera Pasif.");
                    }
                }
            }
        }

        // Vehicle Physics Template'den giriş verilerini alma
        float GetThrottleInput()
        {
            if (inputMonitor != null && inputMonitor.vehicle != null)
            {
                int[] inputData = inputMonitor.vehicle.data.Get(Channel.Input);
                return inputData[InputData.Throttle] / 10000.0f;
            }
            return 0f;
        }

        float GetBrakeInput()
        {
            if (inputMonitor != null && inputMonitor.vehicle != null)
            {
                int[] inputData = inputMonitor.vehicle.data.Get(Channel.Input);
                return inputData[InputData.Brake] / 10000.0f;
            }
            return 0f;
        }

        // Yarış sonuçlarını dışarıdan erişilebilir yapmak için Public Metodlar
        public string GetRaceResultText() { return raceResultText; }
        public float GetFinalRaceTime() { return finalRaceTime; }
        public int GetFinalGasPedalCount() { return finalGasPedalCount; }
        public int GetFinalBrakePedalCount() { return finalBrakePedalCount; }
        public float GetFinalAverageSpeed() { return finalAverageSpeed; }
        public float GetTotalDistance() { return totalDistance; }
        public int GetFinalLeftCameraActivationCount() { return finalLeftCameraActivationCount; }
        public int GetFinalRightCameraActivationCount() { return finalRightCameraActivationCount; }
    }
}