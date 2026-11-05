# Modern UIs Private Room System V1

## Requirements

* ModernUIs V0.12.0

## Private Room Features

* Locking and unlocking the room
* Requesting access to the room
* Viewing who is inside the room
* Hiding who is inside the room
* Allowing staff to always enter the room by adding them to the Whitelist Combiner

## How does this work?

It's just a bunch of already existing Modern UIs scripts and prefabs put together.
There are some limitations because of this. For example, everyone who is in the room automatically "manages" it, including people who were added to the whitelist later.

## Setup

To set up a private room, drag the Room System Prefab into the scene and adjust the following:

1. Move the UIs
    * Move the "Outside UI" to the outside of your private room.
    * Move the "Inside UI" to the inside of your private room.
2. Set up the Notifications
    * Give the system a reference to your Notification Manager in the following places:
        * Whitelist Tools > Whitelist Add Notification
        * Inside UI > Private Room Section > Options > Whitelist Requestor
3. Move the Trigger Zones
    * Move the Player Zone Constraint to your room and adjust its size so it fully encapsulates your room.
    * Move the Whitelists > Whitelist Zone Counter to your room and adjust its size so it fully encapsulates your room.
4. Set up an All Players Whitelist
    * You only need one of these for your entire map, so add the "Whitelist All Players" prefab somewhere in your scene.
    * In Whitelist > Whitelist Switcher, set the whitelist in slot 0 to the "Whitelist All Players" prefab.
5. Move the Teleport Transforms
    * Move the Teleports > Outside Transform to the outside of the room, preferably in front of the Outside UI.
    * Move the Teleports > Inside Transform to the inside of the room, preferably facing the room from the door.

## Credits

* Made by DrBlackRat
* <https://drblackrat.xyz>
