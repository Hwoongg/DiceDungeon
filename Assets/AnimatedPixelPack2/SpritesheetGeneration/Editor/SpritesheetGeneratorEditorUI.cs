using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BlankSourceCode.AnimatedPixelPack2
{
    [CustomEditor(typeof(SpritesheetGenerator), true)]
    [CanEditMultipleObjects]
    public class SpritesheetGeneratorEditorUI : Editor
    {
        public override void OnInspectorGUI()
        {
            SpritesheetGenerator ssg = (SpritesheetGenerator)target;

            GUI.changed = false;

            // Add the custom button
            EditorGUILayout.LabelField("Frames", EditorStyles.boldLabel);
            if (GUILayout.Button("Collect Frame Info"))
            {
                this.CollectFrameInfo(ssg);
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(ssg);
            }

            // Draw the rest of the editor
            EditorGUILayout.Separator();
            DrawDefaultInspector();
        }

        private void CollectFrameInfo(SpritesheetGenerator ssg)
        {
            // Find all the animation clips on the first prefab (we assume all the prefabs are the same)
            Animator animator = ssg.Prefabs[0].GetComponent<Animator>();
            var clips = animator.runtimeAnimatorController.animationClips;

            // Setup the info we need to collect
            Dictionary<string, bool> processedClips = new Dictionary<string, bool>();
            List<AnimationClip> allClips = new List<AnimationClip>();
            List<int> allFrameStarts = new List<int>();
            List<int> allFrameCounts = new List<int>();
            List<float> allFrameTimes = new List<float>();

            // Process each animation clip
            foreach (var c in clips)
            {
                // Only do each one once (since the animator state machine could use the same clip twice)
                if (processedClips.ContainsKey(c.name))
                {
                    continue;
                }
                processedClips.Add(c.name, true);

                // Add the initial frame
                List<float> times = new List<float>();
                times.Add(0);

                // Create a new clip if we need to alter it (disabled for now)
                AnimationClip overrideClip = null;
                //if (c.isLooping)
                //{
                //    overrideClip = new AnimationClip();
                //    overrideClip.name = c.name;
                //}

                // Search through each binding and add it's keyframe time to one we want to record
                var floatBindings = AnimationUtility.GetCurveBindings(c);
                foreach(var b in floatBindings)
                {
                    var curve = AnimationUtility.GetEditorCurve(c, b);
                    foreach(var k in curve.keys)
                    {
                        if (!times.Contains(k.time))
                        {
                            times.Add(k.time);
                        }
                    }

                    if (overrideClip != null)
                    {
                        overrideClip.SetCurve(b.path, b.type, b.propertyName, curve);
                    }
                }

                // Do the same for the other types of bindings
                var objectBindings = AnimationUtility.GetObjectReferenceCurveBindings(c);
                foreach (var b in objectBindings)
                {
                    var curve = AnimationUtility.GetEditorCurve(c, b);
                    foreach (var k in curve.keys)
                    {
                        if (!times.Contains(k.time))
                        {
                            times.Add(k.time);
                        }
                    }

                    if (overrideClip != null)
                    {
                        overrideClip.SetCurve(b.path, b.type, b.propertyName, curve);
                    }
                }

                // Add a new curve so that we can render the last frame even though this clip loops
                if (overrideClip != null)
                {
                    overrideClip.SetCurve("", typeof(Transform), "localPosition.z", AnimationCurve.Constant(times[times.Count - 1] + 0.05f, times[times.Count - 1] + 0.05f, 0));
                }

                // Sort all the keyframes in order
                times.Sort();

                // Add the info into the separate collections (since Unity can't serialize a dictionary)
                allFrameStarts.Add(allFrameTimes.Count);
                allFrameCounts.Add(times.Count);
                allFrameTimes.AddRange(times);
                allClips.Add(overrideClip == null ? c : overrideClip);
            }

            // Add the info to the runtime script
            ssg.clips = allClips;
            ssg.clipFrameStarts = allFrameStarts;
            ssg.clipFrameCounts = allFrameCounts;
            ssg.frameTimes = allFrameTimes;

            // Create all the out folders (one for each prefab), since this needs to be done at edit time
            List<string> allOutFolders = new List<string>();
            foreach  (var p in ssg.Prefabs)
            {
                string folder = "Spritesheets";
                string path = AssetDatabase.GetAssetPath(p);
                path = path.Substring(0, path.LastIndexOf("/", path.LastIndexOf("/") - 1));
                if (!AssetDatabase.IsValidFolder(path + "/" + folder))
                {
                    AssetDatabase.CreateFolder(path, folder);
                }

                path += "/" + folder;
                string name = p.name.Replace(" Variant", "").Replace("(Clone)", "");
                if (!AssetDatabase.IsValidFolder(path + "/" + name))
                {
                    AssetDatabase.CreateFolder(path, name);
                }

                allOutFolders.Add(path + "/" + name + "/");
            }

            // Update the runtime script with the list of output folders
            ssg.outFolderPaths = allOutFolders;
        }
    }
}
