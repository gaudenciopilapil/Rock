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
using System.Linq;
using System.Web.Http;
using System.Web.Routing;
using Newtonsoft.Json;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public partial class MetricValuesController : IHasCustomRoutes
    {
        /// <summary>
        /// Adds the routes.
        /// </summary>
        /// <param name="routes">The routes.</param>
        public void AddRoutes( RouteCollection routes )
        {
            routes.MapHttpRoute(
                name: "MetricValuesControllerGetChartData",
                routeTemplate: "api/MetricValuesController/GetChartData/{metricId}",
                defaults: new
                {
                    controller = "MetricValues",
                    action = "GetChartData"
                } );
        }

        /// <summary>
        /// Gets the chart data in javascript literal notation. It is almost JSON, but with Dates as new Date(..') so that Google Charts can interpret the dates as dates
        /// See http://www.json.org/js.html which explains javascript literal notation vs JSON
        /// </summary>
        /// <param name="metricId">The metric identifier.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <returns></returns>
        public string GetChartData( int metricId, DateTime? startDate = null, DateTime? endDate = null, int? entityId = null )
        {
            var dataQry = this.Get().Where( a => a.MetricId == metricId );
            if (startDate.HasValue)
            {
                dataQry = dataQry.Where( a => a.MetricValueDateTime >= startDate.Value );
            }

            if ( endDate.HasValue )
            {
                dataQry = dataQry.Where( a => a.MetricValueDateTime < endDate.Value );
            }

            if ( entityId.HasValue )
            {
                dataQry = dataQry.Where( a => a.EntityId == entityId );
            }

            dataQry = dataQry.OrderBy( a => a.MetricValueDateTime );

            var dataList = dataQry.ToList().Select( a => new object[]
            {
                a.MetricValueDateTime,
                a.YValue,                        
                System.Web.HttpUtility.JavaScriptStringEncode(a.Note)
            } );

            var jsonText = JsonConvert.SerializeObject( dataList, new JsonSerializerSettings { Converters = new JsonConverter[] { new Rock.Reporting.Dashboard.ChartDateTimeJsonConverter() } } );
            return jsonText;
        }
    }
}