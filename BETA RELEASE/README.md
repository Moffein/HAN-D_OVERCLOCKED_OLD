Beta HAN-D mod. Gameplay is functional and he works in multiplayer, but visual polish is needed.

If you want to make skins for HAN-D, the bodyname is "HANDOverclockedBody"
todo:
- New Model + Anims (Punching primary included)
- VFX (glowing orange while in Overclock, changing swing trail based on passive stacks, etc.)
- Item Displays
- General polish/balancing.

Credits:
Coding - Moffein
Icons - PapaZach
DRONE model - LucidInceptor and TimeSweeper
Additional code help - Enigma

changelog:

0.1.6

- Now has a visual overlay when using OVERCLOCK.
- Added config option to sort HAN-D among the default survivors based on his planned unlock condition (disabled by default).

0.1.5

- DRONE firerate is no longer affected by attack speed, to allow for better control.

0.1.4

- Changed M1 screenshake method to prevent your view from getting messed up at high attack speed.
- Moved debuffs over to RecalculateStatsAPI for better cross-mod compatibility.

0.1.3

- Fixed directional audio.

0.1.2

- Disabled OVERCLOCK stun when King's Kombat Arena is active.
- Disabled DRONE slow when King's Kombat Arena is active.
- DRONE count now resets to 0 on round start in King's Kombat Arena.

0.1.1

- Sniper's Spotter Drone now counts towards HAN-D's passive.
- Fixed DRONE cooldown on hit/kill applying to Essence of Heresy.

0.1.0

This update has some changes to HAN-D's knockback to bring him more in-line with Vanilla characters, instead of being able to easily instakill bosses as a baseline while sending them flying across the map. The goal is for him to still be able to dunk Titans into pits and manhandle flying enemies to the ground with FORCED_REASSEMBLY, just that it should take more effort to do so. Feedback on the changes in this update would help a lot!

- Base accel reduced 40 -> 30 to match MUL-T.
- Base damage reduced 14 -> 12.
- Base armor reduced 20 -> 12.
- New Passive: PARALLEL_COMPUTING. Gain +2.5% damage and +1 armor for every mechanical ally on your team.
- DRONES now orbit around HAN-D. They count towards HAN-D's passive, but do NOT count as actual allies.
- DRONES get stuck less.

- HURT now resets enemy momentum on-hit to prevent multiple hits from sending them flying across the map.
- HURT no longer has any force limits. This will allow you to move Beetle Queens if you knock them into the air.

- Reduced FORCED_REASSEMBLY max force scaling from 2000 to 750. This will make it harder for HAN-D to splat bosses, while still allowing him to bully normal enemies.
- Removed max force scaling when hitting grounded enemies. This will make it easier for HAN-D to launch bosses into the air.
- Networked FORCED_REASSEMBLY squish effect and added a small grace period for triggering it.

- Added Scepter skill: UNETHICAL_REASSEMBLY (Replaces M2)
- Deals bonus damage, zaps enemies, and removes all force limiters.

- Added screenshake when using HURT.
- Extended FORCED_REASSEMBLY screenshake duration from 0.5s -> 0.65s
- Added a small viewkick when FORCED_REASSEMBLY finishes charging.
- Added a small particle effect when beginning/ending OVERCLOCK.

0.0.18

- Fixed for the Anniversary Hotfix update.

0.0.17

- DRONE heal increased 7.5% -> 8.5%
- DRONE cooldown reduction on melee hit reduced from -1.5s to -1.2s

0.0.16

- Fixed HAN-D being unable to deal damage in multiplayer.

0.0.15

- Fixed survivor select position.
- Added new game win text.
- Made old game win text the new escape fail text.

0.0.14

- Fixed HAN-D not receiving DRONEs when killing enemies.

0.0.13

- Updated for Anniversary update.
- OVERCLOCK now springs you into the air if you start it while airborne.
- OVERCLOCK smallhop force reduced 24 -> 22
- DRONE heal reduced 10% -> 7.5%.
- DRONE cooldown reduced 12s -> 10s.
- DRONE cooldown reduction on melee hit increased from -1s to -1.5s.

0.0.12

- Refactored code (Thanks for the help, Enigma!)
- Added updated skill icons (Thanks PapaZach!)
- Added RoR1 sounds.
- Size increased 20%.
- HP increased 150 -> 160 to be in line with other melee characters.
- Damage increased 12 -> 14.
- M1 and M2 are no longer Agile.
- M1 now has a -50% knockback penalty against bosses. This is negated if the boss is flying/airborne, so you can still launch bosses by comboing M2 + M1.
- M2 now flattens enemies on-kill (Thanks EnforcerGang!)
- M2 impact effect is less obtrusive (Thanks Enigma!)
- OVERCLOCK no longer has its ending explosion.
- DRONES reworked. DRONES now heal on hit instead of instantly, and can be used to either debuff enemies or buff and heal allies.
- DRONE cooldown reworked. DRONES now regen every 12s. Killing enemies instantly adds a stock. Hitting enemies with melee reduces the cooldown by 1s.
- Added HUD OVERCLOCK meter. Would like thoughts on the positioning and size!
- Temporarily removed M1 force reset feature. This feature only worked for hosts/singleplayer. This feature would reset enemy momentum every time you hit them so that you wouldn't launch them out of reach at high attack speeds. Plan is to re-add it once I can get it working in both singleplayer and multiplayer.

0.0.11

- Manually cancelling OVERCLOCK now deals damage. Cancel OVERCLOCK to release steam and spring into the air, igniting and stunning enemies for 200%-600% damage. Damage builds based on time spent in OVERCLOCK, and reaches the maximum 600% damage after 6 seconds. This is meant to be a minor bonus to cancelling OVERCLOCK so that it remainds useful even if you have feathers. The damage is low because it is meant to be more of a small bonus rather than a dedicated DPS skill, but I'd feedback on it. Needs proper VFX and a HUD indicator for charge level.
- ATTENTION SKINMAKERS: Renamed body from HANDOverclocked to HANDOverclockedBody.

0.0.10

- Fixed DRONES not healing online. This bug may have been introduced by 0.0.8

0.0.9

- Shortened OVERCLOCK description.
- DRONES now persist between stages.
- Fixed an oversight in DRONE counter networking when gaining DRONES by hitting bosses.
- Reverted "DRONES now heal based on full HP + Shields, instead of just HP" to be consistent with other healing sources.

0.0.8

- Re-enabled OVERCLOCK sounds online. Let me know if they still loop endlessly.
- M1 swing sound now plays online.
- OVERCLOCK M1 stun chance is no longer random. Instead of a random 40% chance, M1 now stuns on every leftwards swing. This should make fighting dangerous enemies like Elder Lemurians more consistent.
- Added numbers to the description of OVERCLOCK.
- Networked drone count online.
- DRONES now heal based on full HP + Shields, instead of just HP.
- Increased M1/M2 max push force scaling from 600kg to 2000kg. This means that bosses can now be dunked. This shouldn't have much effect on physics against normal enemies. Let me know how it feels!

0.0.7

- fixed drones spamming visual effects online

0.0.6

- temporarily disabled overclock sounds online due to a bug with the sound endlessly looping

0.0.5

- fixed language tokens