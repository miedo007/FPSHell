using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using System.Collections.Generic;
using HellishBattle.AI;
using HellishBattle.Campaign;

namespace HellishBattle.Enemies
{
    public class EnemyStates : MonoBehaviour
    {
        //public EnemyType type;

        //[Line("Patrol")]
        public TargetSelectType OrderWaypoints;
        [Suffix("Metres")] public int patrolRange;
        //[ShowIf("OrderWaypoints", 2)] 
        public float ProceduralWaypointRadius = 15f;
        //[HelpBox("If 'Procedurally Generated Waypoints' is selected, the waypoint list is not used", HelpBoxEnum.Info)]
        public Transform[] waypoints;

        //[Line("Alert")]
        [Suffix("Secound")] public float stayAlertTime;

        //[Line("Attack")]
        public EnemyAttackType AttackType;
        public LayerMask raycastMask;

        public List<EnemyMelleAttackClass> MelleAttack;
        public List<EnemyRangeAttackClass> RangeAttack;

        //[Line("Vision")]
        public Transform vision;
        [Suffix("°")] public float viewAngle;

        // Privates

        [HideInInspector] public AlertState alertState;
        [HideInInspector] public AttackState attackState;
        [HideInInspector] public ChaseState chaseState;
        [HideInInspector] public PatrolState patrolState;
        [HideInInspector] public IEnemyAI currentState;
        [HideInInspector] public NavMeshAgent navMeshAgent;
        [HideInInspector] public Transform chaseTarget;
        [HideInInspector] public Vector3 lastKnownPosition;
        [HideInInspector] public Vision visionScript;

        [HideInInspector] public float abilityDamageMultiplier;
        [HideInInspector] public float reduceDelayMultiplier;
        [HideInInspector] public DifficultyLevel difficulty;
        Enemy enemy;

        [HideInInspector] public EnemyMelleAttackClass ActualMelleAttack;
        [HideInInspector] public EnemyRangeAttackClass ActualRangeAttack;
        [HideInInspector] public float DamageMultiplier;
        [HideInInspector] public float MelleDelayMultiplier;
        [HideInInspector] public float RangeDelayMultiplier;

        void Awake()
        {
            // We instantiate each state
            // And we pass the EnemyStates object to them
            visionScript = vision.GetComponent<Vision>();
            alertState = new AlertState(this);
            attackState = new AttackState(this);
            chaseState = new ChaseState(this);
            patrolState = new PatrolState(this);
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        void Start()
        {
            DifficultyManager _tmp = (DifficultyManager)Resources.Load("difficulty");
            difficulty = _tmp.CurrentDifficultyLevel();
            enemy = GetComponent<Enemy>();

            // Check Waypoints
            if (OrderWaypoints == TargetSelectType.Numerically || OrderWaypoints == TargetSelectType.Random)
            {
                List<Transform> tmp = new List<Transform>(waypoints);
                for (int i = 0; i < tmp.Count; i++) { if (tmp[i] == null) { tmp.Remove(tmp[i]); } }
                waypoints = tmp.ToArray();
                if (waypoints.Length == 0) { OrderWaypoints = TargetSelectType.ProcedurallyWaypoints; }
            }

            // Start with Patrol State
            currentState = patrolState;

            // Setup Start Attack
            GetNextMelleAttack();
            GetNextRangeAttack();
        }

        void Update()
        {
            // Every game frame we perform the actions of the current state
            currentState.UpdateActions();

            // Damage Multiplier
            if (enemy.type == EnemyType.BaseEnemy) DamageMultiplier = abilityDamageMultiplier * difficulty.damageMultiplier * enemy.PhaseDamageMultiplier;
            if (enemy.type == EnemyType.BossEnemy) DamageMultiplier = abilityDamageMultiplier * difficulty.damageMultiplier * enemy.PhaseDamageMultiplier;
            if (enemy.type == EnemyType.MinionEnemy) DamageMultiplier = abilityDamageMultiplier * difficulty.damageMultiplier * enemy.PhaseDamageMultiplier;

            // Delay Multiplier
            MelleDelayMultiplier = reduceDelayMultiplier * enemy.PhaseReduceDelayMultiplier;
            RangeDelayMultiplier = reduceDelayMultiplier * enemy.PhaseReduceDelayMultiplier;
        }

        void OnTriggerEnter(Collider otherObj)
        {
            // After interacting with another object
            // Call the OnTriggerEnter functions according to the current state
            currentState.OnTriggerEnter(otherObj);
        }

        // Function responsible for catching the hero's shots
        // The hero's shooting position is set to be the last known position of the hero's whereabouts
        void HiddenShot(Vector3 shotPosition)
        {
            // Debug.Log("Who Shoot?");
            lastKnownPosition = shotPosition;
            currentState = alertState;
        }

        public void GetNextMelleAttack()
        {
            if (AttackType != EnemyAttackType.Range)
            {
                if (MelleAttack.Count > 0) ActualMelleAttack = MelleAttack[Random.Range(0, MelleAttack.Count)];
                else { ActualMelleAttack = new EnemyMelleAttackClass(); }
            }
        }
        public void GetNextRangeAttack()
        {
            if (AttackType != EnemyAttackType.Melle)
            {
                if (RangeAttack.Count > 0) ActualRangeAttack = RangeAttack[Random.Range(0, RangeAttack.Count)];
                else { ActualRangeAttack = new EnemyRangeAttackClass(); }
            }
        }
    }

