using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace SunLigthRef
{
    public class BaseRotateAxis : GH_Component
    {

        public BaseRotateAxis()
          : base("BaseRotateAxis", "BRA",
            "Create first reflect panel rotate axis,based on its vertical distance to sun panel's bottom line and its horizontal distance to sun panel",
            "SunLightRef", "Primitive")
        {
        }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("BaseLine", "BL", "Bottom line of sun panel", GH_ParamAccess.item);
            pManager.AddNumberParameter("VertDist", "VD", "Vertical distance to sun panel", GH_ParamAccess.item);
            pManager.AddNumberParameter("HoriDist","HD","Horizontal distance to sun panel",GH_ParamAccess.item);
            
        }
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("BaseAxis", "BA", "First reflect panel's rotate axis", GH_ParamAccess.item);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Line line = new Line();
            double vertDist = 0.0;
            double horiDist = 0.0;
            

            if (!DA.GetData(0, ref line)) return;
            if (!DA.GetData(1, ref vertDist)) return;
            if (!DA.GetData(2, ref horiDist)) return;

            var trans1 = Transform.Translation(new Vector3d(0, 0, -vertDist));
            var trans2 = Transform.Translation(new Vector3d(0, -horiDist, 0));

            line.Transform(trans1);
            line.Transform(trans2);

            var axis = line;
            DA.SetData(0, axis);
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;
        protected override System.Drawing.Bitmap Icon => Properties.Resources.base_axis;
        public override Guid ComponentGuid => new Guid("f5da1a75-6492-47ea-ac1e-57e6a9745a70");
    }
}