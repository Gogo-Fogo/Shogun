using UnityEngine;
using System;

namespace Shogun.Core.Architecture
{
    /// <summary>
    /// ScriptableObject-based event channel for decoupled communication between systems.
    /// Used to raise and listen for events in a type-safe, inspector-friendly way.
    /// Supports generic event data and is a core part of the project's event-driven architecture.
    /// </summary>
    public abstract class EventChannelSO : ScriptableObject
    {
        [Header("Event Channel Info")]
        [SerializeField] protected string channelDescription = "Event channel for system communication";
        
        /// <summary>
        /// Description of what this event channel is used for.
        /// </summary>
        public string ChannelDescription => channelDescription;
        
        /// <summary>
        /// Called when the event channel is created or loaded.
        /// Override to perform initialization.
        /// </summary>
        protected virtual void OnEnable()
        {
            // Subclasses can override for initialization
        }
        
        /// <summary>
        /// Called when the event channel is unloaded.
        /// Override to perform cleanup.
        /// </summary>
        protected virtual void OnDisable()
        {
            // Subclasses can override for cleanup
        }
    }

    /// <summary>
    /// Generic event channel that can pass any data type.
    /// </summary>
    /// <typeparam name="T">The type of data to pass with the event.</typeparam>
    public abstract class EventChannelSO<T> : EventChannelSO
    {
        /// <summary>
        /// The event that will be raised when Raise() is called.
        /// </summary>
        public event Action<T> OnEventRaised;

        /// <summary>
        /// Raises the event with the specified data.
        /// </summary>
        /// <param name="data">The data to pass with the event.</param>
        public void Raise(T data)
        {
            OnEventRaised?.Invoke(data);
        }

        /// <summary>
        /// Raises the event with the specified data and logs it for debugging.
        /// </summary>
        /// <param name="data">The data to pass with the event.</param>
        /// <param name="logEvent">Whether to log the event for debugging.</param>
        public void Raise(T data, bool logEvent = false)
        {
            if (logEvent)
            {
                Debug.Log($"[{name}] Event raised with data: {data}");
            }
            
            OnEventRaised?.Invoke(data);
        }
    }

    /// <summary>
    /// Generic event channel that can pass two data types.
    /// </summary>
    /// <typeparam name="T1">The first type of data to pass with the event.</typeparam>
    /// <typeparam name="T2">The second type of data to pass with the event.</typeparam>
    public abstract class EventChannelSO<T1, T2> : EventChannelSO
    {
        /// <summary>
        /// The event that will be raised when Raise() is called.
        /// </summary>
        public event Action<T1, T2> OnEventRaised;

        /// <summary>
        /// Raises the event with the specified data.
        /// </summary>
        /// <param name="data1">The first data to pass with the event.</param>
        /// <param name="data2">The second data to pass with the event.</param>
        public void Raise(T1 data1, T2 data2)
        {
            OnEventRaised?.Invoke(data1, data2);
        }

        /// <summary>
        /// Raises the event with the specified data and logs it for debugging.
        /// </summary>
        /// <param name="data1">The first data to pass with the event.</param>
        /// <param name="data2">The second data to pass with the event.</param>
        /// <param name="logEvent">Whether to log the event for debugging.</param>
        public void Raise(T1 data1, T2 data2, bool logEvent = false)
        {
            if (logEvent)
            {
                Debug.Log($"[{name}] Event raised with data: {data1}, {data2}");
            }
            
            OnEventRaised?.Invoke(data1, data2);
        }
    }

    /// <summary>
    /// Generic event channel that can pass three data types.
    /// </summary>
    /// <typeparam name="T1">The first type of data to pass with the event.</typeparam>
    /// <typeparam name="T2">The second type of data to pass with the event.</typeparam>
    /// <typeparam name="T3">The third type of data to pass with the event.</typeparam>
    public abstract class EventChannelSO<T1, T2, T3> : EventChannelSO
    {
        /// <summary>
        /// The event that will be raised when Raise() is called.
        /// </summary>
        public event Action<T1, T2, T3> OnEventRaised;

        /// <summary>
        /// Raises the event with the specified data.
        /// </summary>
        /// <param name="data1">The first data to pass with the event.</param>
        /// <param name="data2">The second data to pass with the event.</param>
        /// <param name="data3">The third data to pass with the event.</param>
        public void Raise(T1 data1, T2 data2, T3 data3)
        {
            OnEventRaised?.Invoke(data1, data2, data3);
        }

        /// <summary>
        /// Raises the event with the specified data and logs it for debugging.
        /// </summary>
        /// <param name="data1">The first data to pass with the event.</param>
        /// <param name="data2">The second data to pass with the event.</param>
        /// <param name="data3">The third data to pass with the event.</param>
        /// <param name="logEvent">Whether to log the event for debugging.</param>
        public void Raise(T1 data1, T2 data2, T3 data3, bool logEvent = false)
        {
            if (logEvent)
            {
                Debug.Log($"[{name}] Event raised with data: {data1}, {data2}, {data3}");
            }
            
            OnEventRaised?.Invoke(data1, data2, data3);
        }
    }
} 