using UnityEngine;

namespace SDG.Unturned;

public class InteractableObjectNPC : InteractableObject
{
    protected ObjectNPCAsset _npcAsset;

    public bool isLookingAtPlayer;

    private bool isInit;

    private Animation anim;

    private HumanAnimator humanAnim;

    private HumanClothes clothes;

    private Transform skull;

    private bool hasEquip;

    private bool hasSafety;

    private bool hasInspect;

    private bool isEquipped;

    private bool isSafe;

    private string stanceIdle;

    private string stanceActive;

    private float lastIdle;

    private float idleDelay;

    private float headBlend;

    private Quaternion headRotation;

    public ObjectNPCAsset npcAsset => _npcAsset;

    internal void SetFaceOverride(byte? faceOverride)
    {
        byte b = (faceOverride.HasValue ? faceOverride.Value : _npcAsset.face);
        if (clothes.face != b)
        {
            clothes.face = b;
            clothes.apply();
        }
    }

    internal void OnStoppedTalkingWithLocalPlayer()
    {
        isLookingAtPlayer = false;
        SetFaceOverride(null);
    }

    private void updateStance()
    {
        stanceActive = null;
        if (npcAsset.pose == ENPCPose.SIT)
        {
            if (Random.value < 0.5f)
            {
                stanceIdle = "Idle_Sit";
            }
            else
            {
                stanceIdle = "Idle_Drive";
            }
        }
        else if (npcAsset.pose == ENPCPose.CROUCH)
        {
            stanceIdle = "Idle_Crouch";
        }
        else if (npcAsset.pose == ENPCPose.PRONE)
        {
            stanceIdle = "Idle_Prone";
        }
        else if (npcAsset.pose == ENPCPose.UNDER_ARREST)
        {
            stanceIdle = "Gesture_Arrest";
        }
        else if (npcAsset.pose == ENPCPose.REST)
        {
            stanceIdle = "Gesture_Rest";
        }
        else if (npcAsset.pose == ENPCPose.SURRENDER)
        {
            stanceIdle = "Gesture_Surrender";
        }
        else if (hasEquip || npcAsset.pose == ENPCPose.ASLEEP)
        {
            stanceIdle = "Idle_Stand";
        }
        else if (Random.value < 0.5f)
        {
            stanceIdle = "Idle_Stand";
        }
        else
        {
            stanceIdle = "Idle_Hips";
        }
    }

    private void updateIdle()
    {
        lastIdle = Time.time;
        idleDelay = Random.Range(5f, 30f);
    }

    private void updateAnimation()
    {
        isEquipped = false;
        updateStance();
        updateIdle();
    }

    public override void updateState(Asset asset, byte[] state)
    {
        base.updateState(asset, state);
        if (!isInit)
        {
            isInit = true;
            _npcAsset = asset as ObjectNPCAsset;
        }
    }

    public override void use()
    {
        DialogueAsset dialogueAsset = npcAsset.FindDialogueAsset();
        if (dialogueAsset == null)
        {
            UnturnedLog.warn("Failed to find NPC dialogue: " + npcAsset.FriendlyName);
            return;
        }
        ObjectManager.useObjectNPC(base.transform);
        Player.player.quests.checkNPC = this;
        PlayerLifeUI.close();
        PlayerLifeUI.npc = this;
        isLookingAtPlayer = true;
        PlayerNPCDialogueUI.open(dialogueAsset, null);
    }

    public override bool checkUseable()
    {
        if (!PlayerUI.window.showCursor)
        {
            return !npcAsset.IsDialogueRefNull();
        }
        return false;
    }

    public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
    {
        message = EPlayerMessage.TALK;
        text = "";
        color = Color.white;
        return !PlayerUI.window.showCursor;
    }

    private void Update()
    {
    }

    private void LateUpdate()
    {
    }

    private void OnEnable()
    {
    }
}
