﻿using System;

namespace Hermes.Modules.Services
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class Alt : Attribute
    {
        public string alt { get; set; }
        public Alt(string Alt)
        {
            alt = Alt;
        }
    }
}