using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System;
using HellishBattle.SaveSystem;

namespace HellishBattle.Weapon
{
    public class UpgradeWeaponMenuManager : MonoBehaviour
    {
        [HideInInspector] public List<BaseWeaponScript> AllWeapons;
        [HideInInspector] public List<RangeWeapon> AllRange;
        [HideInInspector] public List<MelleWeapon> AllMelle;
        [HideInInspector] public List<ThrowableWeapon> AllThrowable;

        // Weapon List
        [Line("Weapon List")]
        public GameObject RangeListContainer;
        public GameObject MelleListContainer;
        public GameObject ThrowableListContainer;
        public GameObject WeaponButtonPrefab;
        [Line("Weapon Customization")]
        public GameObject WeaponCustomizationPanel;
        public GameObject SkinSelectionBtnLeft, SkinSelectionBtnRight;
        public GameObject UpgradeSelection, NoUpgradeSelection;
        [Space(10)]
        public GameObject WeaponUpgradeList;
        public GameObject WeaponUpgradePrefab;
        [Space(10)]
        public GameObject EquipOrBuyUpgrade;
        public GameObject EquipOrBuySkin;
        public Image WeaponUIIcon;
        public TMP_Text WeaponSkinName;
        public TMP_Text WeaponNameLocalization;
        public TMP_Text UpgradeDescription;

        public int CustomizationWeaponID;
        WeaponUpgradeSaveClass WeaponSaveObj;

        List<WeaponSkinClass> skinList;
        List<WeaponUpgradeClass> upgradeList;
        public int selectedUpgrade, selectedSkin;

        private void Start()
        {
            // Generate List of All Weapon
            GenerateWeaponList();
        }

        private void Update()
        {

        }

        public void GenerateWeaponList()
        {
            GameObject[] prefabs = Resources.LoadAll<GameObject>("Weapon");
            AllWeapons.Clear();
            AllRange.Clear();
            AllMelle.Clear();
            AllThrowable.Clear();

            foreach (Transform child in RangeListContainer.transform) { GameObject.Destroy(child.gameObject); }
            foreach (Transform child in MelleListContainer.transform) { GameObject.Destroy(child.gameObject); }
            foreach (Transform child in ThrowableListContainer.transform) { GameObject.Destroy(child.gameObject); }

            for (int i = 0; i < prefabs.Length; i++)
            {
                //WeaponUpgradeSaveClass _tmp = TinySaveSystem.GetWeaponUpgrade($"Weapon_{prefabs[i].GetComponent<BaseWeaponScript>().WeaponID}");
                //if (TinySaveSystem.HasKey($"Weapon_{prefabs[i].GetComponent<BaseWeaponScript>().WeaponID}")) { TinySaveSystem.SetWeaponUpgrade($"Weapon_{prefabs[i].GetComponent<BaseWeaponScript>().WeaponID}", _tmp); this.LogWarning("Overload"); }
                AllWeapons.Add(prefabs[i].GetComponent<BaseWeaponScript>());

                GameObject btn;
                int index = i;

                // Add to Category
                if (prefabs[i].GetComponent<BaseWeaponScript>().Slot == SlotType.MelleWeapon)
                {
                    AllMelle.Add(prefabs[i].GetComponent<MelleWeapon>());
                    btn = Instantiate(WeaponButtonPrefab, MelleListContainer.transform);
                }

                else if (prefabs[i].GetComponent<BaseWeaponScript>().Slot == SlotType.ThrowableWeapon)
                {
                    AllThrowable.Add(prefabs[i].GetComponent<ThrowableWeapon>());
                    btn = Instantiate(WeaponButtonPrefab, ThrowableListContainer.transform);
                }

                else
                {
                    AllRange.Add(prefabs[i].GetComponent<RangeWeapon>());
                    btn = Instantiate(WeaponButtonPrefab, RangeListContainer.transform);
                }

                // Generate Wepaon
                btn.transform.Find("Icon").GetComponent<Image>().sprite = prefabs[i].GetComponent<BaseWeaponScript>().WeaponIcon();
                btn.transform.Find("Name").GetComponent<TMPro.TMP_Text>().text = prefabs[i].GetComponent<BaseWeaponScript>().WeaponName();
                btn.transform.Find("Category").GetComponent<TMPro.TMP_Text>().text = prefabs[i].GetComponent<BaseWeaponScript>().Category.ToString();

                btn.GetComponent<Button>().onClick.AddListener(delegate () { OpenWeaponCustomization(index); });
                btn.GetComponent<Button>().onClick.AddListener(delegate () { WeaponCustomizationPanel.SetActive(true); });
            }
        }

