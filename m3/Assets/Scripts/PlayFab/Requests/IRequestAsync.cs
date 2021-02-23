using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Requests
{
    public interface IRequestAsync
    {
        bool IsProcessing { get; }

        Task<IResultAsync> Process();
    }

    public interface IResultAsync { }
}