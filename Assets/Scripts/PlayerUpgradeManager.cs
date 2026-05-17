using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerUpgradeManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private playerHealth playerHealth;
    [SerializeField] private playerMovementController movement;
    [SerializeField] private UpgradeBannerUI bannerUI;


    private List<PlayerUpgradeType> unlockedUpgrades =
        new List<PlayerUpgradeType>();

    private bool canUpgrade = true;


    private PlayerInputSystem input;

    private void Start()
    {
        if (movement == null)
        {
            Debug.LogError("Movement reference missing");
            enabled = false;
            return;
        }

        input = movement.Input;

        input.Player.KeyboardUpgrade.performed += OnUpgradePressed;
    }

    private void OnDestroy()
    {
        if (input != null)
        {
            input.Player.KeyboardUpgrade.performed -= OnUpgradePressed;
        }
    }

    void OnUpgradePressed(InputAction.CallbackContext ctx)
    {
        Debug.Log("TECLA C PRESIONADA");
        TryBuyUpgrade();
    }

    void TryBuyUpgrade()
    {
        if (!canUpgrade) return;
        // Si tiene 1 vida -> muere
        if (playerHealth.CurrentHearts <= 1)
        {
            canUpgrade = false;
            playerHealth.TakeDirectDamage(1);
            return;
        }

        List<PlayerUpgradeType> available =
            GetAvailableUpgrades();

        if (available.Count == 0)
        {
            bannerUI.ShowMessage("Sin mejoras desbloqueadas");
            return;
        }

        // Pierde vida
        playerHealth.TakeDirectDamage(1);

        // Elegir upgrade aleatorio
        PlayerUpgradeType selected =
            available[Random.Range(0, available.Count)];

        unlockedUpgrades.Add(selected);

        ApplyUpgrade(selected);

        bannerUI.ShowMessage(
            "Mejora otorgada: " + GetUpgradeName(selected)
        );
        StartCoroutine(UpgradeCooldown());
    }

    System.Collections.IEnumerator UpgradeCooldown()
    {
        canUpgrade = false;

        yield return new WaitForSeconds(0.5f);

        canUpgrade = true;
    }

    List<PlayerUpgradeType> GetAvailableUpgrades()
    {
        List<PlayerUpgradeType> upgrades =
            new List<PlayerUpgradeType>();

        AddIfNotOwned(upgrades, PlayerUpgradeType.FasterMeleeAttack);
        AddIfNotOwned(upgrades, PlayerUpgradeType.StrongerMeleeAttack);

        AddIfNotOwned(upgrades, PlayerUpgradeType.FasterSwordAttack);
        AddIfNotOwned(upgrades, PlayerUpgradeType.StrongerSwordAttack);

        AddIfNotOwned(upgrades, PlayerUpgradeType.DoubleJump);

        AddIfNotOwned(upgrades, PlayerUpgradeType.MoveSpeedUp);
        AddIfNotOwned(upgrades, PlayerUpgradeType.FasterSwordThrow);

        // Exclusivos tamaño
        bool hasBig =
            unlockedUpgrades.Contains(PlayerUpgradeType.BiggerSize);

        bool hasSmall =
            unlockedUpgrades.Contains(PlayerUpgradeType.SmallerSize);

        if (!hasBig && !hasSmall)
        {
            upgrades.Add(PlayerUpgradeType.BiggerSize);
            upgrades.Add(PlayerUpgradeType.SmallerSize);
        }

        return upgrades;
    }

    void AddIfNotOwned(
        List<PlayerUpgradeType> list,
        PlayerUpgradeType type
    )
    {
        if (!unlockedUpgrades.Contains(type))
        {
            list.Add(type);
        }
    }

    void ApplyUpgrade(PlayerUpgradeType upgrade)
    {
        switch (upgrade)
        {
            case PlayerUpgradeType.MoveSpeedUp:

                movement.AddMoveSpeed(2f);

                break;

            case PlayerUpgradeType.DoubleJump:

                movement.EnableDoubleJump();

                
                break;

            case PlayerUpgradeType.BiggerSize:

                movement.transform.localScale *= 1.3f;

                break;

            case PlayerUpgradeType.SmallerSize:

                movement.transform.localScale *= 0.7f;

                break;

            case PlayerUpgradeType.FasterMeleeAttack:

                movement.MeleeAttackSpeedMultiplier *= 1.35f;

                break;

            case PlayerUpgradeType.StrongerMeleeAttack:

                movement.MeleeDamage += 1;

                break;

            case PlayerUpgradeType.FasterSwordAttack:

                movement.SwordAttackSpeedMultiplier *= 1.35f;

                break;

            case PlayerUpgradeType.StrongerSwordAttack:

                movement.SwordDamage += 1;

                break;

            case PlayerUpgradeType.FasterSwordThrow:

                movement.SwordThrowForceMultiplier *= 1.4f;

                break;
        }
    }

    string GetUpgradeName(PlayerUpgradeType type)
    {
        switch (type)
        {
            case PlayerUpgradeType.FasterMeleeAttack:
                return "Velocidad ataque cuerpo a cuerpo";

            case PlayerUpgradeType.StrongerMeleeAttack:
                return "Daño cuerpo a cuerpo";

            case PlayerUpgradeType.FasterSwordAttack:
                return "Velocidad ataque espada";

            case PlayerUpgradeType.StrongerSwordAttack:
                return "Daño espada";

            case PlayerUpgradeType.DoubleJump:
                return "Doble salto";

            case PlayerUpgradeType.MoveSpeedUp:
                return "Velocidad movimiento";

            case PlayerUpgradeType.BiggerSize:
                return "Aumento tamaño";

            case PlayerUpgradeType.SmallerSize:
                return "Disminución tamaño";

            case PlayerUpgradeType.FasterSwordThrow:
                return "Lanzamiento espada";
        }

        return "???";
    }
}