        /// <summary>
        /// Funkcja wywo³ywana podczas otwarcia menu customizacji
        /// </summary>
        /// <param name="i"></param>
        void OpenWeaponCustomization(int i)
        {
            CustomizationWeaponID = i;

            // Load Save Info
            WeaponSaveObj = TinySaveSystem.GetWeaponUpgrade($"Weapon_{AllWeapons[i].WeaponID}");
            selectedUpgrade = WeaponSaveObj.selectedUpgrade;
            selectedSkin = WeaponSaveObj.selectedSkin;

            // Load Skin and Upgrade List
            skinList = AllWeapons[i].GetSkinClass();
            upgradeList = AllWeapons[i].GetUpgradeClass();

            //this.Test(skinList.Count);

            // Check Unlocked Upgrade List
            if (WeaponSaveObj.unlockedUpgrade != null)
            {
                string[] unlocked = StringToArray<string>(WeaponSaveObj.unlockedUpgrade);
                List<string> someList = new List<string>(unlocked);

                if (someList.Count == 1 && (someList[0] != "false" || someList[0] != "true")) { someList[0] = "false"; WeaponSaveObj.unlockedUpgrade = ArrayToString<string>(someList.ToArray()); }
                while (someList.Count != upgradeList.Count)
                {
                    // If to many in safe File
                    if (someList.Count > upgradeList.Count) { someList.RemoveAt(someList.Count - 1); Debug.Log("Too Much"); WeaponSaveObj.unlockedUpgrade = ArrayToString<string>(someList.ToArray()); }
                    else if (someList.Count < upgradeList.Count)
                    {
                        //Debug.Log($"Too Less");

                        if (someList.Count == 1 && (someList[0] != "false" || someList[0] != "true")) { someList.RemoveAt(0); WeaponSaveObj.unlockedUpgrade = ArrayToString<string>(someList.ToArray()); }
                        for (int lost = 0; lost < upgradeList.Count - someList.Count; lost++) { someList.Add("false"); }

                        //this.Log(someList.Count, upgradeList.Count);
                        WeaponSaveObj.unlockedUpgrade = ArrayToString<string>(someList.ToArray());
                        continue;
                    }
                }
            }
            else
            {
                //this.Test("Create New Upgrade List");
                List<string> newUpgrade = new List<string>();
                for (int upgrade = 0; upgrade < upgradeList.Count; upgrade++) { newUpgrade.Add("false"); }
                string toSave = ArrayToString<string>(newUpgrade.ToArray());
                WeaponSaveObj.unlockedUpgrade = toSave;
            }

            // Check Unlocked Skin List
            if (WeaponSaveObj.unlockedSkin != null)
            {
                //this.Test(skinList.Count);
                string[] unlocked = StringToArray<string>(WeaponSaveObj.unlockedSkin);
                List<string> someList = new List<string>(unlocked);
                // this.Log(someList.Count != skinList.Count);

                if (someList.Count == 1 && (someList[0] != "false" && someList[0] != "true")) { someList[0] = "false"; WeaponSaveObj.unlockedSkin = ArrayToString<string>(someList.ToArray()); }
                //Debug.Log($"{someList[0]}: {someList[0] != "false"} {someList[0] != "true"}");
                while (someList.Count != skinList.Count)
                {
                    //this.Test("Start Here?");
                    // If to many in safe File
                    if (someList.Count > skinList.Count) { someList.RemoveAt(someList.Count - 1); WeaponSaveObj.unlockedSkin = ArrayToString<string>(someList.ToArray()); }

                    else if (someList.Count < skinList.Count)
                    {
                        /// Debug.Log($"Too Less");

                        if (someList.Count == 1 && (someList[0] != "false" || someList[0] != "true")) { someList.RemoveAt(0); WeaponSaveObj.unlockedSkin = ArrayToString<string>(someList.ToArray()); }
                        for (int lost = 0; lost < skinList.Count - someList.Count; lost++) { someList.Add("false"); WeaponSaveObj.unlockedSkin = ArrayToString<string>(someList.ToArray()); }

                        //this.Log(someList.Count, skinList.Count);
                        WeaponSaveObj.unlockedSkin = ArrayToString<string>(someList.ToArray());
                    }
                }
            }
            else
            {
                //this.Test("Create New Skin List");
                List<string> newUpgrade = new List<string>();
                for (int upgrade = 0; upgrade < skinList.Count; upgrade++) { newUpgrade.Add("false"); }
                string toSave = ArrayToString<string>(newUpgrade.ToArray());
                WeaponSaveObj.unlockedSkin = toSave;
            }

            //this.Log(WeaponSaveObj.unlockedUpgrade, WeaponSaveObj.unlockedSkin);

            // Save
            TinySaveSystem.SetWeaponUpgrade($"Weapon_{AllWeapons[i].WeaponID}", WeaponSaveObj);

            // Show Update Info
            PreviewUpgrade(WeaponSaveObj.selectedUpgrade);

            // Reload UI
            UpdateUI();
        }

