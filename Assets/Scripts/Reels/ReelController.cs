using System.Collections;
using UnityEngine;

public class ReelController : MonoBehaviour
{
    [Header("Reel References")]
    [Tooltip("Drag the symbol Image RectTransforms here in top-to-bottom order")]
    public RectTransform[] symbols;

    [Header("Configurations")]
    public float symbolHeight = 168.4211f;
    public float spinSpeed = 1500f;
    public float baseSpinDuration = 2f;

    private bool isSpinning = false;
    private int totalSymbols;

    private void Start()
    {
        totalSymbols = symbols.Length;
    }

    // The GameManager will call this, passing the atrget symbol index and a delay so they don't all stop at once
    public void SpinToTarget(int targetIndex, float extraSpinTime)
    {
        StartCoroutine(SpinAndStop(targetIndex, extraSpinTime));
    }

    private IEnumerator SpinAndStop(int targetIndex, float extraSpinTime)
    {
        float elapsedTime = 0f;
        float totalSpinTime = baseSpinDuration + extraSpinTime;

        // Phase 1: Full Speed Spin
        while(elapsedTime  < totalSpinTime)
        {
            MoveSymbolsDown(Time.deltaTime * spinSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Phase 2: Slow down and wait for the target symbols to approach the center
        // We wait untill the target is slightly above the center (Y=0)
        while (symbols[targetIndex].anchoredPosition.y < 0 || symbols[targetIndex].anchoredPosition.y > symbolHeight * 2.0f)
        {
            MoveSymbolsDown(Time.deltaTime * (spinSpeed * 0.5f));
            yield return null;
        }

        // Phase 3: Smooth Snap(Lerping exactly to Y = 0)
        float snapDuration = 0.15f;
        float snapTimer = 0f;
        Vector2 startPos = symbols[targetIndex].anchoredPosition;

        while(snapTimer < snapDuration)
        {
            snapTimer += Time.deltaTime;
            float t = snapTimer / snapDuration;

            // Calculate how much to move this exact frame to reach 0 smoothly
            float currentY = Mathf.Lerp(startPos.y, 0, t);
            float difference = symbols[targetIndex].anchoredPosition.y - currentY;

            MoveSymbolsDown(difference);
            yield return null;
        }

        // Guarantee bsolute pixel-perfect zero at the very end
        float finalOffset = symbols[targetIndex].anchoredPosition.y;
        MoveSymbolsDown(finalOffset);
    }

    private void MoveSymbolsDown(float distance)
    {
        for(int i=0; i< totalSymbols; i++)
        {
            // Move the symbols down
            symbols[i].anchoredPosition -= new Vector2(0, distance);

            // Wrapping Logic: If the Symbol goes below the mask's bottom threshold
            // Teleport it to the top of the Column

            if (symbols[i].anchoredPosition.y < -symbolHeight * 2.5f)
            {
                Vector2 newPos = symbols[i].anchoredPosition;

                // Shift it exactly up by the total height of all symbols combined\newPos.y
                newPos.y += symbolHeight * totalSymbols;
                symbols[i].anchoredPosition = newPos;
            }
        }
    }
}
