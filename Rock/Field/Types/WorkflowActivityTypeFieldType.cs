﻿// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to display a dropdown list of Defined Values for a specific Defined Type
    /// </summary>
    [Serializable]
    public class WorkflowActivityFieldType : FieldType
    {
        private const string CONTEXT_ITEM_KEY = "WorkflowType";
        private const string WORKFLOW_TYPE_KEY = "workflowtype";

        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            string formattedValue = string.Empty;

            Guid guid = value.AsGuid();
            if (!guid.IsEmpty())
            { 
                var workflowType = GetContextWorkflowType();
                if (workflowType != null)
                {
                    formattedValue = workflowType.ActivityTypes
                        .Where( a => a.Guid.Equals( guid ) )
                        .Select( a => a.Name )
                        .FirstOrDefault();
                }

                if (string.IsNullOrWhiteSpace(formattedValue))
                {
                    formattedValue = new WorkflowActivityTypeService(new RockContext()).Queryable()
                        .Where( a => a.Guid.Equals( guid ) )
                        .Select( a => a.Name )
                        .FirstOrDefault();
                }
            }

            return base.FormatValue( parentControl, formattedValue, null, condensed );

        }

        /// <summary>
        /// Creates the control(s) neccessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            ListControl editControl;

            editControl = new Rock.Web.UI.Controls.RockDropDownList { ID = id };
            editControl.Items.Add( new ListItem() );

            IEnumerable<WorkflowActivityType> activityTypes = null;

            var workflowType = GetContextWorkflowType();
            if ( workflowType != null )
            {
                activityTypes = workflowType.ActivityTypes;
            }

            if (activityTypes == null && configurationValues != null && configurationValues.ContainsKey( WORKFLOW_TYPE_KEY ) )
            {
                Guid workflowTypeGuid = configurationValues[WORKFLOW_TYPE_KEY].Value.AsGuid();
                if ( !workflowTypeGuid.IsEmpty() )
                {
                    activityTypes = new WorkflowActivityTypeService( new RockContext() )
                        .Queryable()
                        .Where( t => t.WorkflowType.Guid.Equals( workflowTypeGuid ) );
                }
            }

            if (activityTypes != null && activityTypes.Any() )
            {
                foreach ( var activityType in activityTypes.OrderBy( a => a.Order ) )
                {
                    editControl.Items.Add( new ListItem( activityType.Name, activityType.Guid.ToString() ) );
                }
            }

            return editControl;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues"></param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( control != null && control is RockDropDownList )
            {
                return ( (RockDropDownList)control ).SelectedValue;
            }

            return string.Empty;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues"></param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            if ( value != null )
            {
                if ( control != null && control is RockDropDownList )
                {
                    ( (RockDropDownList)control ).SetValue( value );
                }
            }
        }

        private WorkflowType GetContextWorkflowType()
        {
            var httpContext = System.Web.HttpContext.Current;
            if ( httpContext != null && httpContext.Items != null )
            {
                return httpContext.Items[CONTEXT_ITEM_KEY] as WorkflowType;
            }

            return null;
        }

    }
}