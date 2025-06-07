using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using System;
using System.Linq;
using HellishBattle.Level;
using HellishBattle.SaveSystem;
using HellishBattle.Player;

namespace HellishBattle.Weapon
{
    public class WeaponSwitch : MonoBehaviour
    {
        // Reference to the currently equipped weapon
        public BaseWeaponScript actualWeapon;

        // UI elements for displaying ammo count and weapon details
        public TMP_Text ammoText;
        public TMP_Text WeaponNameText;
        public Image WeaponUIImage;
        public Image AmmoIcon;

        // Icons for different types of ammunition
        public List<AmmoIconClass> AmmoSprites;

        // Localization for strings related to the base weapon
        public StringLocalization BaseLocalization;

        // A placeholder for when there's no weapon equipped
        public BaseWeaponScript noWeaponObject;

        // Lists to store different types of weapons
        public List<Transform> AllWeaponList;
        public List<Transform> AllPrimaryWeapon;
        public List<Transform> AllSecoundaryWeapon;
        public List<Transform> AllMelleWeapon;
        public List<Transform> AllThrowableWeapon;

        // Indices for the selected weapons in each category
        public int SelectedPrimary;
        public int Selectedecoundary;
        public int SelectedMelle;
        public int SelectedThrowable;

        // Lists to store weapon slots for each type
        public List<Transform> PrimarySlot;
        public List<Transform> SecoundarySlot;
        public List<Transform> MelleSlot;
        public List<Transform> ThrowableSlot;

        // UI elements for displaying weapon previews and information
        public Sprite EmptyGun;
        public Transform WeaponPreviewUI;
        public GameObject WeaponButtonPrefab;

        // UI elements for displaying inventory statistics
        public GameObject InventoryStatDamage;
        public GameObject InventoryStatRange;
        public GameObject InventoryStatAmmo;
        public GameObject InventoryStatFirerate;
        public GameObject InventoryStatMagazine;
        public GameObject InventoryStatReload;
        public GameObject InventoryStatAlternative;
        public GameObject InventoryStatAltDamage;
        public GameObject InventoryStatAltRange;
        public GameObject InventoryStatBlock;

        // Flags and variables for controlling the inventory display and weapon selection
        [HideInInspector] public bool ShowInventory = false;
        [HideInInspector] public int CategoryID;
        [HideInInspector] public int HighlightWeaponID;

        // Private variables
        [HideInInspector] public bool SafeZone;         // Indicates if the player is in a safe zone
        private TinyInput tinyInput;                    // Reference to a TinyInput component
        [HideInInspector] public int selectedWeapon;    // Index of the currently selected weapon
        float changeTimer;                              // A timer for managing weapon changes
        GameSettingsManger gm;                          // Reference to the GameSettingsManger component

        // Awake is called when the script instance is being loaded
        private void Awake()
        {
            // Load the GameSettingsManger scriptable object from Resources folder
            gm = (GameSettingsManger)Resources.Load("Game_Settings");

            // Find the LevelManager script in the scene
            LevelManager _lm = FindObjectOfType<LevelManager>();

            // Clear the list of all weapons
            AllWeaponList.Clear();

            // Load weapons from save if the game mode is StoryMode and there's a valid save
            if (_lm.Gamemode.GameMode == GameModes.StoryMode && TinySaveSystem.HasKey("StoryMode_Save"))
            {
                // Get the player save data from the TinySaveSystem
                PlayerSaveClass save = TinySaveSystem.GetPlayerSave("StoryMode_Save");

                // SPAWN WEAPON: Instantiate weapons based on saved weapon names
                string[] weaponNames = GameManager.Instance.StringToArray<string>(save.AllWeapon);
                GameObject[] weaponPrefab = Resources.LoadAll<GameObject>("Weapon");

                // Loop through the saved weapon names and instantiate corresponding weapon prefabs
                for (int i = 0; i < weaponNames.Length; i++)
                {
                    for (int all = 0; all < weaponPrefab.Length; all++)
                    {
                        if (weaponNames[i] == weaponPrefab[all].name)
                        {
                            GameObject weapon = Instantiate(weaponPrefab[all], this.transform);
                            weapon.name = weapon.name.Replace("(Clone)", "");
                            AllWeaponList.Add(weapon.transform);
                        }
                    }
                }

                // SETUP SLOT: Assign loaded weapons to their respective slots (Primary, Secondary, Melee, Throwable)
                // Get saved weapon names for each slot and find the corresponding weapon in AllWeaponList
                // Then, assign the weapon to the appropriate slot list (PrimarySlot, SecoundarySlot, MelleSlot, ThrowableSlot)
                // Empty slots are also handled by setting the corresponding slot to null
                string[] primary = GameManager.Instance.StringToArray<string>(save.PrimarySlot);
                for (int i = 0; i < primary.Length; i++)
                {
                    if (primary[i] == "Empty") { PrimarySlot[i] = null; }
                    for (int all = 0; all < AllWeaponList.Count; all++)
                    {
                        if (primary[i] == AllWeaponList[all].name) { PrimarySlot[i] = AllWeaponList[all]; }
                    }
                }

                string[] secoundary = GameManager.Instance.StringToArray<string>(save.SecoundarySlot);
                for (int i = 0; i < secoundary.Length; i++)
                {
                    if (secoundary[i] == "Empty") { SecoundarySlot[i] = null; }
                    for (int all = 0; all < AllWeaponList.Count; all++)
                    {
                        if (secoundary[i] == AllWeaponList[all].name) { SecoundarySlot[i] = AllWeaponList[all]; }
                    }
                }

                string[] melle = GameManager.Instance.StringToArray<string>(save.MelleSlot);
                for (int i = 0; i < melle.Length; i++)
                {
                    if (melle[i] == "Empty") { MelleSlot[i] = null; }
                    for (int all = 0; all < AllWeaponList.Count; all++)
                    {
                        if (melle[i] == AllWeaponList[all].name) { MelleSlot[i] = AllWeaponList[all]; }
                    }
                }

                string[] throwable = GameManager.Instance.StringToArray<string>(save.ThrowableSlot);
                for (int i = 0; i < throwable.Length; i++)
                {
                    if (throwable[i] == "Empty") { ThrowableSlot[i] = null; }
                    for (int all = 0; all < AllWeaponList.Count; all++)
                    {
                        if (throwable[i] == AllWeaponList[all].name) { ThrowableSlot[i] = AllWeaponList[all]; }
                    }
                }
            }
            else    // If not in StoryMode or no valid save, load weapons based on the game's inventory type
            {
                // Instantiate classic weapons defined in LevelManager and add them to the AllWeaponList
                // Also, assign weapons to their respective slots (PrimarySlot, SecoundarySlot, MelleSlot, ThrowableSlot)
                // Empty slots are handled by setting the corresponding slot to null
                if (gm.inventoryType == InventoryType.Classic)
                {

                    PrimarySlot = new List<Transform>(gm.PrimaryWeaponCount);
                    SecoundarySlot = new List<Transform>(gm.SecoundaryWeaponCount);
                    MelleSlot = new List<Transform>(gm.MelleWeaponCount);
                    ThrowableSlot = new List<Transform>(gm.ThrowableWeaponCount);

                    for (int i = 0; i < _lm.ClassicWeapon.Count; i++)
                    {
                        if (noWeaponObject.name != _lm.ClassicWeapon[i].gameObject.name)
                        {
                            Transform weapon2 = Instantiate(_lm.ClassicWeapon[i], this.transform);
                            AllWeaponList.Add(weapon2);
                            if (weapon2.GetComponent<BaseWeaponScript>().Slot == SlotType.PrimaryWeapon) { PrimarySlot.Add(weapon2); }
                            if (weapon2.GetComponent<BaseWeaponScript>().Slot == SlotType.SecoundaryWeapon) { SecoundarySlot.Add(weapon2); }
                            if (weapon2.GetComponent<BaseWeaponScript>().Slot == SlotType.MelleWeapon) { MelleSlot.Add(weapon2); }
                            if (weapon2.GetComponent<BaseWeaponScript>().Slot == SlotType.ThrowableWeapon) { ThrowableSlot.Add(weapon2); }
                        }
                    }
                }

                // Instantiate normal inventory weapons defined in LevelManager and add them to the AllWeaponList
                // Also, assign weapons to their respective slots (PrimarySlot, SecoundarySlot, MelleSlot, ThrowableSlot)
                // Empty slots are handled by setting the corresponding slot to null
                if (gm.inventoryType == InventoryType.Normal)
                {
                    for (int i = 0; i < PrimarySlot.Count; i++)
                    {
                        if (_lm.PrimaryWeapon[i] != null)
                        {
                            Transform weapon = Instantiate(_lm.PrimaryWeapon[i], this.transform);
                            AllWeaponList.Add(weapon);
                            PrimarySlot[i] = weapon;
                        }
                    }
                    for (int i = 0; i < SecoundarySlot.Count; i++)
                    {
                        if (_lm.SecoundaryWeapon[i] != null)
                        {
                            Transform weapon = Instantiate(_lm.SecoundaryWeapon[i], this.transform);
                            AllWeaponList.Add(weapon);
                            SecoundarySlot[i] = weapon;
                        }
                    }
                    for (int i = 0; i < MelleSlot.Count; i++)
                    {
                        if (_lm.MelleWeapon[i] != null)
                        {
                            Transform weapon = Instantiate(_lm.MelleWeapon[i], this.transform);
                            AllWeaponList.Add(weapon);
                            MelleSlot[i] = weapon;
                        }
                    }
                    for (int i = 0; i < ThrowableSlot.Count; i++)
                    {
                        if (_lm.ThrowableWeapon[i] != null)
                        {
                            Transform weapon = Instantiate(_lm.ThrowableWeapon[i], this.transform);
                            AllWeaponList.Add(weapon);
                            ThrowableSlot[i] = weapon;
                        }
                    }

                }
            }

            // Split all weapons into their respective categories (Primary, Secondary, Melee, Throwable)
            for (int i = 0; i < AllWeaponList.Count; i++)
            {
                if (AllWeaponList[i].GetComponent<BaseWeaponScript>().Slot == SlotType.PrimaryWeapon) { AllPrimaryWeapon.Add(AllWeaponList[i]); }
                if (AllWeaponList[i].GetComponent<BaseWeaponScript>().Slot == SlotType.SecoundaryWeapon) { AllSecoundaryWeapon.Add(AllWeaponList[i]); }
                if (AllWeaponList[i].GetComponent<BaseWeaponScript>().Slot == SlotType.MelleWeapon) { AllMelleWeapon.Add(AllWeaponList[i]); }
                if (AllWeaponList[i].GetComponent<BaseWeaponScript>().Slot == SlotType.ThrowableWeapon) { AllThrowableWeapon.Add(AllWeaponList[i]); }
            }

            // Select the first available weapon
            selectedWeapon = 0;
            Transform _tmp = PrimarySlot[0];
            while (_tmp == null)
            {
                // Check which slot the selectedWeapon index points to and skip empty slots
                // If an actual weapon is found, set it as the current weapon (actualWeapon)
                selectedWeapon++;
                if (selectedWeapon < PrimarySlot.Count) { _tmp = PrimarySlot[selectedWeapon]; }
                else if (selectedWeapon < PrimarySlot.Count + SecoundarySlot.Count) { _tmp = SecoundarySlot[selectedWeapon - PrimarySlot.Count]; }
                else if (selectedWeapon < PrimarySlot.Count + SecoundarySlot.Count + MelleSlot.Count) { _tmp = MelleSlot[selectedWeapon - PrimarySlot.Count - SecoundarySlot.Count]; }
                else { _tmp = ThrowableSlot[selectedWeapon - PrimarySlot.Count - SecoundarySlot.Count - MelleSlot.Count]; }
                if (_tmp != null) { actualWeapon = _tmp.GetComponent<BaseWeaponScript>(); }
            }

            // Setup Input System for inventory menu access
            tinyInput = new TinyInput();
            tinyInput.Menu.Inventory.performed += ctx => { if (GameManager.Instance.UIState == PlayerUIState.Inventory || GameManager.Instance.UIState == PlayerUIState.None) { InventoryMenu(); } };
        }

