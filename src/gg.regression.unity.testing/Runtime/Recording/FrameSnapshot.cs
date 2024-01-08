using System;
using System.Collections.Generic;
using System.Linq;
using RegressionGames.Unity.Automation;
using UnityEngine;

namespace RegressionGames.Unity.Recording
{
    /// <summary>
    /// A snapshot of the state of all entities in the game.
    /// This snapshot is not coupled to the live game objects and can be stored safely between frames.
    /// </summary>
    [Serializable]
    public class FrameSnapshot
    {
        /// <summary>A <see cref="FrameInfo"/> containing basic information about the frame.</summary>
        public FrameInfo frame;

        /// <summary>A list of <see cref="EntitySnapshot"/> objects representing the state of each entity in the game.</summary>
        public List<EntitySnapshot> entities;

        /// <summary>
        /// A snapshot of the state of all entities in the game.
        /// </summary>
        /// <param name="frame">A <see cref="FrameInfo"/> representing the current frame.</param>
        /// <param name="entities">A list of <see cref="EntitySnapshot"/> objects representing the state of each entity in the game.</param>
        public FrameSnapshot(FrameInfo frame, List<EntitySnapshot> entities)
        {
            this.frame = frame;
            this.entities = entities;
        }

        /// <summary>
        /// Creates a <see cref="FrameSnapshot"/> representing the current state of the game.
        /// </summary>
        /// <param name="frame">A <see cref="FrameInfo"/> representing the current frame.</param>
        /// <param name="entities">The <see cref="AutomationEntity"/> objects in the game.</param>
        /// <returns>A <see cref="FrameSnapshot"/> that can be stored safely between frames.</returns>
        public static FrameSnapshot Create(FrameInfo frame, IEnumerable<AutomationEntity> entities)
        {
            return new(frame, entities.Select(EntitySnapshot.Create).ToList());
        }
    }

    [Serializable]
    public struct FrameInfo
    {
        /// <summary>
        /// The number of frames that have passed since the game started.
        /// </summary>
        public int frameCount;

        /// <summary>
        /// The time in seconds that have passed since the game started.
        /// </summary>
        public float time;

        /// <summary>
        /// The time scale of the game.
        /// </summary>
        public float timeScale;

        /// <summary>
        /// The time in seconds that have passed since the last frame.
        /// </summary>
        public float deltaTime;

        private FrameInfo(int frameCount, float time, float timeScale, float deltaTime)
        {
            this.frameCount = frameCount;
            this.time = time;
            this.timeScale = timeScale;
            this.deltaTime = deltaTime;
        }

        public static FrameInfo ForCurrentFrame()
        {
            return new(
                UnityEngine.Time.frameCount,
                UnityEngine.Time.time,
                UnityEngine.Time.timeScale,
                UnityEngine.Time.deltaTime);
        }
    }

    /// <summary>
    /// A snapshot of the state of a single entity in the game.
    /// This snapshot is not coupled to the live game objects and can be stored safely between frames.
    /// </summary>
    [Serializable]
    public class EntitySnapshot
    {

        /// <summary>The ID of the entity.</summary>
        public int id;

        /// <summary>The name of the entity.</summary>
        public string name;

        /// <summary>A description of the entity.</summary>
        public string description;

        /// <summary>A list of <see cref="ActionSnapshot"/> objects representing the state of each action on this entity.</summary>
        public List<ActionSnapshot> actions;

        /// <summary>The raw state values for this entity.</summary>
        public List<KeyValuePair<string, object>> state;

        /// <summary>
        /// A snapshot of the state of a single entity in the game.
        /// </summary>
        /// <param name="id">The ID of the entity.</param>
        /// <param name="name">The name of the entity.</param>
        /// <param name="description">A description of the entity.</param>
        /// <param name="actions">A list of <see cref="ActionSnapshot"/> objects representing the state of each action on this entity.</param>
        /// <param name="state">The raw state values for this entity.</param>
        public EntitySnapshot(int id,
            string name,
            string description,
            List<ActionSnapshot> actions,
            List<KeyValuePair<string, object>> state)
        {
            this.id = id;
            this.name = name;
            this.description = description;
            this.actions = actions;
            this.state = state;
        }

        /// <summary>
        /// Creates a <see cref="EntitySnapshot"/> representing the current state of the entity.
        /// </summary>
        /// <param name="entity">The <see cref="AutomationEntity"/> to snapshot.</param>
        /// <returns>An <see cref="EntitySnapshot"/> that can be stored safely between frames.</returns>
        public static EntitySnapshot Create(AutomationEntity entity)
        {
            return new(
                entity.Id,
                entity.Name,
                entity.Description,
                entity.Actions.Values.Select(ActionSnapshot.Create).ToList(),
                entity.GetState().ToList());
        }
    }

    /// <summary>
    /// A snapshot of the state of a single action on an entity in the game.
    /// This snapshot is not coupled to the live game objects and can be stored safely between frames.
    /// </summary>
    [Serializable]
    public class ActionSnapshot
    {
        /// <summary>The name of the action.</summary>
        public string name;

        /// <summary>A description of the action.</summary>
        public string description;

        /// <summary>A boolean indicating if the action was activated this frame.</summary>
        public bool activated;

        /// <summary>
        /// A snapshot of the state of a single action on an entity in the game.
        /// </summary>
        /// <param name="name">The name of the action.</param>
        /// <param name="description">A description of the action.</param>
        /// <param name="activated">A boolean indicating if the action was activated this frame.</param>
        public ActionSnapshot(string name, string description, bool activated)
        {
            this.name = name;
            this.description = description;
            this.activated = activated;
        }

        /// <summary>
        /// Creates a <see cref="ActionSnapshot"/> representing the current state of the action.
        /// </summary>
        /// <param name="action">The <see cref="AutomationAction"/> to snapshot.</param>
        /// <returns>An <see cref="ActionSnapshot"/> that can be stored safely between frames.</returns>
        public static ActionSnapshot Create(AutomationAction action)
        {
            return new(action.Name, action.Description, action.ActivatedThisFrame);
        }
    }
}
