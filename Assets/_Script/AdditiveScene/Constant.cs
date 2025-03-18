using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FXSceneEditor{
    public class Constant {
        public static bool IsDebug = false;
        public static string CurrentSceneId;
        public static string CurrentSceneActive;

        public static bool IsSceneLoading = false;
        public static bool IsCreatingScene = false;

        public static bool IsDraggingCamera = false;

        public static string AdditiveNetworkScene;
        public static GameObject LocalPlayer = null;
        public static GameObject LocalMainPlayer = null;

        public const string ScenarioPlayerData = "ScenarioPlayerData";
        public const string IpNetwork = "localhost";
        // public const string IpNetwork = "your network address";

        // 
        public const string MenuStateOpen = "menustateopen";
        public const string MenuStateClose = "menustateclose";
        public const string AnimationStatePlay = "animationstateplay";
        public const string AnimationStateStop = "animationstatestop";
        public const string AudioStatePlay = "audiostateplay";
        public const string AudioStateStop = "audiostatestop";
        public const string ModelSetting = "modelsettingdefault";

        public const int MinPort = 7802;
        public const int MaxPort = 7821;

        public static string CurrentScenarioId = "";

        public static bool CharacterCreated = false;
    }
}
