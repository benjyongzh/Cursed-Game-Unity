# Cursed-Game-Unity
An attempt to recreate the Cursed game from scratch in Unity. I only worked on it for 4 months before realizing that I did not have the time to actually complete the ambitious game.
I took the time to explore the possibilities of the Unity game engine.

## Things I created
- Basic FPS movement and aiming.
- Sound proximity system, where NPCs get alerted by sounds such as footsteps.
- Ability system, based on the modularity of DotA2's ability system, where abilities have their own API methods to generate various effects and conditions.
- An NPC AI generated from a Behaviour Tree. The tree system was an asset available to public. I tweaked it and used it to generate the desired AI behaviour (wandering, being alert, chasing, fighting).
- AI vision system, where the NPC can detect movements of objects within their cone of sight. This ties in directly with their behaviour.
- UI for players.

## Learning Points
- Because this game was meant to be multiplayer, I tried using Photon, and then Mirror, as networking solutions. While Photon had a simpler API to work with, they proved to be unusable due to their dedicated server location. Mirror is able to run with a host server.
- Working with Unity framework.
