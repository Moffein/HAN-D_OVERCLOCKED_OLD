beta han-d mod. uploaded for multiplayer hosting purposes

if you want to make skinapi skins for HAN-D, the bodyname is "HANDOverclockedBody"
todo:
- Add proper VFX/SFX to all skills
- Replace placeholder R projectile
- skins
- custom anims (hopefully)
- add a proper ragdoll
- HUD overclock meter
- orbiting drones
- do minor number tweaks to the hit physics and general balance
- make m2 impact effect less obstructive
- item displays

bugs:
- m1 physics seems to be different for online clients. On hosts/singleplayer, it resets the velocity of the target before hitting so that they don't get launched out of reach at high attack speed, but this check doesn't seem to work in multiplayer

changelog:
0.0.5 - fixed language tokens
0.0.6 - temporarily disabled overclock sounds online due to a bug with the sound endlessly looping
0.0.7 - fixed drones spamming visual effects online
0.0.8:
- Re-enabled OVERCLOCK sounds online. Let me know if they still loop endlessly.
- M1 swing sound now plays online.
- OVERCLOCK M1 stun chance is no longer random. Instead of a random 40% chance, M1 now stuns on every leftwards swing. This should make fighting dangerous enemies like Elder Lemurians more consistent.
- Added numbers to the description of OVERCLOCK.
- Networked drone count online.
- DRONES now heal based on full HP + Shields, instead of just HP.
- Increased M1/M2 max push force scaling from 600kg to 2000kg. This means that bosses can now be dunked. This shouldn't have much effect on physics against normal enemies. Let me know how it feels!
0.0.9:
- Shortened OVERCLOCK description.
- DRONES now persist between stages.
- Fixed an oversight in DRONE counter networking when gaining DRONES by hitting bosses.
- Reverted "DRONES now heal based on full HP + Shields, instead of just HP" to be consistent with other healing sources.
0.0.10:
- Fixed DRONES not healing online. This bug may have been introduced by 0.0.8
0.0.11:
- Manually cancelling OVERCLOCK now deals damage. Cancel OVERCLOCK to release steam and spring into the air, igniting and stunning enemies for 200%-600% damage. Damage builds based on time spent in OVERCLOCK, and reaches the maximum 600% damage after 6 seconds. This is meant to be a minor bonus to cancelling OVERCLOCK so that it remainds useful even if you have feathers. The damage is low because it is meant to be more of a small bonus rather than a dedicated DPS skill, but I'd feedback on it. Needs proper VFX and a HUD indicator for charge level.
- ATTENTION SKINMAKERS: Renamed body from HANDOverclocked to HANDOverclockedBody.