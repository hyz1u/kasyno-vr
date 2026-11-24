using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// Three-reel slot machine. <see cref="TrySpin"/> takes the bet in tokens,
/// spins the reels, stops them one after another and pays out:
/// three of a kind = bet x symbol multiplier, any pair = bet x pairMultiplier.
/// </summary>
public class SlotMachine : MonoBehaviour
{
    [System.Serializable]
    public class Symbol
    {
        public string label = "7";
        public Color color = Color.red;
        [Tooltip("Payout for three of this symbol, as a multiple of the bet.")]
        public int tripleMultiplier = 10;
        [Tooltip("Relative chance of this symbol landing on a reel.")]
        [Min(0.01f)] public float weight = 1f;
    }

    [Header("Reels")]
    [Tooltip("Reel pivots. Each reel spins around its local X axis; the player looks along +Z.")]
    [SerializeField] Transform[] reels;
    [SerializeField] Symbol[] symbols;
    [SerializeField] float reelRadius = 0.12f;
    [SerializeField] float symbolFontSize = 0.7f;

    [Header("Rules")]
    [SerializeField] int bet = 5;
    [SerializeField] int pairMultiplier = 1;
    [Tooltip("Extra chance per spin that all three reels land on the same symbol (symbol still picked by weight).")]
    [Range(0f, 0.5f)] [SerializeField] float tripleChance = 0.03f;

    [Header("Spin feel")]
    [SerializeField] float spinSpeed = 900f;
    [SerializeField] float firstStopTime = 1.0f;
    [SerializeField] float stopInterval = 0.5f;

    [Header("Displays (optional)")]
    [SerializeField] TMP_Text resultLabel;
    [SerializeField] TMP_Text walletLabel;
    [SerializeField] TMP_Text paytableLabel;

    static readonly Color WinColor = new Color(1f, 0.85f, 0.2f);
    static readonly Color LoseColor = new Color(0.85f, 0.85f, 0.85f);
    static readonly Color ErrorColor = new Color(1f, 0.3f, 0.25f);

    float[] progress;   // degrees each reel has turned (grows while spinning)
    int[] results;

    public bool IsSpinning { get; private set; }
    public int Bet => bet;

    public event System.Action SpinStarted;
    /// <summary>Raised when the reels stop: payout amount and whether it was three of a kind.</summary>
    public event System.Action<int, bool> SpinFinished;

    float Step => 360f / symbols.Length;

    void Awake()
    {
        progress = new float[reels.Length];
        results = new int[reels.Length];
        BuildSymbols();
        for (int i = 0; i < reels.Length; i++)
        {
            results[i] = Random.Range(0, symbols.Length);
            progress[i] = results[i] * Step;
            ApplyRotation(i);
        }
        if (paytableLabel != null) paytableLabel.text = PaytableText();
        Show("Zakład " + bet + " żet. - naciśnij SPIN", LoseColor);
    }

    void OnEnable()
    {
        TokenManager.TokensChanged += UpdateWallet;
        UpdateWallet(TokenManager.Tokens);
    }

    void OnDisable() => TokenManager.TokensChanged -= UpdateWallet;

    /// <summary>Spins once if the machine is idle and the player can pay the bet.</summary>
    [ContextMenu("Spin")]
    public bool TrySpin()
    {
        if (IsSpinning) return false;
        if (!TokenManager.TrySpend(bet))
        {
            Show("Za mało żetonów! Kup je w kasie", ErrorColor);
            return false;
        }
        StartCoroutine(SpinRoutine());
        return true;
    }

    IEnumerator SpinRoutine()
    {
        IsSpinning = true;
        Show("Powodzenia...", LoseColor);
        SpinStarted?.Invoke();

        if (Random.value < tripleChance)
        {
            int s = PickSymbol();
            for (int i = 0; i < reels.Length; i++) results[i] = s;
        }
        else
        {
            for (int i = 0; i < reels.Length; i++) results[i] = PickSymbol();
        }

        var settling = new bool[reels.Length];
        var settleStart = new float[reels.Length];
        var settleFrom = new float[reels.Length];
        var settleTo = new float[reels.Length];
        var settleDuration = new float[reels.Length];
        var done = new bool[reels.Length];
        int stopped = 0;
        float t = 0f;

        while (stopped < reels.Length)
        {
            t += Time.deltaTime;
            for (int i = 0; i < reels.Length; i++)
            {
                if (!settling[i])
                {
                    progress[i] += spinSpeed * Time.deltaTime;
                    if (t >= firstStopTime + i * stopInterval)
                    {
                        // Ease out onto the chosen symbol. Distance and duration are picked so the
                        // reel starts decelerating at its current speed (no visible jerk).
                        settling[i] = true;
                        settleStart[i] = t;
                        settleFrom[i] = progress[i];
                        float target = results[i] * Step;
                        float minDistance = 90f;
                        float baseTurns = Mathf.Ceil((progress[i] + minDistance - target) / 360f);
                        settleTo[i] = target + baseTurns * 360f;
                        settleDuration[i] = 3f * (settleTo[i] - settleFrom[i]) / spinSpeed;
                    }
                }
                else if (!done[i])
                {
                    float u = Mathf.Clamp01((t - settleStart[i]) / settleDuration[i]);
                    float eased = 1f - Mathf.Pow(1f - u, 3f);
                    progress[i] = Mathf.Lerp(settleFrom[i], settleTo[i], eased);
                    if (u >= 1f)
                    {
                        progress[i] = settleTo[i] % 360f;
                        done[i] = true;
                        stopped++;
                    }
                }
                ApplyRotation(i);
            }
            yield return null;
        }

        int win = CalculateWin();
        bool triple = results[0] == results[1] && results[1] == results[2];
        if (win > 0 && !triple && win <= bet)
        {
            TokenManager.Add(win);
            Show("Para - zwrot " + win + " żet.", LoseColor);
            win = 0;
        }
        else if (win > 0)
        {
            TokenManager.Add(win);
            Show((triple ? "JACKPOT! " : "WYGRANA ") + win + " żet.!", WinColor);
        }
        else
        {
            Show("Przegrana. Spróbuj jeszcze raz!", LoseColor);
        }
        IsSpinning = false;
        SpinFinished?.Invoke(win, triple);
        if (win > 0) StartCoroutine(BlinkResult(triple ? 10 : 4));
    }

