﻿// -----------------------------------------------------------------------
// <copyright file="TestPresenter.cs"  company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
// -----------------------------------------------------------------------

namespace UserInterface.Presenters
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using EventArguments;
    using Interfaces;
    using Models;
    using Models.Core;
    using Views;
    using APSIM.Shared.Utilities;

    /// <summary>
    /// The presenter for the Test object
    /// </summary>
    public class TestPresenter : IPresenter
    {
        /// <summary>
        /// The view object
        /// </summary>
        private ITestView view;

        /// <summary>
        /// The tests object
        /// </summary>
        private Tests tests;

        /// <summary>
        /// The explorer presenter used
        /// </summary>
        private ExplorerPresenter explorerPresenter;

        /// <summary>
        /// The data storage object
        /// </summary>
        private DataStore dataStore = null;

        /// <summary>
        /// Attach the model to the view.
        /// </summary>
        /// <param name="model">The model object</param>
        /// <param name="view">The view used</param>
        /// <param name="explorerPresenter">The explorer presenter</param>
        public void Attach(object model, object view, ExplorerPresenter explorerPresenter)
        {
            this.view = view as ITestView;
            this.tests = model as Tests;
            this.explorerPresenter = explorerPresenter;

            this.dataStore = Apsim.Find(this.tests, typeof(DataStore)) as DataStore;
            this.view.Editor.IntelliSenseChars = " :";
            this.view.Editor.ContextItemsNeeded += this.OnContextItemsNeeded;
            this.view.TableNameChanged += this.OnTableNameChanged;

            this.PopulateView();
            this.explorerPresenter.CommandHistory.ModelChanged += this.OnModelChanged;
        }

        /// <summary>
        /// Detach the model from the view.
        /// </summary>
        public void Detach()
        {
            this.view.Editor.ContextItemsNeeded -= this.OnContextItemsNeeded;
            this.explorerPresenter.CommandHistory.ModelChanged -= this.OnModelChanged;
            this.SaveViewToModel();
        }

        /// <summary>
        /// Populate the grid
        /// </summary>
        private void PopulateView()
        {
            // Populate the table list.
            if (this.dataStore != null)
            {
                this.view.TableNames = this.dataStore.TableNames;

                // Set the name of the table.
                if (this.tests.AllTests != null && this.tests.AllTests.Length > 0)
                {
                    this.view.TableName = this.tests.AllTests[0].TableName;
                    this.view.Data = this.dataStore.GetData("*", this.tests.AllTests[0].TableName);
                }
            }

            if (this.tests.AllTests != null && this.tests.AllTests.Length > 0)
            {
                // Work out the test strings that we're going to pass to our view
                List<string> testStrings = new List<string>();
                foreach (Test test in this.tests.AllTests)
                {
                    testStrings.Add(this.TestToString(test));
                }

                this.view.Editor.Lines = testStrings.ToArray();
                this.view.Editor.SetSyntaxHighlighter("Test");
            }
        }

        /// <summary>
        /// Save the state of the view back to the model class.
        /// </summary>
        private void SaveViewToModel()
        {
            // The ChangePropertyCommand below will trigger a call to OnModelChanged. We don't need to 
            // repopulate the grid so stop the event temporarily until end of this method.
            this.explorerPresenter.CommandHistory.ModelChanged -= this.OnModelChanged;

            List<Test> tests = new List<Test>();
            for (int lineNumber = 0; lineNumber < this.view.Editor.Lines.Length; lineNumber++)
            {
                Test test = this.StringToTest(lineNumber);
                if (test != null)
                {
                    tests.Add(test);
                }
            }

            // Store 'tests' in model via a command.
            Commands.ChangeProperty command = new Commands.ChangeProperty(this.tests, "AllTests", tests.ToArray());
            this.explorerPresenter.CommandHistory.Add(command, true);

            // Reinstate the model changed event.
            this.explorerPresenter.CommandHistory.ModelChanged += this.OnModelChanged;
        }

        /// <summary>
        /// Convert a test type to an operator string.
        /// </summary>
        /// <param name="test">The test type</param>
        /// <returns>The operator string</returns>
        private string TestToString(Test test)
        {
            string testString = "Simulation:" + test.SimulationName + " " +
                                "     Test:" + test.ColumnNames + " ";

            string[] parameterBits = null;
            if (test.Parameters != null)
            {
                parameterBits = test.Parameters.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            }
            if (test.Type == Test.TestType.EqualTo)
            {
                testString += " = ";
                if (parameterBits.Length > 0)
                {
                    testString += parameterBits[0];
                }
            }
            else if (test.Type == Test.TestType.LessThan)
            {
                testString += " < ";
                if (parameterBits.Length > 0)
                {
                    testString += parameterBits[0];
                }
            }
            else if (test.Type == Test.TestType.GreaterThan)
            {
                testString += " > ";
                if (parameterBits.Length > 0)
                {
                    testString += parameterBits[0];
                }
            }
            else if (test.Type == Test.TestType.Between)
            {
                testString += " between ";
                if (parameterBits.Length > 1)
                {
                    testString += parameterBits[0] + " " + parameterBits[1];
                }
            }
            else if (test.Type == Test.TestType.AllPos)
            {
                testString += " AllPositive ";
            }
            else if (test.Type == Test.TestType.Mean)
            {
                testString += " mean= ";
                if (parameterBits.Length > 1)
                {
                    testString += parameterBits[1] + " " + parameterBits[0] + "%";
                }
            }
            else if (test.Type == Test.TestType.Tolerance)
            {
                testString += " tolerance= ";
                if (parameterBits.Length > 1)
                {
                    testString += parameterBits[0] + "%";
                }
            }
            else if (test.Type == Test.TestType.CompareToInput)
            {
                testString += " CompareToInput= ";
                if (parameterBits.Length > 1)
                {
                    testString += parameterBits[0] + "%";
                }
            }

            return testString;
        }
        
        /// <summary>
        /// Convert a line from the view to a test.
        /// </summary>
        /// <param name="lineNumber">The line number to examine</param>
        /// <returns>The newly created test or null if line not valid</returns>
        private Test StringToTest(int lineNumber)
        {
            string simulationName = this.GetWordFromLine(lineNumber, "Simulation:", false);
            string testString = this.GetWordFromLine(lineNumber, "Test:", true);
            if (simulationName != null && testString != null)
            {
                string[] testBits = testString.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (testBits.Length >= 2)
                {
                    Test test = new Test();
                    test.SimulationName = simulationName;
                    test.TableName = this.view.TableName;
                    test.ColumnNames = testBits[0];
                    string operatorString = testBits[1];
                    string[] parameterBits = null;
                    if (testBits.Length > 2)
                    {
                        parameterBits = testBits[2].Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    }

                    if (testBits.Length == 4)
                    {
                        Array.Resize(ref parameterBits, parameterBits.Length + 1);
                        parameterBits[parameterBits.Length - 1] = testBits[3];
                    }
                    if (operatorString == "=")
                    {
                        test.Type = Test.TestType.EqualTo;
                        if (parameterBits.Length > 0)
                        {
                            test.Parameters = parameterBits[0];
                        }
                    }
                    else if (operatorString == "<")
                    {
                        test.Type = Test.TestType.LessThan;
                        if (parameterBits.Length > 0)
                        {
                            test.Parameters = parameterBits[0];
                        }
                    }
                    else if (operatorString == ">")
                    {
                        test.Type = Test.TestType.GreaterThan;
                        if (parameterBits.Length > 0)
                        {
                            test.Parameters = parameterBits[0];
                        }
                    }
                    else if (operatorString == "between")
                    {
                        test.Type = Test.TestType.Between;
                        if (parameterBits.Length > 1)
                        {
                            test.Parameters = parameterBits[0] + "," + parameterBits[1];
                        }
                    }
                    else if (operatorString == "AllPositive")
                    {
                        test.Type = Test.TestType.AllPos;
                    }
                    else if (operatorString == "mean=")
                    {
                        test.Type = Test.TestType.Mean;
                        if (parameterBits.Length > 1)
                        {
                            test.Parameters = parameterBits[1].Replace("%", string.Empty) + "," + parameterBits[0];
                        }
                    }
                    else if (operatorString == "tolerance=")
                    {
                        test.Type = Test.TestType.Tolerance;
                        if (parameterBits.Length > 0)
                        {
                            test.Parameters = "1," + parameterBits[0].Replace("%", string.Empty);
                        }
                    }
                    else if (operatorString == "CompareToInput=")
                    {
                        test.Type = Test.TestType.Tolerance;
                        if (parameterBits.Length > 0)
                        {
                            test.Parameters = "1," + parameterBits[0].Replace("%", string.Empty);
                        }
                    }
                    
                    return test;
                }
            }
            return null;
        }

        /// <summary>
        /// Invoked when the view wants context items.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        public void OnContextItemsNeeded(object sender, NeedContextItemsArgs e)
        {
            if (e.ObjectName.Trim() == "Simulation")
            {
                e.AllItems.Add(new NeedContextItemsArgs.ContextItem() { Name = "All" });

                foreach (string simulationName in this.dataStore.SimulationNames)
                    e.AllItems.Add(new NeedContextItemsArgs.ContextItem() { Name = simulationName });
            }
            else if (e.ObjectName.Trim() == "Test")
            {
                string tableName = this.view.TableName;
                if (tableName != null)
                {
                    DataTable data = this.dataStore.GetData("*", tableName);
                    if (data != null)
                    {
                        foreach (string columnName in DataTableUtilities.GetColumnNames(data))
                            e.AllItems.Add(new NeedContextItemsArgs.ContextItem() { Name = columnName });
                    }
                }
            }
            else
            {
                string simulationName = this.GetWordFromLine(this.view.Editor.CurrentLineNumber, "Simulation:", false);
                string testName = this.GetWordFromLine(this.view.Editor.CurrentLineNumber, "Test:", false);
                if (simulationName != null && testName != null)
                {
                    e.AllItems.Add(new NeedContextItemsArgs.ContextItem() { Name = "=" });
                    e.AllItems.Add(new NeedContextItemsArgs.ContextItem() { Name = "<" });
                    e.AllItems.Add(new NeedContextItemsArgs.ContextItem() { Name = ">" });
                    e.AllItems.Add(new NeedContextItemsArgs.ContextItem() { Name = "AllPositive" });
                    e.AllItems.Add(new NeedContextItemsArgs.ContextItem() { Name = "between" });
                    e.AllItems.Add(new NeedContextItemsArgs.ContextItem() { Name = "mean=" });
                    e.AllItems.Add(new NeedContextItemsArgs.ContextItem() { Name = "tolerance=" });
                    e.AllItems.Add(new NeedContextItemsArgs.ContextItem() { Name = "CompareToInput=" });
                }
            }
        }

        /// <summary>
        /// Return the name of the table from the specified line.
        /// </summary>
        /// <param name="lineNumber">The line number to parse</param>
        /// <param name="keyWord">The keyword</param>
        /// <param name="toEndOfLine">If true then the remainder of the line will be returned, otherwise a space is used as a delimiter</param>
        /// <returns>Returns the table name or null if none was specified</returns>
        private string GetWordFromLine(int lineNumber, string keyWord, bool toEndOfLine)
        {
            if (lineNumber < this.view.Editor.Lines.Length)
            {
                string line = this.view.Editor.Lines[lineNumber];
                int pos = line.IndexOf(keyWord);
                if (pos != -1)
                {
                    pos += keyWord.Length; // get past 'TableName'
                    int posSpace;
                    if (toEndOfLine)
                    {
                        posSpace = line.Length;
                    }
                    else
                    {
                        posSpace = line.IndexOf(' ', pos);
                    }
                    if (posSpace == -1)
                    {
                        posSpace = line.Length - 1;
                    }

                    return line.Substring(pos, posSpace - pos).Trim();
                }
            }

            return null;
        }

        /// <summary>
        /// The model has changed, update the grid.
        /// </summary>
        /// <param name="changedModel">The changed model</param>
        private void OnModelChanged(object changedModel)
        {
            if (changedModel == this.tests)
            {
                this.PopulateView();
            }
        }

        /// <summary>
        /// User has changed the table name.
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void OnTableNameChanged(object sender, EventArgs e)
        {
            this.view.Data = this.dataStore.GetData("*", this.view.TableName);
        }
    }
}
