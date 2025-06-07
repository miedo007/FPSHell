using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditorInternal;
using System.Linq;
#endif

namespace HellishBattle
{
    [CreateAssetMenu(fileName = "Loot Table", menuName = "Hellish Battle/Loot Table SO")]
    public class LootDrop : ScriptableObject
    {
        public DropChangeItem[] GuaranteedLootTable = new DropChangeItem[0];
        public DropChangeItem[] OneItemFromList = new DropChangeItem[1];
        public float WeightToNoDrop = 100;

        // Defualt Value
        public int DefualtAdditionalDropCount = 1;
        public float DefualtMinDropRange = 1;
        public float DefualtMaxDropRange = 2;


        // Return List of Guaranteed Drop 
        public List<GameObject> GetGuaranteeedLoot()
        {
            List<GameObject> lootList = new List<GameObject>();

            for (int i = 0; i < GuaranteedLootTable.Length; i++)
            {
                // Adds the drawn number of items to drop
                int count = Random.Range(GuaranteedLootTable[i].MinCountItem, GuaranteedLootTable[i].MaxCountItem);
                for (int j = 0; j < count; j++)
                {
                    lootList.Add(GuaranteedLootTable[i].Drop);
                }
            }

            return lootList;
        }

        // Return List of Optional Drop 
        public List<GameObject> GetRandomLoot(int ChangeCount)
        {
            List<GameObject> lootList = new List<GameObject>();
            float totalWeight = WeightToNoDrop;

            // Executes a function a specified number of times
            for (int j = 0; j < ChangeCount; j++)
            {
                // They add up the entire weight of the items
                for (int i = 0; i < OneItemFromList.Length; i++)
                {
                    totalWeight += OneItemFromList[i].Weight;
                }

                float value = Random.Range(0, totalWeight);
                float timed_value = 0;

                for (int i = 0; i < OneItemFromList.Length; i++)
                {
                    // If timed_value is greater than value, it means this item has been drawn
                    timed_value += OneItemFromList[i].Weight;
                    if (timed_value > value)
                    {
                        int count = Random.Range(OneItemFromList[i].MinCountItem, OneItemFromList[i].MaxCountItem + 1);
                        for (int c = 0; c < count; c++)
                        {
                            lootList.Add(OneItemFromList[i].Drop);
                        }
                        break;
                    }
                }
            }

            return lootList;
        }

        #region Loot Drop Functions

        /// <summary>
        /// Draws Items and Spawns them in the Spawnpoint range |
        /// The radius in the MinRange is excluded from the spawn options
        /// </summary>
        /// <param name="Spawnpoint">The point at which the Drop will appear</param>
        /// <param name="AdditionalItemCount">Number of Additional Items</param>
        /// <param name="MinRange">Range Which is Excluded from Spawnpoint</param>
        /// <param name="MaxRange">Max Range of Drop appearing</param>
        /// <param name="SpawnYEqualTo0">Drop appears at the same Y position as Spawnpoint (Default True)</param>
        public void SpawnDrop(Transform Spawnpoint, int AdditionalItemCount, float MinRange, float MaxRange, bool SpawnYEqualTo0 = true)
        {
            // Generate Loot
            List<GameObject> GuaranteedDropList = GetGuaranteeedLoot();
            List<GameObject> AdditionalDropList = GetRandomLoot(AdditionalItemCount);

            // Spawn Guaranteed Drops
            for (int i = 0; i < GuaranteedDropList.Count; i++)
            {
                // SpawnPosition
                //Vector3 SpawnPosition = new Vector3(Spawnpoint.localPosition.x + GenerateRandomNumber(MinRange, MaxRange), Spawnpoint.localPosition.y + GenerateRandomNumber(MinRange, MaxRange), Spawnpoint.localPosition.z + GenerateRandomNumber(MinRange, MaxRange));
                Vector3 SpawnPosition = GenerateRandomNumber(MinRange, MaxRange);
                SpawnPosition = new Vector3(SpawnPosition.x + Spawnpoint.localPosition.x, SpawnPosition.y + Spawnpoint.localPosition.y, SpawnPosition.z + Spawnpoint.localPosition.z);
                if (SpawnYEqualTo0) { SpawnPosition.y = Spawnpoint.localPosition.y; }

                // Spawn
                Instantiate(GuaranteedDropList[i], SpawnPosition, Quaternion.identity);
            }

            // Spawn Additional Drops
            for (int i = 0; i < AdditionalDropList.Count; i++)
            {
                // SpawnPosition
                //Vector3 SpawnPosition = new Vector3(Spawnpoint.localPosition.x + GenerateRandomNumber(MinRange, MaxRange), Spawnpoint.localPosition.y + GenerateRandomNumber(MinRange, MaxRange), Spawnpoint.localPosition.z + GenerateRandomNumber(MinRange, MaxRange));
                Vector3 SpawnPosition = GenerateRandomNumber(MinRange, MaxRange);
                if (SpawnYEqualTo0) { SpawnPosition.y = Spawnpoint.localPosition.y; }

                // Spawn
                Instantiate(AdditionalDropList[i], SpawnPosition, Quaternion.identity);
            }
        }

