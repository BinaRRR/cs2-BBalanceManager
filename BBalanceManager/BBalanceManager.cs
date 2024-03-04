using System.Text.Json.Serialization;
using System.Xml;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Extensions;
using CounterStrikeSharp.API.Modules.Utils;

namespace BBalanceManager;

public class SettingsConfig : BasePluginConfig
{
    [JsonPropertyName("AcceptableTeamDifference")]
    public int AcceptableTeamDifference { get; set; } = 1;

    [JsonPropertyName("MoveBestPlayer")]
    public bool MoveBestPlayer { get; set; } = false;
}

public class BBalanceManager : BasePlugin, IPluginConfig<SettingsConfig>
{
    public override string ModuleName => "BBalanceManager";
    public override string ModuleAuthor => "BinaR";
    public override string ModuleDescription => "Balances both teams to provide a fair gameplay";
    public override string ModuleVersion => "alpha-1";
    
    private const char Default = '\x01';
    private const char Green = '\x04';

    public static string PluginTag => $" {Green}[{Default}BalanceManager{Green}]{Default} ";

    public override void Load(bool hotReload)
    {
        
    }

    public SettingsConfig Config { get; set; }

    public void OnConfigParsed(SettingsConfig config)
    {
        Config = config;
    }

    [GameEventHandler]
    public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        Console.WriteLine("works");
        List<CCSPlayerController> CTplayers = new();
        List<CCSPlayerController> TTplayers = new();
        foreach (CCSPlayerController player in Utilities.GetPlayers())
        {
            if (player.Team == CsTeam.Terrorist)
            {
                TTplayers.Add(player);
            }

            if (player.Team == CsTeam.CounterTerrorist)
            {
                CTplayers.Add(player);
            }
        }
        //Check for the difference in team sizes.
        int playersDifference = Math.Abs(CTplayers.Count - TTplayers.Count);

        if (Config.AcceptableTeamDifference >= playersDifference)
        {
            PrintToChatAll(PluginTag + $"Drużyny są wyrównane, nie wprowadzono zmian.");
            Console.WriteLine(PluginTag + $"Drużyny są wyrównane. CT: {CTplayers.Count} | TT: {TTplayers.Count} | Różnica: {playersDifference} | Akceptowalna różnica: {Config.AcceptableTeamDifference}");
            return HookResult.Continue;
        }

        int numberOfPlayersToMove = (int)Math.Ceiling((playersDifference - Config.AcceptableTeamDifference / 2.0));
        if (numberOfPlayersToMove <= 0)
        {
            PrintToChatAll(PluginTag + $"Drużyny są wyrównane, nie wprowadzono zmian.");
            Console.WriteLine(PluginTag + $"Drużyny są wyrównane. CT: {CTplayers.Count} | TT: {TTplayers.Count} | Różnica: {playersDifference} | Akceptowalna różnica: {Config.AcceptableTeamDifference}");
            return HookResult.Continue;
        }
        
        //Get players from higher player count team and move them
        List<CCSPlayerController> movedPlayers = new();
        if (CTplayers.Count > TTplayers.Count)
        {
            foreach (CCSPlayerController player in CTplayers.OrderByDescending(p => p.Score).Take(numberOfPlayersToMove))
            {
                player.SwitchTeam(CsTeam.Terrorist);
                movedPlayers.Add(player);
            }
        }
        else
        {
            foreach (CCSPlayerController player in TTplayers.OrderByDescending(p => p.Score).Take(numberOfPlayersToMove))
            {
                player.SwitchTeam(CsTeam.CounterTerrorist);
                movedPlayers.Add(player);
            }
        }
        
        Console.WriteLine(PluginTag + $"Przeniesiono {Green}{string.Join(", ", movedPlayers)} {Default}do przeciwnej drużyny w celu wyrównania rozgrywki.");
        
        PrintToChatAll(PluginTag + $"Drużyny zostały wyrównane. Przeniesiono {Green}{string.Join(", ", movedPlayers)} {Default}do przeciwnej drużyny w celu wyrównania rozgrywki.");
        Console.WriteLine(PluginTag + $"Drużyny zostały wyrównane. CT: {CTplayers.Count} | TT: {TTplayers.Count} | Różnica: {playersDifference} | Akceptowalna różnica: {Config.AcceptableTeamDifference}");
        
        return HookResult.Continue;
    }

    private static void PrintToChatAll(string message)
    {
        Utilities.GetPlayers().ForEach(player => player.PrintToChat(message));
    }
}