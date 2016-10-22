namespace AST.ContentConveyor7.DataTypeConverters
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
    /// Type Converter for a Grid Editor field (v7): recursively traverses the JSON data
    /// and invokes other Type Converters when appropriate
    /// </summary>
    public class GridEditorDataTypeConverter : BaseNestedTypeConverter, IDataTypeConverter
    {

        /// <summary>
        /// Export conversion
        /// </summary>
        public void Export(Property property, XElement propertyTag, Dictionary<int, ObjectTypes> dependantNodes)
        {
            string jsonText = (property.Value ?? "").ToString();
            string newText = ProcessAll(jsonText, OperationTypes.Export, dependantNodes);

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
            return ProcessAll(jsonText, OperationTypes.Import, null);
        }

        /// <summary>
        /// Parse and process the full Grid JSON, for an Export or Import operation 
        /// </summary>
        private string ProcessAll(string jsonText, OperationTypes operation, Dictionary<int, ObjectTypes> dependantNodes)
        {

            if (!string.IsNullOrWhiteSpace(jsonText))
            {
                // deserialize JSON into JToken graph
                JObject jDoc = JsonConvert.DeserializeObject(jsonText) as JObject;
                JProperty jProp;

                JArray jSections;
                if (jDoc != null
                    && (jProp = jDoc.Property("sections")) != null
                    && (jSections = jProp.Value as JArray) != null
                    && jSections.Count() > 0)
                {
                    // iterate through grid sections
                    foreach (JObject jSection in jSections)
                    {
                        JArray jRows;
                        if ((jProp = jSection.Property("rows")) != null
                            && (jRows = jProp.Value as JArray) != null
                            && jRows.Count() > 0)
                        {
                            // iterate through section rows
                            foreach (JObject jRow in jRows)
                            {
                                JArray jAreas;
                                if ((jProp = jRow.Property("areas")) != null
                                    && (jAreas = jProp.Value as JArray) != null
                                    && jAreas.Count() > 0)
                                {
                                    // iterate through row areas
                                    foreach (JObject jArea in jAreas)
                                    {
                                        JArray jControls;
                                        if ((jProp = jArea.Property("controls")) != null
                                            && (jControls = jProp.Value as JArray) != null
                                            && jControls.Count() > 0)
                                        {
                                            // iterate through area controls
                                            foreach (JObject jControl in jControls)
                                            {
                                                JObject jValue = jControl.GetValue("value") as JObject;
                                                if (jValue != null)
                                                {
                                                    JProperty jProp2;
                                                    JObject jInnerValue;
                                                    if ((jProp = jValue.Property("dtgeContentTypeAlias")) != null
                                                        && (jProp2 = jValue.Property("value")) != null
                                                        && (jInnerValue = jProp2.Value as JObject) != null)
                                                    {
                                                        // process Document Type Grid Editor data
                                                        // using a public method of the Nested Content type converter
                                                        var converter = new NestedContentDataTypeConverter();
                                                        string contentTypeAlias = jProp.Value.ToString();
                                                        converter.ProcessDocument(contentTypeAlias, jInnerValue, operation, dependantNodes);
                                                    }
                                                    else
                                                    {
                                                        JObject jEditor = jControl.GetValue("editor") as JObject;
                                                        if (jEditor != null)
                                                        {
                                                            // process other "native" grid editors, i.e. Media
                                                            ProcessOtherEditors(jValue, jEditor, operation, dependantNodes);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                }
                            }
                        }
                    }
                    // re-serialize the updated JSON be returned
                    jsonText = jDoc.ToString(Formatting.Indented);
                }
            }
            return jsonText;
        }

        // Process other "native" grid editors, i.e. Media
        private void ProcessOtherEditors(JObject jValue, JObject jEditor, OperationTypes operation, Dictionary<int, ObjectTypes> dependantNodes)
        {
            JProperty jProp = jEditor.Property("alias");
            if (jProp != null)
            {
                string editorAlias = jProp.Value.ToString();
                if (editorAlias == "media")
                {
                    // media - invoke the MultipleMediaPickerDataTypeConverter
                    JProperty jIdProperty = jValue.Property("id");
                    if (jIdProperty != null)
                    {
                        string value = jIdProperty.Value.ToString();
                        var converter = new MultipleMediaPickerDataTypeConverter();

                        // invoke the Export / Import operation
                        string newValue = operation == OperationTypes.Export ?
                            converter.ExportValue(value, dependantNodes) :
                            converter.ImportValue(value);

                        // update the JSON graph with the converted value
                        jIdProperty.Value = newValue;
                    }
                }
            }
        }
    }
}