        /// <summary>
        /// Draws Items and Spawns them in the Spawnpoint range
        /// </summary>
        /// <param name="Spawnpoint">The point at which the Drop will appear</param>
        /// <param name="AdditionalItemCount">Number of Additional Items</param>
        /// <param name="MaxRange">Max Range of Drop appearing</param>
        /// <param name="SpawnYEqualTo0">Drop appears at the same Y position as Spawnpoint (Default True)</param>
        public void SpawnDrop(Transform Spawnpoint, int AdditionalItemCount, float MaxRange, bool SpawnYEqualTo0 = true)
        {
            SpawnDrop(Spawnpoint, AdditionalItemCount, 0, MaxRange, SpawnYEqualTo0);
        }

        /// <summary>
        /// Draws Items and Spawns them in the Spawnpoint range | Takes Default values for the given Loot Table
        /// </summary>
        /// <param name="Spawnpoint">The point at which the Drop will appear</param>
        public void SpawnDropWithDefualtInSphere(Transform Spawnpoint)
        {
            SpawnDrop(Spawnpoint, DefualtAdditionalDropCount, DefualtMinDropRange, DefualtMaxDropRange, false);
        }

        /// <summary>
        /// Draws Items and Spawns them in the Spawnpoint range | Takes Default values for the given Loot Table with Static Position Y
        /// </summary>
        /// <param name="Spawnpoint">The point at which the Drop will appear</param>
        public void SpawnDropWithDefualtInCircle(Transform Spawnpoint)
        {
            SpawnDrop(Spawnpoint, DefualtAdditionalDropCount, DefualtMinDropRange, DefualtMaxDropRange, true);
        }

        #endregion

        public void GizmoDrawSpawnRange(Transform Spawnpoint, float MinRange, float MaxRange)
        {
            if (Spawnpoint)
            {

                // The number of points on the circle
                int numPoints = 36;
                float angleIncrement = 360f / numPoints;

                Gizmos.color = Color.green;

                // Draw Max Radius
                for (int i = 0; i < numPoints; i++)
                {
                    float angle = i * angleIncrement;
                    float x = Spawnpoint.position.x + Mathf.Sin(Mathf.Deg2Rad * angle) * MaxRange;
                    float y = Spawnpoint.position.y;
                    float z = Spawnpoint.position.z + Mathf.Cos(Mathf.Deg2Rad * angle) * MaxRange;

                    Vector3 startPoint = new Vector3(x, y, z);

                    // Calculate the next angle
                    float nextAngle = (i + 1) * angleIncrement;
                    float nextX = Spawnpoint.position.x + Mathf.Sin(Mathf.Deg2Rad * nextAngle) * MaxRange;
                    float nextZ = Spawnpoint.position.z + Mathf.Cos(Mathf.Deg2Rad * nextAngle) * MaxRange;

                    Vector3 endPoint = new Vector3(nextX, y, nextZ);

                    // Draw the gizmo line
                    Gizmos.DrawLine(startPoint, endPoint);
                }

                if (MinRange > 0)
                {
                    Gizmos.color = Color.red;

                    // Draw Max Radius
                    for (int i = 0; i < numPoints; i++)
                    {
                        float angle = i * angleIncrement;
                        float x = Spawnpoint.position.x + Mathf.Sin(Mathf.Deg2Rad * angle) * MinRange;
                        float y = Spawnpoint.position.y;
                        float z = Spawnpoint.position.z + Mathf.Cos(Mathf.Deg2Rad * angle) * MinRange;

                        Vector3 startPoint = new Vector3(x, y, z);

                        // Calculate the next angle
                        float nextAngle = (i + 1) * angleIncrement;
                        float nextX = Spawnpoint.position.x + Mathf.Sin(Mathf.Deg2Rad * nextAngle) * MinRange;
                        float nextZ = Spawnpoint.position.z + Mathf.Cos(Mathf.Deg2Rad * nextAngle) * MinRange;

                        Vector3 endPoint = new Vector3(nextX, y, nextZ);

                        // Draw the gizmo line
                        Gizmos.DrawLine(startPoint, endPoint);
                    }
                }
            }
        }


