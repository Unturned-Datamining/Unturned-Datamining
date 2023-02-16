using System;
using System.Collections.Generic;
using SDG.NetPak;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class PlayerInput : PlayerCaller
{
    public static readonly uint SAMPLES = 4u;

    public static readonly float RATE = 0.08f;

    public static readonly uint TOCK_PER_SECOND = 50u;

    private const int VANILLA_DIGITAL_KEYS = 10;

    public static PluginKeyTickHandler onPluginKeyTick;

    private float _tick;

    private uint buffer;

    private uint consumed;

    private uint count;

    private uint _simulation;

    private uint _clock;

    private ushort[] flags;

    private bool hasDoneOcclusionCheck;

    private Queue<InputInfo> inputs;

    private PlayerInputPacket clientPendingInput;

    private List<ClientMovementInput> clientInputHistory;

    private Queue<PlayerInputPacket> serversidePackets;

    private uint serverLastReceivedSimulationFrameNumber = uint.MaxValue;

    public int recov;

    private RaycastHit obstruction;

    private float lastInputed;

    private bool hasInputed;

    private bool isDismissed;

    private static readonly float EXPECTED_ASKINPUT_PER_SECOND = 1f / RATE;

    private static readonly int MAX_ASKINPUT_PER_SECOND = (int)(EXPECTED_ASKINPUT_PER_SECOND + 3f);

    private static readonly int KICK_ASKINPUT_PER_SECOND = (int)(EXPECTED_ASKINPUT_PER_SECOND * 5f);

    private int serversideAskInputCount;

    private float initialServersideAskInputTime = -1f;

    private float latestAskInputDismissTestTime = -1f;

    private static readonly int ASKINPUT_WINDOW_LENGTH = 10;

    private int[] rollingWindow = new int[ASKINPUT_WINDOW_LENGTH];

    private int rollingWindowIndex;

    private bool clientHasPendingResimulation;

    private uint clientResimulationFrameNumber;

    private EPlayerStance clientResimulationStance;

    private Vector3 clientResimulationPosition;

    private Vector3 clientResimulationVelocity;

    private byte clientResimulationStamina;

    private int clientResimulationLastTireOffset;

    private int clientResimulationLastRestOffset;

    internal bool isResimulating;

    private static readonly ClientInstanceMethod<uint, EPlayerStance, Vector3, Vector3, byte, int, int> SendSimulateMispredictedInputs = ClientInstanceMethod<uint, EPlayerStance, Vector3, Vector3, byte, int, int>.Get(typeof(PlayerInput), "ReceiveSimulateMispredictedInputs");

    private static readonly ClientInstanceMethod<uint> SendAckGoodInputs = ClientInstanceMethod<uint>.Get(typeof(PlayerInput), "ReceiveAckGoodInputs");

    private static readonly ServerInstanceMethod SendInputs = ServerInstanceMethod.Get(typeof(PlayerInput), "ReceiveInputs");

    private const float MIN_FAKE_LAG_THRESHOLD_SECONDS = 1f;

    private int fakeLagPenaltyFrames;

    internal const float FAKE_LAG_PENALTY_DAMAGE = 0.1f;

    public float tick => _tick;

    public uint simulation => _simulation;

    public bool IsUnderFakeLagPenalty => fakeLagPenaltyFrames > 0;

    public uint clock => _clock;

    public bool[] keys { get; protected set; }

    public bool IsPluginKeyHeld(int index)
    {
        return keys[keys.Length - ControlsSettings.NUM_PLUGIN_KEYS + index];
    }

    public bool hasInputs()
    {
        if (inputs != null)
        {
            return inputs.Count > 0;
        }
        return false;
    }

    public int getInputCount()
    {
        if (inputs == null)
        {
            return 0;
        }
        return inputs.Count;
    }

    public InputInfo getInput(bool doOcclusionCheck, ERaycastInfoUsage usage)
    {
        if (inputs == null)
        {
            return null;
        }
        while (inputs.Count > 0)
        {
            InputInfo inputInfo = inputs.Dequeue();
            if (inputInfo == null)
            {
                return null;
            }
            if (inputInfo.usage != usage)
            {
                continue;
            }
            if (doOcclusionCheck && !hasDoneOcclusionCheck)
            {
                hasDoneOcclusionCheck = true;
                if (inputInfo != null)
                {
                    Vector3 vector = inputInfo.point - base.player.look.aim.position;
                    float magnitude = vector.magnitude;
                    Vector3 vector2 = vector / magnitude;
                    if (magnitude > 0.025f)
                    {
                        Physics.Raycast(new Ray(base.player.look.aim.position, vector2), out obstruction, magnitude - 0.025f, RayMasks.DAMAGE_SERVER);
                        if (obstruction.transform != null && !IsObstructionHitValid())
                        {
                            return null;
                        }
                        Physics.Raycast(new Ray(base.player.look.aim.position + vector2 * (magnitude - 0.025f), -vector2), out obstruction, magnitude - 0.025f, RayMasks.DAMAGE_SERVER);
                        if (obstruction.transform != null && !IsObstructionHitValid())
                        {
                            return null;
                        }
                    }
                }
            }
            return inputInfo;
            bool IsObstructionHitValid()
            {
                if (inputInfo.transform == null)
                {
                    if (!obstruction.transform.CompareTag("Ground"))
                    {
                        return obstruction.transform.CompareTag("Environment");
                    }
                    return true;
                }
                return obstruction.transform.IsChildOf(inputInfo.transform);
            }
        }
        return null;
    }

    [Obsolete("Use the overload of getInput that takes an expected usage parameter instead.")]
    public InputInfo getInput(bool doOcclusionCheck)
    {
        return null;
    }

    public bool isRaycastInvalid(RaycastInfo info)
    {
        if (info.player == null && info.zombie == null && info.animal == null && info.vehicle == null)
        {
            return info.transform == null;
        }
        return false;
    }

    public void sendRaycast(RaycastInfo info, ERaycastInfoUsage usage)
    {
        if (isRaycastInvalid(info))
        {
            return;
        }
        if (Provider.isServer)
        {
            InputInfo inputInfo = new InputInfo();
            inputInfo.usage = usage;
            inputInfo.animal = info.animal;
            inputInfo.direction = info.direction;
            inputInfo.limb = info.limb;
            inputInfo.materialName = info.materialName;
            inputInfo.material = info.material;
            inputInfo.normal = info.normal;
            inputInfo.player = info.player;
            inputInfo.point = info.point;
            inputInfo.transform = info.transform;
            inputInfo.colliderTransform = info.collider?.transform;
            inputInfo.vehicle = info.vehicle;
            inputInfo.zombie = info.zombie;
            inputInfo.section = info.section;
            if (inputInfo.player != null)
            {
                inputInfo.type = ERaycastInfoType.PLAYER;
            }
            else if (inputInfo.zombie != null)
            {
                inputInfo.type = ERaycastInfoType.ZOMBIE;
            }
            else if (inputInfo.animal != null)
            {
                inputInfo.type = ERaycastInfoType.ANIMAL;
            }
            else if (inputInfo.vehicle != null)
            {
                inputInfo.type = ERaycastInfoType.VEHICLE;
            }
            else if (inputInfo.transform != null)
            {
                if (inputInfo.transform.CompareTag("Barricade"))
                {
                    inputInfo.type = ERaycastInfoType.BARRICADE;
                }
                else if (info.transform.CompareTag("Structure"))
                {
                    inputInfo.type = ERaycastInfoType.STRUCTURE;
                }
                else if (info.transform.CompareTag("Resource"))
                {
                    inputInfo.type = ERaycastInfoType.RESOURCE;
                }
                else if (inputInfo.transform.CompareTag("Small") || inputInfo.transform.CompareTag("Medium") || inputInfo.transform.CompareTag("Large"))
                {
                    inputInfo.type = ERaycastInfoType.OBJECT;
                }
                else if (info.transform.CompareTag("Ground") || info.transform.CompareTag("Environment"))
                {
                    inputInfo.type = ERaycastInfoType.NONE;
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
            if (inputInfo != null)
            {
                inputs.Enqueue(inputInfo);
            }
        }
        else
        {
            if (clientPendingInput.clientsideInputs == null)
            {
                clientPendingInput.clientsideInputs = new List<PlayerInputPacket.ClientRaycast>();
            }
            clientPendingInput.clientsideInputs.Add(new PlayerInputPacket.ClientRaycast(info, usage));
        }
    }

    private void advanceRollingWindowIndex(int newIndex)
    {
        do
        {
            rollingWindowIndex++;
            if (rollingWindowIndex >= rollingWindow.Length)
            {
                rollingWindowIndex = 0;
            }
            rollingWindow[rollingWindowIndex] = 0;
        }
        while (rollingWindowIndex != newIndex);
    }

    private void internalDismiss()
    {
        isDismissed = true;
        Provider.dismiss(base.channel.owner.playerID.steamID);
    }

    private void ClientRemoveInputHistory(uint frameNumber)
    {
        if (!clientInputHistory.IsEmpty() && clientInputHistory[0].frameNumber <= frameNumber)
        {
            int i;
            for (i = 1; i < clientInputHistory.Count && clientInputHistory[i].frameNumber <= frameNumber; i++)
            {
            }
            clientInputHistory.RemoveRange(0, i);
        }
    }

    private void ClientResimulate()
    {
        ClientRemoveInputHistory(clientResimulationFrameNumber);
        if (base.player.movement.getVehicle() != null || base.player.movement.hasPendingVehicleChange)
        {
            return;
        }
        isResimulating = true;
        base.player.stance.internalSetStance(clientResimulationStance);
        Quaternion rotation = base.transform.rotation;
        Quaternion rotation2 = base.player.look.aim.rotation;
        base.player.movement.controller.enabled = false;
        base.transform.position = clientResimulationPosition;
        base.player.movement.controller.enabled = true;
        base.player.movement.velocity = clientResimulationVelocity;
        base.player.life.internalSetStamina(clientResimulationStamina);
        base.player.life.lastTire = MathfEx.ClampLongToUInt(clientResimulationFrameNumber - clientResimulationLastTireOffset);
        base.player.life.lastRest = MathfEx.ClampLongToUInt(clientResimulationFrameNumber - clientResimulationLastRestOffset);
        foreach (ClientMovementInput item in clientInputHistory)
        {
            base.transform.rotation = item.rotation;
            base.player.look.aim.rotation = item.aimRotation;
            base.player.life.SimulateStaminaFrame(item.frameNumber);
            base.player.stance.simulate(item.frameNumber, item.crouch, item.prone, item.sprint);
            base.player.movement.simulate(item.frameNumber, 0, item.input_x, item.input_y, 0f, 0f, item.jump, inputSprint: false, RATE);
        }
        base.transform.rotation = rotation;
        base.player.look.aim.rotation = rotation2;
        isResimulating = false;
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public void ReceiveSimulateMispredictedInputs(uint frameNumber, EPlayerStance stance, Vector3 position, Vector3 velocity, byte stamina, int lastTireOffset, int lastRestOffset)
    {
        clientHasPendingResimulation = true;
        clientResimulationFrameNumber = frameNumber;
        clientResimulationStance = stance;
        clientResimulationPosition = position;
        clientResimulationVelocity = velocity;
        clientResimulationStamina = stamina;
        clientResimulationLastTireOffset = lastTireOffset;
        clientResimulationLastRestOffset = lastRestOffset;
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public void ReceiveAckGoodInputs(uint frameNumber)
    {
        if (clientHasPendingResimulation)
        {
            if (frameNumber <= clientResimulationFrameNumber)
            {
                return;
            }
            clientHasPendingResimulation = false;
        }
        ClientRemoveInputHistory(frameNumber);
    }

    [Obsolete]
    public void askInput(CSteamID steamID)
    {
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER)]
    public void ReceiveInputs(in ServerInvocationContext context)
    {
        NetPakReader reader = context.reader;
        if (isDismissed)
        {
            return;
        }
        if (serversideAskInputCount == 0)
        {
            initialServersideAskInputTime = Time.realtimeSinceStartup;
        }
        serversideAskInputCount++;
        float num = Time.realtimeSinceStartup - initialServersideAskInputTime;
        int num2 = (int)num % ASKINPUT_WINDOW_LENGTH;
        if (num2 != rollingWindowIndex)
        {
            advanceRollingWindowIndex(num2);
            if (Provider.configData.Server.Enable_Kick_Input_Spam && num > (float)ASKINPUT_WINDOW_LENGTH)
            {
                if (Time.realtimeSinceStartup - latestAskInputDismissTestTime < (float)(ASKINPUT_WINDOW_LENGTH / 2))
                {
                    int num3 = 0;
                    int[] array = rollingWindow;
                    foreach (int num4 in array)
                    {
                        num3 += num4;
                    }
                    float num5 = num3 / ASKINPUT_WINDOW_LENGTH;
                    if (num5 > (float)KICK_ASKINPUT_PER_SECOND)
                    {
                        string text = Mathf.RoundToInt(num5 / EXPECTED_ASKINPUT_PER_SECOND * 100f).ToString();
                        UnturnedLog.warn("Received {0}% of expected input packets from {1} over the past {2} seconds, so we're dismissing them", text, base.channel.owner.playerID.steamID, ASKINPUT_WINDOW_LENGTH);
                        internalDismiss();
                        return;
                    }
                }
                latestAskInputDismissTestTime = Time.realtimeSinceStartup;
            }
        }
        rollingWindow[rollingWindowIndex]++;
        if (rollingWindow[rollingWindowIndex] > MAX_ASKINPUT_PER_SECOND)
        {
            return;
        }
        reader.ReadBit(out var value);
        PlayerInputPacket playerInputPacket = ((!value) ? ((PlayerInputPacket)new WalkingPlayerInputPacket()) : ((PlayerInputPacket)new DrivingPlayerInputPacket()));
        playerInputPacket.read(base.channel, reader);
        if (serverLastReceivedSimulationFrameNumber != uint.MaxValue && playerInputPacket.clientSimulationFrameNumber <= serverLastReceivedSimulationFrameNumber)
        {
            return;
        }
        serverLastReceivedSimulationFrameNumber = playerInputPacket.clientSimulationFrameNumber;
        serversidePackets.Enqueue(playerInputPacket);
        float num6 = Time.realtimeSinceStartup - lastInputed;
        if (hasInputed && num6 > 1f && num6 > Provider.configData.Server.Fake_Lag_Threshold_Seconds)
        {
            if (Provider.configData.Server.Fake_Lag_Log_Warnings)
            {
                CommandWindow.LogWarning($"{num6} seconds between inputs from \"{base.channel.owner.playerID.playerName}\" steamid: {base.channel.owner.playerID.steamID}");
            }
            float num7 = num6 - 1f;
            fakeLagPenaltyFrames += Mathf.CeilToInt(num7 / RATE);
        }
        lastInputed = Time.realtimeSinceStartup;
        hasInputed = true;
    }

    private void onLifeUpdated(bool isDead)
    {
        serversidePackets.Clear();
    }

    private void FixedUpdate()
    {
        if (isDismissed)
        {
            return;
        }
        if (base.channel.isOwner)
        {
            if (count % SAMPLES == 0)
            {
                _tick = Time.realtimeSinceStartup;
                if (clientHasPendingResimulation)
                {
                    clientHasPendingResimulation = false;
                    ClientResimulate();
                }
                keys[0] = base.player.movement.jump;
                keys[1] = base.player.equipment.primary;
                keys[2] = base.player.equipment.secondary;
                keys[3] = base.player.stance.crouch;
                keys[4] = base.player.stance.prone;
                keys[5] = base.player.stance.sprint;
                keys[6] = base.player.animator.leanLeft;
                keys[7] = base.player.animator.leanRight;
                keys[8] = false;
                keys[9] = base.player.stance.localWantsToSteadyAim;
                bool flag = MenuConfigurationControlsUI.binding == byte.MaxValue;
                for (int i = 0; i < ControlsSettings.NUM_PLUGIN_KEYS; i++)
                {
                    int num = keys.Length - ControlsSettings.NUM_PLUGIN_KEYS + i;
                    keys[num] = flag && InputEx.GetKey(ControlsSettings.getPluginKeyCode(i));
                }
                base.player.life.simulate(simulation);
                bool crouch = base.player.stance.crouch;
                bool prone = base.player.stance.prone;
                bool sprint = base.player.stance.sprint;
                base.player.stance.simulate(simulation, crouch, prone, sprint);
                int input_x = base.player.movement.horizontal - 1;
                int input_y = base.player.movement.vertical - 1;
                bool jump = base.player.movement.jump;
                base.player.movement.simulate(simulation, 0, input_x, input_y, base.player.look.look_x, base.player.look.look_y, jump, sprint, RATE);
                if (Provider.isServer)
                {
                    inputs.Clear();
                }
                else
                {
                    if (base.player.stance.stance == EPlayerStance.DRIVING)
                    {
                        clientPendingInput = new DrivingPlayerInputPacket();
                    }
                    else
                    {
                        WalkingPlayerInputPacket walkingPlayerInputPacket = new WalkingPlayerInputPacket();
                        walkingPlayerInputPacket.analog = (byte)((base.player.movement.horizontal << 4) | base.player.movement.vertical);
                        walkingPlayerInputPacket.clientPosition = base.transform.position;
                        walkingPlayerInputPacket.pitch = base.player.look.pitch;
                        walkingPlayerInputPacket.yaw = base.player.look.yaw;
                        clientPendingInput = walkingPlayerInputPacket;
                        ClientMovementInput item = default(ClientMovementInput);
                        item.frameNumber = simulation;
                        item.crouch = crouch;
                        item.prone = prone;
                        item.input_x = input_x;
                        item.input_y = input_y;
                        item.jump = jump;
                        item.sprint = sprint;
                        item.rotation = base.transform.rotation;
                        item.aimRotation = base.player.look.aim.rotation;
                        clientInputHistory.Add(item);
                    }
                    clientPendingInput.clientSimulationFrameNumber = simulation;
                    clientPendingInput.recov = recov;
                }
                base.player.equipment.simulate(simulation, base.player.equipment.primary, base.player.equipment.secondary, base.player.stance.localWantsToSteadyAim);
                base.player.animator.simulate(simulation, base.player.animator.leanLeft, base.player.animator.leanRight);
                buffer += SAMPLES;
                _simulation++;
            }
            if (consumed < buffer)
            {
                consumed++;
                base.player.equipment.tock(clock);
                _clock++;
            }
            if (consumed == buffer && clientPendingInput != null && !Provider.isServer)
            {
                ushort num2 = 0;
                for (byte b = 0; b < keys.Length; b = (byte)(b + 1))
                {
                    if (keys[b])
                    {
                        num2 = (ushort)(num2 | flags[b]);
                    }
                }
                clientPendingInput.keys = num2;
                if (clientPendingInput is DrivingPlayerInputPacket)
                {
                    DrivingPlayerInputPacket drivingPlayerInputPacket = clientPendingInput as DrivingPlayerInputPacket;
                    InteractableVehicle vehicle = base.player.movement.getVehicle();
                    if (vehicle != null)
                    {
                        Transform transform = vehicle.transform;
                        if (vehicle.asset.engine == EEngine.TRAIN)
                        {
                            drivingPlayerInputPacket.position = InteractableVehicle.PackRoadPosition(vehicle.roadPosition);
                        }
                        else
                        {
                            drivingPlayerInputPacket.position = transform.position;
                        }
                        drivingPlayerInputPacket.rotation = transform.rotation;
                        drivingPlayerInputPacket.speed = (byte)(Mathf.Clamp(vehicle.speed, -100f, 100f) + 128f);
                        drivingPlayerInputPacket.physicsSpeed = (byte)(Mathf.Clamp(vehicle.physicsSpeed, -100f, 100f) + 128f);
                        drivingPlayerInputPacket.turn = (byte)(vehicle.turn + 1);
                    }
                }
                if (true & Provider.isConnected)
                {
                    SendInputs.Invoke(GetNetId(), ENetReliability.Reliable, delegate(NetPakWriter writer)
                    {
                        if (clientPendingInput is DrivingPlayerInputPacket)
                        {
                            writer.WriteBit(value: true);
                        }
                        else
                        {
                            writer.WriteBit(value: false);
                        }
                        clientPendingInput.write(writer);
                    });
                }
            }
            count++;
        }
        else
        {
            if (!Provider.isServer)
            {
                return;
            }
            if (serversidePackets.Count > 0)
            {
                PlayerInputPacket playerInputPacket = serversidePackets.Peek();
                if (playerInputPacket is WalkingPlayerInputPacket || count % SAMPLES == 0)
                {
                    if (simulation > (uint)((Time.realtimeSinceStartup + 5f - tick) / RATE))
                    {
                        return;
                    }
                    playerInputPacket = serversidePackets.Dequeue();
                    if (playerInputPacket == null)
                    {
                        return;
                    }
                    hasDoneOcclusionCheck = false;
                    inputs = playerInputPacket.serversideInputs;
                    for (byte b2 = 0; b2 < keys.Length; b2 = (byte)(b2 + 1))
                    {
                        keys[b2] = (playerInputPacket.keys & flags[b2]) == flags[b2];
                    }
                    if (playerInputPacket is DrivingPlayerInputPacket)
                    {
                        DrivingPlayerInputPacket drivingPlayerInputPacket2 = playerInputPacket as DrivingPlayerInputPacket;
                        if (!base.player.life.isDead)
                        {
                            base.player.life.simulate(simulation);
                            base.player.look.simulate(0f, 0f, RATE);
                            base.player.stance.simulate(simulation, inputCrouch: false, inputProne: false, inputSprint: false);
                            base.player.movement.simulate(simulation, drivingPlayerInputPacket2.recov, keys[0], keys[5], drivingPlayerInputPacket2.position, drivingPlayerInputPacket2.rotation, drivingPlayerInputPacket2.speed - 128, drivingPlayerInputPacket2.physicsSpeed - 128, drivingPlayerInputPacket2.turn - 1, RATE);
                            base.player.equipment.simulate(simulation, keys[1], keys[2], keys[9]);
                            base.player.animator.simulate(simulation, inputLeanLeft: false, inputLeanRight: false);
                        }
                    }
                    else
                    {
                        WalkingPlayerInputPacket walkingPlayerInputPacket2 = playerInputPacket as WalkingPlayerInputPacket;
                        byte analog = walkingPlayerInputPacket2.analog;
                        if (!base.player.life.isDead)
                        {
                            base.player.life.simulate(simulation);
                            base.player.look.simulate(walkingPlayerInputPacket2.yaw, walkingPlayerInputPacket2.pitch, RATE);
                            base.player.stance.simulate(simulation, keys[3], keys[4], keys[5]);
                            int input_x2 = ((analog >> 4) & 0xF) - 1;
                            int input_y2 = (analog & 0xF) - 1;
                            bool inputJump = keys[0];
                            bool inputSprint = keys[5];
                            base.player.movement.simulate(simulation, walkingPlayerInputPacket2.recov, input_x2, input_y2, 0f, 0f, inputJump, inputSprint, RATE);
                            base.player.equipment.simulate(simulation, keys[1], keys[2], keys[9]);
                            base.player.animator.simulate(simulation, keys[6], keys[7]);
                            if (!base.player.movement.hasPendingVehicleChange && base.player.stance.stance != EPlayerStance.DRIVING && base.player.stance.stance != EPlayerStance.SITTING)
                            {
                                Vector3 position = base.transform.position;
                                if ((walkingPlayerInputPacket2.clientPosition - position).sqrMagnitude > 0.0004f)
                                {
                                    int arg = (int)(simulation - base.player.life.lastTire);
                                    int arg2 = (int)(simulation - base.player.life.lastRest);
                                    SendSimulateMispredictedInputs.Invoke(GetNetId(), ENetReliability.Unreliable, base.channel.GetOwnerTransportConnection(), walkingPlayerInputPacket2.clientSimulationFrameNumber, base.player.stance.stance, position, base.player.movement.velocity, base.player.life.stamina, arg, arg2);
                                }
                                else
                                {
                                    SendAckGoodInputs.Invoke(GetNetId(), ENetReliability.Unreliable, base.channel.GetOwnerTransportConnection(), walkingPlayerInputPacket2.clientSimulationFrameNumber);
                                }
                            }
                        }
                    }
                    if (onPluginKeyTick != null)
                    {
                        for (byte b3 = 0; b3 < ControlsSettings.NUM_PLUGIN_KEYS; b3 = (byte)(b3 + 1))
                        {
                            int num3 = keys.Length - ControlsSettings.NUM_PLUGIN_KEYS + b3;
                            onPluginKeyTick(base.player, simulation, b3, keys[num3]);
                        }
                    }
                    buffer += SAMPLES;
                    _simulation++;
                    while (consumed < buffer)
                    {
                        consumed++;
                        if (!base.player.life.isDead)
                        {
                            base.player.equipment.tock(clock);
                        }
                        _clock++;
                    }
                    fakeLagPenaltyFrames = Mathf.Max(0, fakeLagPenaltyFrames - 1);
                }
                count++;
            }
            else
            {
                base.player.movement.simulate();
                if (hasInputed && Time.realtimeSinceStartup - lastInputed > 20f && Provider.configData.Server.Enable_Kick_Input_Timeout)
                {
                    UnturnedLog.warn("Haven't received input from {0} for the past 20 seconds, so we're dismissing them", base.channel.owner.playerID.steamID);
                    internalDismiss();
                }
            }
        }
    }

    internal void InitializePlayer()
    {
        _tick = Time.realtimeSinceStartup;
        _simulation = 0u;
        _clock = 0u;
        if (base.channel.isOwner || Provider.isServer)
        {
            keys = new bool[10 + ControlsSettings.NUM_PLUGIN_KEYS];
            flags = new ushort[10 + ControlsSettings.NUM_PLUGIN_KEYS];
            for (byte b = 0; b < keys.Length; b = (byte)(b + 1))
            {
                flags[b] = (ushort)(1 << (int)b);
            }
        }
        if (base.channel.isOwner && Provider.isServer)
        {
            hasDoneOcclusionCheck = false;
            inputs = new Queue<InputInfo>();
        }
        if (base.channel.isOwner)
        {
            clientPendingInput = null;
            clientInputHistory = new List<ClientMovementInput>();
        }
        else if (Provider.isServer)
        {
            serversidePackets = new Queue<PlayerInputPacket>();
            PlayerLife life = base.player.life;
            life.onLifeUpdated = (LifeUpdated)Delegate.Combine(life.onLifeUpdated, new LifeUpdated(onLifeUpdated));
        }
        recov = -1;
    }
}
