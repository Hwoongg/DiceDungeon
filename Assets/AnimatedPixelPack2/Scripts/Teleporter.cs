using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlankSourceCode.AnimatedPixelPack2
{
    public class Teleporter : MonoBehaviour
    {
        // Editor Properties
        [Tooltip("The target location of where the teleport will take you")]
        public Transform Target;

        void OnTriggerEnter2D(Collider2D c)
        {
            // Check that the collision was from a character
            Character character = c.transform.GetComponent<Character>();
            if (character != null)
            {
                // Move them to the target position instantly
                character.transform.position = Target.position;
            }
        }
    }
}
