using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerHealth))]
[CanEditMultipleObjects]
public class PlayerHealthEditor : Editor
{
    // ============================================================
    // BLUE FOLDOUT INSPECTOR
    // PLAYER HEALTH
    // ============================================================

    private GUIStyle sectionStyle;
    private GUIStyle contentStyle;

    private readonly Color sectionBackground =
        new Color(0.10f, 0.43f, 0.78f, 1f);

    private readonly Color sectionText =
        new Color(0.75f, 0.93f, 1f, 1f);

    // ============================================================
    // UNITY METHODS
    // ============================================================

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        CreateStyles();

        EditorGUILayout.Space(5);

        DrawSection(
            "01 - HEARTS",
            "Hearts",
            "maxHearts"
        );

        DrawSection(
            "02 - INVULNERABILITY",
            "Invulnerability",
            "invulnTime"
        );

        DrawSection(
            "03 - REFERENCES",
            "References",
            "playerRenderer",
            "heartsUI",
            "playerVisual"
        );

        DrawSection(
            "04 - MOVEMENT REFERENCES",
            "Movement References",
            "playerRigidbody",
            "playerController",
            "playerJump",
            "climbHook"
        );

        DrawSection(
            "05 - PLAYER BLINK",
            "Player Blink",
            "blinkCount",
            "blinkInterval"
        );

        DrawSection(
            "06 - DAMAGE AUDIO",
            "Damage Audio",
            "sfxSource",
            "damageClip",
            "damageVolume"
        );

        DrawSection(
            "07 - DEATH AUDIO",
            "Death Audio",
            "deathClip",
            "deathVolume"
        );

        DrawSection(
            "08 - HIT FLASH",
            "Hit Flash",
            "useHitFlash",
            "flashColor",
            "flashDuration"
        );

        DrawSection(
            "09 - HIT PARTICLES",
            "Hit Particles",
            "hitEffectPrefab",
            "hitEffectOffset",
            "effectFollowsPlayer",
            "destroyEffectAfterPlay"
        );

        DrawSection(
            "10 - HIT PARTICLE AMOUNT",
            "Hit Particle Amount",
            "hitEffectSpawnCount",
            "hitEffectSpawnDelay"
        );

        DrawSection(
            "11 - HIT PARTICLE SPREAD",
            "Hit Particle Spread",
            "hitEffectRandomOffsetX",
            "hitEffectRandomOffsetY",
            "hitEffectRandomOffsetZ"
        );

        DrawSection(
            "12 - HIT PARTICLE ROTATION",
            "Hit Particle Rotation",
            "randomizeHitEffectRotation",
            "hitEffectRandomRotationZ"
        );

        DrawSection(
            "13 - HIT PARTICLE SCALE",
            "Hit Particle Scale",
            "randomizeHitEffectScale",
            "hitEffectRandomScale"
        );

        DrawSection(
            "14 - DEATH PARTICLES",
            "Death Particles",
            "deathEffectPrefab",
            "deathEffectOffset",
            "deathEffectFollowsPlayer",
            "destroyDeathEffectAfterPlay"
        );

        DrawSection(
            "15 - DEATH AND RESTART",
            "Death And Restart",
            "deathRestartDelay",
            "hidePlayerOnDeath",
            "disableMovementOnDeath",
            "disablePhysicsOnDeath"
        );

        serializedObject.ApplyModifiedProperties();
    }

    // ============================================================
    // STYLES
    // ============================================================

    private void CreateStyles()
    {
        if (sectionStyle == null)
        {
            sectionStyle = new GUIStyle(EditorStyles.foldout);

            sectionStyle.fontStyle = FontStyle.Bold;
            sectionStyle.fontSize = 12;

            sectionStyle.normal.textColor = sectionText;
            sectionStyle.onNormal.textColor = sectionText;

            sectionStyle.hover.textColor = Color.white;
            sectionStyle.onHover.textColor = Color.white;

            sectionStyle.focused.textColor = Color.white;
            sectionStyle.onFocused.textColor = Color.white;

            sectionStyle.active.textColor = Color.white;
            sectionStyle.onActive.textColor = Color.white;

            sectionStyle.padding = new RectOffset(
                10, 8, 8, 8
            );

            sectionStyle.fixedHeight = 34f;
        }

        if (contentStyle == null)
        {
            contentStyle = new GUIStyle(EditorStyles.helpBox);

            contentStyle.padding = new RectOffset(
                10, 10, 10, 10
            );

            contentStyle.margin = new RectOffset(
                0, 0, 0, 4
            );
        }
    }

    // ============================================================
    // DRAW BLUE SECTION
    // ============================================================

    private void DrawSection(
        string title,
        string sectionId,
        params string[] propertyNames
    )
    {
        string sessionKey =
            "PlayerHealthEditor_" + sectionId;

        bool expanded = SessionState.GetBool(
            sessionKey,
            false
        );

        Rect headerRect =
            GUILayoutUtility.GetRect(
                0f,
                34f,
                GUILayout.ExpandWidth(true)
            );

        EditorGUI.DrawRect(
            headerRect,
            sectionBackground
        );

        expanded = EditorGUI.Foldout(
            headerRect,
            expanded,
            title,
            true,
            sectionStyle
        );

        SessionState.SetBool(
            sessionKey,
            expanded
        );

        if (!expanded)
        {
            EditorGUILayout.Space(3);
            return;
        }

        EditorGUILayout.BeginVertical(
            contentStyle
        );

        for (int i = 0; i < propertyNames.Length; i++)
        {
            DrawProperty(
                propertyNames[i]
            );
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(3);
    }

    // ============================================================
    // DRAW PROPERTY
    // ============================================================

    private void DrawProperty(
        string propertyName
    )
    {
        SerializedProperty property =
            serializedObject.FindProperty(
                propertyName
            );

        if (property == null)
        {
            EditorGUILayout.HelpBox(
                "Property not found: " + propertyName,
                MessageType.Warning
            );

            return;
        }

        EditorGUILayout.PropertyField(
            property,
            true
        );
    }
}