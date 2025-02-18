using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Media;
using System.IO;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Esri.Shapefiles;
using NetTopologySuite.IO;
using System.Collections.ObjectModel;
using CSVJoin.ViewModel;
using Microsoft.Win32;
using NetTopologySuite.IO.Esri.Shapefiles.Writers;
using NetTopologySuite.IO.Esri.Dbf.Fields;
using NetTopologySuite.IO.Esri;


namespace CSVJoin
{
    class GeoJsonIO
    {
        public List<IFeature> readGeoJSON(ObservableCollection<CombinedResult> combinedResults)
        {
            string filePath = "parking_lots.json";
            string json = File.ReadAllText(filePath);

            // Create a GeoJsonReader instance
            var geoJsonReader = new GeoJsonReader();

            // Parse the GeoJSON content into a FeatureCollection
            FeatureCollection featureCollection = geoJsonReader.Read<FeatureCollection>(json);
            // Create a dictionary to hold unique features by Park_ID
            var featureDict = new Dictionary<string, IFeature>();

            foreach (var feature in featureCollection)
            {
                var parkId = feature.Attributes["Park_ID"].ToString();

                // Only add the feature if the Park_ID is not already in the dictionary
                if (!featureDict.ContainsKey(parkId))
                {
                    featureDict.Add(parkId, feature);
                }
            }

            // Perform inner join
            var joinedFeatures = combinedResults
                .Where(data => (data.ParkID != null && featureDict.ContainsKey(data.ParkID))
                            || (data.GisParkID != null && featureDict.ContainsKey(data.GisParkID))
                            || (data.ItsParkID != null && featureDict.ContainsKey(data.ItsParkID))) // Filter only matching keys
                .Select(data =>
                {
                    IFeature feature = null;
                    if (data.ParkID != null && featureDict.ContainsKey(data.ParkID))
                    {
                        feature = featureDict[data.ParkID];
                    }
                    else if (data.GisParkID != null && featureDict.ContainsKey(data.GisParkID))
                    {
                        feature = featureDict[data.GisParkID];
                    }
                    else if (data.ItsParkID != null && featureDict.ContainsKey(data.ItsParkID))
                    {
                        feature = featureDict[data.ItsParkID];
                    }

                    if (feature != null) { 
                        if (!feature.Attributes.Exists("ErrType")) { 
                            feature.Attributes.Add("ErrType", data.ErrType); 
                        } 
                    }
                    return feature;
                })
                .Where(f => f != null) // Filter out null features
                .ToList();
            return joinedFeatures;
        }

        public void saveGeoJSON(ObservableCollection<CombinedResult> combinedResults)
        {
            // Save the results using SaveFileDialog in WPF
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "GeoJSON files (*.geojson)|*.geojson|All files (*.*)|*.*",
                Title = "Save GeoJSON File",
                DefaultExt = "geojson"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var geoJsonWriter = new GeoJsonWriter();
                var joinedFeatures = readGeoJSON(combinedResults);
                string outputGeoJson = geoJsonWriter.Write(joinedFeatures);
                File.WriteAllText(saveFileDialog.FileName, outputGeoJson);
            }
        }

        private string GetUniqueFieldName(string fieldName, HashSet<string> existingFieldNames)
        {
            string originalFieldName = fieldName;
            int counter = 1;

            while (existingFieldNames.Contains(fieldName))
            {
                // Ensure the new field name is also within the 10 character limit
                string suffix = counter.ToString();
                int maxLength = 10 - suffix.Length;
                fieldName = originalFieldName.Substring(0, maxLength) + suffix;
                counter++;
            }

            existingFieldNames.Add(fieldName);
            return fieldName;
        }

        public void saveSHP(ObservableCollection<CombinedResult> combinedResults)
        {
            var features = readGeoJSON(combinedResults);

            // Define the fields based on the first feature's attributes
            var fields = new List<DbfField>();
            var existingFieldNames = new HashSet<string>();

            foreach (var attribute in features[0].Attributes.GetNames())
            {
                string fieldName = attribute.Length > 10 ? attribute.Substring(0, 10) : attribute;

                // Ensure the field name is unique
                fieldName = GetUniqueFieldName(fieldName, existingFieldNames);
                fields.AddCharacterField(fieldName);
            }

            var options = new ShapefileWriterOptions(ShapeType.Polygon, fields.ToArray());

            // Show SaveFileDialog
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Shapefile|*.shp",
                Title = "Save Shapefile"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string shapefilePath = saveFileDialog.FileName;

                using (var shpWriter = Shapefile.OpenWrite(shapefilePath, options))
                {
                    foreach (var feature in features)
                    {
                        shpWriter.Geometry = feature.Geometry;
                        foreach (var field in fields)
                        {
                            var attributeName = field.Name;
                            if (feature.Attributes.Exists(attributeName))
                            {
                                var attributeValue = feature.Attributes[attributeName];
                                if (field is DbfCharacterField dbfCharacterField)
                                {
                                    dbfCharacterField.StringValue = attributeValue.ToString();
                                }
                                else if (field is DbfNumericField dbfNumericField)
                                {
                                    dbfNumericField.Value = Convert.ToDouble(attributeValue);
                                }
                                else if (field is DbfLogicalField dbfLogicalField)
                                {
                                    dbfLogicalField.LogicalValue = Convert.ToBoolean(attributeValue);
                                }
                                else if (field is DbfDateField dbfDateField)
                                {
                                    dbfDateField.DateValue = Convert.ToDateTime(attributeValue);
                                }
                            }
                        }
                        shpWriter.Write();
                    }
                }

                // Write the projection information to a .prj file
                string prjPath = Path.ChangeExtension(shapefilePath, ".prj");
                string wgs84 = @"GEOGCS[""GCS_WGS_1984"",DATUM[""D_WGS_1984"",SPHEROID[""WGS_1984"",6378137.0,298.257223563]],PRIMEM[""Greenwich"",0.0],UNIT[""Degree"",0.0174532925199433]]";

                File.WriteAllText(prjPath, wgs84);
            }
        }

    }
}