using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;

namespace AllanTTS
{
    public class Stuff {
        public Stuff(bool big, float scale) {
            Big = big;
            Scale = scale;
        }

        public bool Big = false;
        public float Scale = 1.0f;
    }

    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class TssSession : MySessionComponentBase
    {
        public Dictionary<long, GridItems> Cache;

        public static TssSession Instance;

        public List<IMyCubeBlock> ProtectionBlocks;

        public Dictionary<long, Stuff> TssSettings;

        private int cacheRefreshCounter = 60;

        public override void LoadData()
        {
            Cache = new Dictionary<long, GridItems>();
            Instance = this;
            ProtectionBlocks = new List<IMyCubeBlock>();
            TssSettings = new Dictionary<long, Stuff>();
        }

        public override void BeforeStart()
        {
            // MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(0, BeforeDamage);

            MyAPIGateway.Utilities.ShowNotification("Session");
        }

        protected override void UnloadData()
        {
            Instance = null;
            Cache = null;
            ProtectionBlocks = null;
            TssSettings = null;
        }

        // Executed every tick, 60 times a second, before physics simulation and only if game is not paused.
        public override void UpdateBeforeSimulation()
        {
            cacheRefreshCounter -= 1;
            if (cacheRefreshCounter <= 0) {
                // Cache.IsFresh = false;
                cacheRefreshCounter = 60;
                if (Cache == null) {
                    Cache = new Dictionary<long, GridItems>();
                }
                foreach (KeyValuePair<long, GridItems> cacheItem in Cache) {
                    cacheItem.Value.IsFresh = false;
                }
            }
        }

        // private void BeforeDamage(object target, ref MyDamageInformation info)
        // {
        //     MyAPIGateway.Utilities.ShowNotification("??? DAMAGE ???");
        //
        //     if (info.Amount == 0)
        //         return;
        //
        //     var slim = target as IMySlimBlock;
        //
        //     if(slim == null)
        //         return;
        //
        //     // if any of the protection blocks are on this grid then protect it
        //     foreach(var block in ProtectionBlocks)
        //     {
        //         // checks for same grid-group to extend protection to piston/rotors/wheels but no connectors (change link type to Physical to include those)
        //         // same grid only check: block.CubeGrid == slim.CubeGrid
        //         if(MyAPIGateway.GridGroups.HasConnection(block.CubeGrid, slim.CubeGrid, GridLinkTypeEnum.Mechanical))
        //         {
        //             info.Amount = 0;
        //             return;
        //         }
        //     }
        // }
    }
}
