namespace Hermes.Modules.Services
{
    /// <summary>
    /// Status of the command
    /// </summary>
    public enum CommandStatus
    {
        /// <summary>
        /// The command executed successfully
        /// </summary>
        Success,
        /// <summary>
        /// There was an error with the execution, look at the exception in <see cref="CustomCommandService.CommandResult.Exception"/>
        /// </summary>
        Error,
        /// <summary>
        /// Could not find a command, if this is a mistake check if you have the <see cref="DiscordCommand"/> attribute attached to the command
        /// </summary>
        NotFound,
        /// <summary>
        /// The command was found but there was not enough parameters passed for the command to execute correctly
        /// </summary>
        NotEnoughParams,
        /// <summary>
        /// The parameters were there but they were unable to be parsed to the correct type
        /// </summary>
        InvalidParams,
        /// <summary>
        /// If the user has incorrect permissions to execute the command
        /// </summary>
        InvalidPermissions,
        Disabled,
        /// <summary>
        /// Something happend that shouldn't have, i dont know what to say here other than :/
        /// </summary>
        MissingGuildPermission,
        Unknown,
        BotMissingPermissions,
        /// <summary>
        /// Server isnt RoleX premium for some reason
        /// </summary>
        ServerNotPremium,
        /// <summary>
        /// Server is on Cooldown
        /// </summary>
        OnCooldown
    }
}