        //
        private void OnValidate()
        {
            // Add New Ammo UI if Needed
            if (AmmoSprites.Count < Enum.GetValues(typeof(AmmoType)).Length)
            {
                while (AmmoSprites.Count != Enum.GetValues(typeof(AmmoType)).Length)
                {
                    AmmoSprites.Add(new AmmoIconClass());
                }
            }
            // Remove Ammo UI if Needed
            if (AmmoSprites.Count > Enum.GetValues(typeof(AmmoType)).Length)
            {
                while (AmmoSprites.Count != Enum.GetValues(typeof(AmmoType)).Length)
                {
                    AmmoSprites.RemoveAt(AmmoSprites.Count - 1);
                }
            }
            // Setup Ammo Type
            for (int i = 0; i < Enum.GetValues(typeof(AmmoType)).Length; i++)
            {
                int enumID = 0;
                if (i != 0) { enumID = (int)AmmoSprites[i - 1].Type + 1; }
                while (((AmmoType)enumID).ToString() == enumID.ToString()) { enumID++; }

                AmmoSprites[i].Type = (AmmoType)enumID;
            }
        }

        // Enable and disable the TinyInput system
        public void OnEnable() { tinyInput.Enable(); }
        public void OnDisable() { tinyInput.Disable(); }

        // Start is called before the first frame update
        void Start() { UpdateWeapon(); }

        // Update is called once per frame
        void Update()
        {
            // Update the changeTimer, which controls the weapon switching cooldown
            if (changeTimer > 0)
            {
                changeTimer -= Time.deltaTime;
            }

            // Check if the player is in a safe zone to prevent weapon updates and UI visibility
            if (!SafeZone)
            {
                // Get the total number of slots (Primary, Secondary, Melee, Throwable)
                int SlotCount = PrimarySlot.Count + SecoundarySlot.Count + MelleSlot.Count + ThrowableSlot.Count;

                // Scroll to change weapons
                // Check if the player scrolled to switch weapons and handle the switch accordingly
                if (tinyInput.Weapon.Select_Weapon.ReadValue<float>() > 0 && changeTimer <= 0) // Next
                {
                    actualWeapon.StopAllCoroutines();
                    selectedWeapon++;
                    if (selectedWeapon == SlotCount) { selectedWeapon = 0; }

                    // Skip Empty Slot
                    bool _tmp = false;
                    if (selectedWeapon < PrimarySlot.Count) { if (PrimarySlot[selectedWeapon] == null) { selectedWeapon++; _tmp = true; } else { _tmp = false; } }
                    else if (selectedWeapon < PrimarySlot.Count + SecoundarySlot.Count) { if (SecoundarySlot[selectedWeapon - PrimarySlot.Count] == null) { selectedWeapon++; _tmp = true; } else { _tmp = false; } }
                    else if (selectedWeapon < PrimarySlot.Count + SecoundarySlot.Count + MelleSlot.Count) { if (MelleSlot[selectedWeapon - PrimarySlot.Count - SecoundarySlot.Count] == null) { selectedWeapon++; _tmp = true; } else { _tmp = false; } }
                    else { if (ThrowableSlot[selectedWeapon - PrimarySlot.Count - SecoundarySlot.Count - MelleSlot.Count] == null) { selectedWeapon++; _tmp = true; } else { _tmp = false; } }
                    if (selectedWeapon == SlotCount) { selectedWeapon = 0; }

                    while (_tmp)
                    {
                        if (selectedWeapon < PrimarySlot.Count) { if (PrimarySlot[selectedWeapon] == null) { selectedWeapon++; _tmp = true; } else { _tmp = false; } }
                        else if (selectedWeapon < PrimarySlot.Count + SecoundarySlot.Count) { if (SecoundarySlot[selectedWeapon - PrimarySlot.Count] == null) { selectedWeapon++; _tmp = true; } else { _tmp = false; } }
                        else if (selectedWeapon < PrimarySlot.Count + SecoundarySlot.Count + MelleSlot.Count) { if (MelleSlot[selectedWeapon - PrimarySlot.Count - SecoundarySlot.Count] == null) { selectedWeapon++; _tmp = true; } else { _tmp = false; } }
                        else { if (ThrowableSlot[selectedWeapon - PrimarySlot.Count - SecoundarySlot.Count - MelleSlot.Count] == null) { selectedWeapon++; _tmp = true; } else { _tmp = false; } }
                        if (selectedWeapon == SlotCount) { selectedWeapon = 0; }
                    }

                    UpdateWeapon();
                    changeTimer = 1 / 15;
                }
                if (tinyInput.Weapon.Select_Weapon.ReadValue<float>() < 0 && changeTimer <= 0) // Previous
                {
                    actualWeapon.StopAllCoroutines();
                    selectedWeapon -= 1;
                    if (selectedWeapon < 0) { selectedWeapon = SlotCount - 1; }

                    // Skip Empty Slot
                    bool _tmp = false;
                    if (selectedWeapon < PrimarySlot.Count) { if (PrimarySlot[selectedWeapon] == null) { selectedWeapon--; _tmp = true; } else { _tmp = false; } }
                    else if (selectedWeapon < PrimarySlot.Count + SecoundarySlot.Count) { if (SecoundarySlot[selectedWeapon - PrimarySlot.Count] == null) { selectedWeapon--; _tmp = true; } else { _tmp = false; } }
                    else if (selectedWeapon < PrimarySlot.Count + SecoundarySlot.Count + MelleSlot.Count) { if (MelleSlot[selectedWeapon - PrimarySlot.Count - SecoundarySlot.Count] == null) { selectedWeapon--; _tmp = true; } else { _tmp = false; } }
                    else { if (ThrowableSlot[selectedWeapon - PrimarySlot.Count - SecoundarySlot.Count - MelleSlot.Count] == null) { selectedWeapon--; _tmp = true; } else { _tmp = false; } }
                    if (selectedWeapon < 0) { selectedWeapon = SlotCount - 1; }

                    while (_tmp)
                    {
                        if (selectedWeapon < PrimarySlot.Count) { if (PrimarySlot[selectedWeapon] == null) { selectedWeapon--; _tmp = true; } else { _tmp = false; } }
                        else if (selectedWeapon < PrimarySlot.Count + SecoundarySlot.Count) { if (SecoundarySlot[selectedWeapon - PrimarySlot.Count] == null) { selectedWeapon--; _tmp = true; } else { _tmp = false; } }
                        else if (selectedWeapon < PrimarySlot.Count + SecoundarySlot.Count + MelleSlot.Count) { if (MelleSlot[selectedWeapon - PrimarySlot.Count - SecoundarySlot.Count] == null) { selectedWeapon--; _tmp = true; } else { _tmp = false; } }
                        else { if (ThrowableSlot[selectedWeapon - PrimarySlot.Count - SecoundarySlot.Count - MelleSlot.Count] == null) { selectedWeapon--; _tmp = true; } else { _tmp = false; } }
                        if (selectedWeapon < 0) { selectedWeapon = SlotCount - 1; }
                    }

                    UpdateWeapon();
                    changeTimer = 1 / 15;
                }

                // Keys to change weapons
                // Check if the player pressed number keys to switch directly to a specific weapon slot
                if (tinyInput.Weapon.Select_Primary.ReadValue<float>() == 1) selectedWeapon = 0;
                if (tinyInput.Weapon.Select_Secoundary.ReadValue<float>() == 1) selectedWeapon = PrimarySlot.Count;
                if (tinyInput.Weapon.Select_Melle.ReadValue<float>() == 1) selectedWeapon = PrimarySlot.Count + SecoundarySlot.Count;
                if (tinyInput.Weapon.Select_Throwable.ReadValue<float>() == 1) selectedWeapon = PrimarySlot.Count + SecoundarySlot.Count + MelleSlot.Count;

                bool _tmp2 = false;
                if (selectedWeapon < PrimarySlot.Count) { if (PrimarySlot[selectedWeapon] == null) { selectedWeapon++; _tmp2 = true; } else { _tmp2 = false; } }
                else if (selectedWeapon < PrimarySlot.Count + SecoundarySlot.Count) { if (SecoundarySlot[selectedWeapon - PrimarySlot.Count] == null) { selectedWeapon++; _tmp2 = true; } else { _tmp2 = false; } }
                else if (selectedWeapon < PrimarySlot.Count + SecoundarySlot.Count + MelleSlot.Count) { if (MelleSlot[selectedWeapon - PrimarySlot.Count - SecoundarySlot.Count] == null) { selectedWeapon++; _tmp2 = true; } else { _tmp2 = false; } }
                else { if (ThrowableSlot[selectedWeapon - PrimarySlot.Count - SecoundarySlot.Count - MelleSlot.Count] == null) { selectedWeapon++; _tmp2 = true; } else { _tmp2 = false; } }
                if (selectedWeapon == SlotCount) { selectedWeapon = 0; }

                while (_tmp2)
                {
                    if (selectedWeapon < PrimarySlot.Count) { if (PrimarySlot[selectedWeapon] == null) { selectedWeapon++; _tmp2 = true; } else { _tmp2 = false; } }
                    else if (selectedWeapon < PrimarySlot.Count + SecoundarySlot.Count) { if (SecoundarySlot[selectedWeapon - PrimarySlot.Count] == null) { selectedWeapon++; _tmp2 = true; } else { _tmp2 = false; } }
                    else if (selectedWeapon < PrimarySlot.Count + SecoundarySlot.Count + MelleSlot.Count) { if (MelleSlot[selectedWeapon - PrimarySlot.Count - SecoundarySlot.Count] == null) { selectedWeapon++; _tmp2 = true; } else { _tmp2 = false; } }
                    else { if (ThrowableSlot[selectedWeapon - PrimarySlot.Count - SecoundarySlot.Count - MelleSlot.Count] == null) { selectedWeapon++; _tmp2 = true; } else { _tmp2 = false; } }
                    if (selectedWeapon == SlotCount) { selectedWeapon = 0; }
                }

                UpdateWeapon();

            }

            // Speed Modification
            if (!SafeZone)
            {
                if (actualWeapon.TryGetComponent<RangeWeapon>(out RangeWeapon range))
                {
                    if (range.aim) { Camera.main.transform.parent.GetComponent<PlayerMovement>().weaponSpeedMultiplier = (actualWeapon.SpeedMultiplier * actualWeapon.AimSpeedMultiplier); }
                    else { Camera.main.transform.parent.GetComponent<PlayerMovement>().weaponSpeedMultiplier = actualWeapon.SpeedMultiplier; }
                }
                else
                {
                    Camera.main.transform.parent.GetComponent<PlayerMovement>().weaponSpeedMultiplier = actualWeapon.SpeedMultiplier;
                }
            }
            else { Camera.main.transform.parent.GetComponent<PlayerMovement>().weaponSpeedMultiplier = 1f; }

            // Update the current weapon and UI elements
            UpdateWeapon();
            UpdateStats();

            // Update UI elements' visibility based on SafeZone status and weapon type
            WeaponUIImage.gameObject.SetActive(!SafeZone);
            WeaponNameText.gameObject.SetActive(!SafeZone);
            if (actualWeapon.TryGetComponent<MelleWeapon>(out MelleWeapon throwable))
            {
                ammoText.gameObject.SetActive(false);
                AmmoIcon.gameObject.SetActive(false);
            }
            else
            {
                ammoText.gameObject.SetActive(!SafeZone);
                AmmoIcon.gameObject.SetActive(!SafeZone);
            }
        }

