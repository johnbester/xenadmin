﻿/* Copyright (c) Citrix Systems, Inc. 
 * All rights reserved. 
 * 
 * Redistribution and use in source and binary forms, 
 * with or without modification, are permitted provided 
 * that the following conditions are met: 
 * 
 * *   Redistributions of source code must retain the above 
 *     copyright notice, this list of conditions and the 
 *     following disclaimer. 
 * *   Redistributions in binary form must reproduce the above 
 *     copyright notice, this list of conditions and the 
 *     following disclaimer in the documentation and/or other 
 *     materials provided with the distribution. 
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND 
 * CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, 
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF 
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE 
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR 
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, 
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR 
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE 
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF 
 * SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XenAdmin.Wizards.PatchingWizard.PlanActions;
using XenAPI;

namespace XenAdmin.Wizards.PatchingWizard
{
    class UpdateProgressBackgroundWorker : BackgroundWorker
    {
        public List<PlanAction> PlanActions { get; private set; }
        public Dictionary<Host, List<PlanAction>> DelayedActionsByHost {get; private set; }
        public List<PlanAction> FinalActions { get; private set; }

        private Host master;

        public List<PlanAction> FinsihedActions = new List<PlanAction>();
        public PlanAction FailedWithExceptionAction = null;
        public List<PlanAction> doneActions = new List<PlanAction>();
        public PlanAction InProgressAction { get; set; }


        private readonly List<string> avoidRestartHosts = new List<string>();
        
        /// <summary>
        /// This list lists uuids of hosts that does not need to be restarted
        /// </summary>
        public List<string> AvoidRestartHosts
        {
            get
            {
                return avoidRestartHosts;
            }
        }

        public UpdateProgressBackgroundWorker(Host master, List<PlanAction> planActions, Dictionary<Host, List<PlanAction>> delayedActionsByHost, 
            List<PlanAction> finalActions)
        {
            this.master = master;
            this.PlanActions = planActions;
            this.DelayedActionsByHost = delayedActionsByHost;
            this.FinalActions = finalActions;
        }

        public int ActionsCount
        {
            get
            {
                return PlanActions.Count + DelayedActionsByHost.Sum(kvp => kvp.Value.Count) + FinalActions.Count;
            }
        }

        public new void CancelAsync()
        {
            if (PlanActions != null)
                PlanActions.ForEach(pa => 
                {
                    if (!pa.IsComplete)
                        pa.Cancel();
                });

            base.CancelAsync();
        }
    }
}
