using System.Collections.Generic;
using System.Linq;

namespace SpacetimeDB.Editor
{
    /// Contains parsed reducers Entity info from`spacetime describe`
    /// Contains friendly syntax hints for end-users
    public class ReducerInfo
    {
        /// Raw Entity structure
        public EntityStructure.Entity ReducerEntity { get; }
        
        /// <returns>a raw list of names with CSV "{names}:{type}" to show end-users</returns>
        /// <remarks>See GetNormalizedStyledSyntaxHints() || GetNormalizedTypesCsv() for parsing opts</remarks>
        public List<string> RawSyntaxHints { get; }
        
        
        #region Common Shortcuts
        public string GetReducerName() => ReducerEntity.Schema.SchemaName;

        /// Eg: [ {firstName:string},{age:int} ]"
        /// - Wraps syntax hints with 'nobr'
        /// - For unstyled, just get the RawSyntaxHints list.
        public List<string> GetNormalizedStyledSyntaxHints()
        {
            const string nameColor = ""; // Use the default label color
            string bracketColor = $"<color={SpacetimeMeta.ERROR_COLOR_HEX}>"; 
            const string typeColor = "<color=white>";
            
            // "Eg: "<color=red>{</color>firstName</color><color=blue>:string</color><color=red>}</color>"
            return GetNormalizedSyntaxHints().Select(s => 
                $"<nobr>{bracketColor}{{</color>{nameColor}{s}" // Color the name and bracket
                    .Replace(":", $"</color>{typeColor}:") // Keep type color open for the entire type
                    + $"</color>{bracketColor}}}</color></nobr>") // Close off the blue tag and color the bracket
                    .ToList();
        }
        
        /// Lowercases the types -> replaces known Rust types with C# aliased types
        public List<string> GetNormalizedSyntaxHints() => RawSyntaxHints
            .Select(s => s
                // Simple type: "String", "Bool", "I32", "U8", "U32", "U64", "F32" - before ToLower()
                .ToLowerInvariant()
                .Replace("u8", "byte")
                .Replace("i16", "short")
                .Replace("i32", "int")
                .Replace("u32", "uint")
                .Replace("u64", "ulong")
                .Replace("f32", "float")
                .Replace("i64", "long")
            ).ToList();
        
        /// Lowercases the types -> replaces known Rust types with C# types
        public List<string> GetNormalizedTypesCsv() => GetNormalizedSyntaxHints()
            .Select(s => s
                .Split(":")
                .Last())
            .ToList();
        #endregion // Common Shortcuts
        
        
        /// Sets { ReducerEntity, RawSyntaxHints }
        public ReducerInfo(EntityStructure.Entity entity)
        {
            this.ReducerEntity = entity;

            string argName, argType;
            this.RawSyntaxHints = entity.Schema.Elements
                .Select(getSyntaxHintFromSchemaElement)
                .ToList();
        }

        private string getSyntaxHintFromSchemaElement(EntityStructure.Element element)
        {
             try
             {
                 string argName = element.ElementName.First().Value;
                 EntityStructure.AlgebraicType algebraicType = element.AlgebraicType;
                
                 // Type will only have 1 child: Builtin || CustomRefNum
                 bool isCustomType = algebraicType.CustomRefNum != null;
                 if (isCustomType)
                 {
                     // The actual custom type is simply a numbered ref, eg: { "Ref": 12 }
                     // We actually have the data, but don't want to go that deep: Just return "{}"
                     return $"{argName}:{{}}";
                 }

                 KeyValuePair<string, object> argTypeNested1_Builtin = algebraicType.Builtin.FirstOrDefault();
                 string argTypeNested1Key = argTypeNested1_Builtin.Key;

                 bool isArrayType = argTypeNested1Key == "Array";
                 bool isKnownSimpleType = !isArrayType && checkIsSimpleArgType(argTypeNested1Key);
                
                 string argType;
                 if (isKnownSimpleType)
                 {
                     // Simple type: "String", "Bool", "I32", "U8", "U32", "U64", "F32"
                     argType = argTypeNested1_Builtin.Key.ToLowerInvariant();
                 }
                 else if (isArrayType)
                 {
                     // Go deeper to get the Array type
                     var argTypeNested2_Builtin = (KeyValuePair<string, object>)argTypeNested1_Builtin.Value;
                     string arrayType = getArrayArgType(argTypeNested2_Builtin);
                     argType = $"{arrayType}[]"; // eg: "U64[]"
                 }
                 else
                 {
                     // Too deep or custom - let's just get the gist of it
                     argType = $"{argTypeNested1_Builtin.Key.ToLowerInvariant()}{{}}"; // eg: "CustomType{}"
                 }
            
                 return $"{argName}:{argType}";
             }
             catch (System.Exception e)
             {
                 string elemName = element.ElementName.First().Value;
                 UnityEngine.Debug.LogError("Error: Failed to parse element " +
                     $"{{ \"some\" : \"{elemName}\" }}` - returning 'elemName{{}}' as type. " +
                     $"Err: {e.Message}");

                 return $"{elemName}{{}}"; // eg: "SomeType{}" 
             }
        }

        /// Simple type: "String", "Bool", "I32", "U8", "U32", "U64", "F32"
        private bool checkIsSimpleArgType(string argType) => argType.ToLowerInvariant() is 
            "string" or "bool" or "i32" or "u8" or "u32" or "u64" or "f32";

        /// <returns>Just the Array type name. Eg: "U64"</returns>
        private string getArrayArgType(KeyValuePair<string, object> argTypeNested2_Builtin)
        {
            // ########################################################
            // argTypeNested1:
            // {
            //   "Array":
            //     {
            //       "Builtin": // argTypeNested1_Builtin
            //       {
            //         "U64": [] // argTypeNested3_DynamicType // (!) Could go even deeper
            //       }
            //      }
            // }
            // ########################################################
            // Read as: Array of U64; parsing into "someType[]"
            // ########################################################
            string argType = null;

            if (argTypeNested2_Builtin.Value is KeyValuePair<string, object> argTypeNested3_DynamicType)
            {
                // Eg: { "U64": [] }
                argType = argTypeNested3_DynamicType.Key.ToLowerInvariant();
            }

            return argType;
        }
    }
}
