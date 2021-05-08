using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace AutoStackFix
{
    [BepInPlugin("MVP.AutoStackFix", "AutoStackFix", "0.0.1")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource logger;

        public void Awake()
        {
            logger = Logger;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), "AutoStackFix");
        }
    }

    [Harmony]
    public static class Patches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ItemDrop), "Awake")]
        private static void Post_ItemDrop_Awake(ItemDrop __instance)
        {
            if (!__instance.gameObject.GetComponent<ZNetView>())
            {
                return;
            }
            if (!__instance.GetComponent<AutoStackFixComponent>())
            {
                __instance.gameObject.AddComponent<AutoStackFixComponent>();
            }
        }
    }

    public class AutoStackFixComponent : MonoBehaviour
    {
        ZNetView m_nview;
        ItemDrop m_itemDrop;

        uint m_lastDataRevision;
        int m_lastStack;

        private void Awake()
        {
            this.m_nview = base.GetComponent<ZNetView>();
            this.m_itemDrop = base.GetComponent<ItemDrop>();
            ZDO zdo = this.m_nview.GetZDO();
            if (zdo == null)
            {
                return;
            }
            this.m_lastDataRevision = zdo.m_dataRevision;
            this.m_lastStack = zdo.GetInt("stack");
        }

        private void FixedUpdate()
        {
            if (this.m_nview == null || !this.m_nview.IsValid())
            {
                return;
            }

            ZDO zdo = this.m_nview.GetZDO();

            if (zdo.IsOwner())
            {
                // ItemDrop owner performs the auto stacking and shouldn't experience stack amount mismatch
                return;
            }

            if (zdo.m_dataRevision > this.m_lastDataRevision)
            {
                ItemDrop.ItemData itemData = this.m_itemDrop.m_itemData;
                int networkedStack = zdo.GetInt("stack");
                if (networkedStack != this.m_lastStack && itemData.m_stack != networkedStack)
                {
                    Plugin.logger.LogInfo($"Fixing ItemDrop ({itemData.m_shared.m_name} / {zdo.m_uid}) stack to networked value: {itemData.m_stack} -> {networkedStack}");
                    itemData.m_stack = networkedStack;
                    this.m_lastStack = networkedStack;
                }
                this.m_lastDataRevision = zdo.m_dataRevision;
            }
        }
    }
}