    /// Custom Editor
#if UNITY_EDITOR

    [CustomEditor(typeof(EnemyStates))]
    [CanEditMultipleObjects]
    public class EnemyStatesEditor : Editor
    {
        private bool waypoints = false;
        private bool RangeAttack = false;
        private bool MelleAttack = false;
        public override void OnInspectorGUI()
        {
            EditorUtility.SetDirty(target);
            EnemyStates enemy = (EnemyStates)target;
            EditorGUI.indentLevel = 0;

            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((EnemyStates)target), typeof(EnemyStates), false);
            GUI.enabled = true;

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal("Toolbar", GUILayout.Height(30));
            EditorGUILayout.LabelField($"Patrol Settings", EditorStyles.boldLabel);
            enemy.OrderWaypoints = (TargetSelectType)EditorGUILayout.EnumPopup(enemy.OrderWaypoints, EditorStyles.toolbarDropDown, GUILayout.Width(160));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("patrolRange"), new GUIContent("Patrol Range"));
            if (enemy.OrderWaypoints == TargetSelectType.ProcedurallyWaypoints) EditorGUILayout.PropertyField(serializedObject.FindProperty("ProceduralWaypointRadius"), new GUIContent("Procedural Waypoint Range"));

            if (enemy.OrderWaypoints == TargetSelectType.Numerically || enemy.OrderWaypoints == TargetSelectType.Random) TinyGUI.ShowArray(serializedObject, "waypoints", "Waypoint List", ref waypoints);

            TinyGUI.EditorTitle("Alert Settings");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("stayAlertTime"), new GUIContent("Stay Alert Time"));

            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal("Toolbar", GUILayout.Height(30));
            EditorGUILayout.LabelField($"Attack Settings", EditorStyles.boldLabel);
            enemy.AttackType = (EnemyAttackType)EditorGUILayout.EnumPopup(enemy.AttackType, EditorStyles.toolbarDropDown, GUILayout.Width(160));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("raycastMask"), new GUIContent("Raycast Mask"));

            if (enemy.AttackType != EnemyAttackType.Range)
            {
                TinyGUI.ShowArray(serializedObject, "MelleAttack", "Melle Attack", ref MelleAttack);
            }

            if (enemy.AttackType != EnemyAttackType.Melle)
            {
                TinyGUI.ShowArray(serializedObject, "RangeAttack", "Range Attack", ref RangeAttack);
            }

            //TinyGUI.EditorTitle("Special Attack");
            //TinyGUI.IconBox("d_P4_DeletedLocal", "ttt");

            TinyGUI.EditorTitle("Vision");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("vision"), new GUIContent("Vision Object"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("viewAngle"), new GUIContent("View Angle"));

            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
    }

#endif

    public enum EnemyAttackType
    {
        Melle = 0, Range = 1, MelleAndRange = 2
    }
    public enum TargetSelectType
    {
        Numerically = 0, Random = 1, ProcedurallyWaypoints = 2, PlayerTarget = 3
    }

    [System.Serializable]
    public class EnemyMelleAttackClass
    {
        [Suffix("Metres")] public float attackRange;
        public float meleeDamage;
        [Suffix("Secound")] public float Delay;
    }
    [System.Serializable]
    public class EnemyRangeAttackClass
    {
        [Suffix("Metres")] public float shootRange;
        public GameObject missile;
        public float missileDamage;
        [Suffix("m/s")] public float missileSpeed;
        [Suffix("Secound")] public float Delay;

        // new

        public bool AutoAim = false;
    }
}
