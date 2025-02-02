﻿// -----------------------------------------------------------------------
// <copyright file="IModel.cs" company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
//-----------------------------------------------------------------------
namespace Models.Core
{
    using System.Collections.Generic;

    /// <summary>
    /// The IModel interface specifies the properties and methods that all
    /// models must have. 
    /// </summary>
    public interface IModel
    {
        /// <summary>
        /// Gets or sets the name of the model.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the parent model. Can be null if model has no parent.
        /// </summary>
        IModel Parent { get; set; }

        /// <summary>
        /// Gets of sets the child models. Can be empty array but never null.
        /// </summary>
        List<Model> Children { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a model is hidden from the user.
        /// </summary>
        bool IsHidden { get; set; }
    }
}