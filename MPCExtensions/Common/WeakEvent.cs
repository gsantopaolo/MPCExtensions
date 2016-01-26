using System;
using System.Diagnostics.CodeAnalysis;

namespace MPCExtensions.Common
{
    /// <summary>
    /// Implements a weak reference that allows owner to be garbage collected even with this weak handler subscribed
    /// </summary>
    /// <typeparam name="TInstance">Type of instance listening for the event.</typeparam>
    /// <typeparam name="TSource">Type of source for the event.</typeparam>
    /// <typeparam name="TEventArgs">Type of event arguments for the event.</typeparam>
    internal class WeakEvent<TInstance, TSource, TEventArgs> where TInstance : class
    {
        /// <summary>
        /// WeakReference to the instance listening for the event.
        /// </summary>
        private WeakReference _weakInstance;

        /// <summary>
        /// Gets or sets the method to call when the event raises.
        /// </summary>
        public Action<TInstance, TSource, TEventArgs> EventAction { get; set; }


        /// <summary>
        /// Gets or sets the method to call when detaching from the event.
        /// </summary>
        public Action<TInstance, WeakEvent<TInstance, TSource, TEventArgs>> DetachAction { get; set; }

        /// <summary>
        /// Initializes a new instances of the WeakReference.
        /// </summary>
        /// <param name="instance">Instance subscribing to the event.</param>
        public WeakEvent(TInstance instance)
        {
            if (null == instance)
            {
                throw new ArgumentNullException("instance");
            }
            _weakInstance = new WeakReference(instance);
        }

        /// <summary>
        /// Handler for the subscribed event calls EventAction to handle it.
        /// </summary>
        /// <param name="source">Event source.</param>
        /// <param name="eventArgs">Event arguments.</param>
        public void Handler(TSource source, TEventArgs eventArgs)
        {
            TInstance target = (TInstance)_weakInstance.Target;
            if (target != null)
            {
                // Call registered action
                if (EventAction != null)
                {
                    EventAction(target, source, eventArgs);
                }
            }
            else
            {
                // Detach from event
                Detach();
            }
        }

        /// <summary>
        /// Detaches from the subscribed event.
        /// </summary>
        public void Detach()
        {
            TInstance target = (TInstance)_weakInstance.Target;
            if (null != DetachAction)
            {
                DetachAction(target, this);
                DetachAction = null;
            }
        }
    }


    /// <summary>
    /// Implements a weak reference that allows owner to be garbage collected even with this weak handler subscribed
    /// </summary>
    /// <typeparam name="TInstance">Type of instance listening for the event.</typeparam>
    /// <typeparam name="TSource">Type of source for the event.</typeparam>
    internal class WeakEvent<TInstance, TSource> where TInstance : class
    {
        /// <summary>
        /// WeakReference to the instance listening for the event.
        /// </summary>
        private WeakReference _weakInstance;

        /// <summary>
        /// Gets or sets the method to call when the event rises.
        /// </summary>
        public Action<TInstance, TSource> EventAction { get; set; }

        /// <summary>
        /// Gets or sets the method to call when detaching from the event.
        /// </summary>
        public Action<TInstance, WeakEvent<TInstance, TSource>> DetachAction { get; set; }

        /// <summary>
        /// Initializes a new instances of the WeakReference class.
        /// </summary>
        /// <param name="instance">Instance subscribing to the event.</param>
        public WeakEvent(TInstance instance)
        {
            if (null == instance)
            {
                throw new ArgumentNullException("instance");
            }
            _weakInstance = new WeakReference(instance);
        }

        /// <summary>
        /// Handler for the subscribed event calls OnEventAction to handle it.
        /// </summary>
        /// <param name="source">Event source.</param>
        public void Handler(TSource source)
        {
            TInstance target = (TInstance)_weakInstance.Target;
            if (target != null)
            {
                // Call registered action
                if (EventAction != null)
                {
                    EventAction(target, source);
                }
            }
            else
            {
                // Detach from event
                Detach();
            }
        }

        /// <summary>
        /// Detaches from the subscribed event.
        /// </summary>
        public void Detach()
        {
            TInstance target = (TInstance)_weakInstance.Target;
            if (null != DetachAction)
            {
                DetachAction(target, this);
                DetachAction = null;
            }
        }
    }

}
