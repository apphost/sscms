﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SS.CMS.Abstractions;

namespace SS.CMS.Core.Plugin
{
    public static class PluginJobManager
    {
        public static Dictionary<string, Func<IJobContext, Task>> GetJobs()
        {
            var jobs = new Dictionary<string, Func<IJobContext, Task>>(StringComparer.CurrentCultureIgnoreCase);

            foreach (var service in PluginManager.Services)
            {
                if (service.Jobs != null && service.Jobs.Count > 0)
                {
                    foreach (var command in service.Jobs.Keys)
                    {
                        jobs[command] = service.Jobs[command];
                    }
                }
            }

            return jobs;
        }
    }
}
