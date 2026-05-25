using System;
using System.IO;
using IDRPG3D.GameplayPrototype;
using UnityEditor;
using UnityEngine;

namespace IDRPG3D.EditorTools
{
    public static class IDRPG3DPrototypeAnimationClipLibrary
    {
        private const string RpgAnimationRoot = "Assets/ThirdParty/ExplosiveLLC/RPG Character Mecanim Animation Pack FREE/Animations";

        private static AnimationClip idleClip;
        private static AnimationClip walkClip;
        private static AnimationClip runClip;
        private static AnimationClip attackClip;
        private static AnimationClip deathClip;
        private static bool loaded;

        public static void Configure(IDRPG3DAnimatorBridge bridge)
        {
            if (bridge == null)
            {
                return;
            }

            LoadOnce();
            bridge.ConfigureClips(idleClip, walkClip, runClip, attackClip, deathClip);
        }

        private static void LoadOnce()
        {
            if (loaded)
            {
                return;
            }

            loaded = true;
            idleClip = FindClip("Unarmed-Idle", "2Hand-Sword-Idle", "Idle");
            walkClip = FindClip("Unarmed-Walk", "2Hand-Sword-Walk", "Walk");
            runClip = FindClip("Unarmed-Run-Forward", "2Hand-Sword-Run-Forward", "Run-Forward", "Run");
            attackClip = FindClip("Unarmed-Attack-R1", "2Hand-Sword-Attack1", "Unarmed-Attack-L1", "Attack1", "Attack");
            deathClip = FindClip("Unarmed-Knockdown1", "2Hand-Sword-Knockdown1", "Knockdown", "Fall", "Dead", "Death");
        }

        private static AnimationClip FindClip(params string[] preferredNames)
        {
            var guids = AssetDatabase.FindAssets("t:AnimationClip", new[] { RpgAnimationRoot });
            AnimationClip bestClip = null;
            var bestScore = int.MinValue;
            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var clips = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
                for (var clipIndex = 0; clipIndex < clips.Length; clipIndex++)
                {
                    if (clips[clipIndex] is not AnimationClip clip || IsEditorPreviewClip(clip, path))
                    {
                        continue;
                    }

                    var score = ScoreClip(clip, path, preferredNames);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestClip = clip;
                    }
                }
            }

            return bestClip;
        }

        private static int ScoreClip(AnimationClip clip, string path, string[] preferredNames)
        {
            var score = 0;
            var name = clip.name;
            for (var i = 0; i < preferredNames.Length; i++)
            {
                if (EqualsPattern(name, preferredNames[i]) || EqualsPattern(Path.GetFileNameWithoutExtension(path), "RPG-Character@" + preferredNames[i]))
                {
                    score += 1000 - i * 10;
                    continue;
                }

                if (Contains(path, preferredNames[i]) || Contains(name, preferredNames[i]))
                {
                    score += 200 - i * 5;
                }
            }

            if (Contains(name, "Forward") || Contains(path, "Forward"))
            {
                score += 80;
            }

            score -= PenaltyForDirectionalOrDisplacementClip(name);
            score -= PenaltyForDirectionalOrDisplacementClip(path);
            return score;
        }

        private static bool IsEditorPreviewClip(AnimationClip clip, string path)
        {
            return clip.name.StartsWith("__preview__", StringComparison.Ordinal)
                || Path.GetFileNameWithoutExtension(path).StartsWith("__preview__", StringComparison.Ordinal);
        }

        private static int PenaltyForDirectionalOrDisplacementClip(string value)
        {
            var penalty = 0;
            if (Contains(value, "Backward")) penalty += 400;
            if (Contains(value, "Strafe")) penalty += 250;
            if (Contains(value, "Left")) penalty += 150;
            if (Contains(value, "Right")) penalty += 150;
            if (Contains(value, "DiveRoll")) penalty += 300;
            if (Contains(value, "Jump")) penalty += 300;
            if (Contains(value, "Knockback")) penalty += 300;
            if (Contains(value, "Run-Forward-Attack")) penalty += 220;
            return penalty;
        }

        private static bool EqualsPattern(string value, string pattern)
        {
            return string.Equals(value, pattern, StringComparison.OrdinalIgnoreCase);
        }

        private static bool Contains(string value, string pattern)
        {
            return value.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
