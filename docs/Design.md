# Rough Design Notes

This document has some rough information about the design of this package.

## Components

The package is made up of a few different components:

* Automation Controller
* Entity Data Model
* Entity Discoverers
* Bots
* Automation Recorder

## Automation Controller

The automation controller is the hub that controls automation.
It is responsible for tracking the entities discovered by the entity discoverers, and for managing bot lifecycle.
All automation components must be children of the automation controller.

The Automation Controller is found in `src/gg.regression.unity.testing/Assets/Scripts/Runtime/Automation/AutomationController.cs`

## Entity Data Model

The entity data model is a set of classes that represent the entities that the automation controller can manage.
These entities are things like scenes, game objects, and other components.

### Entities

An Entity is represented by the `AutomationEntity` base class, and has a set of properties that are common to all entities.

* `Id` - A unique ID for the entity within a given game session (derived from the Instance ID)
* `Name` - The name of the entity
* `Type` - The type of the entity
* Other metadata, such as a description.

In addition, Entities contain a set of Actions and States.

### Actions

Actions represent things that an entity can do.
Bots can trigger entity actions to interact with the entity.
An action is represented by a subclass of `AutomationAction`.
Actions have names, which must be unique among other actions on the same entity.
In addition, they have an `Activate` method that Bots can call to trigger the action.

### States

States are simple key-value pairs that represent the state of an entity.
For example, they may encode position data for a game object, or the current state of a UI element.

## Entity Discoverers

In order to make it easy to integrate automation in to any game, the automation package provides Discoverers that automatically find automatable entities in the game.
For example, the `UIElementDiscoverer` finds all UI elements in the game, and creates Entities for them.

A Discoverer is a Mono Behavior that inherits from `EntityDiscoverer`.
Entities discovered by the discoverer are automatically added to the automation controller.

## Bots

Bots are components that can evaluate states and take actions based on those states.
They are responsible for driving the automation process.
Bots can be created by extending the `Bot` class.
As long as a Bot is a child of the Automation Controller, it will be automatically managed by the controller and can access entities registered in the Controller.

## Automation Recorder

The Automation Recorder is a component that hooks in to the frame update loop and records the state of all Entities in the Automation Controller each frame.
In addition, it takes a screenshot each frame.
These recordings are automatically saved to disk.
