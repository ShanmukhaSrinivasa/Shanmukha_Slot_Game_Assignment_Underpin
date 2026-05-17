using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Game Data")]
    public List<SlotSymbol> availableSymbols;
    public int currentbalance = 1000;
    public int currentBet = 10;

    [Header("Betting UI")]
    public TextMeshProUGUI balanceText;
    public TextMeshProUGUI currentBetText;
    public GameObject bettingPanel;

    private bool isSpinning = false; 

    [Header("Reels")]
    public ReelController[] reels; // Array to hold all 3 reels

    [Header("Lever Animation")]
    public Image leverImage; // The UI image component of the lever
    public Sprite LeverUpSprite;
    public Sprite leverDownSprite;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip leverSound;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip loseSound;

    [Header("VFX")]
    [SerializeField] ParticleSystem coinFountain;

    private void Start()
    {
        UpdateUI();
    }

    // This is the method buttons will call
    public void SetBetAmount(int amount)
    {
        if(isSpinning)
        {
            return;
        }

        currentBet = amount;
        UpdateUI();
        Debug.Log("Bet set to: " + currentBet);

        if(bettingPanel != null )
        {
            bettingPanel.SetActive(false);
        }

        PullLever();
    }

    private void UpdateUI()
    {
        if(balanceText != null)
        {
            balanceText.text = currentbalance.ToString();
        }

        if(currentBetText != null)
        {
            currentBetText.text = currentBet.ToString();
        }
    }

    public void PullLever()
    {
        if(isSpinning || currentbalance < currentBet)
        {
            if(currentbalance < currentBet)
            {
                Debug.Log("Not enough balance!");
                return;
            }
        }

        isSpinning = true;

        StartCoroutine(AnimateLever());

        // Deduct the bet and update the UI
        currentbalance -= currentBet;

        UpdateUI();

        // 1. Math Setup
        int totalSymbols = reels[0].symbols.Length;
        int[] finalResults = new int[3];

        // 2. RNG Win/Loss Logic (30% chance to win for this test)
        bool isWinner = Random.value <= 0.3f;
        int winningIndex = -1; // store this to use later for the payout

        if (isWinner)
        {
            // Pick ONE random symbol index and apply it to all 3 reels
            winningIndex = Random.Range(0, availableSymbols.Count);
            finalResults[0] = winningIndex;
            finalResults[1] = winningIndex;
            finalResults[2] = winningIndex;
            Debug.Log("RNG Rolled a WIN! Target Index: " + winningIndex);
        }
        else
        {
            // pick random indexes, using a do-while loop to guarantee they don't match
            do
            {
                finalResults[0] = Random.Range(0, availableSymbols.Count);
                finalResults[1] = Random.Range(0, availableSymbols.Count);
                finalResults[2] = Random.Range(0, availableSymbols.Count);
            }
            while (finalResults[0] == finalResults[1] && finalResults[1] == finalResults[2]);
            Debug.Log("RNG Rolled a LOSS.");
        }

        // 3. Trigger the Reels with Staggered Stop Times!
        reels[0].SpinToTarget(finalResults[0], 0f);     // Reel 1 stops first
        reels[1].SpinToTarget(finalResults[1], 1.0f);   // Reel 2 stops 1.0s later
        reels[2].SpinToTarget(finalResults[2], 2.0f);   // Reel 3 stops 2.0s later

        // 4. Wait for reels to stop before paying out
        StartCoroutine(HandlePayoutSequence(isWinner, winningIndex));
    }

    private IEnumerator HandlePayoutSequence(bool isWinner, int winningIndex)
    {
        // Wait for the longest reel to finish spinnning
        // baseSpinDuration (2f) + extraSpinTime(2.0f) + snap buffer (~1.0f) = 5 seconds
        yield return new WaitForSeconds(5f);

        if(audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

        if(isWinner && winningIndex != -1)
        {
            // Play the win Sound
            if(audioSource != null && winSound != null)
            {
                audioSource.PlayOneShot(winSound);
            }

            // Retrieve the data from the winning scriptableObject
            SlotSymbol winningSymbol = availableSymbols[winningIndex];

            // Calculate payout based on the bet and the symbol's multiplier
            int winAmount = currentBet * winningSymbol.payoutMultiplier;

            // Save the old balance, calculate the new one, and start the ticking animation
            int oldBalance = currentbalance;
            currentbalance += winAmount;

            // Trigger the coin FOuntain
            if(coinFountain != null)
            {
                coinFountain.Play();
            }

            // Start the visual ticker
            StartCoroutine(AnimateBalanceCounter(oldBalance, currentbalance));

            Debug.Log($"Winner! Payout: {winAmount}G. New Balance: {currentbalance}G");
        }
        else
        {
            // Play the lose Sound
            if (audioSource != null && loseSound != null)
            {
                audioSource.PlayOneShot(loseSound);
            }

            UpdateUI();
        }

        isSpinning = false;

        if (bettingPanel != null)
        {
            bettingPanel.SetActive(true);
        }
    }

    private IEnumerator AnimateBalanceCounter(int startAmount, int targetAmount)
    {
        float animationDuration = 2.0f;
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;

            // calculate the current number to display based on time
            float t = elapsedTime / animationDuration;
            int currentDisplayAmount = Mathf.RoundToInt(Mathf.Lerp(startAmount, targetAmount, t));

            if(balanceText != null)
            {
                balanceText.text = currentDisplayAmount.ToString();

            }
            yield return null;
        }

        if(balanceText != null)
        {
            balanceText.text = targetAmount.ToString(); ;
        }
    }

    private IEnumerator AnimateLever()
    {
        if (audioSource != null && leverSound != null)
        {
            audioSource.PlayOneShot(leverSound);
        }

        // swap to the pulled down image
        if (leverImage != null)
        {
            leverImage.sprite = leverDownSprite;
        }

        // wait for a fraction of a second sp the player actually sees it go down
        yield return new WaitForSeconds(0.2f);

        // Snap it back up

        if(leverImage != null && LeverUpSprite != null)
        {
            leverImage.sprite = LeverUpSprite;
        }
    }

    // A simple method for the exit button
    public void ExitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}
