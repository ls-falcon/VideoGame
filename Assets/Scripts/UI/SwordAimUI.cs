using System.Collections.Generic;
using MagicPigGames;
using UnityEngine;
using UnityEngine.UI;

public class SwordAimUI : MonoBehaviour
{
    [SerializeField] private LineRenderer trajectoryLine;
    [SerializeField] private Transform crosshair;
    [SerializeField] private ProgressBar chargeBar;
    [SerializeField] private Graphic chargeFillGraphic;
    [SerializeField] private Gradient chargeColorGradient;
    [SerializeField] private int trajectoryPoints = 12;
    [SerializeField] private float trajectoryStep = 0.08f;
    [SerializeField] private float distanceBeforeTarget = 0.5f;
    [SerializeField] private float distanceAfterPlayer = 0.6f;
    [SerializeField] private float dotScrollSpeed = 1.5f;
    [SerializeField] private float dotTiling = 4f;
    [SerializeField] private int dotPixels = 2;
    [SerializeField] private int gapPixels = 3;
    [SerializeField] private Color trajectoryStartColor = Color.white;
    [SerializeField] private Color trajectoryEndColor = Color.white;
    [SerializeField] private int lineSortingOrder = 10;
    [SerializeField] private int crosshairSortingOrder = 20;
    [SerializeField] private bool snapCrosshairToPixelGrid = true;
    [SerializeField] private float crosshairPixelsPerUnit = 100f;
    [SerializeField] private float crosshairZOffset = -0.1f;

    private bool previousCursorVisible = true;
    private Material trajectoryMaterial;
    private float dotOffset = 0f;

    private void Reset()
    {
        chargeColorGradient = new Gradient();
        chargeColorGradient.SetKeys(
            new[]
            {
                new GradientColorKey(Color.yellow, 0f),
                new GradientColorKey(new Color(1f, 0.45f, 0f), 0.6f),
                new GradientColorKey(Color.red, 1f)
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            }
        );
    }

    private void OnValidate()
    {
        dotPixels = Mathf.Max(dotPixels, 1);
        gapPixels = Mathf.Max(gapPixels, 1);
        dotTiling = Mathf.Max(dotTiling, 0.1f);

        if (trajectoryLine != null)
        {
            trajectoryMaterial = null;
            SetupDottedLine();
            ApplyTrajectoryColors();
        }
    }

    private void Awake()
    {
        SetupReferences();
        Show(false);
    }

    public void Show(bool visible)
    {
        SetupReferences();

        if (visible)
        {
            previousCursorVisible = Cursor.visible;
            Cursor.visible = false;
        }
        else
        {
            Cursor.visible = previousCursorVisible;
        }

        gameObject.SetActive(visible);

        if (!visible)
        {
            SetCharge(0f);
        }
    }

    public void UpdateAim(Vector2 startPosition, Vector2 targetPosition, float throwForce, float gravityScale, float charge)
    {
        SetupReferences();

        Vector2 direction = (targetPosition - startPosition).normalized;
        transform.position = startPosition;

        if (crosshair != null)
        {
            crosshair.position = GetCrosshairPosition(targetPosition);
            crosshair.rotation = Quaternion.identity;
        }

        dotOffset -= dotScrollSpeed * Time.deltaTime;
        if (trajectoryMaterial != null)
        {
            trajectoryMaterial.mainTextureOffset = new Vector2(dotOffset, 0f);
        }

        SetCharge(charge);
        DrawTrajectory(startPosition, targetPosition, direction, throwForce, gravityScale);
    }

    public void SetCharge(float charge)
    {
        if (chargeBar != null)
        {
            float chargeValue = Mathf.Clamp01(charge);
            chargeBar.SetProgress(chargeValue);
            ApplyChargeColor(chargeValue);
        }
    }

