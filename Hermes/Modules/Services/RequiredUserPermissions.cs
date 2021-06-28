using System;
using Discord;

namespace Hermes.Modules.Services
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RequiredUserPermissions : Attribute
    {
        public GuildPermission[] Permissions { get; set; }

        public RequiredUserPermissions(params GuildPermission[] perms)
            => Permissions = perms;
    }
}