        // UpdateStats method updates the player's stats based on the equipped weapon.
        public void UpdateStats()
        {
            // Check if the actualWeapon is a melee weapon
            if (actualWeapon.TryGetComponent<MelleWeapon>(out MelleWeapon melle))
            {
                if (melle.anim.GetBool("Block") == true)
                {
                    // If the melee weapon is in block mode, update the player's blocking and damage reduction stats.
                    transform.parent.GetComponent<PlayerHealth>().WeaponBlockingChange = actualWeapon.GetComponent<MelleWeapon>().BlockingDamageChange;
                    transform.parent.GetComponent<PlayerHealth>().WeaponDamageReductionPercentage = actualWeapon.GetComponent<MelleWeapon>().DamageReductionPercentage;
                }
                else
                {
                    // If the weapon is not melee, set the player's blocking and damage reduction stats to zero.
                    transform.parent.GetComponent<PlayerHealth>().WeaponBlockingChange = 0;
                    transform.parent.GetComponent<PlayerHealth>().WeaponDamageReductionPercentage = 0;
                }

                // Show the ammoText UI element if the weapon is not a melee weapon.
                ammoText.gameObject.SetActive(false);
            }
            else
            {
                transform.parent.GetComponent<PlayerHealth>().WeaponBlockingChange = 0;
                transform.parent.GetComponent<PlayerHealth>().WeaponDamageReductionPercentage = 0;
                ammoText.gameObject.SetActive(true);

                // If the actualWeapon is a ranged weapon, update the AmmoIcon sprite accordingly.
                if (actualWeapon.TryGetComponent<RangeWeapon>(out RangeWeapon range))
                {
                    AmmoIcon.sprite = UpdateAmmoIcon(range.ammoType);
                }
                // If the actualWeapon is a throwable weapon, update the AmmoIcon sprite accordingly.
                else if (actualWeapon.TryGetComponent<ThrowableWeapon>(out ThrowableWeapon throwable))
                {
                    AmmoIcon.sprite = UpdateAmmoIcon(throwable.AmmoType);
                }
            }

            // Update the WeaponUIImage with the sprite of the equipped weapon.
            WeaponUIImage.sprite = actualWeapon.WeaponIcon();
        }

        // UpdateAmmoIcon method returns the appropriate sprite for the given ammo type.
        Sprite UpdateAmmoIcon(AmmoType type)
        {
            // Check the ammo type and return the corresponding sprite.
            // If the type is not recognized, return the InfinityIcon sprite as a default.
            // The InfinityIcon sprite might represent infinite or unlimited ammo.

            for (int i = 0; i < AmmoSprites.Count; i++)
            {
                if (type == AmmoSprites[i].Type) { return AmmoSprites[i].Icon; }
            }
            return AmmoSprites[0].Icon;
        }

        // UINextWeapon method selects the next weapon in the player's inventory.
        public void UINextWeapon()
        {
            selectedWeapon = (selectedWeapon + 1) % (PrimarySlot.Count + SecoundarySlot.Count + MelleSlot.Count + ThrowableSlot.Count);
        }

        // UIPreviousWeapon method selects the previous weapon in the player's inventory.
        public void UIPreviousWeapon()
        {
            // Ensure that selectedWeapon remains within a valid range even if it goes below zero.
            selectedWeapon = Mathf.Abs(selectedWeapon - 1) % (PrimarySlot.Count + SecoundarySlot.Count + MelleSlot.Count + ThrowableSlot.Count);
        }

        // Primary, Secondary, and Reload methods invoke the corresponding weapon mode functions.
        // These methods likely represent actions the player can take with the equipped weapon.
        public void Primary() { actualWeapon.PrimaryModeFunction(); }
        public void Secoundary() { actualWeapon.SecondaryModeFunction(); }
        public void Reload() { actualWeapon.ReloadFunction(); }

        // InventoryMenu method toggles the inventory menu display.
        public void InventoryMenu()
        {
            // Depending on the ShowInventory flag, handle the UI state and show/hide the inventory tab.
            if (!ShowInventory)
            {
                GameManager.Instance.paused = true;
                GameManager.Instance.CheckPauseGame();
                GameManager.Instance.UIState = PlayerUIState.Inventory;
            }
            else
            {
                GameManager.Instance.paused = false;
                GameManager.Instance.CheckPauseGame();
                GameManager.Instance.UIState = PlayerUIState.None;
            }

            // Toggle the ShowInventory flag to show or hide the inventory menu.
            ShowInventory = !ShowInventory;

            transform.parent.Find("Main Panels").Find("Inventory Tab").gameObject.SetActive(ShowInventory);
            InventoryResetUI();
        }

