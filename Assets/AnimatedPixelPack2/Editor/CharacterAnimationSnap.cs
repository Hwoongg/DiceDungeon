using UnityEditor;
using UnityEngine;

namespace BlankSourceCode.AnimatedPixelPack2
{
    [InitializeOnLoad]
    [ExecuteInEditMode]
    public class CharacterAnimationSnap
    {
        static CharacterAnimationSnap()
        {
            // Hook into the animation changed event
            AnimationUtility.onCurveWasModified += CharacterAnimationSnap.OnCurveWasModified;
        }

        static void OnCurveWasModified(AnimationClip clip, EditorCurveBinding binding, AnimationUtility.CurveModifiedType curveType)
        {
            // Stop listening for the changed event while we edit the animation
            AnimationUtility.onCurveWasModified -= CharacterAnimationSnap.OnCurveWasModified;

            // Ensure we only alter animatinos under our own path to prevent issues with other ones
            string path = AssetDatabase.GetAssetPath(clip).ToLower();
            if (path.IndexOf("/animatedpixelpack2/characters/animations") >= 0)
            {
                // Find the position curves
                if (curveType == AnimationUtility.CurveModifiedType.CurveModified &&
                    binding.type == typeof(Transform) &&
                    binding.propertyName.Contains("m_LocalPosition"))
                {
                    AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);

                    // Snap each keyframe to the 1/16th unit measurement that equates to one character pixel,
                    // Also make the tangent constant between keyframes.
                    // This is how our unity mecanim animations look like spritesheets rather than being
                    // interpolated at subpixel positions between frames.
                    for (int i = 0; i < curve.keys.Length; i++)
                    {
                        var k = curve.keys[i];
                        k.value = Mathf.Round(k.value * 16) / 16f;
                        k.weightedMode = WeightedMode.None;
                        curve.MoveKey(i, k);
                        AnimationUtility.SetKeyBroken(curve, i, true);
                        AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Constant);
                        AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Constant);
                    }

                    // Update the curve
                    AnimationUtility.SetEditorCurve(clip, binding, curve);
                }
            }

            // Begin listening again
            AnimationUtility.onCurveWasModified += CharacterAnimationSnap.OnCurveWasModified;
        }
    }
}
