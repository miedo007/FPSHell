using UnityEngine;
using System.Collections;
using System.IO;
using HellishBattle.Level;

namespace HellishBattle.SaveSystem
{
	public static class TinySaveSystem
	{
		private static string file = "HellishBattle.tss";
		private static bool loaded;
		public static TinyData data;

		public static void Initialize(string fileName)
		{
			if (!loaded) { file = fileName; if (File.Exists(GetPath())) Load(); else data = new TinyData(); loaded = true; }
		}

		static string GetPath()
		{
			return Application.persistentDataPath + "/" + file;
		}

		static void Load()
		{
			data = SerializatorBinary.LoadBinary(GetPath());
		}

		static void ChangeKey(string name, string item)
		{
			bool j = false;
			for (int i = 0; i < data.items.Count; i++)
			{
				if (string.Compare(name, data.items[i].Key) == 0)
				{
					data.items[i].Value = Crypt(item);
					j = true;
					break;
				}
			}

			if (!j) data.AddItem(new TinySaveData(name, Crypt(item)));
		}

		public static bool HasKey(string name)
		{
			if (string.IsNullOrEmpty(name)) return false;
			foreach (TinySaveData data in data.items) { if (string.Compare(name, data.Key) == 0) { return true; } }

			return false;
		}

		// Base

		#region INT
		public static void SetInt(string name, int value)
		{
			if (string.IsNullOrEmpty(name)) return;
			ChangeKey(name, value.ToString());
		}

		public static int GetInt(string name)
		{
			if (string.IsNullOrEmpty(name)) return 0;
			return DecryptInt(name, 0);
		}

		public static int GetInt(string name, int defaultValue)
		{
			if (string.IsNullOrEmpty(name)) return defaultValue;
			return DecryptInt(name, defaultValue);
		}

		static int DecryptInt(string name, int defaultValue)
		{
			if (data != null)
			{
				for (int i = 0; i < data.items.Count; i++)
				{
					if (string.Compare(name, data.items[i].Key) == 0)
					{
						return intParse(Crypt(data.items[i].Value));
					}
				}
			}
			return defaultValue;
		}

		static int intParse(string value)
		{
			int _value;
			if (int.TryParse(value, out _value)) return _value;
			return 0;
		}

		#endregion

		#region FLOAT

		public static void SetFloat(string name, float value)
		{
			if (string.IsNullOrEmpty(name)) return;
			ChangeKey(name, value.ToString());
		}

		public static float GetFloat(string name)
		{
			if (string.IsNullOrEmpty(name)) return 0;
			return DecryptFloat(name, 0);
		}

		public static float GetFloat(string name, float defaultValue)
		{
			if (string.IsNullOrEmpty(name)) return defaultValue;
			return DecryptFloat(name, defaultValue);
		}

		static float DecryptFloat(string name, float defaultValue)
		{
			for (int i = 0; i < data.items.Count; i++)
			{
				if (string.Compare(name, data.items[i].Key) == 0)
				{
					return floatParse(Crypt(data.items[i].Value));
				}
			}

			return defaultValue;
		}

		static float floatParse(string value)
		{
			float _value;
			if (float.TryParse(value, out _value)) return _value;
			return 0;
		}

		#endregion

		#region STRING

		public static void SetString(string name, string value)
		{
			if (string.IsNullOrEmpty(name)) return;
			ChangeKey(name, value);
		}

		public static string GetString(string name)
		{
			if (string.IsNullOrEmpty(name)) return string.Empty;
			return DecryptString(name, string.Empty);
		}

		public static string GetString(string name, string defaultValue)
		{
			if (string.IsNullOrEmpty(name)) return defaultValue;
			return DecryptString(name, defaultValue);
		}

		static string DecryptString(string name, string defaultValue)
		{
			for (int i = 0; i < data.items.Count; i++)
			{
				if (string.Compare(name, data.items[i].Key) == 0)
				{
					return Crypt(data.items[i].Value);
				}
			}

			return defaultValue;
		}

		#endregion

		#region BOOL

		public static void SetBool(string name, bool value)
		{
			if (string.IsNullOrEmpty(name)) return;
			string tmp = string.Empty;
			if (value) tmp = "1"; else tmp = "0";
			ChangeKey(name, tmp);
		}

		public static bool GetBool(string name)
		{
			if (string.IsNullOrEmpty(name)) return false;
			return DecryptBool(name, false);
		}

		public static bool GetBool(string name, bool defaultValue)
		{
			if (string.IsNullOrEmpty(name)) return defaultValue;
			return DecryptBool(name, defaultValue);
		}

