using System;
using Discord;

namespace Hermes.Modules.Services
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RequiredBotPermission : Attribute
    {
        public GuildPermission[] Permissions { get; set; }
        public RequiredBotPermission(params GuildPermission[] perms)
            => Permissions = perms;
    }
}