        // UpdateWeapon method handles the update and display of equipped weapons based on the selectedWeapon index.
        void UpdateWeapon()
        {
            // If the player is in a SafeZone, show the noWeaponObject and deactivate all other weapon objects.
            if (SafeZone)
            {
                for (int i = 0; i < AllPrimaryWeapon.Count; i++) { AllPrimaryWeapon[i].gameObject.SetActive(false); }
                for (int i = 0; i < AllSecoundaryWeapon.Count; i++) { AllSecoundaryWeapon[i].gameObject.SetActive(false); }
                for (int i = 0; i < AllMelleWeapon.Count; i++) { AllMelleWeapon[i].gameObject.SetActive(false); }
                for (int i = 0; i < AllThrowableWeapon.Count; i++) { AllThrowableWeapon[i].gameObject.SetActive(false); }
                noWeaponObject.gameObject.SetActive(true);
                actualWeapon = noWeaponObject;

            }
            else
            {
                // If the player is not in a SafeZone, handle the display of different weapon types based on the selectedWeapon index.
                // Set the actualWeapon to the appropriate weapon in each category (Primary, Secondary, Melee, Throwable).
                // Deactivate other weapon objects not selected.
                // Hide the noWeaponObject.

                if (selectedWeapon < PrimarySlot.Count)
                {
                    for (int i = 0; i < PrimarySlot.Count; i++)
                    {
                        if (i == selectedWeapon)
                        {
                            if (PrimarySlot[i] != null) PrimarySlot[i].gameObject.SetActive(true);
                            actualWeapon = PrimarySlot[i].GetComponent<BaseWeaponScript>();
                        }
                        else
                        {
                            if (PrimarySlot[i] != null) PrimarySlot[i].gameObject.SetActive(false);
                        }
                    }
                    for (int i = 0; i < AllSecoundaryWeapon.Count; i++) { AllSecoundaryWeapon[i].gameObject.SetActive(false); }
                    for (int i = 0; i < AllMelleWeapon.Count; i++) { AllMelleWeapon[i].gameObject.SetActive(false); }
                    for (int i = 0; i < AllThrowableWeapon.Count; i++) { AllThrowableWeapon[i].gameObject.SetActive(false); }
                }
                else if (selectedWeapon < PrimarySlot.Count + SecoundarySlot.Count)
                {
                    for (int i = 0; i < AllPrimaryWeapon.Count; i++) { AllPrimaryWeapon[i].gameObject.SetActive(false); }
                    for (int i = 0; i < SecoundarySlot.Count; i++)
                    {
                        if (i == selectedWeapon - PrimarySlot.Count)
                        {
                            if (SecoundarySlot[i] != null) SecoundarySlot[i].gameObject.SetActive(true);
                            actualWeapon = SecoundarySlot[i].GetComponent<BaseWeaponScript>();
                        }
                        else
                        {
                            if (SecoundarySlot[i] != null) SecoundarySlot[i].gameObject.SetActive(false);
                        }
                    }
                    for (int i = 0; i < AllMelleWeapon.Count; i++) { AllMelleWeapon[i].gameObject.SetActive(false); }
                    for (int i = 0; i < AllThrowableWeapon.Count; i++) { AllThrowableWeapon[i].gameObject.SetActive(false); }
                }
                else if (selectedWeapon < PrimarySlot.Count + SecoundarySlot.Count + MelleSlot.Count)
                {
                    for (int i = 0; i < AllPrimaryWeapon.Count; i++) { AllPrimaryWeapon[i].gameObject.SetActive(false); }
                    for (int i = 0; i < AllSecoundaryWeapon.Count; i++) { AllSecoundaryWeapon[i].gameObject.SetActive(false); }
                    for (int i = 0; i < MelleSlot.Count; i++)
                    {
                        if (i == selectedWeapon - PrimarySlot.Count - SecoundarySlot.Count)
                        {
                            if (MelleSlot[i] != null) MelleSlot[i].gameObject.SetActive(true);
                            actualWeapon = MelleSlot[i].GetComponent<BaseWeaponScript>();
                        }
                        else
                        {
                            if (MelleSlot[i] != null) MelleSlot[i].gameObject.SetActive(false);
                        }
                    }
                    for (int i = 0; i < AllThrowableWeapon.Count; i++) { AllThrowableWeapon[i].gameObject.SetActive(false); }
                }
                else
                {
                    for (int i = 0; i < AllPrimaryWeapon.Count; i++) { AllPrimaryWeapon[i].gameObject.SetActive(false); }
                    for (int i = 0; i < AllSecoundaryWeapon.Count; i++) { AllSecoundaryWeapon[i].gameObject.SetActive(false); }
                    for (int i = 0; i < AllMelleWeapon.Count; i++) { AllMelleWeapon[i].gameObject.SetActive(false); }
                    for (int i = 0; i < ThrowableSlot.Count; i++)
                    {
                        if (i == selectedWeapon - PrimarySlot.Count - SecoundarySlot.Count - MelleSlot.Count)
                        {
                            if (ThrowableSlot[i] != null) ThrowableSlot[i].gameObject.SetActive(true);
                            actualWeapon = ThrowableSlot[i].GetComponent<BaseWeaponScript>();
                        }
                        else
                        {
                            if (ThrowableSlot[i] != null) ThrowableSlot[i].gameObject.SetActive(false);
                        }
                    }
                }

                noWeaponObject.gameObject.SetActive(false);
            }
        }

        // This method is used to register a new weapon in the inventory.
        public void RegisterNewWeapon(Transform weapon)
        {
            // Get all child transforms (weapons) in the inventory.
            Transform[] weapons = GetComponentsInChildren<Transform>();
            bool unique = true;

            // Check if the new weapon's name is unique in the inventory.
            foreach (Transform child in weapons)
            {
                if (child.name == (weapon.name))
                {
                    unique = false;
                }
            }

            // If the weapon is unique, add it to the inventory.
            if (unique)
            {
                // Instantiate the weapon as a child of this GameObject.
                Transform weaponOBJ = Instantiate(weapon, this.transform);
                weaponOBJ.gameObject.SetActive(false);
                AllWeaponList.Add(weaponOBJ);

                // Add the weapon to the appropriate weapon lists based on its slot type.
                if (gm.inventoryType == InventoryType.Normal)
                {
                    // In normal inventory mode, weapons are added to general lists
                    if (weaponOBJ.GetComponent<BaseWeaponScript>().Slot == SlotType.PrimaryWeapon) { AllPrimaryWeapon.Add(weaponOBJ); }
                    if (weaponOBJ.GetComponent<BaseWeaponScript>().Slot == SlotType.SecoundaryWeapon) { AllSecoundaryWeapon.Add(weaponOBJ); }
                    if (weaponOBJ.GetComponent<BaseWeaponScript>().Slot == SlotType.MelleWeapon) { AllMelleWeapon.Add(weaponOBJ); }
                    if (weaponOBJ.GetComponent<BaseWeaponScript>().Slot == SlotType.ThrowableWeapon) { AllThrowableWeapon.Add(weaponOBJ); }
                }
                else
                {
                    // In classic inventory mode, weapons are added to specialized lists.
                    if (weaponOBJ.GetComponent<BaseWeaponScript>().Slot == SlotType.PrimaryWeapon) { AllPrimaryWeapon.Add(weaponOBJ); PrimarySlot.Add(weaponOBJ); }
                    if (weaponOBJ.GetComponent<BaseWeaponScript>().Slot == SlotType.SecoundaryWeapon) { AllSecoundaryWeapon.Add(weaponOBJ); SecoundarySlot.Add(weaponOBJ); }
                    if (weaponOBJ.GetComponent<BaseWeaponScript>().Slot == SlotType.MelleWeapon) { AllMelleWeapon.Add(weaponOBJ); MelleSlot.Add(weaponOBJ); }
                    if (weaponOBJ.GetComponent<BaseWeaponScript>().Slot == SlotType.ThrowableWeapon) { AllThrowableWeapon.Add(weaponOBJ); ThrowableSlot.Add(weaponOBJ); }
                }
            }
        }

