using System;
using UnityEngine;
[Serializable]
public struct PlayerInputFrame
{
    public Vector2 Move;
    public Vector2 Aim;
    public bool Attack;
    public bool Dodge;
    public bool Swap;
    public int Skill; // 0 = none, 1..3 = active-bank slot.
}
