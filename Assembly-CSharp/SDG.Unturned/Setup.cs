using SDG.Framework.Modules;
using UnityEngine;

namespace SDG.Unturned;

public class Setup : MonoBehaviour
{
    public UnturnedPostProcess postProcess;

    public bool awakeDedicator = true;

    public bool awakeLogs = true;

    public bool awakeModuleHook = true;

    public bool awakeProvider = true;

    public bool startModuleHook = true;

    public bool startProvider = true;

    private void Awake()
    {
        UnturnedPlayerLoop.initialize();
        ThreadUtil.setupGameThread();
        if (awakeDedicator)
        {
            GetComponent<Dedicator>().awake();
        }
        if (awakeLogs)
        {
            GetComponent<Logs>().awake();
        }
        if (awakeModuleHook)
        {
            GetComponent<ModuleHook>().awake();
        }
        if (awakeProvider)
        {
            GetComponent<Provider>().awake();
        }
        if (startModuleHook)
        {
            GetComponent<ModuleHook>().start();
        }
        if (startProvider)
        {
            GetComponent<Provider>().start();
        }
        if (!Dedicator.IsDedicatedServer)
        {
            GlazierFactory.Create();
        }
    }

    private void Start()
    {
        if (startProvider)
        {
            GetComponent<Provider>().unityStart();
        }
        postProcess.initialize();
        if (!Dedicator.IsDedicatedServer)
        {
            MenuSettings.load();
            GraphicsSettings.applyResolution();
            LoadingUI.updateScene();
        }
    }
}
