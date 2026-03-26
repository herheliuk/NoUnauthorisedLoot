using System.Linq;

namespace Oxide.Plugins;

[Info("No Unauthorised Access", "&anhe", "1.2.2")]
[Description("Prevents players from accessing other players’ belongings.")]
public class NoUnauthorisedAccess : RustPlugin
{
    private object IsAuthorised(BasePlayer player, BaseEntity entity) =>
        (
            // World
            entity.OwnerID == 0 ||
            // Yours
            entity.OwnerID == player.userID ||
            // Team’s
            RelationshipManager.ServerInstance.FindTeam(player.currentTeam)
                ?.members.Contains(entity.OwnerID) == true ||
            // Authorised
            entity.GetBuildingPrivilege()
                ?.IsAuthed(player) == true ||
            // Admin
            player.IsAdmin
        )
            ? null : false;

    private object OnCupboardAuthorize(BuildingPrivlidge cupboard, BasePlayer player) =>
        (
            // Nobody’s
            cupboard.authorizedPlayers.Count == 0 ||
            // Yours
            cupboard.OwnerID == player.userID ||
            // Team’s
            RelationshipManager.ServerInstance.FindTeam(player.currentTeam)
                ?.members.Any(member => cupboard.authorizedPlayers.Contains(member)) == true ||
            // Admin
            player.IsAdmin
        )
            ? null : false;

    private object CanLootEntity(BasePlayer player, BaseEntity entity) =>
        IsAuthorised(player, entity);
    
    #region Plants

    private object OnRemoveDying(GrowableEntity plant, BasePlayer player) =>
        IsAuthorised(player, plant);

    private object OnGrowableGather(GrowableEntity plant, BasePlayer player) =>
        IsAuthorised(player, plant);
    
    #endregion

    #region Electricity

    private object OnButtonPress(PressButton entity, BasePlayer player) =>
        IsAuthorised(player, entity);

    private object OnSwitchToggle(ElectricSwitch entity, BasePlayer player) =>
        IsAuthorised(player, entity);

    private object OnElevatorButtonPress(ElevatorLift entity, BasePlayer player, Elevator.Direction direction, bool toTopOrBottom) =>
        IsAuthorised(player, entity);

    private object OnWireConnect(BasePlayer player, IOEntity ioA, int inputs, IOEntity ioB, int outputs) =>
        (
            IsAuthorised(player, ioA) == null &&
            IsAuthorised(player, ioB) == null
        )
            ? null : false;
    
    private object OnWireClear(BasePlayer player, IOEntity ioA, int clearIndex, IOEntity ioB, bool isInput) =>
        (
            IsAuthorised(player, ioA) == null &&
            IsAuthorised(player, ioB) == null
        )
            ? null : false;

    #endregion
}