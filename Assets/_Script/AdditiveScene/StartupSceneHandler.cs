using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityCommandLineParser;
using UnityEngine;
using UnityEngine.SceneManagement;
using WebSocketSharp;

namespace FXSceneEditor
{
    public class StartupSceneHandler : MonoBehaviour
    {
        [CommandLineArgument("ismainprocess", "Use this flag to force the build to run main server default(true)")]
        public static bool IsMainProcess = true;
        //public static bool IsMainProcess = false;

        [CommandLineArgument("isserver", "Use this flag to force the build to run like the server build default(false)")]
        //public static bool IsServer = true;
        public static bool IsServer = false;
        public bool isServerOverride = false; // Use to simulate server on editor

        [CommandLineArgument("ipnetwork", "For server only, please set on Constant.IpNetwork for Client")]
        public static string IpNetwork = "54.255.201.165";
        //public static string IpNetwork = "localhost"; 

        [CommandLineArgument("portnumber", "Sets the port of the transport layer default(7777)")]
        public static int PortNumber = 7822;
        // public static int PortNumber = 8008;

        [CommandLineArgument("portweb", "Sets the port of the transport layer default(7779)")]
        public static int PortWeb = 7823;
        // public static int PortNumber = 8008;

        [CommandLineArgument("portlisten", "Sets the port of the udp default(7778)")]
        public static int PortUdpServerBridge = 7824;

        [CommandLineArgument("startscenename", "Automatically switch to scene on Awake default(string.Empty)")]
        public static string StartSceneName = "";

        [CommandLineArgument("guestname", "Automatically switch to scene on Awake default(string.Empty)")]
        public static string GuestName = "";
        // public static string GuestName = Constant.ScenarioPlayerData;

        [CommandLineArgument("scenariokey", "Automatically switch to scene on Awake default(string.Empty)")]
        public static string ScenarioKey = "";
        // public static string ScenarioKey = "2scenario3";

        public static List<string> SceneNames = new List<string>();

        [Scene] [SerializeField] string _serverFirstScene;
        [Scene] [SerializeField] string _clientFirstScene;
        [SerializeField] float _delayTime = 1f;

        private string sceneName;

        void Awake(){
            Constant.AdditiveNetworkScene = _clientFirstScene;
            
            // StartCoroutine(LoadScene(sceneName));
        }

        void Start(){
            sceneName = _clientFirstScene;
            if (Application.isBatchMode)
            {
                Debug.Log("Running in a dedicated server build (headless mode).");
                IsServer = true;
            }
            else
            {
                Debug.Log("Running as a client or host.");
                if (isServerOverride)
                {
                    IsServer = true;
                }
                else
                {
                    IsServer = false;
                }
            }

            if (IsMainProcess){
                if (IsServer)
                {
                    sceneName = _serverFirstScene;
                }
            }

            if (IsServer){
                var _sceneNames = StartSceneName.Split("|");
                foreach (string scene in _sceneNames){
                    if (!scene.IsNullOrEmpty()){
                        SceneNames.Add(scene);
                    }
                }

                SceneManager.LoadScene(sceneName);
                return;
            }

            // LoadingController.Instance.Load(sceneName);
        }

        IEnumerator LoadScene(string sceneName){
            yield return new WaitForSeconds(_delayTime);
            //LoadingController.Instance.Load(sceneName);
        }

        public void StartScene()
        {
            if (IsMainProcess)
            {
                SceneManager.LoadScene(_serverFirstScene);
            }
            else
            {
                SceneManager.LoadScene(_clientFirstScene);
            }
        }
    }

    [Serializable]
    public class SceneSyncData {
        public bool IsLoaded;
        [Scene] public string SceneName;
    }
}