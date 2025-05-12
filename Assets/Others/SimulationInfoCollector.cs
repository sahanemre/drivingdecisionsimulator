using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

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

        private bool raceTimerStarted = false;
        private float raceStartTime = 0f;
        private float raceEndTime = 0f;

        private float firstSpeed = 0f;
        private float avarageSpeed = 0f;
        private float speedSum = 0f;
        private int speedSampleCount = 0;

        private StreamWriter logWriter;

        private string logDirectory = "/Users/memresahan/SAHANET_2023/SAHANET_GAMES/drivingdecisionsimulator/Assets/SimulationLogs/";

        void Start()
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string logFilePath = Path.Combine(logDirectory, "SessionLog_" + timestamp + ".txt");

            try
            {
                // Dosya yazma işlemi
                logWriter = new StreamWriter(logFilePath, true);
                logWriter.WriteLine("[{0}] Simulation Session Started: {1}", timestamp, DateTime.Now);

                Debug.Log("Simulation Session Log Created: " + logFilePath);
            }
            catch (IOException ex)
            {
                Debug.LogError("Error opening log file: " + ex.Message);
            }
        }

        void Update()
        {
            if (!collisionStarted || collisionFinished) return;

            float throttleInput = GetThrottleInput();
            float brakeInput = GetBrakeInput();

            // GAZ kontrolü
            if (throttleInput > 0.1f)
            {
                if (!gasPressed)
                {
                    gasPedalCounter++;
                    gasPressed = true;
                    Log("Gas Pedal Pressed. Count: " + gasPedalCounter);

                    if (!firstGasTouch)
                    {
                        firstGasTouch = true;
                        Log("First Gas Pedal Touch Detected!");
                    }

                    if (!timerStarted)
                    {
                        timerStarted = true;
                        startTime = Time.time;
                        Log("Gas Pedal Press Duration Timer Started");
                    }
                }
            }
            else
            {
                if (gasPressed && timerStarted)
                {
                    float elapsedTime = Time.time - startTime;
                    Log("Gas Pedal Held For: " + elapsedTime.ToString("F2") + " seconds");
                    timerStarted = false;
                }

                gasPressed = false;
            }

            // FREN kontrolü
            if (brakeInput > 0.1f)
            {
                if (!brakePressed)
                {
                    brakePedalCounter++;
                    brakePressed = true;
                    Log("Brake Pedal Pressed. Count: " + brakePedalCounter);
                }
            }
            else
            {
                brakePressed = false;
            }

            // Hız örnekleme
            if (collisionStarted && !collisionFinished)
            {
                float currentSpeed = dashboard.speedMs * 3.6f;
                speedSum += currentSpeed;
                speedSampleCount++;
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("StartLine") && !collisionStarted)
            {
                collisionStarted = true;
                firstSpeed = dashboard.speedMs * 3.6f;
                Debug.Log("StartLine triggered!");
                Log("Start Line Triggered. First Speed: " + firstSpeed.ToString("F2") + " km/h");

                if (!raceTimerStarted)
                {
                    raceTimerStarted = true;
                    raceStartTime = Time.time;
                    Log("Race Timer Started");
                }
            }

            if (other.gameObject.CompareTag("FinishLine") && !collisionFinished)
            {
                collisionFinished = true;
                Log("Finish Line Triggered");

                if (raceTimerStarted)
                {
                    raceEndTime = Time.time;
                    float totalRaceTime = raceEndTime - raceStartTime;
                    Log("Race Finished! Total Race Time: " + totalRaceTime.ToString("F2") + " seconds");

                    if (speedSampleCount > 0)
                    {
                        avarageSpeed = speedSum / speedSampleCount;
                        Log("Average Speed: " + avarageSpeed.ToString("F2") + " km/h");
                    }

                    raceTimerStarted = false;
                }

                Log("Gas Pedal Press Count: " + gasPedalCounter);
                Log("Brake Pedal Press Count: " + brakePedalCounter);
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("TrafficVehicle"))
            {
                Log("Collision with Traffic Vehicle: " + collision.gameObject.name);
            }
        }

        void OnApplicationQuit()
        {
            Log("Simulation Session Ended: " + System.DateTime.Now);

            if (logWriter != null)
            {
                logWriter.Flush();
                logWriter.Close();
                logWriter = null;
            }

            Debug.Log("Log saved to: " + logDirectory);
        }

        void Log(string message)
        {
            string timeStampedMessage = "[" + System.DateTime.Now.ToString("HH:mm:ss") + "] " + message;
            Debug.Log(timeStampedMessage);

            if (logWriter != null)
                logWriter.WriteLine(timeStampedMessage);
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
