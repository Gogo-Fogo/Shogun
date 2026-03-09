using UnityEngine;
using Shogun.Core.Architecture;

namespace Shogun.Events
{
    /// <summary>
    /// Event channel for scene management events.
    /// </summary>
    [CreateAssetMenu(fileName = "SceneEventChannel", menuName = "Shogun/Events/Scene Event Channel")]
    public class SceneEventChannel : EventChannelSO<string>
    {
    }

    /// <summary>
    /// Event channel for combat-related events.
    /// </summary>
    [CreateAssetMenu(fileName = "CombatEventChannel", menuName = "Shogun/Events/Combat Event Channel")]
    public class CombatEventChannel : EventChannelSO<CombatEventData>
    {
    }

    /// <summary>
    /// Event channel for character-related events.
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterEventChannel", menuName = "Shogun/Events/Character Event Channel")]
    public class CharacterEventChannel : EventChannelSO<CharacterEventData>
    {
    }

    /// <summary>
    /// Event channel for UI-related events.
    /// </summary>
    [CreateAssetMenu(fileName = "UIEventChannel", menuName = "Shogun/Events/UI Event Channel")]
    public class UIEventChannel : EventChannelSO<UIEventData>
    {
    }

    /// <summary>
    /// Event channel for gacha/summoning events.
    /// </summary>
    [CreateAssetMenu(fileName = "GachaEventChannel", menuName = "Shogun/Events/Gacha Event Channel")]
    public class GachaEventChannel : EventChannelSO<GachaEventData>
    {
    }

    /// <summary>
    /// Event channel for input/gesture events.
    /// </summary>
    [CreateAssetMenu(fileName = "InputEventChannel", menuName = "Shogun/Events/Input Event Channel")]
    public class InputEventChannel : EventChannelSO<InputEventData>
    {
    }
}

/// <summary>
/// Data structure for combat events.
/// </summary>
[System.Serializable]
public struct CombatEventData
{
    public enum CombatEventType
    {
        BattleStart,
        BattleEnd,
        TurnStart,
        TurnEnd,
        ActionPerformed,
        DamageDealt,
        DamageReceived,
        CharacterDefeated,
        Victory,
        Defeat
    }
    
    public CombatEventType eventType;
    public string sourceId;
    public string targetId;
    public int value;
    public string description;
    
    public CombatEventData(CombatEventType type, string source = "", string target = "", int val = 0, string desc = "")
    {
        eventType = type;
        sourceId = source;
        targetId = target;
        value = val;
        description = desc;
    }
}

/// <summary>
/// Data structure for character events.
/// </summary>
[System.Serializable]
public struct CharacterEventData
{
    public enum CharacterEventType
    {
        CharacterCreated,
        CharacterLeveledUp,
        CharacterEquipped,
        CharacterUnequipped,
        CharacterSummoned,
        CharacterAwakened,
        CharacterLimitBroken,
        CharacterStatsChanged
    }
    
    public CharacterEventType eventType;
    public string characterId;
    public int level;
    public string itemId;
    public string description;
    
    public CharacterEventData(CharacterEventType type, string character = "", int lvl = 0, string item = "", string desc = "")
    {
        eventType = type;
        characterId = character;
        level = lvl;
        itemId = item;
        description = desc;
    }
}

/// <summary>
/// Data structure for UI events.
/// </summary>
[System.Serializable]
public struct UIEventData
{
    public enum UIEventType
    {
        PanelOpened,
        PanelClosed,
        ButtonPressed,
        SliderChanged,
        ToggleChanged,
        MenuNavigated,
        DialogShown,
        DialogClosed
    }
    
    public UIEventType eventType;
    public string panelId;
    public string elementId;
    public float value;
    public bool boolValue;
    public string description;
    
    public UIEventData(UIEventType type, string panel = "", string element = "", float val = 0f, bool boolVal = false, string desc = "")
    {
        eventType = type;
        panelId = panel;
        elementId = element;
        value = val;
        boolValue = boolVal;
        description = desc;
    }
}

/// <summary>
/// Data structure for gacha events.
/// </summary>
[System.Serializable]
public struct GachaEventData
{
    public enum GachaEventType
    {
        SummonStarted,
        SummonCompleted,
        CharacterObtained,
        DuplicateObtained,
        PityTriggered,
        BannerChanged
    }
    
    public GachaEventType eventType;
    public string bannerId;
    public string characterId;
    public int rarity;
    public int summonCount;
    public string description;
    
    public GachaEventData(GachaEventType type, string banner = "", string character = "", int rar = 0, int count = 0, string desc = "")
    {
        eventType = type;
        bannerId = banner;
        characterId = character;
        rarity = rar;
        summonCount = count;
        description = desc;
    }
}

/// <summary>
/// Data structure for input events.
/// </summary>
[System.Serializable]
public struct InputEventData
{
    public enum InputEventType
    {
        Tap,
        Swipe,
        Hold,
        Release,
        Drag,
        Pinch,
        Rotate
    }
    
    public InputEventType eventType;
    public Vector2 position;
    public Vector2 delta;
    public float duration;
    public string description;
    
    public InputEventData(InputEventType type, Vector2 pos = default, Vector2 del = default, float dur = 0f, string desc = "")
    {
        eventType = type;
        position = pos;
        delta = del;
        duration = dur;
        description = desc;
    }
} 
