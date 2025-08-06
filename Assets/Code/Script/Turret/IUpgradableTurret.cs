using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUpgradableTurret
{
    int GetLevel();
    float CalculateBPS(int level);
    float CalculateRange(int level);
    int   CalculateBulletDamage(int level);
}