    private void DrawTrajectory(
        Vector2 startPosition,
        Vector2 targetPosition,
        Vector2 direction,
        float throwForce,
        float gravityScale
    )
    {
        if (trajectoryLine == null) return;

        Vector2 velocity = direction * throwForce;
        Vector2 gravity = Physics2D.gravity * gravityScale;
        int points = Mathf.Max(trajectoryPoints, 2);
        float maxDistance = Mathf.Max(Vector2.Distance(startPosition, targetPosition) - distanceBeforeTarget, 0.1f);
        float minVisibleDistance = Mathf.Min(distanceAfterPlayer, maxDistance * 0.8f);
        List<Vector3> trajectoryPositions = new List<Vector3>();

        for (int i = 0; i < points; i++)
        {
            float time = i * trajectoryStep;
            Vector2 point = startPosition + velocity * time + 0.5f * gravity * time * time;
            float distanceFromStart = Vector2.Distance(startPosition, point);

            if (i > 0 && distanceFromStart > maxDistance)
            {
                break;
            }

            if (distanceFromStart < minVisibleDistance)
            {
                continue;
            }

            trajectoryPositions.Add(point);
        }

        if (trajectoryPositions.Count < 2)
        {
            trajectoryPositions.Add(startPosition + direction * minVisibleDistance);
            trajectoryPositions.Add(startPosition + direction * maxDistance);
        }

        trajectoryLine.positionCount = trajectoryPositions.Count;
        trajectoryLine.SetPositions(trajectoryPositions.ToArray());
    }

    private void SetupReferences()
    {
        if (trajectoryLine == null)
        {
            trajectoryLine = GetComponent<LineRenderer>();
        }

        SetupDottedLine();

        if (crosshair == null)
        {
            Transform crosshairTransform = transform.Find("Crosshair");
            crosshair = crosshairTransform;
        }

        if (chargeBar == null)
        {
            chargeBar = GetComponentInChildren<ProgressBar>(true);
        }

        if (chargeFillGraphic == null && chargeBar != null)
        {
            chargeFillGraphic = FindChargeFillGraphic(chargeBar.transform);
        }

        SetupSorting();
    }

    private Graphic FindChargeFillGraphic(Transform root)
    {
        foreach (Graphic graphic in root.GetComponentsInChildren<Graphic>(true))
        {
            if (graphic.name == "Fill Image")
            {
                return graphic;
            }
        }

        return root.GetComponentInChildren<Graphic>(true);
    }

    private void SetupDottedLine()
    {
        if (trajectoryLine == null || trajectoryMaterial != null) return;

        int textureWidth = Mathf.Max(dotPixels + gapPixels, 2);
        Texture2D dottedTexture = new Texture2D(textureWidth, 1);
        dottedTexture.wrapMode = TextureWrapMode.Repeat;
        dottedTexture.filterMode = FilterMode.Point;

        for (int x = 0; x < dottedTexture.width; x++)
        {
            Color pixelColor = x < dotPixels ? Color.white : Color.clear;
            dottedTexture.SetPixel(x, 0, pixelColor);
        }
        dottedTexture.Apply();

        trajectoryMaterial = new Material(Shader.Find("Sprites/Default"));
        trajectoryMaterial.mainTexture = dottedTexture;
        trajectoryMaterial.mainTextureScale = new Vector2(dotTiling, 1f);

        trajectoryLine.material = trajectoryMaterial;
        trajectoryLine.textureMode = LineTextureMode.Tile;
        trajectoryLine.useWorldSpace = true;
        trajectoryLine.sortingOrder = lineSortingOrder;
        ApplyTrajectoryColors();
    }

    private void SetupSorting()
    {
        if (trajectoryLine != null)
        {
            trajectoryLine.useWorldSpace = true;
            trajectoryLine.sortingOrder = lineSortingOrder;
        }

        if (crosshair != null && crosshair.TryGetComponent(out SpriteRenderer crosshairRenderer))
        {
            crosshairRenderer.sortingOrder = crosshairSortingOrder;
        }
    }

    private void ApplyTrajectoryColors()
    {
        if (trajectoryLine == null) return;

        trajectoryLine.startColor = trajectoryStartColor;
        trajectoryLine.endColor = trajectoryEndColor;
    }

    private void ApplyChargeColor(float charge)
    {
        if (chargeFillGraphic == null || chargeColorGradient == null) return;

        chargeFillGraphic.color = chargeColorGradient.Evaluate(charge);
    }

    private Vector3 GetCrosshairPosition(Vector2 targetPosition)
    {
        if (!snapCrosshairToPixelGrid)
        {
            return new Vector3(targetPosition.x, targetPosition.y, crosshairZOffset);
        }

        float unitsPerPixel = 1f / Mathf.Max(crosshairPixelsPerUnit, 1f);
        float snappedX = Mathf.Round(targetPosition.x / unitsPerPixel) * unitsPerPixel;
        float snappedY = Mathf.Round(targetPosition.y / unitsPerPixel) * unitsPerPixel;

        return new Vector3(snappedX, snappedY, crosshairZOffset);
    }
}
