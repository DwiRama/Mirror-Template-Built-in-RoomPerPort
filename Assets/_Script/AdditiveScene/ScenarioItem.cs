using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.Linq;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace FXSceneEditor
{
    public class ScenarioItem : MonoBehaviour
    {

        public int Port { get; set; }
        public bool IsWaitingToGetPort { get; set; }

        //void LoadScenario(string scenarioData)
        //{
        //    //var json = JsonConvert.DeserializeObject<RequestResponse<ScenarioData>>(scenarioData);
        //    var _scenariosData = JsonConvert.DeserializeObject<RequestResponse<FXmedia.SceneEditor.GridSystem.ScenarioData>>(scenarioData);
        //    SceneEditorDataManager.Instance.scenarioData = _scenariosData.data;
        //    SceneEditorDataManager.Instance.sceneData = _scenariosData.data.scenes[0];
        //    if (_scenariosData.status)
        //    {
        //        var port = 0;
        //        var portWeb = 0;

        //        if (_scenariosData.data.scenes.Count == 0)
        //        {
        //            _NotificationHandler.OnNotify?.Invoke("No scene data... Please add some.", 4f, 0f);
        //            return;
        //        }
        //        if (StartupSceneHandler.SceneNames.Count != 0)
        //        {
        //            StartupSceneHandler.SceneNames.Clear();
        //        }

        //        var scenes = "";
        //        foreach (var scene in _scenariosData.data.scenes)
        //        {
        //            StartupSceneHandler.SceneNames.Add(SceneDataPersistenData.Instance.FXSceneEditorData.PresetSceneDatas.SceneName[scene.id]);
        //            scenes += SceneDataPersistenData.Instance.FXSceneEditorData.PresetSceneDatas.SceneName[scene.id] + "|";
        //        }

        //        // check port
        //        var _isGuest = DatabaseController.IsGuest;
        //        DatabaseController.IsGuest = true;
        //        var scenarioKey = ScenarioResponse.id + _scenariosData.data.name.ToLower().Replace(" ", "");
        //        DatabaseController.LoadData(scenarioKey, async onSuccess =>
        //        {
        //            var portData = JsonConvert.DeserializeObject<RequestResponse<ScenarioServerData>>(onSuccess);

        //            if (portData.status)
        //            {
        //                if (portData.data.is_active)
        //                {
        //                    port = portData.data.port_number;
        //                    portWeb = portData.data.port_web;
        //                }
        //                else
        //                {
        //                    if (portData.data.port_number != 0)
        //                    {
        //                        port = portData.data.port_number;
        //                        portWeb = portData.data.port_web;
        //                    }
        //                    else
        //                    {
        //                        IsWaitingToGetPort = true;
        //                        GetRandomAvailablePort();
        //                        await UniTask.WaitUntil(() => !IsWaitingToGetPort);
        //                        port = Port;
        //                        portWeb = await GeneratePortWeb(port);
        //                        SessionHandler.Instance.CreateLocalSession(port.ToString(), portWeb.ToString(), scenes, Constant.ScenarioPlayerData, scenarioKey, StartupSceneHandler.IpNetwork, false, true);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                IsWaitingToGetPort = true;
        //                GetRandomAvailablePort();
        //                await UniTask.WaitUntil(() => !IsWaitingToGetPort);
        //                port = Port;
        //                portWeb = await GeneratePortWeb(port);
        //                SessionHandler.Instance.CreateLocalSession(port.ToString(), portWeb.ToString(), scenes, Constant.ScenarioPlayerData, scenarioKey, StartupSceneHandler.IpNetwork, false, true);
        //            }
                    
        //            // Debug.Log(port.ToString()+", "+ scenes);
        //            StartupSceneHandler.StartSceneName = scenes;
        //            StartupSceneHandler.IsMainProcess = false;
        //            StartupSceneHandler.IsServer = false;
        //            StartupSceneHandler.PortNumber = port;
        //            StartupSceneHandler.PortWeb = portWeb;

        //            SceneDataPersistenData.Instance.LoadSceneDatas(scenarioData);

        //            DatabaseController.IsGuest = _isGuest;
        //        }, onError =>
        //        {

        //        }, Constant.ScenarioPlayerData, true);
        //    }
        //    else
        //    {
        //        DebugLog.LogDebug("Scenario Not Found");
        //    }
        //}

        //async void LoadScenarioDebug(string scenarioData){
        //    var json  = JsonConvert.DeserializeObject<RequestResponse<ScenarioData>>(scenarioData);
        //    if (json.status){
        //        var port = 0;
        //        var portWeb = 0;
                
        //        if (StartupSceneHandler.SceneNames.Count != 0){
        //            StartupSceneHandler.SceneNames.Clear();
        //        }

        //        var scenes = "";
        //        foreach (var scene in json.data.scenes){
        //            StartupSceneHandler.SceneNames.Add(SceneDataPersistenData.Instance.FXSceneEditorData.PresetSceneDatas.SceneName[scene.id]);
        //            scenes += SceneDataPersistenData.Instance.FXSceneEditorData.PresetSceneDatas.SceneName[scene.id]+"|";
        //        }

        //        if (json.data.is_active){
        //            port = json.data.port_number;
        //            portWeb = json.data.port_web;
        //        }else{
        //            if (json.data.port_number != 0){
        //                port = json.data.port_number;
        //                portWeb = json.data.port_web;
        //            }else{
        //                IsWaitingToGetPort = true;
        //                GetRandomAvailablePort();
        //                await UniTask.WaitUntil(() => !IsWaitingToGetPort);
        //                port = Port;
        //                portWeb = await GeneratePortWeb(port);
        //            }
                    
        //            SessionHandler.Instance.CreateLocalSession(port.ToString(), portWeb.ToString(), scenes, Constant.ScenarioPlayerData, "scenarioKey", StartupSceneHandler.IpNetwork, false, true);
        //            json.data.port_number = port;
        //            json.data.port_web = portWeb;
        //            json.data.is_active = true;

        //            string jsonString = JsonUtility.ToJson(json, true);
        //            File.WriteAllText(Application.dataPath + "/../_FXSceneEditor/_DummyData/ScenarioDetail_"+ScenarioResponse.id+".json", jsonString);
        //        }
            
        //        StartupSceneHandler.StartSceneName = scenes;
        //        StartupSceneHandler.IsMainProcess = false;
        //        StartupSceneHandler.IsServer = false;
        //        StartupSceneHandler.PortNumber = port;
        //        StartupSceneHandler.PortWeb = portWeb;

        //        SceneDataPersistenData.Instance.LoadSceneDatas(scenarioData);
        //    }else{
        //        DebugLog.LogDebug("Scenario Not Found");
        //    }
        //}

        public async void CreateJoinSession()
        {
            FXMirrorPlayerLobbyInstance.localPlayer.OpenNewPort();

            await UniTask.WaitForSeconds(2);

            SessionHandler.Instance.CreateLocalSession(StartupSceneHandler.PortNumber.ToString(), StartupSceneHandler.PortWeb.ToString(), "MirrorRoomPerPort_Online Scene Session", Constant.ScenarioPlayerData, "0001", StartupSceneHandler.IpNetwork, false, true);

            StartupSceneHandler.IsMainProcess = false;
            await UniTask.WaitForSeconds(2);
            SceneManager.LoadScene(2);
            Debug.Log("Scene loaded: " + SceneManager.GetSceneByBuildIndex(2).name);
        }

        void GetRandomAvailablePort(){
            IsWaitingToGetPort = true;
            SessionHandler.SessionHandlerNetworkBehaviour.CmdGetRandomAvailablePort(Constant.LocalMainPlayer, transform.GetSiblingIndex(), Constant.MinPort, Constant.MaxPort, 15);
        }

        async UniTask<int> GeneratePortWeb(int portNumber){
            var portWeb = 0;
            bool _isPortWebGenerated = false;

            while(!_isPortWebGenerated){
                GetRandomAvailablePort();
                await UniTask.WaitUntil(() => !IsWaitingToGetPort);
                var newPortWeb = Port;
                
                if (!(newPortWeb == portNumber)){
                    _isPortWebGenerated = true;
                    portWeb = newPortWeb;
                }
            }

            return portWeb;
        }
    }
}