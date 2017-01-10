﻿//The MIT License (MIT)

//Copyright (c) 2015 Thornton Tomasetti

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using System;
using System.Dynamic;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Timers;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Spectacles.GrasshopperExporter.Properties;

using Newtonsoft.Json;

namespace Spectacles.GrasshopperExporter
{
    public class Spectacles_MeshColoredVertices : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Spectacles_MeshColoredVertices class.
        /// </summary>
        public Spectacles_MeshColoredVertices()
            : base("Spectacles_MeshColoredVertices", "Spectacles_MeshColoredVertices",
                "Creates a Spectacles mesh and a material from a grasshopper mesh with color data.",
                "TT Toolbox", "Spectacles")
        {
        }

        public override GH_Exposure Exposure
        {
            get
            {
                //return GH_Exposure.hidden;
                return GH_Exposure.primary;
            }
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "A Grasshopper Mesh", GH_ParamAccess.item);
            pManager.AddTextParameter("Attribute Names", "[aN]", "Attribute Names", GH_ParamAccess.list);
            pManager[1].Optional = true;
            pManager.AddTextParameter("Attribute Values", "[aV]", "Attribute Values", GH_ParamAccess.list);
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Mesh Element", "Me", "Mesh element output to feed into scene compiler component", GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //local varaibles
            GH_Mesh mesh = null;
            System.Collections.Generic.List<GH_String> attributeNames = new System.Collections.Generic.List<GH_String>();
            System.Collections.Generic.List<GH_String> attributeValues = new System.Collections.Generic.List<GH_String>();
            Dictionary<string, object> attributesDict = new Dictionary<string, object>();

            //catch inputs and populate local variables
            if (!DA.GetData(0, ref mesh))
            {
                return;
            }
            if (mesh == null)
            {
                return;
            }
            DA.GetDataList(1, attributeNames);
            DA.GetDataList(2, attributeValues);
            if (attributeValues.Count != attributeNames.Count)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Please provide equal numbers of attribute names and values.");
                return;
            }

            //populate dictionary
            int i = 0;
            foreach (var a in attributeNames)
            {
                attributesDict.Add(a.Value, attributeValues[i].Value);
                i++;
            }


            //create json from mesh
            string outJSON = _Utilities.geoJSONColoredVertices(mesh.Value, attributesDict);


            Material material = new Material(MaterialWithVertexColors(), SpectaclesMaterialType.Mesh);
            Element e = new Element(outJSON, SpectaclesElementType.Mesh, material, new Layer("Default"));

            DA.SetData(0, e);
        }

        public string MaterialWithVertexColors()
        {
            dynamic JsonMat = new ExpandoObject();

            JsonMat.uuid = Guid.NewGuid();
            JsonMat.type = "MeshLambertMaterial";
            JsonMat.color = _Utilities.hexColor(new GH_Colour(System.Drawing.Color.White));
            JsonMat.ambient = _Utilities.hexColor(new GH_Colour(System.Drawing.Color.White));
            JsonMat.side = 2;
            JsonMat.vertexColors = 2;
            return JsonConvert.SerializeObject(JsonMat);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                return Resources.MESH_VERTICES;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{fe109f9b-75a5-404c-8945-42e1f530c9b0}"); }
        }
    }
}