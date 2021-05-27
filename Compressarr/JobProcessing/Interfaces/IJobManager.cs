﻿using Compressarr.Presets.Models;
using Compressarr.Filtering;
using Compressarr.Filtering.Models;
using Compressarr.JobProcessing.Models;
using Compressarr.Services.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Compressarr.JobProcessing
{
    public interface IJobManager
    {
        HashSet<Job> Jobs { get; }
        
        Task<bool> AddJobAsync(Job newJob);
        void CancelJob(Job job);
        Task DeleteJob(Job job);
        bool FilterInUse(string filterName);
        Task InitialiseJob(Job job);
        void InitialiseJobs(Filter filter);
        void InitialiseJobs(MediaSource source);
        void InitialiseJobs(FFmpegPreset preset);
        bool PresetInUse(FFmpegPreset preset);
        Job ReloadJob(Job job);
        void RunJob(Job job);
    }
}