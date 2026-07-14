# Peg-and-Hole VR Force Task

This project implements a VR peg-and-hole task designed to study **force control during object manipulation**. Participants place colored pegs into matching holes while applying specific trigger-force levels with a VR controller.

Each trial presents a **different peg-and-hole configuration**, which is displayed in the scene for participants to follow. The required force ranges associated with each peg color are randomized between trials, requiring participants to adapt their force output throughout the experiment.

## Demo

## Demo

> **Click the GIF below to watch the full demo on YouTube.**

[![Watch the Ultraleap Hand Tracking Demo](PegHoleForce.gif)]([https://youtu.be/BdMO90y-6E8](https://www.youtube.com/watch?v=_yWt1ZSOngU))

## Main Scenes

* **`InitialInteractionScene_ControllerTracking_Task.unity`** – Controller-based peg-and-hole task with force-sensitive placement.
* **`HandTrackingScene.unity`** – Records finger joint data for hand pose and motion analysis.

## Task Overview

<img width="889" height="459" alt="image" src="https://github.com/user-attachments/assets/7ac4f07c-2412-4034-ba81-582d3cb00cb1" />


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
<img width="182" height="161" alt="image" src="https://github.com/user-attachments/assets/7dc31931-b4da-4fc8-a1a7-9b1a6d5e83e7" />

* 🔴 Red — Force is outside the valid range.
<img width="191" height="161" alt="image" src="https://github.com/user-attachments/assets/3fd918de-0401-4b9f-8fcd-d113baef4849" />

* 🔵 Blue — Peg has been successfully inserted and locked.
<img width="186" height="173" alt="image" src="https://github.com/user-attachments/assets/c47d96c5-7ea1-47f7-a568-d154140ba846" />

## Trial Flow

* A peg-and-hole configuration is randomly selected and displayed.
* Participants complete the configuration by inserting all required pegs.
* After completion, a short survey is presented.
* The scene reloads and begins the next trial with a new randomized configuration and force mapping.

## Trial Configurations

Participants are shown one of several peg-and-hole configurations at the start of each trial. The displayed configuration serves as a visual guide indicating where each peg should be placed.

<table>
<tr>
<td align="center">
<b>Configuration 1</b><br>
<img src="Assets/Prefabs/Patterns/Configuration1.png" width="250"/>
</td>

<td align="center">
<b>Configuration 2</b><br>
<img src="Assets/Prefabs/Patterns/Configuration2.png" width="250"/>
</td>

<td align="center">
<b>Configuration 2</b><br>
<img src="Assets/Prefabs/Patterns/Configuration3.png" width="250"/>
</td>

<td align="center">
<b>Configuration 3</b><br>
<img src="Assets/Prefabs/Patterns/Configuration4.png" width="250"/>
</td>
</tr>
</table>

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
The `HandTrackingScene` includes a hand joint logging workflow that samples tracked hand joints every 100 ms during Play Mode. For each sample, the logger records the hand, joint name, world-space joint position, and tracking state to a timestamped CSV file in `Assets/Data/`. This allows finger joint trajectories to be analyzed over time outside Unity.
