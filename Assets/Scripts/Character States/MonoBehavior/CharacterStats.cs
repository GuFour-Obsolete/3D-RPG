using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
  public event Action<int, int> UpdateHealthBarOnAttack;
  public CharactorData_SO templateData;
  public CharactorData_SO characterData;

  public AttackData_SO attackData;

  [HideInInspector]
  public bool isCritical;

  private void Awake()
  {
    if (templateData != null)
    {
      characterData = Instantiate(templateData);
    }
  }

  #region Read from Data_SO

  public int MaxHealth
  {
    get { if (characterData != null) return characterData.maxHealth; else return 0; }
    set { characterData.maxHealth = value; }
  }

  public int CurrentHealth
  {
    get { if (characterData != null) return characterData.currentHealth; else return 0; }
    set { characterData.currentHealth = value; }
  }

  public int BaseDefence
  {
    get { if (characterData != null) return characterData.baseDefence; else return 0; }
    set { characterData.baseDefence = value; }
  }

  public int CurrentDefence
  {
    get { if (characterData != null) return characterData.currentDefence; else return 0; }
    set { characterData.currentDefence = value; }
  }

  #endregion Read from Data_SO

  #region Character Combat

  /// <summary>
  /// 造成伤害
  /// </summary>
  /// <param name="attacker">攻击者</param>
  /// <param name="defener">被攻击者</param>
  public void TakeDamage(CharacterStats attacker, CharacterStats defener)
  {
    int damage = Mathf.Max(attacker.CurrentDamage() - defener.CurrentDefence, 0);
    CurrentHealth = Mathf.Max(CurrentHealth - damage, 0);

    if (attacker.isCritical)
    {
      defener.GetComponent<Animator>().SetTrigger("Hit");
    }
    //TODO:Update UI
    UpdateHealthBarOnAttack?.Invoke(CurrentHealth, MaxHealth);
    //TODO:经验Update
    if (CurrentHealth <= 0)
    {
      attacker.characterData.UpdateExp(characterData.killPoint);
    }
  }

  public void TakeDamage(int damage, CharacterStats defener)
  {
    int currentDamage = Mathf.Max(damage - defener.CurrentDefence, 0);
    CurrentHealth = Mathf.Max(CurrentHealth - currentDamage, 0);
    UpdateHealthBarOnAttack?.Invoke(CurrentHealth, MaxHealth);
    GameManager.Instance.playerStats.characterData.UpdateExp(characterData.killPoint);
  }

  private int CurrentDamage()
  {
    float coreDamage = UnityEngine.Random.Range(attackData.minDamge, attackData.maxDamage);
    if (isCritical)
    {
      coreDamage *= attackData.criticalMultiplier;
    }
    return (int)coreDamage;
  }

  #endregion Character Combat
}