        /// <summary>
        /// Przechodzi do nastêpnego skina 
        /// </summary>
        public void UINextSkin()
        {
            selectedSkin++;
            if (skinList.Count == selectedSkin || skinList.Count == 0) { selectedSkin = -1; }

            UpdateUI();
        }

        /// <summary>
        /// Przechodzi do poprzedniego skina 
        /// </summary>
        public void UIPreviousSkin()
        {
            selectedSkin--;
            if (-2 == selectedSkin) { selectedSkin = skinList.Count - 1; }

            UpdateUI();
        }

        /// <summary>
        /// Funkcja u¿ywanie przy odœwierzaniu ca³ego UI zwi¹zanego z customizacj¹ Broni
        /// </summary>
        public void UpdateUI()
        {
            /* BASE */
            WeaponNameLocalization.text = AllWeapons[CustomizationWeaponID].WeaponName();

            /* SKINS */
            PreviewSkin(selectedSkin);

            // Hide UI
            if (skinList.Count == 0) { SkinSelectionBtnLeft.SetActive(false); SkinSelectionBtnRight.SetActive(false); }
            else { SkinSelectionBtnLeft.SetActive(true); SkinSelectionBtnRight.SetActive(true); }

            // Hide UI
            if (upgradeList.Count == 0) { UpgradeSelection.SetActive(false); NoUpgradeSelection.SetActive(true); }
            else { UpgradeSelection.SetActive(true); NoUpgradeSelection.SetActive(false); }

            // Weapon Upgrade Buttons
            if (upgradeList.Count != 0)
            {
                // Reset Buttons
                foreach (Transform child in WeaponUpgradeList.transform) { GameObject.Destroy(child.gameObject); }
                // Spawn Defualt Button
                GameObject _base = Instantiate(WeaponUpgradePrefab, WeaponUpgradeList.transform);
                _base.transform.Find("WeaponIcon").GetComponent<Image>().sprite = AllWeapons[CustomizationWeaponID].weaponIcon;
                _base.transform.Find("WeaponName").GetComponent<TMP_Text>().text = AllWeapons[CustomizationWeaponID].weaponName.GetLocalization();
                _base.transform.Find("Locked").gameObject.SetActive(false);
                _base.transform.Find("Selected").gameObject.SetActive(WeaponSaveObj.selectedUpgrade == -1);

                // Preview
                _base.GetComponent<Button>().onClick.AddListener(delegate () { PreviewUpgrade(-1); });


                for (int i = 0; i < upgradeList.Count; i++)
                {
                    int index = i;

                    GameObject _upgrade = Instantiate(WeaponUpgradePrefab, WeaponUpgradeList.transform);
                    if (upgradeList[i].SkinUIIcon != null) { _upgrade.transform.Find("WeaponIcon").GetComponent<Image>().sprite = upgradeList[i].SkinUIIcon; }
                    else { _upgrade.transform.Find("WeaponIcon").GetComponent<Image>().sprite = AllWeapons[CustomizationWeaponID].weaponIcon; }

                    // Check if Unlocked
                    string[] unlocked = StringToArray<string>(WeaponSaveObj.unlockedUpgrade);
                    _upgrade.transform.Find("Locked").gameObject.SetActive(unlocked[i] != "true");
                    if (unlocked[i] != "true") { _upgrade.transform.Find("Locked").Find("Price").GetComponent<TMP_Text>().text = upgradeList[i].Cost + " P"; }
                    _upgrade.transform.Find("Selected").gameObject.SetActive(WeaponSaveObj.selectedUpgrade == i);
                    // Debug.Log(unlocked[i]);

                    _upgrade.transform.Find("Selected").gameObject.SetActive(WeaponSaveObj.selectedUpgrade == i);
                    _upgrade.transform.Find("WeaponName").GetComponent<TMP_Text>().text = upgradeList[i].WeaponName.GetLocalization();

                    // Preview
                    _upgrade.GetComponent<Button>().onClick.AddListener(delegate () { PreviewUpgrade(index); });
                }
            }
        }

