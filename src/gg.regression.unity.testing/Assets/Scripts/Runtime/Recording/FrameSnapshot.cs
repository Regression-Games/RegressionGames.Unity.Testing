using System;
using System.Collections.Generic;
using System.Linq;
using RegressionGames.Unity.Automation;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        /// <summary>A <see cref="SceneInfo"/> containing information about the active scene.</summary>
        public SceneInfo activeScene;

        /// <summary>A list of <see cref="EntitySnapshot"/> objects representing the state of each entity in the game.</summary>
        public List<EntitySnapshot> entities;

        /// <summary>
        /// A snapshot of the state of all entities in the game.
        /// </summary>
        /// <param name="frame">A <see cref="FrameInfo"/> representing the current frame.</param>
        /// <param name="activeScene">A <see cref="SceneInfo"/> representing the active scene.</param>
        /// <param name="entities">A list of <see cref="EntitySnapshot"/> objects representing the state of each entity in the game.</param>
        public FrameSnapshot(FrameInfo frame, SceneInfo activeScene, List<EntitySnapshot> entities)
        {
            this.frame = frame;
            this.activeScene = activeScene;
            this.entities = entities;
        }

        /// <summary>
        /// Compares this snapshot to another snapshot and returns a boolean indicating if this snapshot has changed since the other snapshot.
        /// This comparison ignores the <see cref="frame"/> field, because it is expected that the frame will always change.
        /// </summary>
        /// <param name="other">The <see cref="FrameSnapshot"/> to compare against. </param>
        /// <returns>A boolean indicating if this snapshot has changes compared to the provided snapshot.</returns>
        public bool HasChangesFrom(FrameSnapshot other)
        {
            return !Equals(activeScene, other.activeScene) ||
                   !Enumerable.SequenceEqual(entities, other.entities);
        }
    }

    [Serializable]
    public struct SceneInfo : IEquatable<SceneInfo>
    {
        /// <summary>
        /// The name of the scene.
        /// </summary>
        public string name;

        /// <summary>
        /// The path to the scene file.
        /// </summary>
        public string path;

        public static SceneInfo ForCurrentScene() => Create(SceneManager.GetActiveScene());

        private static SceneInfo Create(Scene scene)
        {
            return new()
            {
                name = scene.name,
                path = scene.path,
            };
        }

        public bool Equals(SceneInfo other) => name == other.name && path == other.path;
        public override bool Equals(object obj) => obj is SceneInfo other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(name, path);
    }

    [Serializable]
    public struct FrameInfo: IEquatable<FrameInfo>
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
                Time.frameCount,
                Time.time,
                Time.timeScale,
                Time.deltaTime);
        }

        public bool Equals(FrameInfo other)
        {
            return frameCount == other.frameCount && time.Equals(other.time) && timeScale.Equals(other.timeScale) && deltaTime.Equals(other.deltaTime);
        }

        public override bool Equals(object obj)
        {
            return obj is FrameInfo other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(frameCount, time, timeScale, deltaTime);
        }
    }

    /// <summary>
    /// A snapshot of the state of a single entity in the game.
    /// This snapshot is not coupled to the live game objects and can be stored safely between frames.
    /// </summary>
    [Serializable]
    public class EntitySnapshot: IEquatable<EntitySnapshot>
    {

        /// <summary>The ID of the entity.</summary>
        public int id;

        /// <summary>The name of the entity.</summary>
        public string name;

        /// <summary>The type of the entity.</summary>
        public string type;

        /// <summary>A description of the entity.</summary>
        public string description;

        /// <summary>A list of <see cref="ActionSnapshot"/> objects representing the state of each action on this entity.</summary>
        public List<ActionSnapshot> actions;

        /// <summary>The raw state values for this entity.</summary>
        public List<KeyValuePair<string, StateSnapshot>> state;

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
            string type,
            string description,
            List<ActionSnapshot> actions,
            List<KeyValuePair<string, StateSnapshot>> state)
        {
            this.id = id;
            this.name = name;
            this.type = type;
            this.description = description;
            this.actions = actions;
            this.state = state;
        }

        public bool Equals(EntitySnapshot other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return id == other.id && name == other.name && description == other.description && actions.SequenceEqual(other.actions) && state.SequenceEqual(other.state);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((EntitySnapshot) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(id, name, description, actions, state);
        }
    }

    [Serializable]
    public struct StateSnapshot
    {
        /// <summary>
        /// The value of the state property.
        /// </summary>
        public object value;

        /// <summary>
        /// A description of the state property.
        /// </summary>
        public string description;

        public StateSnapshot(object value, string description)
        {
            this.value = value;
            this.description = description;
        }
    }


    /// <summary>
    /// A snapshot of the state of a single action on an entity in the game.
    /// This snapshot is not coupled to the live game objects and can be stored safely between frames.
    /// </summary>
    [Serializable]
    public class ActionSnapshot: IEquatable<ActionSnapshot>
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

        public bool Equals(ActionSnapshot other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return name == other.name && description == other.description && activated == other.activated;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ActionSnapshot) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(name, description, activated);
        }
    }
}