        // This method generates a list of weapons in the inventory based on the specified type.
        public void GenerateWeaponList(int type)
        {
            // Find the weapon selector object in the UI.
            Transform selector = transform.parent.Find("Main Panels").Find("Inventory Tab").Find("Weapon Selector");
            selector.gameObject.SetActive(true);

            // Clear the existing weapon list in the selector.
            foreach (Transform child in selector.Find("WeaponList").Find("Viewport").Find("Content"))
            {
                Destroy(child.gameObject);
            }

            // Set the category ID for the current weapon type.
            CategoryID = type;

            // Depending on the type, populate the weapon list in the selector UI.
            if (type == (int)SlotType.PrimaryWeapon)
            {
                // Iterate through all primary weapons and create buttons in the selector UI.
                for (int i = 0; i < AllPrimaryWeapon.Count; i++)
                {
                    // Create a button for each weapon and set its icon, title, and description.
                    // Also, add a click event listener to show the weapon preview.
                    // Note: The ShowPreview method is defined elsewhere in the code.
                    // It's not included in this snippet.

                    int index = i;
                    GameObject btn = Instantiate(WeaponButtonPrefab, selector.Find("WeaponList").Find("Viewport").Find("Content"));
                    btn.transform.Find("Weapon Icon").GetComponent<Image>().sprite = AllPrimaryWeapon[i].GetComponent<BaseWeaponScript>().WeaponIcon();
                    btn.transform.Find("Weapon Title").GetComponent<TMP_Text>().text = AllPrimaryWeapon[i].GetComponent<BaseWeaponScript>().WeaponName();
                    btn.transform.Find("Weapon Description").GetComponent<TMP_Text>().text = AllPrimaryWeapon[i].GetComponent<BaseWeaponScript>().Category.ToString();

                    int _slot = -1;
                    for (int j = 0; j < PrimarySlot.Count; j++)
                    {
                        if (PrimarySlot[j] == AllPrimaryWeapon[i]) { _slot = j; }
                    }

                    if (_slot != -1)
                    {
                        btn.transform.Find("Weapon Slot").gameObject.SetActive(true);
                        btn.transform.Find("Weapon Slot").GetComponent<TMP_Text>().text = string.Format(BaseLocalization.GetString("Equipped"), (_slot + 1));
                    }

                    btn.GetComponent<Button>().onClick.AddListener(delegate { ShowPreview(index); });
                }
                ShowPreview(PrimarySlot[SelectedPrimary]);
            }
            if (type == (int)SlotType.SecoundaryWeapon)
            {
                for (int i = 0; i < AllSecoundaryWeapon.Count; i++)
                {
                    // Create a button for each weapon and set its icon, title, and description.
                    // Also, add a click event listener to show the weapon preview.
                    // Note: The ShowPreview method is defined elsewhere in the code.
                    // It's not included in this snippet.

                    int index = i;
                    GameObject btn = Instantiate(WeaponButtonPrefab, selector.Find("WeaponList").Find("Viewport").Find("Content"));
                    btn.transform.Find("Weapon Icon").GetComponent<Image>().sprite = AllSecoundaryWeapon[i].GetComponent<BaseWeaponScript>().WeaponIcon();
                    btn.transform.Find("Weapon Title").GetComponent<TMP_Text>().text = AllSecoundaryWeapon[i].GetComponent<BaseWeaponScript>().WeaponName();
                    btn.transform.Find("Weapon Description").GetComponent<TMP_Text>().text = AllSecoundaryWeapon[i].GetComponent<BaseWeaponScript>().Category.ToString();

                    int _slot = -1;
                    for (int j = 0; j < SecoundarySlot.Count; j++)
                    {
                        if (SecoundarySlot[j] == AllSecoundaryWeapon[i]) { _slot = j; }
                    }

                    if (_slot != -1)
                    {
                        btn.transform.Find("Weapon Slot").gameObject.SetActive(true);
                        btn.transform.Find("Weapon Slot").GetComponent<TMP_Text>().text = string.Format(BaseLocalization.GetString("Equipped"), (_slot + 1));
                    }

                    btn.GetComponent<Button>().onClick.AddListener(delegate { ShowPreview(index); });
                }
                ShowPreview(SecoundarySlot[Selectedecoundary]);
            }
            if (type == (int)SlotType.MelleWeapon)
            {
                for (int i = 0; i < AllMelleWeapon.Count; i++)
                {
                    // Create a button for each weapon and set its icon, title, and description.
                    // Also, add a click event listener to show the weapon preview.
                    // Note: The ShowPreview method is defined elsewhere in the code.
                    // It's not included in this snippet.

                    int index = i;
                    GameObject btn = Instantiate(WeaponButtonPrefab, selector.Find("WeaponList").Find("Viewport").Find("Content"));
                    btn.transform.Find("Weapon Icon").GetComponent<Image>().sprite = AllMelleWeapon[i].GetComponent<BaseWeaponScript>().WeaponIcon();
                    btn.transform.Find("Weapon Title").GetComponent<TMP_Text>().text = AllMelleWeapon[i].GetComponent<BaseWeaponScript>().WeaponName();
                    btn.transform.Find("Weapon Description").GetComponent<TMP_Text>().text = AllMelleWeapon[i].GetComponent<BaseWeaponScript>().Category.ToString();

                    int _slot = -1;
                    for (int j = 0; j < MelleSlot.Count; j++)
                    {
                        if (MelleSlot[j] == AllMelleWeapon[i]) { _slot = j; }
                    }

                    if (_slot != -1)
                    {
                        btn.transform.Find("Weapon Slot").gameObject.SetActive(true);
                        btn.transform.Find("Weapon Slot").GetComponent<TMP_Text>().text = string.Format(BaseLocalization.GetString("Equipped"), (_slot + 1));
                    }

                    btn.GetComponent<Button>().onClick.AddListener(delegate { ShowPreview(index); });
                }
                ShowPreview(MelleSlot[SelectedMelle]);
            }
            if (type == (int)SlotType.ThrowableWeapon)
            {
                for (int i = 0; i < AllThrowableWeapon.Count; i++)
                {
                    // Create a button for each weapon and set its icon, title, and description.
                    // Also, add a click event listener to show the weapon preview.
                    // Note: The ShowPreview method is defined elsewhere in the code.
                    // It's not included in this snippet.

                    int index = i;
                    GameObject btn = Instantiate(WeaponButtonPrefab, selector.Find("WeaponList").Find("Viewport").Find("Content"));
                    btn.transform.Find("Weapon Icon").GetComponent<Image>().sprite = AllThrowableWeapon[i].GetComponent<BaseWeaponScript>().WeaponIcon();
                    btn.transform.Find("Weapon Title").GetComponent<TMP_Text>().text = AllThrowableWeapon[i].GetComponent<BaseWeaponScript>().WeaponName();
                    btn.transform.Find("Weapon Description").GetComponent<TMP_Text>().text = AllThrowableWeapon[i].GetComponent<BaseWeaponScript>().Category.ToString();

                    int _slot = -1;
                    for (int j = 0; j < ThrowableSlot.Count; j++)
                    {
                        if (ThrowableSlot[j] == AllThrowableWeapon[i]) { _slot = j; }
                    }

                    if (_slot != -1)
                    {
                        btn.transform.Find("Weapon Slot").gameObject.SetActive(true);
                        btn.transform.Find("Weapon Slot").GetComponent<TMP_Text>().text = string.Format(BaseLocalization.GetString("Equipped"), (_slot + 1));
                    }

                    btn.GetComponent<Button>().onClick.AddListener(delegate { ShowPreview(index); });
                }
                ShowPreview(ThrowableSlot[SelectedThrowable]);
            }

            // Create an empty button to represent no weapon equipped and add a click event to handle it.
            GameObject empty = Instantiate(WeaponButtonPrefab, selector.Find("WeaponList").Find("Viewport").Find("Content"));
            empty.transform.Find("Weapon Icon").GetComponent<Image>().sprite = EmptyGun;
            empty.transform.Find("Weapon Title").GetComponent<TMP_Text>().text = "No Weapon";
            empty.transform.Find("Weapon Description").gameObject.SetActive(false);
            empty.GetComponent<Button>().onClick.AddListener(delegate { EquipWeapon(null); WeaponPreviewUI.parent.gameObject.SetActive(false); });
        }

        // This method handles moving to the previous slot in the inventory for the specified type.
        public void InventoryPreviousSlot(int type)
        {
            // Decrement the index of the selected weapon for the specified type.
            // If it goes below zero, loop back to the last weapon in the list.

            if (type == (int)SlotType.PrimaryWeapon)
            {
                SelectedPrimary--;
                if (SelectedPrimary < 0) { SelectedPrimary = gm.PrimaryWeaponCount - 1; }
            }
            if (type == (int)SlotType.SecoundaryWeapon)
            {
                Selectedecoundary--;
                if (Selectedecoundary < 0) { Selectedecoundary = gm.SecoundaryWeaponCount - 1; }
            }
            if (type == (int)SlotType.MelleWeapon)
            {
                SelectedMelle--;
                if (SelectedMelle < 0) { SelectedMelle = gm.MelleWeaponCount - 1; }
            }
            if (type == (int)SlotType.ThrowableWeapon)
            {
                SelectedThrowable--;
                if (SelectedThrowable < 0) { SelectedThrowable = gm.ThrowableWeaponCount - 1; }
            }
            InventoryResetUI();
        }

        // This method handles moving to the next slot in the inventory for the specified type.
        public void InventoryNextSlot(int type)
        {
            // Increment the index of the selected weapon for the specified type.
            // If it reaches the end of the list, loop back to the first weapon.

            if (type == (int)SlotType.PrimaryWeapon)
            {
                SelectedPrimary++;
                if (SelectedPrimary == gm.PrimaryWeaponCount) { SelectedPrimary = 0; }
            }
            if (type == (int)SlotType.SecoundaryWeapon)
            {
                Selectedecoundary++;
                if (Selectedecoundary == gm.SecoundaryWeaponCount) { Selectedecoundary = 0; }
            }
            if (type == (int)SlotType.MelleWeapon)
            {
                SelectedMelle++;
                if (SelectedMelle == gm.MelleWeaponCount) { SelectedMelle = 0; }
            }
            if (type == (int)SlotType.ThrowableWeapon)
            {
                SelectedThrowable++;
                if (SelectedThrowable == gm.ThrowableWeaponCount) { SelectedThrowable = 0; }
            }

            InventoryResetUI();
        }

