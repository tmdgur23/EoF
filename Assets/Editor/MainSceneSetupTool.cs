using UnityEditor;
using UnityEngine;
using MainScene;
using Battle.RewardMenu;
using System.Collections.Generic;

namespace MainScene.Editor
{
    public class MainSceneSetupTool : EditorWindow
    {
        [MenuItem("Tools/Setup Main Scene Interaction")]
        public static void Setup()
        {
            // 1. Setup RoomAttributeManager
            GameObject managerObj = GameObject.Find("MainSceneManager");
            if (managerObj == null) managerObj = new GameObject("MainSceneManager");
            
            RoomAttributeManager manager = managerObj.GetComponent<RoomAttributeManager>() ?? managerObj.AddComponent<RoomAttributeManager>();

            // Assign Card Prefab to Manager (Find by GUID)
            SerializedObject soManager = new SerializedObject(manager);
            string cardPrefabPath = AssetDatabase.GUIDToAssetPath("8af1b4a23f78dbf449dadfb2c1ff243a");
            soManager.FindProperty("cardPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(cardPrefabPath);
            soManager.ApplyModifiedProperties();

            // 2. Setup Reward UI
            GameObject rewardUI = FindOrInstantiateRewardUI();
            if (rewardUI != null)
            {
                // Ensure there's an EventSystem in the scene
                if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
                {
                    GameObject eventSystem = new GameObject("EventSystem");
                    eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                    eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                }

                // Ensure there's a PopUpHandler in the scene (required for CloseAll() calls if any old code remains)
                if (Misc.PopUp.PopUpHandler.Instance == null)
                {
                    GameObject popupObj = new GameObject("PopUpHandler");
                    popupObj.AddComponent<Misc.PopUp.PopUpHandler>();
                    Debug.Log("MainSceneSetupTool: Created PopUpHandler placeholder.");
                }

                // Configuration for Canvas - ALWAYS use Overlay for guaranteed visibility
                Canvas canvas = rewardUI.GetComponent<Canvas>() ?? rewardUI.GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvas.sortingOrder = 999;
                }

                // Force UI Layer (5) on all children
                SetLayerRecursive(rewardUI, 5);

                // Setup new MainSceneRewardUI
                var mainRewardUI = rewardUI.GetComponent<MainSceneRewardUI>();
                if (mainRewardUI == null) mainRewardUI = rewardUI.AddComponent<MainSceneRewardUI>();
                
                // Cleanup old logic to avoid conflicts
                var oldOpener = rewardUI.GetComponent<Deck.CardCollectionViewOpener>();
                if (oldOpener != null) Object.DestroyImmediate(oldOpener);

                // Auto-link sub-elements for the new UI
                SerializedObject soReward = new SerializedObject(mainRewardUI);
                
                // Find Background
                Transform bgTrans = rewardUI.transform.Find("Background");
                soReward.FindProperty("m_background").objectReferenceValue = bgTrans != null ? bgTrans.gameObject : null;
                
                // Find Content (where cards will be spawned)
                Transform contentTrans = rewardUI.transform.Find("CardReward/CardSelection");
                soReward.FindProperty("m_contentTransform").objectReferenceValue = contentTrans;
                
                // Assign Card Prefab (Find by GUID)
                string cardPath = AssetDatabase.GUIDToAssetPath("8af1b4a23f78dbf449dadfb2c1ff243a");
                soReward.FindProperty("m_cardPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(cardPath);
                
                soReward.ApplyModifiedProperties();
                
                // Hide reward UI by default
                rewardUI.SetActive(false);
            }

            // 3. Setup Player Interaction
            Camera cam = Camera.main;
            if (cam != null)
            {
                PlayerInteraction interaction = cam.gameObject.GetComponent<PlayerInteraction>() ?? cam.gameObject.AddComponent<PlayerInteraction>();
                
                // Set interactLayer to "Everything" and distance to 100
                SerializedObject interactionSo = new SerializedObject(interaction);
                interactionSo.FindProperty("interactLayer").intValue = -1; // -1 means "Everything"
                interactionSo.FindProperty("interactDistance").floatValue = 100f;
                interactionSo.ApplyModifiedProperties();
            }

            // 4. Setup Doors in Rooms
            for (int i = 1; i <= 6; i++)
            {
                GameObject room = GameObject.Find("Room" + i);
                if (room != null)
                {
                    SetupDoorsInRoom(room, i);
                }
            }

            Debug.Log("Main Scene Interaction Setup Completed!");
        }

        private static GameObject FindOrInstantiateRewardUI()
        {
            // Search robustly for any GameObject named "RewardUI" (even if inactive)
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            GameObject existing = null;
            
            foreach (var obj in allObjects)
            {
                // Only look at scene objects, not assets
                if ((obj.name == "Reward" || obj.name == "RewardUI") && !EditorUtility.IsPersistent(obj))
                {
                    // If we find multiple, keep one and delete others to clean up
                    if (existing == null) existing = obj;
                    else Object.DestroyImmediate(obj);
                }
            }

            if (existing != null) return existing;

            // If still not found, instantiate from prefab
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Battle/RewardMenu/Reward.prefab");
            if (prefab != null)
            {
                GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                inst.name = "RewardUI";
                return inst;
            }
            return null;
        }

        private static void SetLayerRecursive(GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursive(child.gameObject, layer);
            }
        }

        private static void SetupDoorsInRoom(GameObject room, int index)
        {
            // Search for "door" in children
            foreach (Transform child in room.GetComponentsInChildren<Transform>(true))
            {
                if (child.name.ToLower().Contains("door") && child.name.ToLower().Contains("wall"))
                {
                    // Ensure layer is 0 (Default) or a specific layer for interaction
                    child.gameObject.layer = 0;

                    // Add InteractableDoor if not exists
                    InteractableDoor door = child.gameObject.GetComponent<InteractableDoor>() ?? child.gameObject.AddComponent<InteractableDoor>();
                    door.SetRoomIndex(index);

                    // Ensure there's a collider for raycasting
                    Collider existingCol = child.gameObject.GetComponent<Collider>();
                    if (existingCol == null)
                    {
                        if (child.gameObject.GetComponent<MeshFilter>() != null)
                        {
                            MeshCollider mc = child.gameObject.AddComponent<MeshCollider>();
                            mc.convex = true; // Use convex for better reliability if it's a trigger or complex
                        }
                        else
                        {
                            child.gameObject.AddComponent<BoxCollider>();
                        }
                    }
                    else if (existingCol is MeshCollider mc)
                    {
                        mc.convex = true;
                    }
                    
                    // Mark dirty for saving
                    EditorUtility.SetDirty(child.gameObject);
                }
            }
        }
    }
}
