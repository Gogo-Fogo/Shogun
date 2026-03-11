using System;
using UnityEngine;

namespace Shogun.Core
{
    public enum FrameRateMode
    {
        BatterySaver = 30,
        Balanced = 60,
        HighRefresh = 120
    }

    public readonly struct GameSettingsSnapshot
    {
        public GameSettingsSnapshot(FrameRateMode frameRateMode, float masterVolume, bool vibrationEnabled, bool screenShakeEnabled)
        {
            FrameRateMode = frameRateMode;
            MasterVolume = Mathf.Clamp01(masterVolume);
            VibrationEnabled = vibrationEnabled;
            ScreenShakeEnabled = screenShakeEnabled;
        }

        public FrameRateMode FrameRateMode { get; }
        public float MasterVolume { get; }
        public bool VibrationEnabled { get; }
        public bool ScreenShakeEnabled { get; }
        public int TargetFrameRate => (int)FrameRateMode;
    }

    public static class GameSettingsService
    {
        private const string SaveKey = "Shogun.GameSettings.v1";
        private const float DefaultMasterVolume = 0.82f;

        [Serializable]
        private sealed class GameSettingsData
        {
            public int frameRateMode = (int)FrameRateMode.Balanced;
            public float masterVolume = DefaultMasterVolume;
            public bool vibrationEnabled = true;
            public bool screenShakeEnabled = true;
        }

        private static GameSettingsData cachedData;
        private static bool isLoaded;

        public static event Action SettingsChanged;

        public static GameSettingsSnapshot GetSnapshot()
        {
            EnsureLoaded();
            return BuildSnapshot();
        }

        public static void ApplyRuntimeSettings(int fallbackFrameRate = (int)FrameRateMode.Balanced)
        {
            EnsureLoaded();
            ApplyResolvedSettings(fallbackFrameRate);
        }

        public static void SetFrameRateMode(FrameRateMode frameRateMode)
        {
            EnsureLoaded();
            if ((FrameRateMode)cachedData.frameRateMode == frameRateMode)
                return;

            cachedData.frameRateMode = (int)frameRateMode;
            SaveAndBroadcast();
        }

        public static void SetMasterVolume(float masterVolume)
        {
            EnsureLoaded();
            float clamped = Mathf.Clamp01(masterVolume);
            if (Mathf.Approximately(cachedData.masterVolume, clamped))
                return;

            cachedData.masterVolume = clamped;
            SaveAndBroadcast();
        }

        public static void SetVibrationEnabled(bool vibrationEnabled)
        {
            EnsureLoaded();
            if (cachedData.vibrationEnabled == vibrationEnabled)
                return;

            cachedData.vibrationEnabled = vibrationEnabled;
            SaveAndBroadcast();
        }

        public static void SetScreenShakeEnabled(bool screenShakeEnabled)
        {
            EnsureLoaded();
            if (cachedData.screenShakeEnabled == screenShakeEnabled)
                return;

            cachedData.screenShakeEnabled = screenShakeEnabled;
            SaveAndBroadcast();
        }

        public static void ResetToDefaults()
        {
            cachedData = CreateDefaultData();
            isLoaded = true;
            SaveAndBroadcast();
        }

        public static string GetFrameRateLabel(FrameRateMode frameRateMode)
        {
            return frameRateMode switch
            {
                FrameRateMode.BatterySaver => "BATTERY SAVER",
                FrameRateMode.HighRefresh => "HIGH REFRESH",
                _ => "BALANCED"
            };
        }

        public static string GetFrameRateSummary(FrameRateMode frameRateMode)
        {
            return frameRateMode switch
            {
                FrameRateMode.BatterySaver => "30 FPS • lower battery and thermal pressure",
                FrameRateMode.HighRefresh => "120 FPS • best touch feel on supported devices",
                _ => "60 FPS • primary intended look for the mobile slice"
            };
        }

        private static void EnsureLoaded()
        {
            if (isLoaded)
                return;

            cachedData = LoadData();
            SanitizeData(cachedData);
            isLoaded = true;
            ApplyResolvedSettings((int)FrameRateMode.Balanced);
        }

        private static GameSettingsData LoadData()
        {
            if (!PlayerPrefs.HasKey(SaveKey))
                return CreateDefaultData();

            string raw = PlayerPrefs.GetString(SaveKey, string.Empty);
            if (string.IsNullOrWhiteSpace(raw))
                return CreateDefaultData();

            GameSettingsData loaded = JsonUtility.FromJson<GameSettingsData>(raw);
            return loaded ?? CreateDefaultData();
        }

        private static GameSettingsData CreateDefaultData()
        {
            return new GameSettingsData();
        }

        private static void SanitizeData(GameSettingsData data)
        {
            if (data == null)
                return;

            FrameRateMode frameRateMode = ResolveFrameRateMode(data.frameRateMode);
            data.frameRateMode = (int)frameRateMode;
            data.masterVolume = Mathf.Clamp01(data.masterVolume);
        }

        private static FrameRateMode ResolveFrameRateMode(int rawFrameRate)
        {
            return rawFrameRate switch
            {
                (int)FrameRateMode.BatterySaver => FrameRateMode.BatterySaver,
                (int)FrameRateMode.HighRefresh => FrameRateMode.HighRefresh,
                _ => FrameRateMode.Balanced
            };
        }

        private static GameSettingsSnapshot BuildSnapshot()
        {
            FrameRateMode frameRateMode = ResolveFrameRateMode(cachedData.frameRateMode);
            return new GameSettingsSnapshot(frameRateMode, cachedData.masterVolume, cachedData.vibrationEnabled, cachedData.screenShakeEnabled);
        }

        private static void SaveAndBroadcast()
        {
            Save();
            ApplyResolvedSettings((int)FrameRateMode.Balanced);
            SettingsChanged?.Invoke();
        }

        private static void Save()
        {
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(cachedData));
            PlayerPrefs.Save();
        }

        private static void ApplyResolvedSettings(int fallbackFrameRate)
        {
            FrameRateMode frameRateMode = ResolveFrameRateMode(cachedData != null ? cachedData.frameRateMode : fallbackFrameRate);
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = ResolveTargetFrameRate(frameRateMode, fallbackFrameRate);
            AudioListener.volume = cachedData != null ? Mathf.Clamp01(cachedData.masterVolume) : DefaultMasterVolume;
        }

        private static int ResolveTargetFrameRate(FrameRateMode frameRateMode, int fallbackFrameRate)
        {
            int target = (int)frameRateMode;
            return target > 0 ? target : Mathf.Max(30, fallbackFrameRate);
        }
    }
}