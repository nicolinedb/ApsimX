﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Models.Core;
using System.Xml.Serialization;

namespace Models.PMF.Functions
{
    /// <summary>
    /// Takes the value of the child as the x value and returns the y value from a sigmoid of the form y = Xmax * 1/1+exp(-(x-Xo)/b)
    /// </summary>
    [Serializable]
    [Description("Takes the value of the child as the x value and returns the y value from a sigmoid of the form y = Xmax * 1/1+exp(-(x-Xo)/b)")]
    public class SigmoidFunction2 : Model, IFunction
    {
        /// <summary>The ymax</summary>
        [Link]
        IFunction Ymax = null;
        /// <summary>The x value</summary>
        [Link]
        IFunction XValue = null;

        /// <summary>The xo</summary>
        [XmlElement("Xo")]
        public double Xo { get; set; }
        //public double Xo = 1.0;
        /// <summary>The b</summary>
        [XmlElement("b")]
        public double b { get; set; }
        //public double b = 1.0;



        /// <summary>Gets the value.</summary>
        /// <value>The value.</value>
        /// <exception cref="System.Exception">Error with values to Sigmoid function</exception>
        public double Value
        {
            get
            {

                try
                {
                    double _return = Ymax.Value * 1 / (1 + Math.Exp(-(XValue.Value - Xo) / b));
                    return _return;
                }
                catch (Exception)
                {
                    throw new Exception("Error with values to Sigmoid function");
                }
            }
        }

    }
}