		static bool DecryptBool(string name, bool defaultValue)
		{
			for (int i = 0; i < data.items.Count; i++)
			{
				if (string.Compare(name, data.items[i].Key) == 0)
				{
					if (string.Compare(Crypt(data.items[i].Value), "1") == 0) return true; else return false;
				}
			}

			return defaultValue;
		}

		#endregion

		// Additional

		#region VECTOR_2

		public static void SetVector2(string name, Vector2 value)
		{
			if (string.IsNullOrEmpty(name)) return;
			SetString(name, value.x + "|" + value.y);
		}


		public static Vector2 GetVector2(string name)
		{
			if (string.IsNullOrEmpty(name)) return Vector2.zero;
			return iVector2(name, Vector2.zero);
		}

		public static Vector2 GetVector2(string name, Vector2 defaultValue)
		{
			if (string.IsNullOrEmpty(name)) return defaultValue;
			return iVector2(name, defaultValue);
		}

		static Vector2 iVector2(string name, Vector2 defaultValue)
		{
			Vector2 vector = Vector2.zero;

			for (int i = 0; i < data.items.Count; i++)
			{
				if (string.Compare(name, data.items[i].Key) == 0)
				{
					string[] t = Crypt(data.items[i].Value).Split(new char[] { '|' });
					if (t.Length == 2)
					{
						vector.x = floatParse(t[0]);
						vector.y = floatParse(t[1]);
						return vector;
					}
					break;
				}
			}

			return defaultValue;
		}

		#endregion

		#region VECTOR_3

		public static void SetVector3(string name, Vector3 value)
		{
			if (string.IsNullOrEmpty(name)) return;
			SetString(name, value.x + "|" + value.y + "|" + value.z);
		}

		public static Vector3 GetVector3(string name)
		{
			if (string.IsNullOrEmpty(name)) return Vector3.zero;
			return iVector3(name, Vector3.zero);
		}

		public static Vector3 GetVector3(string name, Vector3 defaultValue)
		{
			if (string.IsNullOrEmpty(name)) return defaultValue;
			return iVector3(name, defaultValue);
		}

		static Vector3 iVector3(string name, Vector3 defaultValue)
		{
			Vector3 vector = Vector3.zero;

			for (int i = 0; i < data.items.Count; i++)
			{
				if (string.Compare(name, data.items[i].Key) == 0)
				{
					string[] t = Crypt(data.items[i].Value).Split(new char[] { '|' });
					if (t.Length == 3)
					{
						vector.x = floatParse(t[0]);
						vector.y = floatParse(t[1]);
						vector.z = floatParse(t[2]);
						return vector;
					}
					break;
				}
			}

			return defaultValue;
		}

		#endregion

		#region VECTOR_4

		public static void SetVector4(string name, Vector4 value)
		{
			if (string.IsNullOrEmpty(name)) return;
			SetString(name, value.x + "|" + value.y + "|" + value.z + "|" + value.w);
		}

		public static Vector4 GetVector4(string name)
		{
			if (string.IsNullOrEmpty(name)) return Vector4.zero;
			return iVector4(name, Vector4.zero);
		}

		public static Vector4 GetVector4(string name, Vector4 defaultValue)
		{
			if (string.IsNullOrEmpty(name)) return defaultValue;
			return iVector4(name, defaultValue);
		}

		static Vector4 iVector4(string name, Vector4 defaultValue)
		{
			Vector4 vector = Vector4.zero;

			for (int i = 0; i < data.items.Count; i++)
			{
				if (string.Compare(name, data.items[i].Key) == 0)
				{
					string[] t = Crypt(data.items[i].Value).Split(new char[] { '|' });
					if (t.Length == 3)
					{
						vector.x = floatParse(t[0]);
						vector.y = floatParse(t[1]);
						vector.z = floatParse(t[2]);
						vector.w = floatParse(t[3]);
						return vector;
					}
					break;
				}
			}

			return defaultValue;
		}

		#endregion

		#region QUATERNION

		public static void SetQuaternion(string name, Quaternion value)
		{
			if (string.IsNullOrEmpty(name)) return;
			SetString(name, value.x + "|" + value.y + "|" + value.z + "|" + value.w);
		}

		public static Quaternion GetQuaternion(string name)
		{
			if (string.IsNullOrEmpty(name)) return new Quaternion(0, 0, 0, 0);
			return iQuaternion(name, new Quaternion(0, 0, 0, 0));
		}

