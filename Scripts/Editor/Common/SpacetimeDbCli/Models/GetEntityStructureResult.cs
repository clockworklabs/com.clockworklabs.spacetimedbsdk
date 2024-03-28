using System;
using Newtonsoft.Json;

namespace SpacetimeDB.Editor
{
    /// Result of `spacetime server list`
    public class GetEntityStructureResult : SpacetimeCliResult
    {
        /// Serializable from CliOutput
        public EntityStructure EntityStructure { get; private set; }
        
        /// isSuccess?
        public bool HasEntityStructure => EntityStructure is { HasEntities: true };
        
        public GetEntityStructureResult(SpacetimeCliResult cliResult)
            : base(cliResult.CliOutput, cliResult.CliError)
        {
            // ##################################################################
            // Example raw JSON result (minimized) below: Copy it to a prettifier
            // {"entities":{"Add":{"arity":2,"schema":{"elements":[{"algebraic_type":{"Builtin":{"String":[]}},"name":{"some":"name"}},{"algebraic_type":{"Builtin":{"I32":[]}},"name":{"some":"age"}}],"name":"Add"},"type":"reducer"},"Person":{"arity":3,"schema":{"elements":[{"algebraic_type":{"Builtin":{"I32":[]}},"name":{"some":"Id"}},{"algebraic_type":{"Builtin":{"String":[]}},"name":{"some":"Name"}},{"algebraic_type":{"Builtin":{"I32":[]}},"name":{"some":"Age"}}]},"type":"table"},"SayHello":{"arity":0,"schema":{"elements":[],"name":"SayHello"},"type":"reducer"}},"typespace":[{"Product":{"elements":[{"algebraic_type":{"Builtin":{"I32":[]}},"name":{"some":"Id"}},{"algebraic_type":{"Builtin":{"String":[]}},"name":{"some":"Name"}},{"algebraic_type":{"Builtin":{"I32":[]}},"name":{"some":"Age"}}]}}]}
            // ##################################################################
            
            // Initialize the list to store nicknames
            try
            {
                this.EntityStructure = JsonConvert.DeserializeObject<EntityStructure>(CliOutput);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Error: {e}");
                throw;
            }
        }
    }
}