        // Private Function to Randomize MinRange with MaxRange
        Vector3 GenerateRandomNumber(float innerRadius, float outerRadius)
        {
            Vector3 randomVector;
            Vector3 tmp;

            do
            {
                randomVector = new Vector3(Random.Range(-outerRadius, outerRadius), Random.Range(-outerRadius, outerRadius), Random.Range(-outerRadius, outerRadius));

                tmp = new Vector3(randomVector.x, 0, randomVector.z);

            } while (tmp.magnitude <= innerRadius || tmp.magnitude >= outerRadius);

            return randomVector;
        }
    }

    /* --------------------- */
    // Drop Item Change Class
    /* --------------------- */

    [System.Serializable]
    public class DropChangeItem
    {
        public float Weight;
        public GameObject Drop;
        public int MinCountItem;
        public int MaxCountItem;
    }


    /* --------------------- */
    // Custom Editor & Property Draw (look)
    /* --------------------- */


#if UNITY_EDITOR

    /* --------------------- */
    // Custom Editor
    /* --------------------- */

    [CustomEditor(typeof(LootDrop))]
    public class LootDropEditor : Editor
    {
        // Guaranteed
        SerializedProperty guaranteedList;
        ReorderableList reorderableGuaranteed;
        // Change
        SerializedProperty changeToGetList;
        ReorderableList reorderableChange;

        LootDrop ld;

        private void OnEnable()
        {
            /* GUARANTEED */
            guaranteedList = serializedObject.FindProperty("GuaranteedLootTable");
            reorderableGuaranteed = new ReorderableList(serializedObject, guaranteedList, true, true, true, true);
            // Functions
            reorderableGuaranteed.drawElementCallback = DrawGuaranteedListItems;
            reorderableGuaranteed.drawHeaderCallback = DrawHeaderGuaranteed;

            /* Change */
            changeToGetList = serializedObject.FindProperty("OneItemFromList");
            reorderableChange = new ReorderableList(serializedObject, changeToGetList, true, true, true, true);
            // Functions
            reorderableChange.drawElementCallback += DrawChangeListItems;
            reorderableChange.drawHeaderCallback += DrawHeaderChange;

            ld = target as LootDrop;
        }



        void DrawHeaderGuaranteed(Rect rect) { EditorGUI.LabelField(rect, "Guaranteed Loot Table"); }
        void DrawHeaderChange(Rect rect) { EditorGUI.LabelField(rect, "Change To Get Loot Table"); }

        void DrawGuaranteedListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            LootDrop loot = (LootDrop)target;
            reorderableGuaranteed.elementHeight = 42;

