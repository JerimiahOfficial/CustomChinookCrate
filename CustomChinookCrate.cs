/*
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣶⣄⠀⠀⢀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢸⣿⣿⣷⣴⣿⡄⠀⠀⠀⠀⠀⠀⢀⡀
⠀⠀⠀⠀⠀⠀⠀⠀⠰⣶⣾⣿⣿⣿⣿⣿⡇⠀⢠⣷⣤⣶⣿⡇
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠙⣿⣿⣿⣿⣿⣿⣿⣀⣿⣿⣿⣿⣿⣧⣀
⠀⠀⠀⠀⠀⠀⠀⣷⣦⣀⠘⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠃
⠀⠀⠀⠀⢲⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⠁
⠀⠀⠀⠀⠀⠙⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡟⠁
⠀⠀⠀⠀⠀⠚⠻⢿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⠿⠿⠂
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠉⠙⢻⣿⣿⡿⠛⠉⡇
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀ ⠘⠋⠁⠀⠀ ⣧
Created by Jerimiah
https://github.com/JerimiahOfficial/
*/

using Oxide.Core.Plugins;
using UnityEngine;

namespace Oxide.Plugins {

    [Info("CustomChinookCrate", "Jerimiah", "0.0.1")]
    [Description("Drops a crate at the specified coordinates.")]
    internal class CustomChinookCrate : CovalencePlugin {
        private CH47HelicopterAIController CustomChinook;
        private Vector3 dropPos = new Vector3(0, 0, 0);
        private Vector3 SpawnLocation;

        private void Init() {
            timer.Every(3600, () => {
                if (CustomChinook != null)
                    return;

                Chinook();
            });
        }

        [Command("Drop")]
        private void Drop() {
            Puts(": Dropping a crate.");
            HackableLockedCrate Crate = GameManager.server.CreateEntity("assets/prefabs/deployable/chinooklockedcrate/codelockedhackablecrate.prefab", dropPos) as HackableLockedCrate;
            Crate.Spawn();
        }

        [Command("Call")]
        private void Call() {
            Puts(": Calling Chinook.");
            Chinook();
        }

        private void Chinook() {
            if (CustomChinook != null)
                return;

            Puts(": Spawning Chinook.");
            CH47HelicopterAIController CH47 = GameManager.server.CreateEntity("assets/prefabs/npc/ch47/ch47scientists.entity.prefab") as CH47HelicopterAIController;

            if (CH47 == null)
                return;

            CH47.TriggeredEventSpawn();
            CH47.Spawn();
            SpawnLocation = CH47.GetPosition();

            if (CH47 == null || CH47.IsDestroyed)
                return;

            CustomChinook = CH47;
        }

        private void OnTick() {
            if (CustomChinook == null)
                return;

            CustomChinook._aimDirection = (CustomChinook.numCrates == 1 ? dropPos : SpawnLocation);
            CustomChinook._moveTarget = (CustomChinook.numCrates == 1 ? dropPos : SpawnLocation);

            if (CustomChinook.numCrates != 0) {
                if (Vector3Ex.Distance2D(CustomChinook.GetPosition(), dropPos) < 15) {
                    Puts(": Chinook has reached destination.");
                    CustomChinook.DropCrate();
                }
            }

            if (CustomChinook.numCrates == 0) {
                if (Vector3Ex.Distance2D(CustomChinook.GetPosition(), SpawnLocation) < 15) {
                    Puts(": Chinook has left.");
                    CustomChinook.Kill();
                    CustomChinook = null;
                }
            }
        }

        private void OnCrateDropped(HackableLockedCrate crate) {
            if (Vector3Ex.Distance2D(crate.dropPosition, dropPos) < 15) {
                timer.Once(1, () => {
                    Puts(": Chinook has dropped a crate.");
                    crate.LandCheck();
                    crate.Kill();
                    Drop();
                });
            }
        }

        private void OnEntityKill(CH47HelicopterAIController chinook) {
            if (chinook == CustomChinook) {
                CustomChinook = null;
                Puts(": Chinook was killed.");
            }
        }

        private void OnPluginUnloaded(Plugin name) {
            Puts(": Killing Chinooks.");
            CustomChinook.Kill();
            CustomChinook = null;
            Puts(": Unloaded.");
        }

        private void OnServerShutdown() {
            Puts(": Killing Chinooks.");
            CustomChinook.Kill();
            CustomChinook = null;
            Puts(": Unloaded.");
        }
    }
}