        // This method updates the UI to show the currently selected weapons in the inventor
        public void InventoryResetUI()
        {
            // Update the UI to show the currently selected weapons in the inventory.
            // Note: The gm (game manager) object and PrimarySlot, SecondarySlot, etc. lists are used to get the weapons' data.
            // The UI elements are updated with the weapon's icon, name, and slot number.
            // Note: The BaseLocalization class is used to localize some strings in the UI.
            // The localization keys are not shown in this snippet as they are not relevant to the code flow.
            // The method handles both normal and classic inventory modes by showing the appropriate UI elements.
            // Note: The EquipWeapon method is called when clicking on a weapon to equip it.
            // The WeaponPreviewUI is shown or hidden accordingly based on the selected weapon

            if (gm.inventoryType == InventoryType.Normal)
            {
                transform.parent.Find("Main Panels").Find("Inventory Tab").Find("List").gameObject.SetActive(true);
                transform.parent.Find("Main Panels").Find("Inventory Tab").Find("Classic_List").gameObject.SetActive(false);

                if (PrimarySlot[SelectedPrimary] != null)
                {
                    transform.parent.Find("Main Panels").Find("Inventory Tab").Find("List").Find("Primary").Find("Weapon Icon").GetComponent<Image>().sprite = PrimarySlot[SelectedPrimary].GetComponent<BaseWeaponScript>().WeaponIcon();
                    transform.parent.Find("Main Panels").Find("Inventory Tab").Find("List").Find("Primary").Find("Weapon Description").GetComponent<TMP_Text>().text = PrimarySlot[SelectedPrimary].GetComponent<BaseWeaponScript>().WeaponName();

                }
                else
                {
                    transform.parent.Find("Main Panels").Find("Inventory Tab").Find("List").Find("Primary").Find("Weapon Icon").GetComponent<Image>().sprite = EmptyGun;
                    transform.parent.Find("Main Panels").Find("Inventory Tab").Find("List").Find("Primary").Find("Weapon Description").GetComponent<TMP_Text>().text = "No Weapon";
                }
                transform.parent.Find("Main Panels").Find("Inventory Tab").Find("List").Find("Primary").Find("Weapon Slot").GetComponent<TMP_Text>().text = BaseLocalization.GetString("Slot") + " " + (SelectedPrimary + 1);

                if (SecoundarySlot[Selectedecoundary] != null)
                {
                    transform.parent.Find("Main Panels").Find("Inventory Tab").Find("List").Find("Secoundary").Find("Weapon Icon").GetComponent<Image>().sprite = SecoundarySlot[Selectedecoundary].GetComponent<BaseWeaponScript>().WeaponIcon();
                    transform.parent.Find("Main Panels").Find("Inventory Tab").Find("List").Find("Secoundary").Find("Weapon Description").GetComponent<TMP_Text>().text = SecoundarySlot[Selectedecoundary].GetComponent<BaseWeaponScript>().WeaponName();
                }
                else
                {
                    transform.parent.Find("Main Panels").Find("Inventory Tab").Find("List").Find("Secoundary").Find("Weapon Icon").GetComponent<Image>().sprite = EmptyGun;
                    transform.parent.Find("Main Panels").Find("Inventory Tab").Find("List").Find("Secoundary").Find("Weapon Description").GetComponent<TMP_Text>().text = "No Weapon";
                }
                transform.parent.Find("Main Panels").Find("Inventory Tab").Find("List").Find("Secoundary").Find("Weapon Slot").GetComponent<TMP_Text>().text = BaseLocalization.GetString("Slot") + " " + (Selectedecoundary + 1);

                if (MelleSlot[SelectedMelle] != null)
                {
                    transform.parent.Find("Main Panels").Find("Inventory Tab").Find("List").Find("Melle").Find("Weapon Icon").GetComponent<Image>().sprite = MelleSlot[SelectedMelle].GetComponent<BaseWeaponScript>().WeaponIcon();
                    transform.parent.Find("Main Panels").Find("Inventory Tab").Find("List").Find("Melle").Find("Weapon Description").GetComponent<TMP_Text>().text = MelleSlot[SelectedMelle].GetComponent<BaseWeaponScript>().WeaponName();
                }
                else
                {
                    transform.parent.Find("Main Panels").Find("Inventory Tab").Find("List").Find("Melle").Find("Weapon Icon").GetComponent<Image>().sprite = EmptyGun;
                    transform.parent.Find("Main Panels").Find("Inventory Tab").Find("List").Find("Melle").Find("Weapon Description").GetComponent<TMP_Text>().text = "No Weapon";
                }
                transform.parent.Find("Main Panels").Find("Inventory Tab").Find("List").Find("Melle").Find("Weapon Slot").GetComponent<TMP_Text>().text = BaseLocalization.GetString("Slot") + " " + (SelectedMelle + 1);

                if (ThrowableSlot[SelectedThrowable] != null)
                {
                    transform.parent.Find("Main Panels").Find("Inventory Tab").Find("List").Find("Throwable").Find("Weapon Icon").GetComponent<Image>().sprite = ThrowableSlot[SelectedThrowable].GetComponent<BaseWeaponScript>().WeaponIcon();
                    transform.parent.Find("Main Panels").Find("Inventory Tab").Find("List").Find("Throwable").Find("Weapon Description").GetComponent<TMP_Text>().text = ThrowableSlot[SelectedThrowable].GetComponent<BaseWeaponScript>().WeaponName();
                }
                else
                {
                    transform.parent.Find("Main Panels").Find("Inventory Tab").Find("List").Find("Throwable").Find("Weapon Icon").GetComponent<Image>().sprite = EmptyGun;
                    transform.parent.Find("Main Panels").Find("Inventory Tab").Find("List").Find("Throwable").Find("Weapon Description").GetComponent<TMP_Text>().text = "No Weapon";
                }
                transform.parent.Find("Main Panels").Find("Inventory Tab").Find("List").Find("Throwable").Find("Weapon Slot").GetComponent<TMP_Text>().text = BaseLocalization.GetString("Slot") + " " + (SelectedThrowable + 1);
            }
            else
            {
                transform.parent.Find("Main Panels").Find("Inventory Tab").Find("List").gameObject.SetActive(false);
                transform.parent.Find("Main Panels").Find("Inventory Tab").Find("Classic_List").gameObject.SetActive(true);

                // Primary
                foreach (Transform child in transform.parent.Find("Main Panels").Find("Inventory Tab").Find("Classic_List").Find("Primary").Find("Viewport").Find("Content")) { Destroy(child.gameObject); }
                for (int i = 0; i < PrimarySlot.Count; i++)
                {
                    GameObject weapon_Icon = Instantiate(transform.parent.Find("Main Panels").Find("Inventory Tab").Find("Classic_List").Find("Icon").gameObject, transform.parent.Find("Main Panels").Find("Inventory Tab").Find("Classic_List").Find("Primary").Find("Viewport").Find("Content"));
                    weapon_Icon.GetComponent<Image>().sprite = PrimarySlot[i].GetComponent<BaseWeaponScript>().WeaponIcon();
                    weapon_Icon.SetActive(true);
                }

                // Secoundary
                foreach (Transform child in transform.parent.Find("Main Panels").Find("Inventory Tab").Find("Classic_List").Find("Secoundary").Find("Viewport").Find("Content")) { Destroy(child.gameObject); }
                for (int i = 0; i < SecoundarySlot.Count; i++)
                {
                    GameObject weapon_Icon = Instantiate(transform.parent.Find("Main Panels").Find("Inventory Tab").Find("Classic_List").Find("Icon").gameObject, transform.parent.Find("Main Panels").Find("Inventory Tab").Find("Classic_List").Find("Secoundary").Find("Viewport").Find("Content"));
                    weapon_Icon.GetComponent<Image>().sprite = SecoundarySlot[i].GetComponent<BaseWeaponScript>().WeaponIcon();
                    weapon_Icon.SetActive(true);
                }

                // Melle
                foreach (Transform child in transform.parent.Find("Main Panels").Find("Inventory Tab").Find("Classic_List").Find("Melle").Find("Viewport").Find("Content")) { Destroy(child.gameObject); }
                for (int i = 0; i < MelleSlot.Count; i++)
                {
                    GameObject weapon_Icon = Instantiate(transform.parent.Find("Main Panels").Find("Inventory Tab").Find("Classic_List").Find("Icon").gameObject, transform.parent.Find("Main Panels").Find("Inventory Tab").Find("Classic_List").Find("Melle").Find("Viewport").Find("Content"));
                    weapon_Icon.GetComponent<Image>().sprite = MelleSlot[i].GetComponent<BaseWeaponScript>().WeaponIcon();
                    weapon_Icon.SetActive(true);
                }

                // Throwable
                foreach (Transform child in transform.parent.Find("Main Panels").Find("Inventory Tab").Find("Classic_List").Find("Throwable").Find("Viewport").Find("Content")) { Destroy(child.gameObject); }
                for (int i = 0; i < ThrowableSlot.Count; i++)
                {
                    GameObject weapon_Icon = Instantiate(transform.parent.Find("Main Panels").Find("Inventory Tab").Find("Classic_List").Find("Icon").gameObject, transform.parent.Find("Main Panels").Find("Inventory Tab").Find("Classic_List").Find("Throwable").Find("Viewport").Find("Content"));
                    weapon_Icon.GetComponent<Image>().sprite = ThrowableSlot[i].GetComponent<BaseWeaponScript>().WeaponIcon();
                    weapon_Icon.SetActive(true);
                }

            }
        }



        public void EquipWeapon(int type)
        {
            if (type == (int)SlotType.PrimaryWeapon)
            {
                SelectedPrimary++;
                if (SelectedPrimary == gm.PrimaryWeaponCount) { SelectedPrimary = 0; }
            }
            if (type == (int)SlotType.SecoundaryWeapon)
            {
                Selectedecoundary++;
                if (Selectedecoundary == gm.SecoundaryWeaponCount) { Selectedecoundary = 0; }
            }
            if (type == (int)SlotType.MelleWeapon)
            {
                SelectedMelle++;
                if (SelectedMelle == gm.MelleWeaponCount) { SelectedMelle = 0; }
            }
            if (type == (int)SlotType.ThrowableWeapon)
            {
                SelectedThrowable++;
                if (SelectedThrowable == gm.ThrowableWeaponCount) { SelectedThrowable = 0; }
            }
        }