		public static Quaternion GetQuaternion(string name, Quaternion defaultValue)
		{
			if (string.IsNullOrEmpty(name)) return defaultValue;
			return iQuaternion(name, defaultValue);
		}

		static Quaternion iQuaternion(string name, Quaternion defaultValue)
		{
			Quaternion vector = new Quaternion(0, 0, 0, 0);

			for (int i = 0; i < data.items.Count; i++)
			{
				if (string.Compare(name, data.items[i].Key) == 0)
				{
					string[] t = Crypt(data.items[i].Value).Split(new char[] { '|' });
					if (t.Length == 3)
					{
						vector.x = floatParse(t[0]);
						vector.y = floatParse(t[1]);
						vector.z = floatParse(t[2]);
						vector.w = floatParse(t[3]);
						return vector;
					}
					break;
				}
			}

			return defaultValue;
		}

		#endregion

		#region COLOR

		public static void SetColor(string name, Color value)
		{
			if (string.IsNullOrEmpty(name)) return;
			SetString(name, value.r + "|" + value.g + "|" + value.b + "|" + value.a);
		}

		public static Color GetColor(string name)
		{
			if (string.IsNullOrEmpty(name)) return Color.white;
			return iColor(name, Color.white);
		}

		public static Color GetColor(string name, Color defaultValue)
		{
			if (string.IsNullOrEmpty(name)) return defaultValue;
			return iColor(name, defaultValue);
		}

		static Color iColor(string name, Color defaultValue)
		{
			Color color = Color.clear;

			for (int i = 0; i < data.items.Count; i++)
			{
				if (string.Compare(name, data.items[i].Key) == 0)
				{
					string[] t = Crypt(data.items[i].Value).Split(new char[] { '|' });
					if (t.Length == 4)
					{
						color.r = floatParse(t[0]);
						color.g = floatParse(t[1]);
						color.b = floatParse(t[2]);
						color.a = floatParse(t[3]);
						return color;
					}
					break;
				}
			}

			return defaultValue;
		}

		#endregion

		#region TROPHY

		public static void SetTrophy(string name, Trophy value)
		{
			if (string.IsNullOrEmpty(name)) return;

			string tmp1; if (value.Requirement_1) tmp1 = "1"; else tmp1 = "0";
			string tmp2; if (value.Requirement_2) tmp2 = "1"; else tmp2 = "0";
			string tmp3; if (value.Requirement_3) tmp3 = "1"; else tmp3 = "0";
			string tmp4; if (value.Requirement_4) tmp4 = "1"; else tmp4 = "0";
			string tmp5; if (value.Requirement_5) tmp5 = "1"; else tmp5 = "0";

			SetString(name, tmp1 + "|" + tmp2 + "|" + tmp3 + "|" + tmp4 + "|" + tmp5);
		}

		public static Trophy GetTrophy(string name)
		{
			if (string.IsNullOrEmpty(name)) return new Trophy();
			return iTrophy(name, new Trophy());
		}

		public static Trophy GetTrophy(string name, Trophy defaultValue)
		{
			if (string.IsNullOrEmpty(name)) return defaultValue;
			return iTrophy(name, defaultValue);
		}

		static Trophy iTrophy(string name, Trophy defaultValue)
		{
			Trophy vector = new Trophy();

			for (int i = 0; i < data.items.Count; i++)
			{
				if (string.Compare(name, data.items[i].Key) == 0)
				{
					string[] t = Crypt(data.items[i].Value).Split(new char[] { '|' });
					if (t.Length == 5)
					{
						if (t[0] == "1") vector.Requirement_1 = true; else vector.Requirement_1 = false;
						if (t[1] == "1") vector.Requirement_2 = true; else vector.Requirement_2 = false;
						if (t[2] == "1") vector.Requirement_3 = true; else vector.Requirement_3 = false;
						if (t[3] == "1") vector.Requirement_4 = true; else vector.Requirement_4 = false;
						if (t[4] == "1") vector.Requirement_5 = true; else vector.Requirement_5 = false;
						return vector;
					}
					break;
				}
			}

			return defaultValue;
		}

		#endregion

		#region PlayerSave

		public static void SetPlayerSave(string name, PlayerSaveClass value)
		{
			if (string.IsNullOrEmpty(name)) return;

			SetString(name, value.Health + "|" + value.Armor + "|" + value.Map + "|" + value.Ammo + "|" + value.AllWeapon + "|" + value.PrimarySlot + "|" + value.SecoundarySlot + "|" + value.MelleSlot + "|" + value.ThrowableSlot);
		}

