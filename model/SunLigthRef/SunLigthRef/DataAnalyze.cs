using System;
using System.Collections;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SunLigthRef
{
    public class DataAnalyze : GH_Component
    {
        public DataAnalyze()
          : base("DataAnalyze", "DA",
              "Analyze the emulation data.",
              "SunLightRef", "Primitive")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("OptimalDataSet","ODS","Optimal data set.",GH_ParamAccess.list);
            pManager.AddIntegerParameter("Altitudes","A","Altitudes",GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Length","LD","Rods optimal length",GH_ParamAccess.item);
            pManager.AddTextParameter("OptimalRod1Length", "ORL", "Optimal rod1 length.", GH_ParamAccess.list);
            pManager.AddTextParameter("OptimalRod2Length", "ORL", "Optimal rod2 length.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<string> data = new List<string>();
            int angle = 0;

            if (!DA.GetDataList(0, data)) return;
            if (!DA.GetData(1, ref angle)) return;

            var length1 = new List<String>();
            var length2 = new List<String>();

            foreach (var i in data)
            {
                var s = i.Split(' ');
                length1.Add(s[1]);
                length2.Add(s[2]);
            }

            var index = angle - 34;
            var item = data[index];
            var result = item.Split(' ');

            var length = result[1] + "  " + result[2];

            DA.SetData(0, length);
            DA.SetDataList(1, length1);
            DA.SetDataList(2, length2);
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.data_analyze;


        public override Guid ComponentGuid
        {
            get { return new Guid("33CFCCF1-FACF-4C5B-8B3B-89BE36FB8730"); }
        }
    }
}