﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Models.Core;

namespace Models.PMF.Functions
{
    /// <summary>Multiplies the values of the children of this node</summary>
    /// \pre All children have to contain a public function "Value"
    /// \retval Product of value of all children of this node. Return 1 if no child.
    [Serializable]
    [Description("Returns the product of all children function values")]
    public class MultiplyFunction : Model, IFunction
    {
        /// <summary>The child functions</summary>
        private List<IModel> ChildFunctions;
        /// <summary>Gets the value.</summary>
        /// <value>The value.</value>
        public double Value
        {
            get
            {
                if (ChildFunctions == null)
                    ChildFunctions = Apsim.Children(this, typeof(IFunction));

                double returnValue = 1.0;

                foreach (IFunction F in ChildFunctions)
                    returnValue = returnValue * F.Value;
                return returnValue;
            }
        }

    }
}