namespace Lagrange.Core.Common.Interface;

public static class BotFactory
{
    /// <summary>
    /// Create new Bot from existing <see cref="BotKeystore"/>
    /// </summary>
    /// <param name="config">The config for Bot</param>
    /// <param name="keystore">Existing Keystore from deserialization</param>
    /// <returns>Created BotContext Instance</returns>
    public static BotContext Create(BotConfig config, BotKeystore keystore) => 
        new(config, keystore, BotAppInfo.ProtocolToAppInfo[config.Protocol]);
    
    /// <summary>
    /// Create new Bot from Empty
    /// </summary>
    /// <param name="config">The config for Bot</param>
    /// <returns>Created BotContext Instance</returns>
    public static BotContext Create(BotConfig config) => 
        new(config, BotKeystore.CreateEmpty(), BotAppInfo.ProtocolToAppInfo[config.Protocol]);
}