        /// <summary>
        /// Preview Selected Upgrade
        /// </summary>
        /// <param name="i">Numer Broni</param>
        public void PreviewUpgrade(int i)
        {
            // Clear Listener
            EquipOrBuyUpgrade.GetComponent<Button>().onClick.RemoveAllListeners();
            if (selectedSkin != -1) { /* Dont Change Sprite */ }
            else if (i == -1) { WeaponUIIcon.sprite = AllWeapons[CustomizationWeaponID].weaponIcon; }
            else { WeaponUIIcon.sprite = WeaponUIIcon.sprite = upgradeList[i].SkinUIIcon; }

            if (i == -1)
            {
                // Not Selected
                if (selectedUpgrade != -1)
                {
                    EquipOrBuyUpgrade.GetComponent<TMP_Text>().text = $"Equip {AllWeapons[CustomizationWeaponID].weaponName.GetLocalization()}";
                    EquipOrBuyUpgrade.GetComponent<Button>().onClick.AddListener(delegate () { EquipUpgrade(i); });
                }
                // Selected
                else { EquipOrBuyUpgrade.GetComponent<TMP_Text>().text = $"Equipped"; }

                if (AllWeapons[CustomizationWeaponID].weaponDesc.localization != null) UpgradeDescription.text = AllWeapons[CustomizationWeaponID].weaponDesc.GetLocalization();
                else { UpgradeDescription.text = "/// // / NO DATA / // ///"; }
            }
            else
            {
                string[] upgrade = StringToArray<string>(WeaponSaveObj.unlockedUpgrade);
                // To Buy
                if (upgrade[i] == "false")
                {
                    EquipOrBuyUpgrade.GetComponent<TMP_Text>().text = $"Buy {upgradeList[i].WeaponName.GetLocalization()} \n[ {upgradeList[i].Cost} p ]";
                    EquipOrBuyUpgrade.GetComponent<Button>().onClick.AddListener(delegate () { TryBuyUpgrade(i); });
                }

                // if Unlocked
                else
                {
                    // Not Selected
                    if (selectedUpgrade != i)
                    {
                        EquipOrBuyUpgrade.GetComponent<TMP_Text>().text = $"Equip {upgradeList[i].WeaponName.GetLocalization()}";
                        EquipOrBuyUpgrade.GetComponent<Button>().onClick.AddListener(delegate () { EquipUpgrade(i); });
                    }

                    // Selected
                    else { EquipOrBuyUpgrade.GetComponent<TMP_Text>().text = $"Equipped"; }
                }

                if (upgradeList[i].UpgradeDescription.localization != null) UpgradeDescription.text = upgradeList[i].UpgradeDescription.GetLocalization();
                else { UpgradeDescription.text = "/// // / NO DATA / // ///"; }
            }
        }

        /// <summary>
        /// Preview Selected Upgrade
        /// </summary>
        /// <param name="i">Numer Broni</param>
        public void PreviewSkin(int i)
        {
            // Icon
            if (i == -1) { WeaponUIIcon.sprite = AllWeapons[CustomizationWeaponID].weaponIcon; }
            else { WeaponUIIcon.sprite = WeaponUIIcon.sprite = skinList[selectedSkin].SkinUIIcon; }

            // Load Skin Name
            if (selectedSkin == -1) { WeaponSkinName.text = "Defualt Skin"; }
            else { WeaponSkinName.text = skinList[selectedSkin].WeaponName.GetLocalization(); }

            // Clear Listener
            EquipOrBuySkin.GetComponent<Button>().onClick.RemoveAllListeners();


            if (i == -1)
            {
                // Not Selected
                if (WeaponSaveObj.selectedSkin != -1)
                {
                    EquipOrBuySkin.GetComponent<TMP_Text>().text = $"Equip Base Skin";
                    EquipOrBuySkin.GetComponent<Button>().onClick.AddListener(delegate () { EquipSkin(i); });
                }
                // Selected
                else { EquipOrBuySkin.GetComponent<TMP_Text>().text = $"Equipped"; }
            }
            else
            {
                string[] upgrade = StringToArray<string>(WeaponSaveObj.unlockedSkin);
                // To Buy
                if (upgrade[i] == "false")
                {
                    EquipOrBuySkin.GetComponent<TMP_Text>().text = $"Buy {skinList[i].WeaponName.GetLocalization()} \n[ {skinList[i].Cost} p ]";
                    EquipOrBuySkin.GetComponent<Button>().onClick.AddListener(delegate () { TryBuySkin(i); });
                }

                // if Unlocked
                else
                {
                    // Not Selected
                    if (WeaponSaveObj.selectedSkin != i)
                    {
                        EquipOrBuySkin.GetComponent<TMP_Text>().text = $"Equip {skinList[i].WeaponName.GetLocalization()}";
                        EquipOrBuySkin.GetComponent<Button>().onClick.AddListener(delegate () { EquipSkin(i); });
                    }

                    // Selected
                    else { EquipOrBuySkin.GetComponent<TMP_Text>().text = $"Equipped"; }
                }
            }
        }