    IEnumerator BlinkResult(int blinks)
    {
        if (resultLabel == null) yield break;
        for (int i = 0; i < blinks * 2 && !IsSpinning; i++)
        {
            resultLabel.color = i % 2 == 0 ? Color.white : WinColor;
            yield return new WaitForSeconds(0.12f);
        }
        if (!IsSpinning) resultLabel.color = WinColor;
    }

    /// <summary>Exact odds for the current symbol weights and multipliers (3 independent reels).</summary>
    [ContextMenu("Log Odds")]
    public void LogOdds()
    {
        float total = 0f;
        foreach (var s in symbols) total += s.weight;
        double rtp = 0, pTriple = 0, pPair = 0, forcedRtp = 0;
        foreach (var s in symbols)
        {
            double p = s.weight / total;
            pTriple += p * p * p;
            rtp += p * p * p * s.tripleMultiplier;
            pPair += 3 * p * p * (1 - p);
            forcedRtp += p * s.tripleMultiplier;
        }
        rtp += pPair * pairMultiplier;
        double b = tripleChance;
        rtp = b * forcedRtp + (1 - b) * rtp;
        pTriple = b + (1 - b) * pTriple;
        pPair *= 1 - b;
        Debug.Log($"[SlotMachine] RTP {rtp:P1} | triple {pTriple:P2} | pair {pPair:P1} | " +
                  $"any payout {pTriple + pPair:P1} | profit chance {(pairMultiplier > 1 ? pTriple + pPair : pTriple):P1}", this);
    }

    int CalculateWin()
    {
        int a = results[0], b = results[1], c = results[2];
        if (a == b && b == c) return bet * symbols[a].tripleMultiplier;
        if (a == b || b == c || a == c) return bet * pairMultiplier;
        return 0;
    }

    int PickSymbol()
    {
        float total = 0f;
        foreach (var s in symbols) total += s.weight;
        float roll = Random.value * total;
        for (int i = 0; i < symbols.Length; i++)
        {
            roll -= symbols[i].weight;
            if (roll <= 0f) return i;
        }
        return symbols.Length - 1;
    }

    // Reels turn so symbols move downward past the window; symbol i faces the player
    // when the reel has turned i * Step degrees.
    void ApplyRotation(int reel) =>
        reels[reel].localRotation = Quaternion.AngleAxis(-progress[reel], Vector3.right);

    /// <summary>(Re)creates the symbol labels around every reel.</summary>
    [ContextMenu("Rebuild Symbols")]
    public void BuildSymbols()
    {
        if (reels == null || symbols == null || symbols.Length == 0) return;
        foreach (var reel in reels)
        {
            var holder = reel.Find("Symbols");
            if (holder != null && holder.childCount == symbols.Length)
            {
                for (int i = 0; i < symbols.Length; i++)
                {
                    var existing = holder.GetChild(i).GetComponent<TextMeshPro>();
                    if (existing != null) StyleSymbol(existing, symbols[i]);
                }
                continue;
            }
            if (holder != null) DestroyImmediateSafe(holder.gameObject);

            holder = new GameObject("Symbols").transform;
            holder.SetParent(reel, false);
            for (int i = 0; i < symbols.Length; i++)
            {
                var rot = Quaternion.AngleAxis(i * Step, Vector3.right);
                var go = new GameObject("Symbol_" + symbols[i].label);
                go.transform.SetParent(holder, false);
                go.transform.localPosition = rot * new Vector3(0f, 0f, -(reelRadius + 0.003f));
                go.transform.localRotation = rot;
                StyleSymbol(go.AddComponent<TextMeshPro>(), symbols[i]);
            }
        }
    }

    // Auto-size keeps wide labels like "BAR" inside the drum instead of spilling onto the next reel.
    void StyleSymbol(TextMeshPro text, Symbol symbol)
    {
        text.text = symbol.label;
        text.color = symbol.color;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.enableAutoSizing = true;
        text.fontSizeMin = symbolFontSize * 0.3f;
        text.fontSizeMax = symbolFontSize;
        text.rectTransform.sizeDelta = new Vector2(0.1f, 0.08f);
    }

    string PaytableText()
    {
        var sb = new StringBuilder();
        foreach (var s in symbols)
            sb.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(s.color)).Append(">")
              .Append(s.label).Append(' ').Append(s.label).Append(' ').Append(s.label)
              .Append("</color>  x").Append(s.tripleMultiplier).Append('\n');
        if (pairMultiplier == 1) sb.Append("dowolna para  zwrot");
        else sb.Append("dowolna para  x").Append(pairMultiplier);
        return sb.ToString();
    }

    void Show(string message, Color color)
    {
        if (resultLabel == null) return;
        resultLabel.text = message;
        resultLabel.color = color;
    }

    void UpdateWallet(int tokens)
    {
        if (walletLabel != null) walletLabel.text = "Żetony: " + tokens;
    }

    static void DestroyImmediateSafe(Object o)
    {
        if (Application.isPlaying) Destroy(o);
        else DestroyImmediate(o);
    }
}
