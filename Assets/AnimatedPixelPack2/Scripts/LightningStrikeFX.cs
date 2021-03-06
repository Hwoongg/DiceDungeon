﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BlankSourceCode.AnimatedPixelPack2
{
    public class LightningStrikeFX : WeaponFX
    {
        // EditorProperties
        [Header("Strike")]
        public float StrikeHeight = 6;
        public int ControlPoints = 2;
        public float PatternChangeInterval = 0.05f;
        public float StrikeWidth = 1f;

        [Header("Effect")]
        public bool ChangeBackgroundColor = true;
        public Color BackgroundColor = Color.black;

        // Members
        private LineRenderer strikeLine;
        private float changeTime;

        protected override void Update()
        {
            if (this.IsFXStarted)
            {
                this.changeTime -= Time.deltaTime;

                if (this.changeTime <= 0)
                {
                    // Update the strike pattern
                    this.UpdateStrikePattern();
                    this.changeTime = this.PatternChangeInterval;
                }
            }
        }

        public override void BeginFX()
        {
            base.BeginFX();

            // Get the line renderer that draws the lightning
            this.strikeLine = GetComponent<LineRenderer>();

            // We need to hide it until we have set the control points, 
            // otherwise it will be drawn in the wrong place
            this.strikeLine.enabled = false;

            // Set the new background color if that was chosen
            if (this.ChangeBackgroundColor)
            {
                WeaponFX.SetBackgroundColor(this.BackgroundColor);
            }
        }

        public override void EndFX()
        {
            base.EndFX();

            // Hide the effect straight away
            this.strikeLine.enabled = false;
        }

        private void UpdateStrikePattern()
        {
            // Set the starting position
            Vector3[] points = new Vector3[this.ControlPoints + 2];
            points[0] = this.Target.position;

            // Generate a random position for each point
            for (int i = 1; i < points.Length; i++)
            {
                float x = this.Target.position.x + Random.Range(-this.StrikeWidth, this.StrikeWidth);
                float y = ((float)i / (this.ControlPoints + 1)) * StrikeHeight;
                points[i] = new Vector3(x, this.Target.position.y + y, 0);
            }

            // Set the points on the line renderer
            this.strikeLine.positionCount = points.Length;
            for (int i = 0; i < points.Length; i++)
            {
                this.strikeLine.SetPosition(i, points[i]);
            }

            // Show the renderer
            this.strikeLine.enabled = true;
        }
    }
}