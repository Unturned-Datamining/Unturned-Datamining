using System.Collections.Generic;
using SDG.NetPak;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class PlayerInputPacket
{
    public struct ClientRaycast
    {
        public RaycastInfo info;

        public ERaycastInfoUsage usage;

        public ClientRaycast(RaycastInfo info, ERaycastInfoUsage usage)
        {
            this.info = info;
            this.usage = usage;
        }
    }

    /// <summary>
    /// Worst case scenario, maybe shotgun hit or fast spray SMG.
    /// </summary>
    private static int MAX_CLIENTSIDE_INPUTS = 16;

    public List<ClientRaycast> clientsideInputs;

    public Queue<InputInfo> serversideInputs;

    public uint clientSimulationFrameNumber;

    public int recov;

    public ushort keys;

    public EAttackInputFlags primaryAttack;

    public EAttackInputFlags secondaryAttack;

    public virtual void read(SteamChannel channel, NetPakReader reader)
    {
        reader.ReadUInt32(out clientSimulationFrameNumber);
        reader.ReadInt32(out recov);
        reader.ReadUInt16(out keys);
        reader.ReadBits(2, out var value);
        if ((value & 1) == 1)
        {
            primaryAttack |= EAttackInputFlags.Start;
        }
        if ((value & 2) == 2)
        {
            primaryAttack |= EAttackInputFlags.Stop;
        }
        reader.ReadBits(2, out var value2);
        if ((value2 & 1) == 1)
        {
            secondaryAttack |= EAttackInputFlags.Start;
        }
        if ((value2 & 2) == 2)
        {
            secondaryAttack |= EAttackInputFlags.Stop;
        }
        reader.ReadUInt8(out var value3);
        int num = value3;
        if (num <= 0)
        {
            return;
        }
        num = Mathf.Min(num, MAX_CLIENTSIDE_INPUTS);
        serversideInputs = new Queue<InputInfo>(num);
        for (int i = 0; i < num; i++)
        {
            InputInfo inputInfo = new InputInfo();
            reader.ReadEnum(out inputInfo.type);
            switch (inputInfo.type)
            {
            case ERaycastInfoType.NONE:
                reader.ReadEnum(out inputInfo.usage);
                reader.ReadClampedVector3(out inputInfo.point);
                reader.ReadNormalVector3(out inputInfo.normal);
                reader.ReadString(out inputInfo.materialName, 6);
                inputInfo.material = PhysicsTool.GetLegacyMaterialByName(inputInfo.materialName);
                break;
            case ERaycastInfoType.SKIP:
                inputInfo = null;
                break;
            case ERaycastInfoType.PLAYER:
            {
                reader.ReadEnum(out inputInfo.usage);
                reader.ReadClampedVector3(out inputInfo.point);
                reader.ReadNormalVector3(out inputInfo.direction);
                reader.ReadNormalVector3(out inputInfo.normal);
                reader.ReadEnum(out inputInfo.limb);
                reader.ReadSteamID(out CSteamID value9);
                Player player = PlayerTool.getPlayer(value9);
                if (player != null)
                {
                    float num2 = 256f;
                    if (player.movement.getVehicle() != null)
                    {
                        num2 = 512f;
                    }
                    if ((inputInfo.point - player.transform.position).sqrMagnitude < num2)
                    {
                        inputInfo.materialName = "Flesh_Dynamic";
                        inputInfo.material = EPhysicsMaterial.FLESH_DYNAMIC;
                        inputInfo.player = player;
                        inputInfo.transform = player.transform;
                    }
                    else
                    {
                        inputInfo = null;
                    }
                }
                else
                {
                    inputInfo = null;
                }
                break;
            }
            case ERaycastInfoType.ZOMBIE:
            {
                reader.ReadEnum(out inputInfo.usage);
                reader.ReadClampedVector3(out inputInfo.point);
                reader.ReadNormalVector3(out inputInfo.direction);
                reader.ReadNormalVector3(out inputInfo.normal);
                reader.ReadEnum(out inputInfo.limb);
                reader.ReadUInt16(out var value7);
                Zombie zombie = ZombieManager.getZombie(inputInfo.point, value7);
                if (zombie != null)
                {
                    if (new Vector2(inputInfo.point.x - zombie.transform.position.x, inputInfo.point.z - zombie.transform.position.z).sqrMagnitude < 256f)
                    {
                        if (zombie.isRadioactive)
                        {
                            inputInfo.materialName = "Alien_Dynamic";
                            inputInfo.material = EPhysicsMaterial.ALIEN_DYNAMIC;
                        }
                        else
                        {
                            inputInfo.materialName = "Flesh_Dynamic";
                            inputInfo.material = EPhysicsMaterial.FLESH_DYNAMIC;
                        }
                        inputInfo.zombie = zombie;
                        inputInfo.transform = zombie.transform;
                    }
                    else
                    {
                        inputInfo = null;
                    }
                }
                else
                {
                    inputInfo = null;
                }
                break;
            }
            case ERaycastInfoType.ANIMAL:
            {
                reader.ReadEnum(out inputInfo.usage);
                reader.ReadClampedVector3(out inputInfo.point);
                reader.ReadNormalVector3(out inputInfo.direction);
                reader.ReadNormalVector3(out inputInfo.normal);
                reader.ReadEnum(out inputInfo.limb);
                reader.ReadUInt16(out var value14);
                Animal animal = AnimalManager.getAnimal(value14);
                if (animal != null && (inputInfo.point - animal.transform.position).sqrMagnitude < 256f)
                {
                    inputInfo.materialName = "Flesh_Dynamic";
                    inputInfo.material = EPhysicsMaterial.FLESH_DYNAMIC;
                    inputInfo.animal = animal;
                    inputInfo.transform = animal.transform;
                }
                else
                {
                    inputInfo = null;
                }
                break;
            }
            case ERaycastInfoType.VEHICLE:
            {
                reader.ReadEnum(out inputInfo.usage);
                reader.ReadClampedVector3(out inputInfo.point);
                reader.ReadNormalVector3(out inputInfo.normal);
                reader.ReadString(out inputInfo.materialName, 6);
                inputInfo.material = PhysicsTool.GetLegacyMaterialByName(inputInfo.materialName);
                reader.ReadUInt32(out var value8);
                reader.ReadTransform(out inputInfo.colliderTransform);
                InteractableVehicle interactableVehicle = VehicleManager.findVehicleByNetInstanceID(value8);
                if (interactableVehicle != null && (interactableVehicle == channel.owner.player.movement.getVehicle() || (inputInfo.point - interactableVehicle.transform.position).sqrMagnitude < 4096f))
                {
                    inputInfo.vehicle = interactableVehicle;
                    inputInfo.transform = interactableVehicle.transform;
                }
                else
                {
                    inputInfo = null;
                }
                break;
            }
            case ERaycastInfoType.BARRICADE:
            {
                reader.ReadEnum(out inputInfo.usage);
                reader.ReadClampedVector3(out inputInfo.point);
                reader.ReadNormalVector3(out inputInfo.normal);
                reader.ReadString(out inputInfo.materialName, 6);
                inputInfo.material = PhysicsTool.GetLegacyMaterialByName(inputInfo.materialName);
                reader.ReadNetId(out var value13);
                reader.ReadTransform(out inputInfo.colliderTransform);
                BarricadeDrop barricadeDrop = NetIdRegistry.Get<BarricadeDrop>(value13);
                if (barricadeDrop != null)
                {
                    Transform model = barricadeDrop.model;
                    if (model != null && (inputInfo.point - model.position).sqrMagnitude < 256f)
                    {
                        inputInfo.transform = model;
                    }
                    else
                    {
                        inputInfo = null;
                    }
                }
                else
                {
                    inputInfo = null;
                }
                break;
            }
            case ERaycastInfoType.STRUCTURE:
            {
                reader.ReadEnum(out inputInfo.usage);
                reader.ReadClampedVector3(out inputInfo.point);
                reader.ReadNormalVector3(out inputInfo.direction);
                reader.ReadNormalVector3(out inputInfo.normal);
                reader.ReadString(out inputInfo.materialName, 6);
                inputInfo.material = PhysicsTool.GetLegacyMaterialByName(inputInfo.materialName);
                reader.ReadNetId(out var value15);
                reader.ReadTransform(out inputInfo.colliderTransform);
                StructureDrop structureDrop = NetIdRegistry.Get<StructureDrop>(value15);
                if (structureDrop != null)
                {
                    Transform model2 = structureDrop.model;
                    if (model2 != null && (inputInfo.point - model2.position).sqrMagnitude < 256f)
                    {
                        inputInfo.transform = model2;
                    }
                    else
                    {
                        inputInfo = null;
                    }
                }
                else
                {
                    inputInfo = null;
                }
                break;
            }
            case ERaycastInfoType.RESOURCE:
            {
                reader.ReadEnum(out inputInfo.usage);
                reader.ReadClampedVector3(out inputInfo.point);
                reader.ReadNormalVector3(out inputInfo.direction);
                reader.ReadNormalVector3(out inputInfo.normal);
                reader.ReadString(out inputInfo.materialName, 6);
                inputInfo.material = PhysicsTool.GetLegacyMaterialByName(inputInfo.materialName);
                reader.ReadUInt8(out var value10);
                reader.ReadUInt8(out var value11);
                reader.ReadUInt16(out var value12);
                reader.ReadTransform(out inputInfo.colliderTransform);
                Transform resource = ResourceManager.getResource(value10, value11, value12);
                if (resource != null && (inputInfo.point - resource.transform.position).sqrMagnitude < 256f)
                {
                    inputInfo.transform = resource;
                }
                else
                {
                    inputInfo = null;
                }
                break;
            }
            case ERaycastInfoType.OBJECT:
            {
                reader.ReadEnum(out inputInfo.usage);
                reader.ReadClampedVector3(out inputInfo.point);
                reader.ReadNormalVector3(out inputInfo.direction);
                reader.ReadNormalVector3(out inputInfo.normal);
                reader.ReadString(out inputInfo.materialName, 6);
                inputInfo.material = PhysicsTool.GetLegacyMaterialByName(inputInfo.materialName);
                reader.ReadUInt8(out inputInfo.section);
                reader.ReadUInt8(out var value4);
                reader.ReadUInt8(out var value5);
                reader.ReadUInt16(out var value6);
                reader.ReadTransform(out inputInfo.colliderTransform);
                LevelObject @object = ObjectManager.getObject(value4, value5, value6);
                if (@object != null && @object.transform != null && (inputInfo.point - @object.transform.position).sqrMagnitude < 256f)
                {
                    inputInfo.transform = @object.transform;
                }
                else
                {
                    inputInfo.type = ERaycastInfoType.NONE;
                }
                break;
            }
            }
            if (inputInfo != null)
            {
                serversideInputs.Enqueue(inputInfo);
            }
        }
    }

    public virtual void write(NetPakWriter writer)
    {
        writer.WriteUInt32(clientSimulationFrameNumber);
        writer.WriteInt32(recov);
        writer.WriteUInt16(keys);
        writer.WriteBits((uint)primaryAttack, 2);
        writer.WriteBits((uint)secondaryAttack, 2);
        if (clientsideInputs == null)
        {
            writer.WriteUInt8(0);
            return;
        }
        int num = clientsideInputs.Count;
        if (num > MAX_CLIENTSIDE_INPUTS)
        {
            UnturnedLog.warn("Discarding excessive hit inputs {0}/{1}", num, MAX_CLIENTSIDE_INPUTS);
            num = MAX_CLIENTSIDE_INPUTS;
        }
        writer.WriteUInt8((byte)num);
        for (int i = 0; i < num; i++)
        {
            RaycastInfo info = clientsideInputs[i].info;
            ERaycastInfoUsage usage = clientsideInputs[i].usage;
            if (info.player != null)
            {
                writer.WriteEnum(ERaycastInfoType.PLAYER);
                writer.WriteEnum(usage);
                writer.WriteClampedVector3(info.point);
                writer.WriteNormalVector3(info.direction);
                writer.WriteNormalVector3(info.normal);
                writer.WriteEnum(info.limb);
                writer.WriteSteamID(info.player.channel.owner.playerID.steamID);
            }
            else if (info.zombie != null)
            {
                writer.WriteEnum(ERaycastInfoType.ZOMBIE);
                writer.WriteEnum(usage);
                writer.WriteClampedVector3(info.point);
                writer.WriteNormalVector3(info.direction);
                writer.WriteNormalVector3(info.normal);
                writer.WriteEnum(info.limb);
                writer.WriteUInt16(info.zombie.id);
            }
            else if (info.animal != null)
            {
                writer.WriteEnum(ERaycastInfoType.ANIMAL);
                writer.WriteEnum(usage);
                writer.WriteClampedVector3(info.point);
                writer.WriteNormalVector3(info.direction);
                writer.WriteNormalVector3(info.normal);
                writer.WriteEnum(info.limb);
                writer.WriteUInt16(info.animal.index);
            }
            else if (info.vehicle != null)
            {
                writer.WriteEnum(ERaycastInfoType.VEHICLE);
                writer.WriteEnum(usage);
                writer.WriteClampedVector3(info.point);
                writer.WriteNormalVector3(info.normal);
                writer.WriteString(info.materialName, 6);
                writer.WriteUInt32(info.vehicle.instanceID);
                writer.WriteTransform(info.collider?.transform);
            }
            else if (info.transform != null)
            {
                if (info.transform.CompareTag("Barricade"))
                {
                    writer.WriteEnum(ERaycastInfoType.BARRICADE);
                    writer.WriteEnum(usage);
                    info.transform = DamageTool.getBarricadeRootTransform(info.transform);
                    BarricadeDrop barricadeDrop = BarricadeManager.FindBarricadeByRootTransform(info.transform);
                    if (barricadeDrop != null)
                    {
                        writer.WriteClampedVector3(info.point);
                        writer.WriteNormalVector3(info.normal);
                        writer.WriteString(info.materialName, 6);
                        writer.WriteNetId(barricadeDrop.GetNetId());
                    }
                    else
                    {
                        writer.WriteClampedVector3(Vector3.zero);
                        writer.WriteNormalVector3(Vector3.up);
                        writer.WriteString(null, 6);
                        writer.WriteNetId(NetId.INVALID);
                    }
                    writer.WriteTransform(info.collider?.transform);
                }
                else if (info.transform.CompareTag("Structure"))
                {
                    writer.WriteEnum(ERaycastInfoType.STRUCTURE);
                    writer.WriteEnum(usage);
                    info.transform = DamageTool.getStructureRootTransform(info.transform);
                    StructureDrop structureDrop = StructureManager.FindStructureByRootTransform(info.transform);
                    if (structureDrop != null)
                    {
                        writer.WriteClampedVector3(info.point);
                        writer.WriteNormalVector3(info.direction);
                        writer.WriteNormalVector3(info.normal);
                        writer.WriteString(info.materialName, 6);
                        writer.WriteNetId(structureDrop.GetNetId());
                    }
                    else
                    {
                        writer.WriteClampedVector3(Vector3.zero);
                        writer.WriteNormalVector3(Vector3.up);
                        writer.WriteNormalVector3(Vector3.up);
                        writer.WriteString(null, 6);
                        writer.WriteNetId(NetId.INVALID);
                    }
                    writer.WriteTransform(info.collider?.transform);
                }
                else if (info.transform.CompareTag("Resource"))
                {
                    writer.WriteEnum(ERaycastInfoType.RESOURCE);
                    writer.WriteEnum(usage);
                    info.transform = DamageTool.getResourceRootTransform(info.transform);
                    if (ResourceManager.tryGetRegion(info.transform, out var x, out var y, out var index))
                    {
                        writer.WriteClampedVector3(info.point);
                        writer.WriteNormalVector3(info.direction);
                        writer.WriteNormalVector3(info.normal);
                        writer.WriteString(info.materialName, 6);
                        writer.WriteUInt8(x);
                        writer.WriteUInt8(y);
                        writer.WriteUInt16(index);
                    }
                    else
                    {
                        writer.WriteClampedVector3(Vector3.zero);
                        writer.WriteNormalVector3(Vector3.up);
                        writer.WriteNormalVector3(Vector3.up);
                        writer.WriteString(null, 6);
                        writer.WriteUInt8(0);
                        writer.WriteUInt8(0);
                        writer.WriteUInt16(ushort.MaxValue);
                    }
                    writer.WriteTransform(info.collider?.transform);
                }
                else if (info.transform.CompareTag("Small") || info.transform.CompareTag("Medium") || info.transform.CompareTag("Large"))
                {
                    writer.WriteEnum(ERaycastInfoType.OBJECT);
                    writer.WriteEnum(usage);
                    InteractableObjectRubble componentInParent = info.transform.GetComponentInParent<InteractableObjectRubble>();
                    if (componentInParent != null)
                    {
                        info.transform = componentInParent.transform;
                        info.section = componentInParent.getSection(info.collider.transform);
                    }
                    if (ObjectManager.tryGetRegion(info.transform, out var x2, out var y2, out var index2))
                    {
                        writer.WriteClampedVector3(info.point);
                        writer.WriteNormalVector3(info.direction);
                        writer.WriteNormalVector3(info.normal);
                        writer.WriteString(info.materialName, 6);
                        writer.WriteUInt8(info.section);
                        writer.WriteUInt8(x2);
                        writer.WriteUInt8(y2);
                        writer.WriteUInt16(index2);
                    }
                    else
                    {
                        writer.WriteClampedVector3(Vector3.zero);
                        writer.WriteNormalVector3(Vector3.up);
                        writer.WriteNormalVector3(Vector3.up);
                        writer.WriteString(null, 6);
                        writer.WriteUInt8(byte.MaxValue);
                        writer.WriteUInt8(0);
                        writer.WriteUInt8(0);
                        writer.WriteUInt16(ushort.MaxValue);
                    }
                    writer.WriteTransform(info.collider?.transform);
                }
                else if (info.transform.CompareTag("Ground") || info.transform.CompareTag("Environment"))
                {
                    writer.WriteEnum(ERaycastInfoType.NONE);
                    writer.WriteEnum(usage);
                    writer.WriteClampedVector3(info.point);
                    writer.WriteNormalVector3(info.normal);
                    writer.WriteString(info.materialName, 6);
                }
                else
                {
                    writer.WriteEnum(ERaycastInfoType.SKIP);
                }
            }
            else
            {
                writer.WriteEnum(ERaycastInfoType.SKIP);
            }
        }
    }
}
