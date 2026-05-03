using System.Linq;
using System.Collections.Generic;

namespace Oxide.Plugins;

[Info("No Unauthorised Access", "&anhe", "1.2.5")]
[Description("Prevents players from accessing other players’ belongings.")]
public class NoUnauthorisedAccess : RustPlugin
{
    #region Authorisation Check

    private object IsAuthorised(BasePlayer player, BaseEntity entity) =>
        // Allow if
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

    private object IsAuthorised(BasePlayer player, ulong ownerId) =>
        // Allow if
        (
            // World
            ownerId == 0 ||
            // Yours
            ownerId == player.userID ||
            // Team’s
            RelationshipManager.ServerInstance.FindTeam(player.currentTeam)
                ?.members.Contains(ownerId) == true ||
            // Admin
            player.IsAdmin
        )
            ? null : false;

    #endregion

    #region Generic Entities

    private object CanLootEntity(BasePlayer player, BaseEntity entity) =>
        entity.ShortPrefabName.Contains("mailbox.deployed")
            ? null : IsAuthorised(player, entity);
    
    private bool CanPickupEntity(BasePlayer player, BaseEntity entity) =>
        IsAuthorised(player, entity) == null
            ? true : false;

    private object OnOvenToggle(BaseOven oven, BasePlayer player) =>
        IsAuthorised(player, oven);

    #endregion

    #region Cupboard

    private object OnCupboardAuthorize(BuildingPrivlidge cupboard, BasePlayer player) =>
        // Allow if
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
    
    #endregion
    
    #region Backpacks

    private Dictionary<ItemId, ulong> dropOwners = new Dictionary<ItemId, ulong>();

    private bool IsBackpack(Item item) =>
        item?.info?.shortname.Contains("backpack") == true;

    private void OnItemDropped(Item item, BaseEntity entity)
    {
        if (
            IsBackpack(item) &&
            item.GetOwnerPlayer() is {} owner
        )
            dropOwners[item.uid] = owner.userID;
    }

    private object OnItemPickup(Item item, BasePlayer player)
    {
        ulong ownerId;
        if (
            IsBackpack(item) &&
            dropOwners.TryGetValue(item.uid, out ownerId)
        ) {
            dropOwners.Remove(item.uid);
            return IsAuthorised(player, ownerId);
        }
        
        return null;
    }

    #endregion

    #region Plants

    private object OnRemoveDying(GrowableEntity plant, BasePlayer player) =>
        IsAuthorised(player, plant);

    private object OnGrowableGather(GrowableEntity plant, BasePlayer player) =>
        IsAuthorised(player, plant);

    private object CanTakeCutting(BasePlayer player, GrowableEntity plant) =>
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

    #region Racked Weapons

    private object OnRackedWeaponSwap(Item local0, WeaponRackSlot local2, BasePlayer player, WeaponRack weaponRack) =>
        IsAuthorised(player, weaponRack);

    private object OnRackedWeaponTake(Item local1, BasePlayer player, WeaponRack weaponRack) =>
        IsAuthorised(player, weaponRack);

    private object OnRackedWeaponLoad(Item local4, ItemDefinition local7, BasePlayer player, WeaponRack weaponRack) =>
        IsAuthorised(player, weaponRack);

    private object OnRackedWeaponUnload(Item local1, BasePlayer player, WeaponRack weaponRack) =>
        IsAuthorised(player, weaponRack);

    private object OnRackedWeaponMount(Item item, BasePlayer player, WeaponRack weaponRack) =>
        IsAuthorised(player, weaponRack);

    // Carbon Hooks

    private object CanPlaceOnRack(WeaponRack rack, BasePlayer player, Item item, int gridCellIndex, int rotation) =>
        IsAuthorised(player, rack);

    private object CanPickupFromRack(WeaponRack rack, BasePlayer player, Item item, int mountSlotIndex, int playerBeltIndex, bool tryHold) =>
        IsAuthorised(player, rack);

    private object CanPickupAllFromRack(WeaponRack rack, BasePlayer player, int mountSlotIndex) =>
        IsAuthorised(player, rack);

    #endregion
}