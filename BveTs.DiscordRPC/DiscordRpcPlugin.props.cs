using System.Reflection;

namespace BveTs.DiscordRPC;

public partial class DiscordRpcPlugin
{
	private static readonly Assembly currentAssembly = Assembly.GetExecutingAssembly();

	public override string Location => currentAssembly.Location;

	public override string Name => currentAssembly.GetName().Name ?? "BveTs.DiscordRPC";

	public override string Title => "Discord Rich Presence for BVE";

	public override string Version => currentAssembly.GetName().Version?.ToString() ?? "1.0.0";

	public override string Description => "Displays BVE Trainsim driving information on Discord Rich Presence";

	public override string Copyright => "Copyright 2024 Tetsu Otter";
}
