﻿namespace AST.ContentConveyor7.DataTypeConverters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;

    using AST.ContentConveyor7;
    using AST.ContentConveyor7.Enums;
    using AST.ContentConveyor7.Utilities;

    using Umbraco.Core.Models;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Type Converter for a Nested Content field: recursively traverses the JSON data
    /// and invokes other Type Converters when appropriate
    /// </summary>
    public class NestedContentDataTypeConverter : BaseNestedTypeConverter, IDataTypeConverter
    {
        /// <summary>
        /// Export conversion
        /// </summary>
        public void Export(Property property, XElement propertyTag, Dictionary<int, ObjectTypes> dependantNodes)
        {
            string jsonText = (property.Value ?? "").ToString();
            string newText = ProcessAll(jsonText, OperationType.Export, dependantNodes);

            if (propertyTag.FirstNode is XCData)
            {
                // JSON was wrapped in CDATA tag - should do the same 
                propertyTag.RemoveNodes();
                propertyTag.Add(new XCData(newText));
            }
            else
            {
                // no CDATA (i.e. nested Nested Content)
                propertyTag.Value = newText;
            }
        }

        /// <summary>
        /// Import conversion
        /// </summary>
        public string Import(XElement propertyTag)
        {
            string jsonText = (propertyTag.Value ?? "").ToString();
            return ProcessAll(jsonText, OperationType.Import, null);
        }

        /// <summary>
        /// Parse and process the full Nested Content JSON, for an Export or Import operation 
        /// </summary>
        private string ProcessAll(string jsonText, OperationType operation, Dictionary<int, ObjectTypes> dependantNodes)
        {
            if (!string.IsNullOrWhiteSpace(jsonText))
            {
                // deserialize JSON into JToken graph
                var doc = JsonConvert.DeserializeObject(jsonText);
                JArray jDocs;
                JObject jDoc;

                if ((jDocs = doc as JArray) != null)
                {
                    // process a multiple document Nested Component field
                    foreach (JObject jDoc1 in jDocs)
                    {
                        ProcessDocument(jDoc1, operation, dependantNodes);
                    }
                    // re-serialize the updated JSON be returned
                    jsonText = jDocs.ToString(Formatting.None);
                }
                else if ((jDoc = doc as JObject) != null)
                {
                    // process a single document Nested Component field
                    ProcessDocument(jDoc, operation, dependantNodes);

                    // re-serialize the updated JSON be returned
                    jsonText = jDoc.ToString(Formatting.None);
                }
            }
            return jsonText;
        }

        /// <summary>
        /// Parse and process the a single document inside the Nested Content JSON, for an Export or Import operation 
        /// </summary>
        private void ProcessDocument(JObject jDoc, OperationType operation, Dictionary<int, ObjectTypes> dependantNodes)
        {
            // prepare dictionary of property types in the current Nested Content document type
            string contentTypeAlias = jDoc["ncContentTypeAlias"].ToString();
            IContentType contentType = Services.ContentTypeService.GetContentType(contentTypeAlias);
            Dictionary<string, PropertyType> propertyTypeDictionary = contentType.CompositionPropertyTypes.ToDictionary(p => p.Alias);

            // lookup each property present in the JSON data
            foreach (JProperty jProperty in jDoc.Properties())
            {
                PropertyType propertyType;
                string value;
                if (!String.IsNullOrWhiteSpace(value = jProperty.Value.ToString())
                    && propertyTypeDictionary.TryGetValue(jProperty.Name, out propertyType))
                {
                    // this is an actual field of the nested document type
                    var guid = propertyType.DataTypeId;
                    string typeAlias;

                    // look for a custom TypeConverter for this field
                    if (SpecialDataTypes.TryGetValue(guid, out typeAlias))
                    {
                        // found! then wrap the value in a Property to recursively invoke the TypeConverter
                        var typeConverter = GetDataTypeConverterInterface(typeAlias);

                        // invoke the Export / Import operation
                        string newValue = operation == OperationType.Export?
                            NestedExport(typeConverter, value, propertyType, dependantNodes):
                            NestedImport(typeConverter, value);
                        
                        // update the JSON graph with the converted value
                        jProperty.Value = newValue;
                    }
                }
            }
        }
    }
}