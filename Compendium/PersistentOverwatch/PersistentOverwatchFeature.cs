using Compendium.Events;
using Compendium.Features;
using Compendium.IO.Saving;
using PlayerRoles;
using PluginAPI.Events;

namespace Compendium.PersistentOverwatch;

public class PersistentOverwatchFeature : FeatureBase
{
	public override string Name => "Persistent Overwatch";

	public static SaveFile<CollectionSaveData<string>> Storage { get; set; }

	public override void Load()
	{
		base.Load();
		Storage = new SaveFile<CollectionSaveData<string>>(Directories.GetDataPath("SavedOverwatchPlayers", "overwatchPlayers"));
		PlayerRoleManager.OnRoleChanged += OnRoleChanged;
		FLog.Info("Overwatch storage loaded.");
	}

	public override void Reload()
	{
		Storage?.Load();
		FLog.Info("Reloaded.");
	}

	public override void Unload()
	{
		base.Unload();
		Storage?.Save();
		Storage = null;
		PlayerRoleManager.OnRoleChanged -= OnRoleChanged;
		FLog.Info("Unloaded.");
	}

	private static void OnRoleChanged(ReferenceHub hub, PlayerRoleBase prevRole, PlayerRoleBase newRole)
	{
		if (!FeatureManager.GetFeature<PersistentOverwatchFeature>().IsEnabled || Storage == null)
		{
			return;
		}
		if (prevRole.RoleTypeId == RoleTypeId.Overwatch)
		{
			if (newRole.RoleTypeId != RoleTypeId.Overwatch && Storage.Data.Remove(hub.UserId()))
			{
				Storage.Save();
				hub.Hint("\n\n<b>Persistent Overwatch is now <color=#FF0000>disabled</color>.");
			}
		}
		else if (newRole.RoleTypeId == RoleTypeId.Overwatch && !Storage.Data.Contains(hub.UserId()))
		{
			Storage.Data.Add(hub.UserId());
			Storage.Save();
			hub.Hint("\n\n<b>Persistent Overwatch is now <color=#90FF33>active</color>.</b>");
		}
	}

	[Event]
	private static void OnPlayerJoined(PlayerJoinedEvent ev)
	{
		if (Storage != null && Storage.Data.Contains(ev.Player.UserId))
		{
			Calls.Delay(0.7f, delegate
			{
				ev.Player.SetRole(RoleTypeId.Overwatch);
				ev.Player.ReferenceHub.Hint("\n\n<b><color=#33FFA5>[Persistent Overwatch]</color></b>\n<b>Role changed to <color=#90FF33>Overwatch</color>.</b>", 3f);
			});
		}
	}
}
