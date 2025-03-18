using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace FXSceneEditor
{
    public class ScenarioHandler : MonoBehaviourSingleton<ScenarioHandler>
    {
        [SerializeField] bool _isDebug = false;
        [SerializeField] Transform _scenarioParent;
        [SerializeField] string _username;
        [SerializeField] string _password;

        protected override void Awake(){
            base.Awake();
            Constant.IsDebug = _isDebug;
        }

        public void GetRandomAvailablePort(int scenarioItemIndex, int port)
        {
            //var _scenarioItem = _scenarioParent.GetChild(scenarioItemIndex).GetComponent<ScenarioItem>();
            //_scenarioItem.Port = port;
            //_scenarioItem.IsWaitingToGetPort = false;
        }

        //void GetScenarios(string data){
        //    var json  = JsonConvert.DeserializeObject<RequestResponse<List<ScenarioResponse>>>(data);

        //    foreach (ScenarioResponse scenarioResponse in json.data){
        //        PopulateScenarioItem(scenarioResponse);
        //    }
        //}

        //void PopulateScenarioItem(ScenarioResponse scenarioResponse){
        //    var newItem = Instantiate(_scenarioPrefab, _scenarioParent);
        //    newItem.SetupItem(scenarioResponse);
        //}
    }
}