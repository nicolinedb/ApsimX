using System;
using System.Collections.Generic;
using System.Text;
using Models.Core;

namespace Models.PMF.Functions
{
    /// <summary>
    /// Determines which PhaseLookupValue child functions start and end stages bracket the current phenological stage and returns the value of the grand child function decending from the applicable PhaseLookupValue function.
    /// </summary>
    [Serializable]
    [Description("Determines which PhaseLookupValue child functions start and end stages bracket the current phenological stage and returns the value of the grand child function decending from the applicable PhaseLookupValue function.")]
    public class PhaseLookup : Model, IFunction
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

                foreach (IFunction F in ChildFunctions)
                {
                    PhaseLookupValue P = F as PhaseLookupValue;
                    if (P.InPhase)
                        return P.Value;
                }
                return 0;  // Default value is zero
            }
        }

    }
}


