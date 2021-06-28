using System;
using System.Collections.Generic;
using System.ComponentModel;
using Discord.Commands;

namespace RoleX.Modules.Services
{
    /// <summary>
    /// The settings for the Command Service
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// The Default prefix for the command service
        /// </summary>
        public char DefaultPrefix { get; set; }
        /// <summary>
        /// This method will be called and when a command is called and checkes if the user has permission to execute the command, this result will be in the <see cref="CommandModuleBase.HasExecutePermission"/> if you dont set this, the <see cref="CommandModuleBase.HasExecutePermission"/> will always be <see langword="true"/>
        /// </summary>
        public Func<SocketCommandContext, bool> HasPermissionMethod { get; set; }
        /// <summary>
        /// A Dictionary containing specific permission methods for guilds, The Key would be the Guilds ID, and the value would be a Method that takes <see cref="SocketCommandContext"/> and returns a <see cref="bool"/>. this will overwrite the <see cref="HasPermissionMethod"/> if the guilds permission method is added
        /// </summary>
        public Dictionary<ulong, Func<SocketCommandContext, bool>> CustomGuildPermissionMethod { get; set; }
        /// <summary>
        /// Boolean indicating if commands can be accessable in Direct messages, default value is <see cref="false"/>
        /// </summary>
        [DefaultValue(false)]
        public bool DMCommands { get; set; }
        /// <summary>
        /// If the user has invalid permissions to execute the command and this bool is <see langword="true"/> then the command will still execute with the <see cref="CommandModuleBase.HasExecutePermission"/> set to <see langword="false"/>. Default is <see langword="false"/>
        /// </summary>
        public bool AllowCommandExecutionOnInvalidPermissions { get; set; }
    }
}