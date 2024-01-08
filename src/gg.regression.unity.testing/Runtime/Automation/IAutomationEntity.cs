using System.Collections.Generic;
using UnityEngine;

namespace RegressionGames.Unity.Automation
{
    /// <summary>
    /// Represents an entity that can be used by automation tools.
    /// </summary>
    public interface IAutomationEntity
    {
        /// <summary>
        /// The unique ID of this entity.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Gets the user-visible display name of this entity.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a description of what the entity represents.
        /// This can be provided to a language model or other tool to help it understand what the action does.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets the actions that can be performed on this entity.
        /// </summary>
        IReadOnlyDictionary<string, IAutomationAction> Actions => EmptyDictionary.Of<string, IAutomationAction>();

        /// <summary>
        /// Gets the state of this entity.
        /// </summary>
        IReadOnlyDictionary<string, object> GetState() => EmptyDictionary.Of<string, object>();
    }

    /// <summary>
    /// Represents an action that can be performed by a bot.
    /// </summary>
    public interface IAutomationAction
    {
        /// <summary>
        /// Gets the name of this action.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a description of what the action does.
        /// This can be provided to a language model or other tool to help it understand what the action does.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets a boolean indicating if this action can be activated on this frame.
        /// </summary>
        /// <returns></returns>
        bool CanActivateThisFrame();

        /// <summary>
        /// Activates this action.
        /// </summary>
        void Activate();
    }
}
