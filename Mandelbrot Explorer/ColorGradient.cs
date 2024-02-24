﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ConsoleMandelBrot
{
    class ColorGradient
    {
        public List<ControlPoint> controlPoints = new List<ControlPoint>();

        public ColorGradient(List<ControlPoint> controlPoints)
        {
            double position = 1 + 1.0 / (controlPoints.Count - 1);
            controlPoints.Add(new ControlPoint(1,controlPoints[0].Color));
            for (int i = 0; i < controlPoints.Count-1; i++)
            {
                controlPoints[i].Position = controlPoints[i].Position / position;
            }
            this.controlPoints=controlPoints;
        }

        public List<ControlPoint> GetControlPoints()
        {
            return controlPoints;
        }

        public Color GetColor(double position)
        {
            int i = 0;
            foreach(var controlPoint in controlPoints)
            {
                if (position == controlPoint.Position) return controlPoint.Color;
                if (position < controlPoint.Position)
                {
                    int red = Convert.ToInt32(controlPoints[i - 1].Color.R + (position - controlPoints[i - 1].Position) / (controlPoints[i].Position - controlPoints[i - 1].Position) * (controlPoints[i].Color.R - controlPoints[i - 1].Color.R));
                    int green = Convert.ToInt32(controlPoints[i - 1].Color.G + (position - controlPoints[i - 1].Position) / (controlPoints[i].Position - controlPoints[i - 1].Position) * (controlPoints[i].Color.G - controlPoints[i - 1].Color.G));
                    int blue = Convert.ToInt32(controlPoints[i - 1].Color.B + (position - controlPoints[i - 1].Position) / (controlPoints[i].Position - controlPoints[i - 1].Position) * (controlPoints[i].Color.B - controlPoints[i - 1].Color.B));
                    return Color.FromArgb(red, green, blue);
                }
                i++;
            }
            return Color.FromArgb(255, 255, 100);
        }

    }
}
