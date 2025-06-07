/*  
    Hellish Battle - 2.5D Retro FPS
    Added in Version: 1.0.0a
    Updated in Version: 1.0.0a
*/

using System;
using System.Collections;
using UnityEngine.Scripting;
using UnityEngine;

namespace HellishBattle.AI
{
    public interface IEnemyAI
    {

        void UpdateActions();

        void OnTriggerEnter(Collider enemy);

        void ToPatrolState();

        void ToAttackState();

        void ToAlertState();

        void ToChaseState();

    }
}