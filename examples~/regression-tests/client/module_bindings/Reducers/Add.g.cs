// THIS FILE IS AUTOMATICALLY GENERATED BY SPACETIMEDB. EDITS TO THIS FILE
// WILL NOT BE SAVED. MODIFY TABLES IN YOUR MODULE SOURCE CODE INSTEAD.

// This was generated using spacetimedb cli version 1.1.1 (commit bc3d453e871c797c17fdab2d772019832cd9b73e).

#nullable enable

using System;
using SpacetimeDB.ClientApi;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SpacetimeDB.Types
{
    public sealed partial class RemoteReducers : RemoteBase
    {
        public delegate void AddHandler(ReducerEventContext ctx, uint id, uint indexed);
        public event AddHandler? OnAdd;

        public void Add(uint id, uint indexed)
        {
            conn.InternalCallReducer(new Reducer.Add(id, indexed), this.SetCallReducerFlags.AddFlags);
        }

        public bool InvokeAdd(ReducerEventContext ctx, Reducer.Add args)
        {
            if (OnAdd == null)
            {
                if (InternalOnUnhandledReducerError != null)
                {
                    switch (ctx.Event.Status)
                    {
                        case Status.Failed(var reason): InternalOnUnhandledReducerError(ctx, new Exception(reason)); break;
                        case Status.OutOfEnergy(var _): InternalOnUnhandledReducerError(ctx, new Exception("out of energy")); break;
                    }
                }
                return false;
            }
            OnAdd(
                ctx,
                args.Id,
                args.Indexed
            );
            return true;
        }
    }

    public abstract partial class Reducer
    {
        [SpacetimeDB.Type]
        [DataContract]
        public sealed partial class Add : Reducer, IReducerArgs
        {
            [DataMember(Name = "id")]
            public uint Id;
            [DataMember(Name = "indexed")]
            public uint Indexed;

            public Add(
                uint Id,
                uint Indexed
            )
            {
                this.Id = Id;
                this.Indexed = Indexed;
            }

            public Add()
            {
            }

            string IReducerArgs.ReducerName => "Add";
        }
    }

    public sealed partial class SetReducerFlags
    {
        internal CallReducerFlags AddFlags;
        public void Add(CallReducerFlags flags) => AddFlags = flags;
    }
}
