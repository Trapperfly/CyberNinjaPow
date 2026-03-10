// Place this file in any folder named "Editor" inside your Assets directory.
// e.g. Assets/Editor/CardEditor.cs
//
// GRID CELL SYNTAX (comma-separate multiple tokens per cell):
//   d{n}        — damage, e.g. "d5" = 5 damage
//   pN/pS/pE/pW — push in direction, e.g. "pN" = push North
//   p{n}N etc.  — push distance + direction, e.g. "p2N" = push 2 North
//   proj        — is a projectile (uses card's default projectile direction)
//   projN/S/E/W — projectile with explicit direction, e.g. "projN"
//   pierce{n}   — projectile pierces n targets (use with proj)
//   r{n}        — repeat n times, e.g. "r3"
//   ri{f}       — repeat interval in seconds, e.g. "ri0.5"
//   burn{n}     — burn stacks, e.g. "burn2"
//   psn{n}      — poison stacks
//   stun        — stun 1 stack
//   slow        — slow 1 stack
//   weak        — weaken 1 stack
//   vuln        — vulnerable 1 stack
//   shld{n}     — shield stacks
//   regen{n}    — regen stacks
//
// EXAMPLES:
//   "d5,pN"         — 5 damage and push North
//   "d3,burn2,r2"   — 3 damage, burn 2 stacks, repeat twice
//   "projN,stop,d4" — northward projectile that stops on first hit for 4 damage
//   "X" or "x"      — marks the player/origin tile (no effect generated)
//   (empty)         — no effect on this tile

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class CardEditorWindow : EditorWindow
{
    private const int GridSize = 9;

    private Card _selectedCard;
    private string[,] _grid = new string[GridSize, GridSize];
    private Vector2 _scrollPos;

    // Style caches
    private GUIStyle _cellStyle;
    private GUIStyle _originStyle;
    private GUIStyle _activeStyle;
    private bool _stylesInitialised;

    [MenuItem("Tools/Card Grid Editor")]
    public static void Open()
    {
        var window = GetWindow<CardEditorWindow>("Card Grid Editor");
        window.minSize = new Vector2(520, 620);
    }

    private void OnEnable()
    {
        ClearGrid();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GUI
    // ─────────────────────────────────────────────────────────────────────────

    private void OnGUI()
    {
        InitStyles();

        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

        DrawHeader();
        EditorGUILayout.Space(8);

        if (_selectedCard != null)
        {
            DrawGrid();
            EditorGUILayout.Space(10);
            DrawActionButtons();
            EditorGUILayout.Space(10);
            DrawSyntaxReference();
        }
        else
        {
            EditorGUILayout.HelpBox("Select a Card scriptable object above to begin editing.", MessageType.Info);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawHeader()
    {
        EditorGUILayout.LabelField("Card Grid Editor", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        _selectedCard = (Card)EditorGUILayout.ObjectField("Card Asset", _selectedCard, typeof(Card), false);
        if (EditorGUI.EndChangeCheck() && _selectedCard != null)
            LoadFromCard();
    }

    private void DrawGrid()
    {
        int center = GridSize / 2;
        float cellWidth  = (EditorGUIUtility.currentViewWidth - 32f) / GridSize;
        float cellHeight = 36f;

        EditorGUILayout.LabelField("Grid (centre = player origin)", EditorStyles.miniLabel);

        for (int row = 0; row < GridSize; row++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int col = 0; col < GridSize; col++)
            {
                bool isOrigin = (row == center && col == center);
                bool hasContent = !string.IsNullOrWhiteSpace(_grid[row, col]) &&
                                  !_grid[row, col].Equals("X", StringComparison.OrdinalIgnoreCase);

                GUIStyle style = isOrigin ? _originStyle : (hasContent ? _activeStyle : _cellStyle);
                _grid[row, col] = EditorGUILayout.TextField(_grid[row, col] ?? "", style,
                                      GUILayout.Width(cellWidth), GUILayout.Height(cellHeight));
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawActionButtons()
    {
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("⬆  Parse Grid → Card", GUILayout.Height(30)))
            ParseGridToCard();

        if (GUILayout.Button("⬇  Load Card → Grid", GUILayout.Height(30)))
            LoadFromCard();

        if (GUILayout.Button("✕  Clear Grid", GUILayout.Height(30)))
            ClearGrid();

        EditorGUILayout.EndHorizontal();
    }

    private void DrawSyntaxReference()
    {
        EditorGUILayout.LabelField("Syntax Reference", EditorStyles.boldLabel);
        string syntax =
            "d{n}         damage (e.g. d5)\n" +
            "p{n}N/S/E/W  push distance + dir (e.g. p2N, pW)\n" +
            "proj / projN/S/E/W  projectile + optional dir\n" +
            "pierce{n}    projectile does not stop n times \n" +
            "r{n}         repeat n times (e.g. r3)\n" +
            "ri{f}        repeat interval seconds (e.g. ri0.5)\n" +
            "burn{n}  psn{n}  stun  slow  weak  vuln  shld{n}  regen{n}\n" +
            "X            origin / player tile (ignored)";
        EditorGUILayout.HelpBox(syntax, MessageType.None);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Parse grid → Card
    // ─────────────────────────────────────────────────────────────────────────

    private void ParseGridToCard()
    {
        if (_selectedCard == null) return;

        Undo.RecordObject(_selectedCard, "Parse Card Grid");

        _selectedCard.tileEffects.Clear();
        int center = GridSize / 2;

        for (int row = 0; row < GridSize; row++)
        {
            for (int col = 0; col < GridSize; col++)
            {
                string cell = (_grid[row, col] ?? "").Trim();
                if (string.IsNullOrWhiteSpace(cell)) continue;
                if (cell.Equals("X", StringComparison.OrdinalIgnoreCase)) continue;

                // Grid position relative to centre (positive Y = North)
                var gridPos = new Vector2Int(col - center, center - row);
                TileEffect effect = ParseCell(cell, gridPos);
                _selectedCard.tileEffects.Add(effect);
            }
        }

        EditorUtility.SetDirty(_selectedCard);
        AssetDatabase.SaveAssets();
        Debug.Log($"[CardEditor] Parsed {_selectedCard.tileEffects.Count} tile effects onto '{_selectedCard.cardName}'.");
    }

    private TileEffect ParseCell(string cell, Vector2Int gridPos)
    {
        var effect = new TileEffect { gridPosition = gridPos };
        string[] tokens = cell.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string raw in tokens)
        {
            string t = raw.Trim().ToLower();

            // Damage: d{n}
            var dmgMatch = Regex.Match(t, @"^d(\d+)$");
            if (dmgMatch.Success) { effect.damage = int.Parse(dmgMatch.Groups[1].Value); continue; }

            // Push: p{n?}N/S/E/W
            var pushMatch = Regex.Match(t, @"^p(\d*)([nsew])$");
            if (pushMatch.Success)
            {
                effect.pushDistance = pushMatch.Groups[1].Value == "" ? 1 : int.Parse(pushMatch.Groups[1].Value);
                effect.pushDirection = ParseDirection(pushMatch.Groups[2].Value);
                continue;
            }

            // Projectile: proj or projN/S/E/W
            var projMatch = Regex.Match(t, @"^proj([nsew]{1,2})$");
            var pierceMatch = Regex.Match(t, @"^pierce(\d+)$");
            if (pierceMatch.Success)
            {
                continue;
            }
            if (projMatch.Success)
            {
                var pierceData = 0; 
                if (pierceMatch.Success) { pierceData = int.Parse(pierceMatch.Groups[1].Value); }
                effect.projectiles.Add(new ProjectileData
                {
                    direction = ParseDirection(projMatch.Groups[1].Value),
                    pierce = pierceData
                });
                continue;
            }

            // Repeat count: r{n}
            var repMatch = Regex.Match(t, @"^r(\d+)$");
            if (repMatch.Success) { effect.repeating = true; effect.repeatCount = int.Parse(repMatch.Groups[1].Value); continue; }

            // Repeat interval: ri{f}
            var riMatch = Regex.Match(t, @"^ri([\d.]+)$");
            if (riMatch.Success) { effect.repeatInterval = float.Parse(riMatch.Groups[1].Value); continue; }

            // Status effects
            TryParseStatus(t, effect.statusEffects);
        }

        return effect;
    }

    private void TryParseStatus(string t, List<StatusEffectEntry> list)
    {
        // burn{n}, psn{n}, shld{n}, regen{n}
        var stackedEffects = new Dictionary<string, StatusEffect>
        {
            { "burn",  StatusEffect.Burn    },
            { "psn",   StatusEffect.Poison  },
            { "shld",  StatusEffect.Shield  },
            { "regen", StatusEffect.Regen   },
            { "weak",  StatusEffect.Weaken  },
            { "vuln",  StatusEffect.Vulnerable },
        };

        foreach (var kvp in stackedEffects)
        {
            var m = Regex.Match(t, $@"^{kvp.Key}(\d*)$");
            if (m.Success)
            {
                int stacks = m.Groups[1].Value == "" ? 1 : int.Parse(m.Groups[1].Value);
                list.Add(new StatusEffectEntry { effect = kvp.Value, stacks = stacks, duration = 1 });
                return;
            }
        }

        // Single-stack keyword effects
        if (t == "stun") list.Add(new StatusEffectEntry { effect = StatusEffect.Stun, stacks = 1, duration = 1 });
        if (t == "slow") list.Add(new StatusEffectEntry { effect = StatusEffect.Slow, stacks = 1, duration = 1 });
    }

    private Direction ParseDirection(string s) => s.ToLower() switch
    {
        "n" => Direction.North,
        "s" => Direction.South,
        "e" => Direction.East,
        "w" => Direction.West,
        "ne" => Direction.NorthEast,
        "nw" => Direction.NorthWest,
        "se" => Direction.SouthEast,
        "sw" => Direction.SouthWest,
        _ => Direction.None,
    };

    // ─────────────────────────────────────────────────────────────────────────
    // Load Card → Grid
    // ─────────────────────────────────────────────────────────────────────────

    private void LoadFromCard()
    {
        ClearGrid();
        if (_selectedCard == null) return;

        int center = GridSize / 2;
        _grid[center, center] = "X";

        foreach (var effect in _selectedCard.tileEffects)
        {
            int col = effect.gridPosition.x + center;
            int row = center - effect.gridPosition.y;

            if (col < 0 || col >= GridSize || row < 0 || row >= GridSize) continue;

            _grid[row, col] = EffectToString(effect);
        }
    }

    private string EffectToString(TileEffect e)
    {
        var parts = new List<string>();

        if (e.damage > 0)          parts.Add($"d{e.damage}");
        foreach (var proj in e.projectiles)
        {
            parts.Add($"proj{DirStr(proj.direction)}");
            if (proj.pierce > 0) parts.Add($"pierce{proj.pierce}");
        }
        if (e.pushDistance > 0)    parts.Add($"p{e.pushDistance}{DirStr(e.pushDirection)}");
        if (e.repeating)           parts.Add($"r{e.repeatCount}");
        if (e.repeatInterval > 0)  parts.Add($"ri{e.repeatInterval}");

        foreach (var se in e.statusEffects)
        {
            string key = se.effect switch
            {
                StatusEffect.Burn       => "burn",
                StatusEffect.Poison     => "psn",
                StatusEffect.Shield     => "shld",
                StatusEffect.Regen      => "regen",
                StatusEffect.Weaken     => "weak",
                StatusEffect.Vulnerable => "vuln",
                StatusEffect.Stun       => "stun",
                StatusEffect.Slow       => "slow",
                _                       => ""
            };
            if (key != "") parts.Add(se.stacks > 1 ? $"{key}{se.stacks}" : key);
        }

        return string.Join(",", parts);
    }

    private string DirStr(Direction d) => d switch
    {
        Direction.North => "N",
        Direction.South => "S",
        Direction.East => "E",
        Direction.West => "W",
        Direction.NorthEast => "NE",
        Direction.NorthWest => "NW",
        Direction.SouthEast => "SE",
        Direction.SouthWest => "SW",
        _ => "?",
    };

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    private void ClearGrid()
    {
        for (int r = 0; r < GridSize; r++)
            for (int c = 0; c < GridSize; c++)
                _grid[r, c] = "";

        int center = GridSize / 2;
        _grid[center, center] = "X";
    }

    private void InitStyles()
    {
        if (_stylesInitialised) return;

        _cellStyle = new GUIStyle(EditorStyles.textField)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize   = 10,
        };

        _originStyle = new GUIStyle(_cellStyle);
        _originStyle.normal.background = MakeTex(Color.black);
        _originStyle.normal.textColor  = Color.white;
        _originStyle.focused.textColor = Color.white;

        _activeStyle = new GUIStyle(_cellStyle);
        _activeStyle.normal.background = MakeTex(new Color(0.2f, 0.4f, 0.6f, 1f));
        _activeStyle.normal.textColor  = Color.white;
        _activeStyle.focused.textColor = Color.white;

        _stylesInitialised = true;
    }

    private Texture2D MakeTex(Color col)
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, col);
        tex.Apply();
        return tex;
    }
}
