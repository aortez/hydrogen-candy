﻿using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;

namespace AllanTTS
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class TssSession : MySessionComponentBase
    {
        public static TssSession Instance; // NOTE: this is the only acceptable static if you nullify it afterwards.

        public List<IMyCubeBlock> ProtectionBlocks = new List<IMyCubeBlock>();

        public GridItems Cache = new GridItems();

        private int cacheRefreshCounter = 60;

        public override void LoadData()
        {
            Instance = this;
        }

        public override void BeforeStart()
        {
            MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(0, BeforeDamage);

            MyAPIGateway.Utilities.ShowNotification("Session");
        }

        protected override void UnloadData()
        {
            Instance = null; // important to avoid this object instance from remaining in memory on world unload/reload
        }

        // Executed every tick, 60 times a second, before physics simulation and only if game is not paused.
        public override void UpdateBeforeSimulation()
        {
            cacheRefreshCounter -= 1;
            if (cacheRefreshCounter <= 0) {
                Cache.IsFresh = false;
                cacheRefreshCounter = 60;
            }
        }

        private void BeforeDamage(object target, ref MyDamageInformation info)
        {
            MyAPIGateway.Utilities.ShowNotification("??? DAMAGE ???");

            if (info.Amount == 0)
                return;

            var slim = target as IMySlimBlock;

            if(slim == null)
                return;

            // if any of the protection blocks are on this grid then protect it
            foreach(var block in ProtectionBlocks)
            {
                // checks for same grid-group to extend protection to piston/rotors/wheels but no connectors (change link type to Physical to include those)
                // same grid only check: block.CubeGrid == slim.CubeGrid
                if(MyAPIGateway.GridGroups.HasConnection(block.CubeGrid, slim.CubeGrid, GridLinkTypeEnum.Mechanical))
                {
                    info.Amount = 0;
                    return;
                }
            }
        }
    }
}
