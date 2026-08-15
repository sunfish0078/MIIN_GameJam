using ChoiJeongYun.Scripts.Enemy;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class DevCheatListener : MonoBehaviour
{
    private const string OneHitCode = "sunfish";
    private const string CowardCode = "miin";
    private static readonly int MaxCodeLength = Mathf.Max(OneHitCode.Length, CowardCode.Length);

    private string typed = "";

    [SerializeField] private AudioClip devMod;
    [SerializeField] private AudioClip miinMod;

    private void Update()
    {
        if (DevMode.OneHitKillMonsters && DevMode.CowardMode) return;
        if (Keyboard.current == null) return;

        foreach (KeyControl key in Keyboard.current.allKeys)
        {
            if (!key.wasPressedThisFrame) continue;

            char c = KeyToChar(key.keyCode);
            if (c == '\0') continue;

            typed += c;
            if (typed.Length > MaxCodeLength)
                typed = typed.Substring(typed.Length - MaxCodeLength);

            if (!DevMode.OneHitKillMonsters && typed.EndsWith(OneHitCode))
            {
                DevMode.Enable();
                typed = "";

                if (SoundManager.Instance != null)
                    SoundManager.Instance.PlaySFX(devMod);

                break;
            }

            if (!DevMode.CowardMode && typed.EndsWith(CowardCode))
            {
                DevMode.EnableCowardMode();
                typed = "";

                if (SoundManager.Instance != null)
                    SoundManager.Instance.PlaySFX(miinMod);

                break;
            }
        }
    }

    private char KeyToChar(Key key)
    {
        if (key >= Key.A && key <= Key.Z)
            return (char)('a' + (key - Key.A));

        return '\0';
    }
}
