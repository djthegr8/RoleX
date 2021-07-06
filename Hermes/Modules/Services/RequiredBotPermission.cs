using System;
using Discord;

namespace Hermes.Modules.Services
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RequiredBotPermission : Attribute
    {
        public RequiredBotPermission(params GuildPermission[] perms)
        {
            Permissions = perms;
        }

        public GuildPermission[] Permissions { get; set; }
    }
}