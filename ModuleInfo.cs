using Discord.Commands;
using Discord.WebSocket;
using GLaDOSV3.Helpers;
using System;
using System.Reflection;
using System.Runtime.Loader;
using GLaDOSV3.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic.CompilerServices;

namespace GLaDOSV3.Module.Logger
{
    public class ModuleInfo : GladosModule
    {
        public override string Name => "Logger";
        
        public override string Version => "0.0.0.1";

        public override string Author => "BlackOfWorld#8125";
    }
}
