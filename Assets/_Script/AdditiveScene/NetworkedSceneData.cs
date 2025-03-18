using System;
using Mirror;

namespace FXSceneEditor.Network
{
    [Serializable]
    public class NetworkedSceneData
    {
        public string Key;
        [Scene] public string SceneName;
    }
}