        public void TryBuyUpgrade(int i)
        {
            // Check if can Buy
            if (TinySaveSystem.GetInt("unlockable_point") >= upgradeList[i].Cost)
            {
                // Buy Upgrade
                string[] upgrade = StringToArray<string>(WeaponSaveObj.unlockedUpgrade);
                upgrade[i] = "true";
                WeaponSaveObj.unlockedUpgrade = ArrayToString<string>(upgrade);

                // Remove Points
                TinySaveSystem.SetInt("unlockable_point", (TinySaveSystem.GetInt("unlockable_point") - upgradeList[i].Cost));

                // Select Upgrade
                WeaponSaveObj.selectedUpgrade = i;
                selectedUpgrade = i;

                // Save Buy
                TinySaveSystem.SetWeaponUpgrade($"Weapon_{AllWeapons[CustomizationWeaponID].WeaponID}", WeaponSaveObj);

                // Reload UI
                UpdateUI();
            }
            else
            {
                TinyDebug.Warning("Need More Points");
            }
        }
        public void TryBuySkin(int i)
        {
            // Check if can Buy
            if (TinySaveSystem.GetInt("unlockable_point") >= skinList[i].Cost)
            {
                // Buy Upgrade
                string[] upgrade = StringToArray<string>(WeaponSaveObj.unlockedSkin);
                upgrade[i] = "true";
                WeaponSaveObj.unlockedSkin = ArrayToString<string>(upgrade);

                // Remove Points
                TinySaveSystem.SetInt("unlockable_point", (TinySaveSystem.GetInt("unlockable_point") - skinList[i].Cost));

                // Select Upgrade
                WeaponSaveObj.selectedSkin = i;
                selectedSkin = i;

                // Save Buy
                TinySaveSystem.SetWeaponUpgrade($"Weapon_{AllWeapons[CustomizationWeaponID].WeaponID}", WeaponSaveObj);

                // Reload UI
                UpdateUI();
            }
            else
            {
                TinyDebug.Warning("Need More Points");
            }
        }

        public void EquipUpgrade(int i)
        {
            // Select Upgrade
            WeaponSaveObj.selectedUpgrade = i;
            selectedUpgrade = i;

            // Save Buy
            TinySaveSystem.SetWeaponUpgrade($"Weapon_{AllWeapons[CustomizationWeaponID].WeaponID}", WeaponSaveObj);

            // Reload UI
            UpdateUI();
        }
        public void EquipSkin(int i)
        {
            // Select Upgrade
            WeaponSaveObj.selectedSkin = i;
            selectedSkin = i;

            // Save Buy
            TinySaveSystem.SetWeaponUpgrade($"Weapon_{AllWeapons[CustomizationWeaponID].WeaponID}", WeaponSaveObj);

            // Reload UI
            UpdateUI();
        }



        string ArrayToString<T>(T[] array)
        {
            string separator = ", ";
            return string.Join(separator, array);
        }
        T[] StringToArray<T>(string value)
        {
            string separator = ", ";

            string[] stringArray = value.Split(new string[] { separator }, StringSplitOptions.None);
            T[] resultArray = new T[stringArray.Length];

            for (int i = 0; i < stringArray.Length; i++) { resultArray[i] = (T)Convert.ChangeType(stringArray[i], typeof(T)); }

            return resultArray;
        }
    }
}