        public void ShowPreview(int id)
        {
            Transform _tmp = null;
            if (CategoryID == (int)SlotType.PrimaryWeapon)
            {
                WeaponPreviewUI.Find("Name").GetComponent<TMP_Text>().text = AllPrimaryWeapon[id].GetComponent<BaseWeaponScript>().WeaponName();
                WeaponPreviewUI.Find("Category").GetComponent<TMP_Text>().text = AllPrimaryWeapon[id].GetComponent<BaseWeaponScript>().Category.ToString();
                WeaponPreviewUI.Find("Icon").GetComponent<Image>().sprite = AllPrimaryWeapon[id].GetComponent<BaseWeaponScript>().WeaponIcon();
                _tmp = AllPrimaryWeapon[id];
            }
            if (CategoryID == (int)SlotType.SecoundaryWeapon)
            {
                WeaponPreviewUI.Find("Name").GetComponent<TMP_Text>().text = AllSecoundaryWeapon[id].GetComponent<BaseWeaponScript>().WeaponName();
                WeaponPreviewUI.Find("Category").GetComponent<TMP_Text>().text = AllSecoundaryWeapon[id].GetComponent<BaseWeaponScript>().Category.ToString();
                WeaponPreviewUI.Find("Icon").GetComponent<Image>().sprite = AllSecoundaryWeapon[id].GetComponent<BaseWeaponScript>().WeaponIcon();
                _tmp = AllSecoundaryWeapon[id];
            }
            if (CategoryID == (int)SlotType.MelleWeapon)
            {
                WeaponPreviewUI.Find("Name").GetComponent<TMP_Text>().text = AllMelleWeapon[id].GetComponent<BaseWeaponScript>().WeaponName();
                WeaponPreviewUI.Find("Category").GetComponent<TMP_Text>().text = AllMelleWeapon[id].GetComponent<BaseWeaponScript>().Category.ToString();
                WeaponPreviewUI.Find("Icon").GetComponent<Image>().sprite = AllMelleWeapon[id].GetComponent<BaseWeaponScript>().WeaponIcon();
                _tmp = AllMelleWeapon[id];
            }
            if (CategoryID == (int)SlotType.ThrowableWeapon)
            {
                WeaponPreviewUI.Find("Name").GetComponent<TMP_Text>().text = AllThrowableWeapon[id].GetComponent<BaseWeaponScript>().WeaponName();
                WeaponPreviewUI.Find("Category").GetComponent<TMP_Text>().text = AllThrowableWeapon[id].GetComponent<BaseWeaponScript>().Category.ToString();
                WeaponPreviewUI.Find("Icon").GetComponent<Image>().sprite = AllThrowableWeapon[id].GetComponent<BaseWeaponScript>().WeaponIcon();
                _tmp = AllThrowableWeapon[id];
            }
            HighlightWeaponID = id;
            GenerateWeaponStatsUI(_tmp);
            // Get Stats

        }

        public void ShowPreview(Transform weapon)
        {
            if (weapon == null)
            {
                WeaponPreviewUI.Find("Name").GetComponent<TMP_Text>().text = "No Weapon";
                WeaponPreviewUI.Find("Category").GetComponent<TMP_Text>().text = "";
                WeaponPreviewUI.Find("Icon").GetComponent<Image>().sprite = EmptyGun;
            }
            else
            {
                BaseWeaponScript Weapon = weapon.GetComponent<BaseWeaponScript>();
                WeaponPreviewUI.Find("Name").GetComponent<TMP_Text>().text = Weapon.WeaponName();
                WeaponPreviewUI.Find("Category").GetComponent<TMP_Text>().text = Weapon.Category.ToString();
                WeaponPreviewUI.Find("Icon").GetComponent<Image>().sprite = Weapon.WeaponIcon();
            }
            GenerateWeaponStatsUI(weapon);
        }

        void GenerateWeaponStatsUI(Transform weapon)
        {
            if (weapon == null)
            {
                InventoryStatDamage.gameObject.SetActive(false);
                InventoryStatRange.gameObject.SetActive(false);
                InventoryStatAmmo.gameObject.SetActive(false);
                InventoryStatFirerate.gameObject.SetActive(false);
                InventoryStatMagazine.gameObject.SetActive(false);
                InventoryStatReload.gameObject.SetActive(false);
                InventoryStatAlternative.gameObject.SetActive(false);
                InventoryStatAltDamage.gameObject.SetActive(false);
                InventoryStatAltRange.gameObject.SetActive(false);
                InventoryStatBlock.gameObject.SetActive(false);
            }
            else
            {
                if (weapon.gameObject.TryGetComponent<RangeWeapon>(out RangeWeapon wepScript))
                {
                    InventoryStatDamage.gameObject.SetActive(true); if (wepScript.bulletCountPerShot == 1) InventoryStatDamage.transform.Find("Value").GetComponent<TMP_Text>().text = wepScript.weaponDamage.ToString();
                    else InventoryStatDamage.transform.Find("Value").GetComponent<TMP_Text>().text = wepScript.weaponDamage.ToString() + " x" + wepScript.bulletCountPerShot;
                    InventoryStatRange.gameObject.SetActive(true); if (wepScript.WeaponShootType == WeaponTypeShooting.Hitscan) InventoryStatRange.transform.Find("Value").GetComponent<TMP_Text>().text = wepScript.hitscanRange + " M";
                    if (wepScript.WeaponShootType == WeaponTypeShooting.Projectile) InventoryStatRange.transform.Find("Value").GetComponent<TMP_Text>().text = "Soon";
                    InventoryStatAmmo.gameObject.SetActive(true); InventoryStatAmmo.transform.Find("Value").GetComponent<TMP_Text>().text = wepScript.ammoType.ToString();
                    InventoryStatFirerate.gameObject.SetActive(true); InventoryStatFirerate.transform.Find("Value").GetComponent<TMP_Text>().text = (1 / wepScript.timeBetweenShot) + " / Sec";
                    InventoryStatMagazine.gameObject.SetActive(true); InventoryStatMagazine.transform.Find("Value").GetComponent<TMP_Text>().text = wepScript.magazineSize.ToString();
                    InventoryStatReload.gameObject.SetActive(true); InventoryStatReload.transform.Find("Value").GetComponent<TMP_Text>().text = "Soon";
                    InventoryStatAlternative.gameObject.SetActive(false);
                    InventoryStatAltDamage.gameObject.SetActive(false);
                    InventoryStatAltRange.gameObject.SetActive(false);
                    InventoryStatBlock.gameObject.SetActive(false);
                }
                else if (weapon.gameObject.TryGetComponent<MelleWeapon>(out MelleWeapon wepScript2))
                {
                    InventoryStatDamage.gameObject.SetActive(true); InventoryStatDamage.transform.Find("Value").GetComponent<TMP_Text>().text = wepScript2.weaponDamage + "";
                    InventoryStatRange.gameObject.SetActive(true); InventoryStatRange.transform.Find("Value").GetComponent<TMP_Text>().text = wepScript2.range + " M";
                    InventoryStatAmmo.gameObject.SetActive(false);
                    InventoryStatFirerate.gameObject.SetActive(false);
                    InventoryStatMagazine.gameObject.SetActive(false);
                    InventoryStatReload.gameObject.SetActive(false);
                    InventoryStatAlternative.gameObject.SetActive(true); InventoryStatAlternative.transform.Find("Value").GetComponent<TMP_Text>().text = wepScript2.AlternativeMode + "";
                    if (wepScript2.AlternativeMode == AlternativeMelleType.AlternativeAttack)
                    {
                        InventoryStatAltDamage.gameObject.SetActive(true); InventoryStatAltDamage.transform.Find("Value").GetComponent<TMP_Text>().text = wepScript2.AlternativeWeaponDamage + "";
                        InventoryStatAltRange.gameObject.SetActive(true); InventoryStatAltRange.transform.Find("Value").GetComponent<TMP_Text>().text = wepScript2.AlternativeRange + " M";
                    }
                    else
                    {
                        InventoryStatAltDamage.gameObject.SetActive(false);
                        InventoryStatAltRange.gameObject.SetActive(false);
                    }
                    if (wepScript2.AlternativeMode == AlternativeMelleType.Block)
                    {
                        InventoryStatBlock.gameObject.SetActive(true); InventoryStatBlock.transform.Find("Value").GetComponent<TMP_Text>().text = wepScript2.BlockingDamageChange + " %";
                    }
                    else
                    {
                        InventoryStatBlock.gameObject.SetActive(false);
                    }
                }
                else if (weapon.gameObject.TryGetComponent<ThrowableWeapon>(out ThrowableWeapon wepScript3))
                {
                    InventoryStatDamage.gameObject.SetActive(true); InventoryStatDamage.transform.Find("Value").GetComponent<TMP_Text>().text = wepScript3.Damage + "";
                    InventoryStatRange.gameObject.SetActive(true); InventoryStatRange.transform.Find("Value").GetComponent<TMP_Text>().text = "Soon";
                    InventoryStatAmmo.gameObject.SetActive(true); InventoryStatAmmo.transform.Find("Value").GetComponent<TMP_Text>().text = wepScript3.AmmoType + "";
                    InventoryStatFirerate.gameObject.SetActive(false);
                    InventoryStatMagazine.gameObject.SetActive(false);
                    InventoryStatReload.gameObject.SetActive(false);
                    InventoryStatAlternative.gameObject.SetActive(false);
                    InventoryStatAltDamage.gameObject.SetActive(false);
                    if (wepScript3.AlternativeThrow)
                    {
                        InventoryStatAltRange.gameObject.SetActive(true); InventoryStatAltRange.transform.Find("Value").GetComponent<TMP_Text>().text = wepScript2.AlternativeRange + " M";
                    }
                    else
                    {
                        InventoryStatAltRange.gameObject.SetActive(false);
                    }
                    InventoryStatBlock.gameObject.SetActive(false);
                }
            }
        }

