﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Models.Core;
using System.Xml.Serialization;

namespace Models.Factorial
{
    /// <summary>
    /// A model representing an experiment's factors
    /// </summary>
    [Serializable]
    [ValidParent(typeof(Experiment))]
    public class Factors : Model
    {
        /// <summary>Gets the factors.</summary>
        /// <value>The factors.</value>
        [XmlIgnore]
        public List<IModel> factors { get { return Apsim.Children(this, typeof(Factor)); } }
    }
}