            SerializedProperty element = reorderableGuaranteed.serializedProperty.GetArrayElementAtIndex(index);


            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, rect.height), element, GUIContent.none);

            //LootDrop loot = (LootDrop)target;
        }

        void DrawChangeListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            reorderableChange.elementHeight = 42;

            SerializedProperty element = reorderableChange.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, rect.height), element, GUIContent.none);
        }

        public override void OnInspectorGUI()
        {

            EditorUtility.SetDirty(target);
            LootDrop loot = (LootDrop)target;
            EditorGUILayout.BeginVertical();

            EditorGUI.indentLevel = 0;

            // Title
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(new GUIContent(loot.name), EditorStyles.boldLabel);
            EditorGUILayout.Space(-8); EditorGUILayout.LabelField(new GUIContent("Loot Table"), EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();

            // Sort Button
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(5);
            if (TinyGUI.IconButton("AlphabeticalSorting", " Sort", "Sort All Drop by Weight", GUILayout.Height(30)))
            {
                List<DropChangeItem> tmp = new List<DropChangeItem>(loot.OneItemFromList);
                List<DropChangeItem> tmp2 = new List<DropChangeItem>(loot.GuaranteedLootTable);

                loot.OneItemFromList = tmp.OrderByDescending(ch => ch.Weight).ToArray();
                loot.GuaranteedLootTable = tmp2.OrderByDescending(ch => ch.Weight).ToArray();

                serializedObject.ApplyModifiedProperties();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();
            bool dddd = true;
            TinyGUI.ShowArray(serializedObject, "GuaranteedLootTable", "Guaranteed Drop", ref dddd);
            TinyGUI.ShowArray(serializedObject, "OneItemFromList", "Additional Drop", ref dddd);
            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck())
            {
                for (int index = 0; index < loot.OneItemFromList.Length; index++)
                {
                    SerializedProperty OIFElement = reorderableChange.serializedProperty.GetArrayElementAtIndex(index);
                    loot.OneItemFromList[index].Weight = OIFElement.FindPropertyRelative("Weight").floatValue;
                    loot.OneItemFromList[index].Drop = (GameObject)OIFElement.FindPropertyRelative("Drop").objectReferenceValue;
                    loot.OneItemFromList[index].MinCountItem = OIFElement.FindPropertyRelative("MinCountItem").intValue;
                    loot.OneItemFromList[index].MaxCountItem = OIFElement.FindPropertyRelative("MaxCountItem").intValue;
                }
                for (int index = 0; index < loot.GuaranteedLootTable.Length; index++)
                {
                    SerializedProperty GuaranteedElement = reorderableGuaranteed.serializedProperty.GetArrayElementAtIndex(index);
                    loot.GuaranteedLootTable[index].Weight = GuaranteedElement.FindPropertyRelative("Weight").floatValue;
                    loot.GuaranteedLootTable[index].Drop = (GameObject)GuaranteedElement.FindPropertyRelative("Drop").objectReferenceValue;
                    loot.GuaranteedLootTable[index].MinCountItem = GuaranteedElement.FindPropertyRelative("MinCountItem").intValue;
                    loot.GuaranteedLootTable[index].MaxCountItem = GuaranteedElement.FindPropertyRelative("MaxCountItem").intValue;
                }
            }
            loot.WeightToNoDrop = EditorGUILayout.FloatField("No Drop Weight", loot.WeightToNoDrop);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();



            ///
            /// Drop Change Visualization
            ///

            float totalWeight = loot.WeightToNoDrop;
            float guaranteedHeight = 0;

            if (loot.OneItemFromList != null)
            {
                for (int j = 0; j < loot.OneItemFromList.Length; j++)
                {
                    totalWeight += loot.OneItemFromList[j].Weight;
                }
            }

            EditorGUILayout.BeginVertical("HelpBox");

            EditorGUILayout.LabelField("Guaranteed Drop", EditorStyles.boldLabel);
            for (int i = 0; i < loot.GuaranteedLootTable.Length; i++)
            {
                string line1 = "";
                string line2 = "";

                if (loot.GuaranteedLootTable[i].Drop != null)
                {
                    // Setup Line First
                    line1 = $"{loot.GuaranteedLootTable[i].Drop.name}";

                    if (loot.GuaranteedLootTable[i].MinCountItem >= 0)
                    {
                        if (loot.GuaranteedLootTable[i].MinCountItem <= loot.GuaranteedLootTable[i].MaxCountItem)
                        {
                            // Setup Line Secound
                            if (loot.GuaranteedLootTable[i].MinCountItem == loot.GuaranteedLootTable[i].MaxCountItem)
                            {
                                line2 = $"Drawn [{loot.GuaranteedLootTable[i].MaxCountItem}] {loot.GuaranteedLootTable[i].Drop.name} ";
                            }
                            else
                            {
                                line2 = $"Drawn [{loot.GuaranteedLootTable[i].MinCountItem}-{loot.GuaranteedLootTable[i].MaxCountItem}] {loot.GuaranteedLootTable[i].Drop.name} ";
                            }
                            if (loot.GuaranteedLootTable[i].MinCountItem == 0)
                            {
                                TinyGUI.InfoBox($"{line1}\n{line2}\nMin Count = 0, so despite being drawn, the object will not appear", "RectTransformBlueprint");
                            }
                            else
                            {
                                TinyGUI.InfoBox($"{line1}\n{line2}\nGuaranteed", "sv_icon_dot11_pix16_gizmo");
                            }
                        }
                        else  // Error: MinCount > MaxCount
                        {
                            TinyGUI.InfoBox($"\"{loot.GuaranteedLootTable[i].Drop.name}\" Error: Min Count > Max Count\nMin Count cannot be greater than Max Count\nSolution: Replace Min Count with Max Count", "HighPassFilter Icon");
                        }
                    }
                    else // Error: MinCount < 0
                    {
                        TinyGUI.InfoBox($"\"{loot.GuaranteedLootTable[i].Drop.name}\" Error: Min Count < 0\nMin Count must not be less than 0\nSolution: replace Min Count with a positive number", "Toolbar Minus");
                    }
                }
                else // Error: No Drop 
                {
                    TinyGUI.InfoBox($"\"Drop ID: {i + 1}\" Error: No Drop Object\nItem To Drop cannot be empty!\nSolution: Assign to Item To Drop the object to be drawn", "PhysicsRaycaster Icon");
                }
            }

            EditorGUILayout.LabelField("Additional Drop Change", EditorStyles.boldLabel);
            for (int i = 0; i < loot.OneItemFromList.Length; i++)
            {
                string line1 = "";
                string line2 = "";

                if (loot.OneItemFromList[i].Weight >= 0)
                {
                    if (loot.OneItemFromList[i].Drop != null)
                    {
                        // Setup Line First
                        line1 = $"{(loot.OneItemFromList[i].Weight / totalWeight * 100).ToString("F2")}% Change | {loot.OneItemFromList[i].Drop.name}";

                        if (loot.OneItemFromList[i].MinCountItem >= 0)
                        {
                            if (loot.OneItemFromList[i].MinCountItem <= loot.OneItemFromList[i].MaxCountItem)
                            {
                                // Setup Line Secound
                                if (loot.OneItemFromList[i].MinCountItem == loot.OneItemFromList[i].MaxCountItem)
                                {
                                    line2 = $"Drawn [{loot.OneItemFromList[i].MaxCountItem}] {loot.OneItemFromList[i].Drop.name} ";
                                }
                                else
                                {
                                    line2 = $"Drawn [{loot.OneItemFromList[i].MinCountItem}-{loot.OneItemFromList[i].MaxCountItem}] {loot.OneItemFromList[i].Drop.name} ";
                                }
                                if (loot.OneItemFromList[i].MinCountItem == 0)
                                {
                                    TinyGUI.InfoBox($"{line1}\n{line2}\nMin Count = 0, so despite being drawn, the object will not appear", "RectTransformBlueprint");
                                }
                                else
                                {
                                    TinyGUI.InfoBox($"{line1}\n{line2}\n", "sv_icon_dot2_pix16_gizmo");
                                }
                            }
                            else  // Error: MinCount > MaxCount
                            {
                                TinyGUI.InfoBox($"\"{loot.OneItemFromList[i].Drop.name}\" Error: Min Count > Max Count\nMin Count cannot be greater than Max Count\nSolution: Replace Min Count with Max Count", "HighPassFilter Icon");
                            }
                        }
                        else // Error: MinCount < 0
                        {
                            TinyGUI.InfoBox($"\"{loot.OneItemFromList[i].Drop.name}\" Error: Min Count < 0\nMin Count must not be less than 0\nSolution: replace Min Count with a positive number", "Toolbar Minus");
                        }
                    }
                    else // Error: No Drop 
                    {
                        TinyGUI.InfoBox($"\"Drop ID: {i + 1}\" Error: No Drop Object\nItem To Drop cannot be empty!\nSolution: Assign to Item To Drop the object to be drawn", "PhysicsRaycaster Icon");
                    }
                }
                else // Error: Weight < 0
                {
                    if (loot.OneItemFromList[i].Drop == null) // Also No Drop Object
                    {
                        TinyGUI.InfoBox($"\"Drop ID: {i + 1}\" Error: Weight < 0\nWeight must not be less than 0\nSolution: set Weight to a positive number", "CollabDeleted Icon");
                    }
                    else
                    {
                        TinyGUI.InfoBox($"\"{loot.OneItemFromList[i].Drop.name}\" Error: Weight < 0\nWeight must not be less than 0\nSolution: set Weight to a positive number", "CollabDeleted Icon");
                    }
                }
            }

            //EditorGUILayout.LabelField("No Additional Drop", EditorStyles.boldLabel);

            TinyGUI.InfoBox($"\n{(loot.WeightToNoDrop / totalWeight * 100).ToString("F2")}% Change to No Additional Drop\n", "FolderEmpty Icon");
            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginVertical("HelpBox");
            EditorGUILayout.LabelField("Visual Additional Drop", EditorStyles.boldLabel);

            Rect r = EditorGUILayout.BeginVertical();
            //myStyle.fontStyle = FontStyle.Bold;
            //myStyle.fontSize = 20;
            var _oldColor = GUI.backgroundColor;

            for (int i = 0; i < loot.OneItemFromList.Length; i++)
            {
                string _tmpString = "";
                if (loot.OneItemFromList[i].Drop == null) { _tmpString = "No Drop Object"; } else { _tmpString = loot.OneItemFromList[i].Drop.name; }
                if (loot.OneItemFromList[i].Weight / totalWeight < 0)
                {
                    //GUI.backgroundColor = Color.red;
                    EditorGUI.ProgressBar(new Rect(r.x + 5, r.y + (25 * i) + guaranteedHeight, r.width - 10, 25), 0, "Error: Weight < 0");
                }
                else
                {
                    EditorGUI.ProgressBar(new Rect(r.x + 5, r.y + (25 * i) + guaranteedHeight, r.width - 10, 25), loot.OneItemFromList[i].Weight / totalWeight, $"{(loot.OneItemFromList[i].Weight / totalWeight * 100).ToString("f2")}% | {_tmpString}");
                }
                GUI.backgroundColor = _oldColor;
            }

            EditorGUILayout.Space(25 * loot.OneItemFromList.Length + guaranteedHeight);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginVertical("HelpBox");
            EditorGUILayout.LabelField("Defualt Drop Settings", EditorStyles.boldLabel);

            loot.DefualtAdditionalDropCount = EditorGUILayout.IntField("Additional Drop Count", loot.DefualtAdditionalDropCount);
            loot.DefualtMinDropRange = EditorGUILayout.FloatField("Min Drop Range", loot.DefualtMinDropRange);
            loot.DefualtMaxDropRange = EditorGUILayout.FloatField("Max Drop Range", loot.DefualtMaxDropRange);

            EditorGUILayout.EndVertical();
        }


        void ValidateOneItemFromList(LootDrop loot)
        {
            bool _countError = false;
            bool _prefabError = false;
            bool _weightError = false;

            for (int index = 0; index < loot.OneItemFromList.Length; index++)
            {
                if (loot.OneItemFromList[index].Drop == null) { _prefabError = true; }
                if (loot.OneItemFromList[index].MinCountItem <= 0) { _countError = true; }
                if (loot.OneItemFromList[index].MinCountItem > loot.OneItemFromList[index].MaxCountItem) { _countError = true; }
                if (loot.OneItemFromList[index].Weight < 0) { _weightError = true; }
            }
            if (_prefabError == true) { TinyGUI.InfoBox("One of the List Items does not have ''Item To Drop'' assigned, which will cause an error if it is drawn", "console.erroricon"); }
            if (_countError == true) { TinyGUI.InfoBox("One of the List Items has an incorrect number of items, which will result in items not appearing when drawn", "console.warnicon"); }
            if (_weightError == true) { TinyGUI.InfoBox("One of the List Items has an incorrect Weight, this will cause erroneous data readings or the whole system will crash", "console.erroricon"); }
        }
        void ValidateGuaranteedList(LootDrop loot)
        {
            bool _countError = false;
            bool _prefabError = false;
            bool _weightError = false;

            for (int index = 0; index < loot.GuaranteedLootTable.Length; index++)
            {
                if (loot.GuaranteedLootTable[index].Drop == null) { _prefabError = true; }
                if (loot.GuaranteedLootTable[index].MinCountItem <= 0) { _countError = true; }
                if (loot.GuaranteedLootTable[index].MinCountItem > loot.GuaranteedLootTable[index].MaxCountItem) { _countError = true; }
                if (loot.GuaranteedLootTable[index].Weight < 0) { _weightError = true; }
            }
            if (_prefabError == true) { TinyGUI.InfoBox("One of the List Items does not have ''Item To Drop'' assigned, which will cause an error if it is drawn", "console.erroricon"); }
            if (_countError == true) { TinyGUI.InfoBox("One of the List Items has an incorrect number of items, which will result in items not appearing when drawn", "console.warnicon"); }
            if (_weightError == true) { TinyGUI.InfoBox("One of the List Items has an incorrect Weight, this will cause erroneous data readings or the whole system will crash", "console.erroricon"); }
        }
    }


    /* --------------------- */
    // Custom Property Draw
    /* --------------------- */

    [CustomPropertyDrawer(typeof(DropChangeItem))]
    public class DropChangeItemDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var _oldColor = GUI.backgroundColor;
            EditorGUI.BeginProperty(position, label, property);
            //GUI.backgroundColor = Color.red;

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var weightRectLabel = new Rect(position.x, position.y, 55, 18);
            var weightRect = new Rect(position.x, position.y + 20, 55, 18);

            EditorGUI.LabelField(weightRectLabel, "Weight");
            if (property.FindPropertyRelative("Weight").floatValue < 0) { GUI.backgroundColor = Color.red; }
            EditorGUI.PropertyField(weightRect, property.FindPropertyRelative("Weight"), GUIContent.none);
            GUI.backgroundColor = _oldColor;

            var ItemRectLabel = new Rect(position.x + 60, position.y, position.width - 60, 18);
            var ItemRect = new Rect(position.x + 60, position.y + 20, position.width - 60 - 75, 18);

            EditorGUI.LabelField(ItemRectLabel, "Item To Drop");
            if (property.FindPropertyRelative("Drop").objectReferenceValue == null) { GUI.backgroundColor = Color.red; }
            EditorGUI.PropertyField(ItemRect, property.FindPropertyRelative("Drop"), GUIContent.none);
            GUI.backgroundColor = _oldColor;

            var MinMaxRectLabel = new Rect(position.x + position.width - 70, position.y, 70, 18);

            var MinRect = new Rect(position.x + position.width - 70, position.y + 20, 30, 18);
            var MinMaxRect = new Rect(position.x + position.width - 39, position.y + 20, 9, 18);
            var MaxRect = new Rect(position.x + position.width - 30, position.y + 20, 30, 18);

            if (property.FindPropertyRelative("MinCountItem").intValue < 0) { GUI.backgroundColor = Color.red; }
            if (property.FindPropertyRelative("MaxCountItem").intValue < property.FindPropertyRelative("MinCountItem").intValue) { GUI.backgroundColor = Color.red; }

            EditorGUI.LabelField(MinMaxRectLabel, "Min  -  Max");
            EditorGUI.PropertyField(MinRect, property.FindPropertyRelative("MinCountItem"), GUIContent.none);
            EditorGUI.LabelField(MinMaxRect, "-");
            EditorGUI.PropertyField(MaxRect, property.FindPropertyRelative("MaxCountItem"), GUIContent.none);
            GUI.backgroundColor = _oldColor;

            EditorGUI.EndProperty();
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 40;
        }
    }

#endif
}