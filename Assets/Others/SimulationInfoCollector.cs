using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VehiclePhysics.UI;

namespace VehiclePhysics.UI
{
    public class SimulationInfoCollector : MonoBehaviour
    {
        public bool collisionFinished = false;
        public bool collisionStarted = false;

        public Dashboard dashboard;
        public InputMonitor inputMonitor;

        // Sayaçlar ve kontroller
        public int gasPedalCounter = 0;
        public int brakePedalCounter = 0;

        private bool gasPressed = false;
        private bool brakePressed = false;

        private bool firstGasTouch = false;
        private bool timerStarted = false;
        private float startTime = 0f;

        // --- Eklenenler: Genel yarış süresi için ---
        private bool raceTimerStarted = false;
        private float raceStartTime = 0f;
        private float raceEndTime = 0f;

        void Update()
        {
            if (!collisionStarted || collisionFinished) return;  // Yalnızca yarış başladıysa ve bitmediyse çalışsın

            float throttleInput = GetThrottleInput();
            float brakeInput = GetBrakeInput();

            // GAZ pedal kontrol
            if (throttleInput > 0.1f)
            {
                if (!gasPressed)
                {
                    gasPedalCounter++;
                    gasPressed = true;
                    Debug.Log("Gas Pedal Pressed. Count: " + gasPedalCounter);

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
                    Debug.Log("Brake Pedal Pressed. Count: " + brakePedalCounter);
                }
            }
            else
            {
                brakePressed = false;
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("StartLine") && !collisionStarted)
            {
                collisionStarted = true;
                Debug.Log("Collision Started");

                // Yarış zamanı başlat
                if (!raceTimerStarted)
                {
                    raceTimerStarted = true;
                    raceStartTime = Time.time;
                    Debug.Log("Race Timer Started at: " + raceStartTime + " seconds");
                }
            }

            if (other.gameObject.CompareTag("FinishLine") && !collisionFinished)
            {
                collisionFinished = true;
                Debug.Log("Collision Finished");

                if (raceTimerStarted)
                {
                    raceEndTime = Time.time;
                    float totalRaceTime = raceEndTime - raceStartTime;
                    Debug.Log("Race Finished! Total Race Time: " + totalRaceTime + " seconds");

                    raceTimerStarted = false;
                }
            }
        }

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
    }
}