		public static PlayerSaveClass GetPlayerSave(string name)
		{
			if (string.IsNullOrEmpty(name)) return new PlayerSaveClass();
			return iPlayerSave(name, new PlayerSaveClass());
		}

		static PlayerSaveClass iPlayerSave(string name, PlayerSaveClass defaultValue)
		{
			PlayerSaveClass vector = new PlayerSaveClass();

			for (int i = 0; i < data.items.Count; i++)
			{
				if (string.Compare(name, data.items[i].Key) == 0)
				{
					string[] t = Crypt(data.items[i].Value).Split(new char[] { '|' });
					if (t.Length == 9)
					{
						vector.Health = float.Parse(t[0]);
						vector.Armor = float.Parse(t[1]);
						vector.Map = t[2];

						vector.Ammo = t[3];

						vector.AllWeapon = t[4];
						vector.PrimarySlot = t[5];
						vector.SecoundarySlot = t[6];
						vector.MelleSlot = t[7];
						vector.ThrowableSlot = t[8];

						return vector;
					}
					break;
				}
			}

			return defaultValue;
		}

		#endregion

		#region WeaponUpgrade

		public static void SetWeaponUpgrade(string name, WeaponUpgradeSaveClass value)
		{
			if (string.IsNullOrEmpty(name)) return;

			SetString(name, value.selectedUpgrade + "|" + value.selectedSkin + "|" + value.unlockedUpgrade + "|" + value.unlockedSkin);
		}
		public static WeaponUpgradeSaveClass GetWeaponUpgrade(string name)
		{
			if (string.IsNullOrEmpty(name)) return new WeaponUpgradeSaveClass();
			return iWeaponUpgrade(name, new WeaponUpgradeSaveClass());
		}


		static WeaponUpgradeSaveClass iWeaponUpgrade(string name, WeaponUpgradeSaveClass defaultValue)
		{
			WeaponUpgradeSaveClass vector = new WeaponUpgradeSaveClass();

			for (int i = 0; i < data.items.Count; i++)
			{
				if (string.Compare(name, data.items[i].Key) == 0)
				{
					string[] t = Crypt(data.items[i].Value).Split(new char[] { '|' });
					if (t.Length == 4)
					{
						vector.selectedUpgrade = int.Parse(t[0]);
						vector.selectedSkin = int.Parse(t[1]);
						vector.unlockedUpgrade = t[2];
						vector.unlockedSkin = t[3];
						return vector;
					}
					break;
				}
			}

			return defaultValue;
		}

		#endregion



		public static void SaveToDisk()
		{
			if (data.items.Count == 0) return;
			SerializatorBinary.SaveBinary(data, GetPath());
		}

		static string Crypt(string text)
		{
			string result = string.Empty;
			foreach (char j in text) result += (char)((int)j ^ 42);
			return result;
		}
	}

	public class PlayerSaveClass
	{
		/* PLAYER */
		public float Health = 100;          // Player Health
		public float Armor;                 // Player Armor
		public string Map;                  // Actual Level
		/* WEAPON */
		public string Ammo;                 // All Ammo Available
		public string AllWeapon;            // All Available Weapon
		public string PrimarySlot;          // Primary Slot
		public string SecoundarySlot;       // Secoundary Slot
		public string MelleSlot;            // Melle Slot
		public string ThrowableSlot;        // Throwable Slot

		public PlayerSaveClass(float Health, float Armor, string Map, string Ammo, string AllWeapon, string PrimarySlot, string SecoundarySlot, string MelleSlot, string ThrowableSlot)
		{
			this.Health = Health;
			this.Armor = Armor;
			this.Map = Map;

			this.Ammo = Ammo;

			this.AllWeapon = AllWeapon;
			this.PrimarySlot = PrimarySlot;
			this.SecoundarySlot = SecoundarySlot;
			this.MelleSlot = MelleSlot;
			this.ThrowableSlot = ThrowableSlot;
		}

		public PlayerSaveClass()
		{
		}
	}

	public class WeaponUpgradeSaveClass
	{
		public int selectedUpgrade = -1;
		public int selectedSkin = -1;
		// Unlocked
		public string unlockedUpgrade;
		public string unlockedSkin;

		public WeaponUpgradeSaveClass() { }
		public WeaponUpgradeSaveClass(int upgrade, int skin)
		{
			this.selectedUpgrade = upgrade;
			this.selectedSkin = skin;
		}
		public WeaponUpgradeSaveClass(string upgrade, string skin)
		{
			this.unlockedUpgrade = upgrade;
			this.unlockedSkin = skin;
		}
	}
}