        public void EquipWeapon(Transform weapon)
        {
            if (CategoryID == (int)SlotType.PrimaryWeapon)
            {
                PrimarySlot[SelectedPrimary] = weapon;
            }
            if (CategoryID == (int)SlotType.SecoundaryWeapon)
            {
                SecoundarySlot[Selectedecoundary] = weapon;
            }
            if (CategoryID == (int)SlotType.MelleWeapon)
            {
                MelleSlot[SelectedMelle] = weapon;
            }
            if (CategoryID == (int)SlotType.ThrowableWeapon)
            {
                ThrowableSlot[SelectedThrowable] = weapon;
            }
            InventoryResetUI();
        }

        public void EquipWeapon()
        {
            if (CategoryID == (int)SlotType.PrimaryWeapon)
            {
                // Switch Weapon if Equip
                for (int i = 0; i < PrimarySlot.Count; i++)
                {
                    if (PrimarySlot[i] == AllPrimaryWeapon[HighlightWeaponID]) { PrimarySlot[i] = PrimarySlot[SelectedPrimary]; }
                }
                PrimarySlot[SelectedPrimary] = AllPrimaryWeapon[HighlightWeaponID];
            }
            if (CategoryID == (int)SlotType.SecoundaryWeapon)
            {
                // Switch Weapon if Equip
                for (int i = 0; i < SecoundarySlot.Count; i++)
                {
                    if (SecoundarySlot[i] == AllSecoundaryWeapon[HighlightWeaponID]) { SecoundarySlot[i] = SecoundarySlot[Selectedecoundary]; }
                }
                SecoundarySlot[Selectedecoundary] = AllSecoundaryWeapon[HighlightWeaponID];
            }
            if (CategoryID == (int)SlotType.MelleWeapon)
            {
                // Switch Weapon if Equip
                for (int i = 0; i < MelleSlot.Count; i++)
                {
                    if (MelleSlot[i] == AllMelleWeapon[HighlightWeaponID]) { MelleSlot[i] = MelleSlot[SelectedMelle]; }
                }
                MelleSlot[SelectedMelle] = AllMelleWeapon[HighlightWeaponID];
            }
            if (CategoryID == (int)SlotType.ThrowableWeapon)
            {
                // Switch Weapon if Equip
                for (int i = 0; i < ThrowableSlot.Count; i++)
                {
                    if (ThrowableSlot[i] == AllThrowableWeapon[HighlightWeaponID]) { ThrowableSlot[i] = ThrowableSlot[SelectedThrowable]; }
                }
                ThrowableSlot[SelectedThrowable] = AllThrowableWeapon[HighlightWeaponID];
            }
            InventoryResetUI();
        }

    }

    /// Custom Editor
#if UNITY_EDITOR

    [CustomEditor(typeof(WeaponSwitch))]
    public class WeaponSwitchEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorUtility.SetDirty(target);
            WeaponSwitch Inventory = (WeaponSwitch)target;
            EditorGUI.indentLevel = 0;

            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((WeaponSwitch)target), typeof(WeaponSwitch), false);
            GUI.enabled = true;

            EditorGUILayout.BeginVertical("box");

            string _weaponList = "";
            string _slotList = "";

            EditorGUILayout.BeginHorizontal("Toolbar", GUILayout.Height(30));
            EditorGUILayout.LabelField($"Inventory", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("actualWeapon"), new GUIContent("Actual Weapon"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("BaseLocalization"), new GUIContent("Base Localization"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("noWeaponObject"), new GUIContent("Empty Weapon Object"));

            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal("Toolbar", GUILayout.Height(30));
            EditorGUILayout.LabelField($"Weapon UI", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("ammoText"), new GUIContent("Ammo Text"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("WeaponNameText"), new GUIContent("Weapon Name Text"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("WeaponUIImage"), new GUIContent("Weapon Icon"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("AmmoIcon"), new GUIContent("Ammo Icon"));

            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal("Toolbar", GUILayout.Height(30));
            EditorGUILayout.LabelField($"Inventory UI", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("EmptyGun"), new GUIContent("Empty Gun Sprite"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("WeaponPreviewUI"), new GUIContent("Weapon Preview UI"));

            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal("Toolbar", GUILayout.Height(30));
            EditorGUILayout.LabelField($"Ammo Icon UI", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);

            for (int i = 0; i < Inventory.AmmoSprites.Count; i++)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("AmmoSprites").GetArrayElementAtIndex(i).FindPropertyRelative("Icon"), new GUIContent($"{(Inventory.AmmoSprites[i].Type).ToString()} Icon"));
            }

            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal("Toolbar", GUILayout.Height(30));
            EditorGUILayout.LabelField($"Inventory Stat UI", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);

            EditorGUILayout.BeginVertical();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("InventoryStatDamage"), new GUIContent("Damage"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("InventoryStatRange"), new GUIContent("Range"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("InventoryStatAmmo"), new GUIContent("Ammo"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("InventoryStatFirerate"), new GUIContent("Fire Rate"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("InventoryStatMagazine"), new GUIContent("Magazine"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("InventoryStatReload"), new GUIContent("Reload"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("InventoryStatAlternative"), new GUIContent("Alternative"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("InventoryStatAltDamage"), new GUIContent("Alt Damage"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("InventoryStatAltRange"), new GUIContent("Alt Range"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("InventoryStatBlock"), new GUIContent("Block"));

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();

            //EditorGUILayout.PropertyField(serializedObject.FindProperty("Boss"), new GUIContent("Enemy"));
            if (EditorApplication.isPlaying)
            {
                _weaponList = "";
                for (int i = 0; i < Inventory.AllPrimaryWeapon.Count; i++)
                {
                    _weaponList += "\n- ";
                    _weaponList += Inventory.AllPrimaryWeapon[i].GetComponent<BaseWeaponScript>().WeaponName();
                }
                EditorGUILayout.HelpBox($"Available Primary Weapon: {Inventory.AllPrimaryWeapon.Count}{_weaponList}", MessageType.Info);

                _slotList = "";
                for (int i = 0; i < Inventory.PrimarySlot.Count; i++)
                {
                    _slotList += "\n- ";
                    if (Inventory.PrimarySlot[i] != null) _slotList += Inventory.PrimarySlot[i].GetComponent<BaseWeaponScript>().WeaponName();
                    else _slotList += "Empty Slot";
                }
                EditorGUILayout.HelpBox($"Primary Weapon Slot: {Inventory.PrimarySlot.Count}{_slotList}", MessageType.Info);

                _weaponList = "";
                for (int i = 0; i < Inventory.AllSecoundaryWeapon.Count; i++)
                {
                    _weaponList += "\n- ";
                    _weaponList += Inventory.AllSecoundaryWeapon[i].GetComponent<BaseWeaponScript>().WeaponName();
                }
                EditorGUILayout.HelpBox($"Available Secoundary Weapon: {Inventory.AllSecoundaryWeapon.Count}{_weaponList}", MessageType.Info);

                _slotList = "";
                for (int i = 0; i < Inventory.SecoundarySlot.Count; i++)
                {
                    _slotList += "\n- ";
                    if (Inventory.SecoundarySlot[i] != null) _slotList += Inventory.SecoundarySlot[i].GetComponent<BaseWeaponScript>().WeaponName();
                    else _slotList += "Empty Slot";
                }
                EditorGUILayout.HelpBox($"Secoundary Weapon Slot: {Inventory.SecoundarySlot.Count}{_slotList}", MessageType.Info);

                _weaponList = "";
                for (int i = 0; i < Inventory.AllMelleWeapon.Count; i++)
                {
                    _weaponList += "\n- ";
                    _weaponList += Inventory.AllMelleWeapon[i].GetComponent<BaseWeaponScript>().WeaponName();
                }
                EditorGUILayout.HelpBox($"Available Melle Weapon: {Inventory.AllMelleWeapon.Count}{_weaponList}", MessageType.Info);

                _slotList = "";
                for (int i = 0; i < Inventory.MelleSlot.Count; i++)
                {
                    _slotList += "\n- ";
                    if (Inventory.MelleSlot[i] != null) _slotList += Inventory.MelleSlot[i].GetComponent<BaseWeaponScript>().WeaponName();
                    else _slotList += "Empty Slot";
                }
                EditorGUILayout.HelpBox($"Melle Weapon Slot: {Inventory.MelleSlot.Count}{_slotList}", MessageType.Info);

                _weaponList = "";
                for (int i = 0; i < Inventory.AllThrowableWeapon.Count; i++)
                {
                    _weaponList += "\n- ";
                    _weaponList += Inventory.AllThrowableWeapon[i].GetComponent<BaseWeaponScript>().WeaponName();
                }
                EditorGUILayout.HelpBox($"Available Throwable Weapon: {Inventory.AllThrowableWeapon.Count}{_weaponList}", MessageType.Info);

                _slotList = "";
                for (int i = 0; i < Inventory.ThrowableSlot.Count; i++)
                {
                    _slotList += "\n- ";
                    if (Inventory.ThrowableSlot[i] != null) _slotList += Inventory.ThrowableSlot[i].GetComponent<BaseWeaponScript>().WeaponName();
                    else _slotList += "Empty Slot";
                }
                EditorGUILayout.HelpBox($"Throwable Weapon Slot: {Inventory.ThrowableSlot.Count}{_slotList}", MessageType.Info);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }

#endif
}
