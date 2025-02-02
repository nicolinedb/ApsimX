using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using MathNet.Numerics.Interpolation;

using System.Collections;
using Models.Core;
using APSIM.Shared.Utilities;

namespace Models.PMF.Functions
{
    /// <summary>
    /// A value is returned via Akima spline interpolation of a given set of XY pairs
    /// </summary>
    [Serializable]
    [Description("A value is returned via Akima spline interpolation of a given set of XY pairs")]
    public class SplineInterpolationFunction : Model, IFunction
    {
        /// <summary>Gets or sets the xy pairs.</summary>
        /// <value>The xy pairs.</value>
        public XYPairs XYPairs { get; set; }

        /// <summary>The x property</summary>
        public string XProperty = "";

        /// <summary>The spline</summary>
        [NonSerialized]
        private CubicSpline spline = null;
        /// <summary>The property name</summary>
        private string PropertyName;
        /// <summary>The array spec</summary>
        private string ArraySpec;

        /// <summary>
        /// Initializes a new instance of the <see cref="SplineInterpolationFunction"/> class.
        /// </summary>
        public SplineInterpolationFunction()
        {
            PropertyName = XProperty;
            ArraySpec = StringUtilities.SplitOffBracketedValue(ref PropertyName, '[', ']');
        }

        /// <summary>Gets the value.</summary>
        /// <value>The value.</value>
        /// <exception cref="System.Exception">Cannot find value for  + Name +  XProperty:  + XProperty</exception>
        public double Value
        {
            get
            {
                double XValue = 0;
                try
                {
                    object v = Apsim.Get(this, XProperty);
                    if (v == null)
                        throw new Exception("Cannot find value for " + Name + " XProperty: " + XProperty);
                    XValue = Convert.ToDouble(v);
                }
                catch (IndexOutOfRangeException)
                {
                }

                if (spline == null)
                {
                    spline = CubicSpline.InterpolateBoundaries(XYPairs.X, XYPairs.Y, SplineBoundaryCondition.FirstDerivative, 0, SplineBoundaryCondition.FirstDerivative, 0);
                    
                }

                return Interpolate(XValue);
            }
        }

        /// <summary>Interpolates the specified x.</summary>
        /// <param name="x">The x.</param>
        /// <returns></returns>
        private double Interpolate(double x)
        {
            return spline.Interpolate(x);
        }
    }

}