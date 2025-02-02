﻿//-----------------------------------------------------------------------
// <copyright file="ExcelInput.cs" company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
//-----------------------------------------------------------------------
namespace Models.PostSimulationTools
{
    using System;
    using System.Data;
    using System.IO;
    using Excel;
    using Models.Core;
    using System.Xml.Serialization;
    using APSIM.Shared.Utilities;

    /// <summary>
    /// Reads the contents of a specific sheet from an EXCEL file and stores into the DataStore. 
    /// </summary>
    [Serializable]
    [ViewName("UserInterface.Views.GridView")]
    [PresenterName("UserInterface.Presenters.PropertyPresenter")]
    public class ExcelInput : Model, IPostSimulationTool
    {
        /// <summary>
        /// Gets or sets the file name to read from.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the full file name (with path). The user interface uses this. 
        /// </summary>
        [XmlIgnore]
        [Description("EXCEL file name")]
        public string FullFileName
        {
            get
            {
                Simulations simulations = Apsim.Parent(this, typeof(Simulations)) as Simulations;
                return PathUtilities.GetAbsolutePath(this.FileName, simulations.FileName);
            }

            set
            {
                Simulations simulations = Apsim.Parent(this, typeof(Simulations)) as Simulations;
                this.FileName = PathUtilities.GetRelativePath(value, simulations.FileName);
            }
        }

        /// <summary>
        /// Gets or sets the list of EXCEL sheet names to read from.
        /// </summary>
        [Description("EXCEL sheet names (csv)")]
        public string[] SheetNames { get; set; }

        /// <summary>
        /// Gets the parent simulation or null if not found
        /// </summary>
        private Simulation Simulation
        {
            get
            {
                return Apsim.Parent(this, typeof(Simulation)) as Simulation;
            }
        }

        /// <summary>
        /// Main run method for performing our calculations and storing data.
        /// </summary>
        /// <param name="dataStore">The data store to store the data</param>
        public void Run(DataStore dataStore)
        {
            string fullFileName = FullFileName;
            if (fullFileName != null && File.Exists(fullFileName))
            {
                dataStore.DeleteTable(this.Name);
                
                // Open the file
                FileStream stream = File.Open(fullFileName, FileMode.Open, FileAccess.Read);

                // Create a reader.
                IExcelDataReader excelReader;
                if (Path.GetExtension(fullFileName).Equals(".xls", StringComparison.CurrentCultureIgnoreCase))
                {
                    // Reading from a binary Excel file ('97-2003 format; *.xls)
                    excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
                }
                else
                {
                    // Reading from a OpenXml Excel file (2007 format; *.xlsx)
                    excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                }

                // Read all sheets from the EXCEL file as a data set
                excelReader.IsFirstRowAsColumnNames = true;
                DataSet dataSet = excelReader.AsDataSet();

                // Write all sheets that are specified in 'SheetNames' to the data store
                foreach (DataTable table in dataSet.Tables)
                {
                    bool keep = StringUtilities.IndexOfCaseInsensitive(this.SheetNames, table.TableName) != -1;
                    if (keep)
                    {
                        dataStore.WriteTable(null, table.TableName, table);
                    }
                }

                // Close the reader and free resources.
                excelReader.Close();
            }
        }
    }
}
