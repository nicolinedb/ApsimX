﻿// -----------------------------------------------------------------------
// <copyright file="Summary.cs" company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
// -----------------------------------------------------------------------
namespace Models
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Reflection;
    using Models.Core;
    using APSIM.Shared.Utilities;

    /// <summary>
    /// This model collects the simulation initial conditions and stores into the DataStore.
    /// It also provides an API for writing messages to the DataStore.
    /// </summary>
    [Serializable]
    [ViewName("UserInterface.Views.SummaryView")]
    [PresenterName("UserInterface.Presenters.SummaryPresenter")]
    public class Summary : Model, ISummary
    {      
        /// <summary>
        /// The messages data table.
        /// </summary>
        private DataTable messagesTable;

        /// <summary>
        /// A link to the clock in the simulation.
        /// </summary>
        [Link] 
        private Clock clock = null;

        /// <summary>
        /// Gets a link to the simulation.
        /// </summary>
        public Simulation Simulation
        {
            get
            {
                return Apsim.Parent(this, typeof(Simulation)) as Simulation;
            }
        }

        /// <summary>
        /// Write the summary report to the specified writer.
        /// </summary>
        /// <param name="dataStore">The data store to write a summary report from</param>
        /// <param name="simulationName">The simulation name to produce a summary report for</param>
        /// <param name="writer">Text writer to write to</param>
        /// <param name="apsimSummaryImageFileName">The file name for the logo. Can be null</param>
        /// <param name="html">Indicates whether to produce html format</param>
        public static void WriteReport(
            DataStore dataStore,
            string simulationName,
            TextWriter writer,
            string apsimSummaryImageFileName,
            bool html)
        {
            if (html)
            {
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                writer.WriteLine("<html>");
                writer.WriteLine("<body>");
            }

            // Get the initial conditions table.            
            DataTable initialConditionsTable = dataStore.GetData(simulationName, "InitialConditions");
            if (initialConditionsTable != null)
            {
                // Convert the 'InitialConditions' table in the DataStore to a series of
                // DataTables for each model.
                List<DataTable> tables = new List<DataTable>();
                ConvertInitialConditionsToTables(initialConditionsTable, tables);

                // Now write all tables to our report.
                for (int i = 0; i < tables.Count; i += 2)
                {
                    // Only write something to the summary file if we have something to write.
                    if (tables[i].Rows.Count > 0 || tables[i + 1].Rows.Count > 0)
                    {
                        string heading = tables[i].TableName;
                        WriteHeading(writer, heading, html);

                        // Write the manager script.
                        if (tables[i].Rows.Count == 1 && tables[i].Rows[0][0].ToString() == "Script code: ")
                        {
                            WriteScript(writer, tables[i].Rows[0], html);
                        }
                        else
                        {
                            // Write the properties table if we have any properties.
                            if (tables[i].Rows.Count > 0)
                            {
                                WriteTable(writer, tables[i], html, includeHeadings: false, className: "PropertyTable");
                            }

                            // Write the general data table if we have any data.
                            if (tables[i + 1].Rows.Count > 0)
                            {
                                WriteTable(writer, tables[i + 1], html, includeHeadings: true, className: "ApsimTable");
                            }
                        }

                        writer.WriteLine("<br/>");
                    }
                }
            }

            // Write out all messages.
            WriteHeading(writer, "Simulation log:", html);
            DataTable messageTable = GetMessageTable(dataStore, simulationName);
            WriteMessageTable(writer, messageTable, html, false, "MessageTable");

            if (html)
            {
                writer.WriteLine("</body>");
                writer.WriteLine("</html>");
            }
        }

        /// <summary>
        /// All simulations have been completed. 
        /// </summary>
        [EventSubscribe("Completed")]
        private void OnSimulationCompleted(object sender, EventArgs e)
        {
            DataStore dataStore = new DataStore(this);
            dataStore.DeleteOldContentInTable(this.Simulation.Name, "Messages");
            dataStore.WriteTable(this.Simulation.Name, "Messages", this.messagesTable);
            dataStore.Disconnect();
        }

        /// <summary>
        /// Write a message to the summary
        /// </summary>
        /// <param name="model">The model writing the message</param>
        /// <param name="message">The message to write</param>
        public void WriteMessage(IModel model, string message)
        {
            string modelFullPath = Apsim.FullPath(model);
            string relativeModelPath = modelFullPath.Replace(Apsim.FullPath(Simulation) + ".", string.Empty);
                
            DataRow newRow = this.messagesTable.NewRow();
            newRow["ComponentName"] = relativeModelPath;
            newRow["Date"] = this.clock.Today;
            newRow["Message"] = message;
            newRow["MessageType"] = Convert.ToInt32(DataStore.ErrorLevel.Information);
            this.messagesTable.Rows.Add(newRow);
        }

        /// <summary>
        /// Write a warning message to the summary
        /// </summary>
        /// <param name="model">The model writing the message</param>
        /// <param name="message">The warning message to write</param>
        public void WriteWarning(IModel model, string message)
        {
            if (this.messagesTable != null)
            {
                DataRow newRow = this.messagesTable.NewRow();
                newRow["ComponentName"] = Apsim.FullPath(model);
                newRow["Date"] = this.clock.Today;
                newRow["Message"] = message;
                newRow["MessageType"] = Convert.ToInt32(DataStore.ErrorLevel.Warning);
                this.messagesTable.Rows.Add(newRow);
            }
        }

        #region Private static summary report generation
                
        /// <summary>
        /// Create a message table ready for writing.
        /// </summary>
        /// <param name="dataStore">The data store to read the message table from</param>
        /// <param name="simulationName">The simulation name to get messages for</param>
        /// <returns>The filled message table</returns>
        private static DataTable GetMessageTable(DataStore dataStore, string simulationName)
        {
            DataTable messageTable = new DataTable();
            DataTable messages = dataStore.GetData(simulationName, "Messages");
            if (messages != null && messages.Rows.Count > 0)
            {
                messageTable.Columns.Add("Date", typeof(string));
                messageTable.Columns.Add("Message", typeof(string));
                string previousCol1Text = null;
                string previousMessage = null;
                foreach (DataRow row in messages.Rows)
                {
                    // Work out the column 1 text.
                    string modelName = (string)row[1];
                    DateTime date = (DateTime)row[2];
                    string col1Text = date.ToString("yyyy-MM-dd") + " " + modelName;

                    // If the date and model name have changed then write a row.
                    if (col1Text != previousCol1Text)
                    {
                        if (previousCol1Text != null)
                        {
                            messageTable.Rows.Add(new object[] { previousCol1Text, previousMessage });
                        }

                        previousMessage = string.Empty;
                        previousCol1Text = col1Text;
                    }
                    else
                    {
                        col1Text = null;
                    }

                    string message = (string)row[3];
                    Models.DataStore.ErrorLevel errorLevel = (Models.DataStore.ErrorLevel)Enum.Parse(typeof(Models.DataStore.ErrorLevel), row[4].ToString());

                    if (errorLevel == DataStore.ErrorLevel.Error)
                    {
                        previousMessage += "FATAL ERROR: " + message;
                    }
                    else if (errorLevel == DataStore.ErrorLevel.Warning)
                    {
                        previousMessage += "WARNING: " + message;
                    }
                    else
                    {
                        previousMessage += message;
                    }

                    previousMessage += "\r\n";
                }
            }

            return messageTable;
        }

        /// <summary>
        /// Write the specified heading to the TextWriter.
        /// </summary>
        /// <param name="writer">Text writer to write to</param>
        /// <param name="heading">The heading to write</param>
        /// <param name="html">Indicates whether to produce html format</param>
        private static void WriteHeading(TextWriter writer, string heading, bool html)
        {
            if (html)
            {
                writer.WriteLine("<h2>" + heading + "</h2>");
            }
            else
            {
                writer.WriteLine(heading.ToUpper());
                writer.WriteLine(new string('-', heading.Length));
            }
        }

        /// <summary>
        /// Write out manager script
        /// </summary>
        /// <param name="writer">Text writer to write to</param>
        /// <param name="row">The data table row containing the script</param>
        /// <param name="html">Indicates whether to produce html format</param>
        private static void WriteScript(TextWriter writer, DataRow row, bool html)
        {
            string st = row[1].ToString();
            st = st.Replace("\t", "    ");
            if (html)
            {
                st = st.Replace("<", "LE");
                st = st.Replace(">", "GE");
                st = st.Replace("&&", "and");
                st = st.Replace("\r", string.Empty);
                st = st.Replace("\n", "<br/>");
                st = st.Replace("<br/>", "<br/>\r\n");
            }

            writer.WriteLine(st);
        }

        /// <summary>
        /// Write the specified table to the TextWriter.
        /// </summary>
        /// <param name="writer">The writer to write to</param>
        /// <param name="table">The table to write</param>
        /// <param name="html">Indicates whether html format should be produced</param>
        /// <param name="includeHeadings">Include headings in the html table produced?</param>
        /// <param name="className">The class name of the generated html table</param>
        private static void WriteTable(TextWriter writer, DataTable table, bool html, bool includeHeadings, string className)
        {
            if (html)
            {
                bool showHeadings = className != "PropertyTable";
                string line = DataTableUtilities.DataTableToText(table, 0, "  ", showHeadings);
                line = line.Replace("\r\n", "<br/>");
                writer.WriteLine(line);
            }
            else
            {
                bool showHeadings = className != "PropertyTable";
                string line = DataTableUtilities.DataTableToText(table, 0, "  ", showHeadings);
                writer.WriteLine(line);
            }
        }

        /// <summary>
        /// Write the specified table to the TextWriter.
        /// </summary>
        /// <param name="writer">The writer to write to</param>
        /// <param name="table">The table to write</param>
        /// <param name="html">Indicates whether html format should be produced</param>
        /// <param name="includeHeadings">Include headings in the html table produced?</param>
        /// <param name="className">The class name of the generated html table</param>
        private static void WriteMessageTable(TextWriter writer, DataTable table, bool html, bool includeHeadings, string className)
        {
            foreach (DataRow row in table.Rows)
            {
                if (html)
                {
                    writer.WriteLine("<h3>" + row[0] + "</h3>");
                }
                else
                {
                    writer.WriteLine();
                    writer.WriteLine();
                    writer.WriteLine(row[0].ToString());
                }

                string st = row[1].ToString();
                st = st.Replace("\t", "    ");
                if (html)
                {
                    st = st.Replace("\r", string.Empty);
                    st = st.Replace("\n", "<br/>");
                    st = st.Replace("<br/>", "<br/>\r\n");
                }
                else
                {
                    st = StringUtilities.IndentText(st, 4);
                }

                writer.WriteLine(st);
            }
        }

        /// <summary>
        /// Create an initial conditions table in the DataStore.
        /// </summary>
        /// <param name="simulation">The simulation to create an table for</param>
        private static void CreateInitialConditionsTable(Simulation simulation)
        {
            // Create our initial conditions table.
            DataTable initialConditionsTable = new DataTable("InitialConditions");
            initialConditionsTable.Columns.Add("ModelPath", typeof(string));
            initialConditionsTable.Columns.Add("Name", typeof(string));
            initialConditionsTable.Columns.Add("Description", typeof(string));
            initialConditionsTable.Columns.Add("DataType", typeof(string));
            initialConditionsTable.Columns.Add("Units", typeof(string));
            initialConditionsTable.Columns.Add("DisplayFormat", typeof(string));
            initialConditionsTable.Columns.Add("Total", typeof(int));
            initialConditionsTable.Columns.Add("Value", typeof(string));

            initialConditionsTable.Rows.Add(
                new object[] { Apsim.FullPath(simulation), "Simulation name", "Simulation name", "String", string.Empty, string.Empty, false, simulation.Name });

            // Get all model properties and store in 'initialConditionsTable'
            foreach (Model model in Apsim.FindAll(simulation))
            {
                string relativeModelPath = Apsim.FullPath(model).Replace(Apsim.FullPath(simulation) + ".", string.Empty);
                List<VariableProperty> properties = new List<VariableProperty>();

                FindAllProperties(model, properties);

                foreach (VariableProperty property in properties)
                {
                    string value = property.ValueWithArrayHandling.ToString();
                    if (value != string.Empty)
                    {
                        if (value != null && property.DataType == typeof(DateTime))
                        {
                            value = ((DateTime)property.Value).ToString("yyyy-MM-dd hh:mm:ss");
                        }

                        bool showTotal = !double.IsNaN(property.Total);

                        initialConditionsTable.Rows.Add(new object[]
                          { 
                              relativeModelPath, 
                              property.Name,
                              property.Description,
                              property.DataType.Name,
                              property.Units,
                              property.Format,
                              showTotal,
                              value
                          });
                    }
                }
            }

            // Write to data store.
            DataStore dataStore = new DataStore(simulation);
            dataStore.DeleteOldContentInTable(simulation.Name, "InitialConditions");
            dataStore.WriteTable(simulation.Name, "InitialConditions", initialConditionsTable);
            dataStore.Disconnect();
        }

        /// <summary>
        /// Find all properties from the model and fill this.properties.
        /// </summary>
        /// <param name="model">The model to search for properties</param>
        /// <param name="properties">The list of properties to fill</param>
        private static void FindAllProperties(Model model, List<VariableProperty> properties)
        {
            if (model != null)
            {
                foreach (PropertyInfo property in model.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy))
                {
                    // Properties must have a [Summary] attribute
                    bool includeProperty = property.IsDefined(typeof(SummaryAttribute), false);

                    if (includeProperty)
                    {
                        properties.Add(new VariableProperty(model, property));
                    }
                }
            }
        }

        /// <summary>
        /// Converts a flat 'InitialConditions' table (from the data store) to a series of data tables.
        /// </summary>
        /// <param name="initialConditionsTable">The table to read the rows from</param>
        /// <param name="tables">The list of tables to create</param>
        private static void ConvertInitialConditionsToTables(DataTable initialConditionsTable, List<DataTable> tables)
        {
            DataTable propertyDataTable = null;
            DataTable generalDataTable = null;
            string previousModel = null;
            foreach (DataRow row in initialConditionsTable.Rows)
            {
                string modelPath = row["ModelPath"].ToString();

                // If this is a new model then write a new section for it.
                if (modelPath != previousModel)
                {
                    // Add a new properties table for this model.
                    propertyDataTable = new DataTable(modelPath);
                    propertyDataTable.Columns.Add("Name", typeof(string));
                    propertyDataTable.Columns.Add("Value", typeof(string));
                    tables.Add(propertyDataTable);

                    // Add a new data table for this model.
                    generalDataTable = new DataTable("General " + modelPath);
                    tables.Add(generalDataTable);

                    previousModel = modelPath;
                }

                // Work out the property name.
                string propertyName = row["Description"].ToString();
                if (propertyName == string.Empty)
                    propertyName = row["Name"].ToString();
                string units = row["Units"].ToString();
                string displayFormat = row["DisplayFormat"].ToString();

                // If the data type is an array then write the general datatable.
                if (row["DataType"].ToString().Contains("[]"))
                {
                    if (units != null && units != string.Empty)
                    {
                        propertyName += " (" + units + ")";
                    }

                    bool showTotal = Convert.ToInt32(row["Total"]) == 1;
                    AddArrayToTable(propertyName, row["DataType"].ToString(), displayFormat, showTotal, row["Value"], generalDataTable);
                }
                else
                {
                    string value = FormatPropertyValue(row["DataType"].ToString(), row["Value"], displayFormat);
                    if (units != null && units != string.Empty)
                    {
                        value += " (" + units + ")";
                    }

                    propertyDataTable.Rows.Add(new object[] 
                    {
                        propertyName + ": ",
                        value
                    });
                }
            }
        }

        /// <summary>
        /// Add a column to the specified table based on values in the 'value'
        /// </summary>
        /// <param name="heading">The new column heading</param>
        /// <param name="dataTypeName">The data type of the value</param>
        /// <param name="displayFormat">The display format to use when writing the column</param>
        /// <param name="showTotal">A value indicating whether a total should be added.</param>
        /// <param name="value">The values containing the array</param>
        /// <param name="table">The table where a column should be added to</param>
        private static void AddArrayToTable(string heading, string dataTypeName, string displayFormat, bool showTotal, object value, DataTable table)
        {
            if (displayFormat == null)
            {
                displayFormat = "N3";
            }

            string[] stringValues = value.ToString().Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (dataTypeName == "Double[]")
            {
                List<double> values = new List<double>();
                values.AddRange(MathUtilities.StringsToDoubles(stringValues));
                if (showTotal)
                {
                    values.Add(MathUtilities.Sum(values));
                }

                stringValues = MathUtilities.DoublesToStrings(values, displayFormat);
            }
            else if (dataTypeName == "Int32[]")
            {
                List<double> values = new List<double>();
                values.AddRange(MathUtilities.StringsToDoubles(stringValues));
                if (showTotal)
                {
                    values.Add(MathUtilities.Sum(values));
                }

                stringValues = MathUtilities.DoublesToStrings(values, "N0");
            }
            else if (dataTypeName != "String[]")
            {
                throw new ApsimXException(null, "Invalid property type: " + dataTypeName);
            }

            DataTableUtilities.AddColumn(table, heading, stringValues);
        }

        /// <summary>
        /// Format the specified value into a string and return the string.
        /// </summary>
        /// <param name="dataTypeName">The name of the data type</param>
        /// <param name="value">The value to format</param>
        /// <param name="format">The format to use for the value</param>
        /// <returns>The formatted value as a string</returns>
        private static string FormatPropertyValue(string dataTypeName, object value, string format)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (dataTypeName == "Double" || dataTypeName == "Single")
            {
                double doubleValue = Convert.ToDouble(value);
                if (format == null || format == string.Empty)
                {
                    return string.Format("{0:F3}", doubleValue);
                }
                else
                {
                    return string.Format("{0:" + format + "}", doubleValue);
                }
            }
            else if (dataTypeName == "DateTime")
            {
                DateTime date = DateTime.ParseExact(value.ToString(), "yyyy-MM-dd hh:mm:ss", null);
                return date.ToString("yyyy-MM-dd");
            }
            else
            {
                return value.ToString();
            }
        }

        #endregion

        /// <summary>
        /// Simulation is commencing.
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        [EventSubscribe("DoInitialSummary")]
        private void OnDoInitialSummary(object sender, EventArgs e)
        {
            // Create our Messages table.
            this.messagesTable = new DataTable("Messages");
            this.messagesTable.Columns.Add("ComponentName", typeof(string));
            this.messagesTable.Columns.Add("Date", typeof(DateTime));
            this.messagesTable.Columns.Add("Message", typeof(string));
            this.messagesTable.Columns.Add("MessageType", typeof(int));

            // Create an initial conditions table in the DataStore.
            CreateInitialConditionsTable(Simulation);
        }
    }
}
