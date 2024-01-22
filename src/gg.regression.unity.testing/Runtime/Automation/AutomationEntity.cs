using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RegressionGames.Unity.Automation
{
    /// <summary>
    /// Provides an interface to <see cref="AutomationEntity"/>s that represent <see cref="Component"/>s in the scene.
    /// </summary>
    internal interface IAutomationEntityWithComponent
    {
        Component Component { get; }
    }

    /// <summary>
    /// Represents an entity that can be used by automation tools.
    /// </summary>
    public abstract class AutomationEntity
    {
        /// <summary>
        /// The unique ID of this entity.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Gets the user-visible display name of this entity.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// A user-visible type name for the entity, to allow for easier identification.
        /// By default, this is the .NET type name of the entity.
        /// </summary>
        public virtual string Type => GetType().FullName;

        /// <summary>
        /// Gets a description of what the entity represents.
        /// This can be provided to a language model or other tool to help it understand what the action does.
        /// </summary>
        public string Description { get; }

        protected AutomationEntity(int id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }

        /// <summary>
        /// Gets the actions that can be performed on this entity.
        /// </summary>
        public virtual IReadOnlyDictionary<string, AutomationAction> Actions => EmptyDictionary.Of<string, AutomationAction>();

        /// <summary>
        /// Gets the state of this entity.
        /// </summary>
        // TODO: This API has similar problems to the existing RGState APIs (multiple dictionaries, lots of allocations, etc.)
        // Once the refactor of the RGState mechanism lands in the main SDK we can use that to inform how this can evolve.
        // This is an IEnumerable instead of an IReadOnlyDictionary because it's much more efficient when all you're doing is dumping the state to a file.
        public virtual IEnumerable<AutomationStateProperty> GetState() => Enumerable.Empty<AutomationStateProperty>();
    }

    public abstract class AutomationEntity<T> : AutomationEntity, IAutomationEntityWithComponent where T : Component
    {
        private readonly T m_Component;

        public T Component => m_Component;

        /// <summary>
        /// A user-visible type name for the entity, to allow for easier identification.
        /// By default, this is the .NET type name of the component.
        /// </summary>
        public override string Type => typeof(T).FullName;

        Component IAutomationEntityWithComponent.Component => m_Component;

        protected AutomationEntity(T component, string description) : base(component.transform.GetInstanceID(),
            component.gameObject.name, description)
        {
            m_Component = component;
        }
    }

    /// <summary>
    /// Represents an action that can be performed by a bot.
    /// </summary>
    public abstract class AutomationAction
    {
        private AutomationController m_AutomationController;
        private AutomationActionActivation m_LastActivation;

        /// <summary>
        /// Gets a boolean indicating if the action was activated this frame.
        /// </summary>
        public bool ActivatedThisFrame => m_LastActivation.frame == Time.frameCount;

        /// <summary>
        /// Gets the <see cref="AutomationActionActivation"/> that represents the last time this action was activated.
        /// </summary>
        public AutomationActionActivation LastActivation => m_LastActivation;

        /// <summary>
        /// Gets the name of this action.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets a description of what the action does.
        /// This can be provided to a language model or other tool to help it understand what the action does.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the <see cref="AutomationEntity"/> that this action belongs to.
        /// </summary>
        public AutomationEntity Entity { get; }

        protected AutomationAction(string name, string description, AutomationEntity entity)
        {
            Name = name;
            Description = description;
            Entity = entity;
        }

        /// <summary>
        /// Gets a boolean indicating if this action can be activated on this frame.
        /// </summary>
        /// <returns></returns>
        public abstract bool CanActivateThisFrame();

        public void Activate()
        {
            m_LastActivation = new(Time.frameCount);
            Execute();
        }

        /// <summary>
        /// Activates this action.
        /// </summary>
        protected abstract void Execute();
    }

    public struct AutomationStateProperty
    {
        /// <summary>
        /// The name of the state property.
        /// </summary>
        public string name;

        /// <summary>
        /// A description of what the state property represents.
        /// </summary>
        public string description;

        /// <summary>
        /// The value of the state property.
        /// </summary>
        public object value;

        public AutomationStateProperty(string name, string description, object value)
        {
            this.name = name;
            this.description = description;
            this.value = value;
        }
    }

    // TODO: Capture arguments here.
    [Serializable]
    public struct AutomationActionActivation
    {
        public int frame;

        public AutomationActionActivation(int frame)
        {
            this.frame = frame;
        }
    }
}
