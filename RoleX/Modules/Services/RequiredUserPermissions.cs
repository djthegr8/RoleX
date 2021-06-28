using System;
using Discord;

namespace RoleX.Modules.Services
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RequiredUserPermissions : Attribute
    {
        public GuildPermission[] Permissions { get; set; }

        public RequiredUserPermissions(params GuildPermission[] perms)
            => Permissions = perms;
    }
}