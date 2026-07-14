# Peg-and-Hole VR Force Task

This project implements a VR peg-and-hole task designed to study **force control during object manipulation**. Participants place colored pegs into matching holes while applying specific trigger-force levels with a VR controller.

Each trial presents a **different peg-and-hole configuration**, which is displayed in the scene for participants to follow. The required force ranges associated with each peg color are randomized between trials, requiring participants to adapt their force output throughout the experiment.

## Main Scenes

* **`InitialInteractionScene_ControllerTracking_Task.unity`** – Controller-based peg-and-hole task with force-sensitive placement.
* **`HandTrackingScene.unity`** – Records finger joint data for hand pose and motion analysis.

## Task Overview

Participants are presented with a board containing red and green pegs and a visual configuration indicating where each peg should be placed.

**To successfully complete the task**, participants must:

* Follow the displayed peg-and-hole configuration.
* Insert each peg into the matching colored hole.
* Apply the correct controller trigger force for that peg color.

A peg is accepted only when both the **correct location** and the **correct force range** are achieved.

## Force-Based Interaction

Controller trigger force is represented as a normalized value between **0.0 and 1.0**.

For every trial:

* Each peg color is assigned a randomized target force range.
* Force ranges are unique to each color and change between trials.
* Participants must learn and apply the appropriate force while completing the displayed configuration.

During insertion, the task provides real-time visual feedback:

* 🟢 Green — Force is within the valid range.
* 🔴 Red — Force is outside the valid range.
* 🔵 Blue — Peg has been successfully inserted and locked.

## Trial Flow

* A peg-and-hole configuration is randomly selected and displayed.
* Participants complete the configuration by inserting all required pegs.
* After completion, a short survey is presented.
* The scene reloads and begins the next trial with a new randomized configuration and force mapping.

## Data Logging

The project automatically records:

* Continuous controller force data (CSV)
* Trial summary data (JSON)

Recorded information includes:

* Trial and configuration indices
* Force ranges
* Force samples during interaction
* Placement events
* Completion time
* Survey responses

## Main Scripts

* **`TrialManager.cs`** – Trial flow, configuration selection, timing, surveys, and logging.
* **`AutomaticPegIdentifier.cs`** – Assigns peg colors and randomized force ranges.
* **`TriggerLoadingIndicator.cs`** – Reads controller trigger force.
* **`ForceValidator.cs`** – Validates force against the target range.
* **`PegHoleInsertionForceValidated.cs`** – Handles insertion logic, feedback, and peg locking.
* **`ForceLogger.cs`** – Records force and interaction events.

## Hand Tracking Scene

`HandTrackingScene.unity` is provided for collecting finger joint positions and hand pose data independently of the force task. This scene can be used for recording or analyzing hand kinematics during tracked hand interactions.
