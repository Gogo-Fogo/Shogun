using UnityEngine;
using UnityEngine.Events;

namespace Shogun.Core.Architecture
{
    // EventListener.cs
    // MonoBehaviour component for listening to ScriptableObject event channels.
    // Attach to GameObjects to respond to events raised by EventChannelSO.
    // Supports UnityEvents for inspector-based event wiring and decoupled system design.

    /// <summary>
    /// Base class for event listeners that can be attached to GameObjects.
    /// Provides a way for GameObjects to respond to ScriptableObject events.
    /// </summary>
    public abstract class EventListener : MonoBehaviour
    {
        [Header("Event Listener Settings")]
        [SerializeField] protected bool logEvents = false;
        [SerializeField] protected bool listenOnStart = true;
        
        /// <summary>
        /// Whether to log events for debugging purposes.
        /// </summary>
        public bool LogEvents => logEvents;
        
        /// <summary>
        /// Whether to automatically start listening when the component starts.
        /// </summary>
        public bool ListenOnStart => listenOnStart;
        
        protected virtual void Start()
        {
            if (listenOnStart)
            {
                StartListening();
            }
        }
        
        protected virtual void OnDestroy()
        {
            StopListening();
        }
        
        protected virtual void OnEnable()
        {
            if (listenOnStart)
            {
                StartListening();
            }
        }
        
        protected virtual void OnDisable()
        {
            StopListening();
        }
        
        /// <summary>
        /// Start listening for events. Override in subclasses.
        /// </summary>
        protected virtual void StartListening()
        {
            // Override in subclasses
        }
        
        /// <summary>
        /// Stop listening for events. Override in subclasses.
        /// </summary>
        protected virtual void StopListening()
        {
            // Override in subclasses
        }
        
        /// <summary>
        /// Log an event message if logging is enabled.
        /// </summary>
        /// <param name="message">The message to log.</param>
        protected void LogEvent(string message)
        {
            if (logEvents)
            {
                Debug.Log($"[{name}] {message}");
            }
        }
    }

    /// <summary>
    /// Generic event listener that can listen for events with one parameter.
    /// </summary>
    /// <typeparam name="T">The type of data the event passes.</typeparam>
    public abstract class EventListener<T> : EventListener
    {
        [Header("Event Channel")]
        [SerializeField] protected EventChannelSO<T> eventChannel;
        
        [Header("Response")]
        [SerializeField] protected UnityEvent<T> onEventRaised;
        
        /// <summary>
        /// The event channel to listen to.
        /// </summary>
        public EventChannelSO<T> EventChannel
        {
            get => eventChannel;
            set => eventChannel = value;
        }
        
        /// <summary>
        /// Unity event that will be invoked when the event is raised.
        /// </summary>
        public UnityEvent<T> OnEventRaised => onEventRaised;
        
        protected override void StartListening()
        {
            if (eventChannel != null)
            {
                eventChannel.OnEventRaised += HandleEvent;
                LogEvent($"Started listening to {eventChannel.name}");
            }
            else
            {
                Debug.LogWarning($"[{name}] No event channel assigned to EventListener!");
            }
        }
        
        protected override void StopListening()
        {
            if (eventChannel != null)
            {
                eventChannel.OnEventRaised -= HandleEvent;
                LogEvent($"Stopped listening to {eventChannel.name}");
            }
        }
        
        /// <summary>
        /// Handle the event when it's raised. Override to provide custom behavior.
        /// </summary>
        /// <param name="data">The data passed with the event.</param>
        protected virtual void HandleEvent(T data)
        {
            LogEvent($"Event received: {data}");
            onEventRaised?.Invoke(data);
        }
        
        /// <summary>
        /// Manually raise the event for testing purposes.
        /// </summary>
        /// <param name="data">The data to pass with the event.</param>
        [ContextMenu("Test Event")]
        public void TestEvent(T data)
        {
            HandleEvent(data);
        }
    }

    /// <summary>
    /// Generic event listener that can listen for events with two parameters.
    /// </summary>
    /// <typeparam name="T1">The first type of data the event passes.</typeparam>
    /// <typeparam name="T2">The second type of data the event passes.</typeparam>
    public abstract class EventListener<T1, T2> : EventListener
    {
        [Header("Event Channel")]
        [SerializeField] protected EventChannelSO<T1, T2> eventChannel;
        
        [Header("Response")]
        [SerializeField] protected UnityEvent<T1, T2> onEventRaised;
        
        /// <summary>
        /// The event channel to listen to.
        /// </summary>
        public EventChannelSO<T1, T2> EventChannel
        {
            get => eventChannel;
            set => eventChannel = value;
        }
        
        /// <summary>
        /// Unity event that will be invoked when the event is raised.
        /// </summary>
        public UnityEvent<T1, T2> OnEventRaised => onEventRaised;
        
        protected override void StartListening()
        {
            if (eventChannel != null)
            {
                eventChannel.OnEventRaised += HandleEvent;
                LogEvent($"Started listening to {eventChannel.name}");
            }
            else
            {
                Debug.LogWarning($"[{name}] No event channel assigned to EventListener!");
            }
        }
        
        protected override void StopListening()
        {
            if (eventChannel != null)
            {
                eventChannel.OnEventRaised -= HandleEvent;
                LogEvent($"Stopped listening to {eventChannel.name}");
            }
        }
        
        /// <summary>
        /// Handle the event when it's raised. Override to provide custom behavior.
        /// </summary>
        /// <param name="data1">The first data passed with the event.</param>
        /// <param name="data2">The second data passed with the event.</param>
        protected virtual void HandleEvent(T1 data1, T2 data2)
        {
            LogEvent($"Event received: {data1}, {data2}");
            onEventRaised?.Invoke(data1, data2);
        }
        
        /// <summary>
        /// Manually raise the event for testing purposes.
        /// </summary>
        /// <param name="data1">The first data to pass with the event.</param>
        /// <param name="data2">The second data to pass with the event.</param>
        [ContextMenu("Test Event")]
        public void TestEvent(T1 data1, T2 data2)
        {
            HandleEvent(data1, data2);
        }
    }
} 