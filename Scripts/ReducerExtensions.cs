﻿using System;

namespace SpacetimeDB
{
    public class ReducerClassAttribute : Attribute
    {
    }

    public class ReducerCallbackAttribute : Attribute
    {
        public string FunctionName { get; set; }
    }

    public class DeserializeEventAttribute : Attribute
    {
        public string FunctionName { get; set; }
    }
}
