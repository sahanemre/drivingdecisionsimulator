using System; // Action kullanmak için
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VehiclePhysics.UI;

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
        public Dashboard dashboard;
        public InputMonitor inputMonitor;

        [Header("Pedal Sayaçları")]
        public int gasPedalCounter = 0;
        public int brakePedalCounter = 0;

        // Özel değişkenler
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
        private Vector3 startPosition;
        private Vector3 finishPosition;
        private float totalDistance = 0f;

        // Yarış bitişinde gösterilecek sonuçlar
        [Header("Yarış Sonuçları")]
        public string raceResultText = "";
        public float finalRaceTime = 0f;
        public int finalGasPedalCount = 0;
        public int finalBrakePedalCount = 0;
        public float finalAverageSpeed = 0f;

        void Update()
        {
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
                    // Debug.Log("Gas Pedal Pressed. Count: " + gasPedalCounter); // Gürültüyü azaltmak için kapatıldı

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
                    // Debug.Log("Brake Pedal Pressed. Count: " + brakePedalCounter); // Gürültüyü azaltmak için kapatıldı
                }
            }
            else
            {
                brakePressed = false;
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
                    startPosition = transform.position;
                    Debug.Log("Race Timer Started at: " + raceStartTime + " seconds. Start Position: " + startPosition);

                    // Sayaçları ve sonuçları sıfırla
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

                    // Yarışın başladığını bildiren olayı fırlat
                    OnRaceStarted?.Invoke(); // Abone olan tüm metodları çağırır
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
                    finishPosition = transform.position;

                    totalRaceTime = raceEndTime - raceStartTime;
                    totalDistance = Vector3.Distance(startPosition, finishPosition);

                    if (totalRaceTime > 0)
                    {
                        finalAverageSpeed = totalDistance / totalRaceTime;
                    }
                    else
                    {
                        finalAverageSpeed = 0f;
                    }

                    Debug.Log("Race Finished! Total Race Time: " + totalRaceTime + " seconds. Finish Position: " + finishPosition);
                    Debug.Log("Total Distance: " + totalDistance + " meters.");
                    Debug.Log("Average Speed: " + finalAverageSpeed + " m/s");

                    // Yarış sonuçlarını kaydet
                    finalRaceTime = totalRaceTime;
                    finalGasPedalCount = gasPedalCounter;
                    finalBrakePedalCount = brakePedalCounter;

                    // Sonuç mesajını oluştur
                    raceResultText = $"Yarış Tamamlandı!\n" +
                                     $"Toplam Süre: {finalRaceTime:F2} saniye\n" +
                                     $"Kat Edilen Mesafe: {totalDistance:F2} metre\n" +
                                     $"Ortalama Hız: {finalAverageSpeed:F2} m/s\n" +
                                     $"Gaz Pedalı Basılma Sayısı: {finalGasPedalCount}\n" +
                                     $"Fren Pedalı Basılma Sayısı: {finalBrakePedalCount}";


                    Debug.Log(raceResultText);

                    raceTimerStarted = false;

                    // Yarışın bittiğini bildiren olayı fırlat
                    OnRaceFinished?.Invoke(); // Abone olan tüm metodları çağırır
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
    }
}