using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Runtime;

namespace SunLigthRef
{
    public class TransformUnit : GH_Component
    {
        public TransformUnit()
          : base("TransformUnit", "TU",
              "Transform the unit to position",
              "SunLightRef", "Primitive")
        {
        }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("OriginUnit", "OU", "Original unit.", GH_ParamAccess.list);
            pManager.AddCurveParameter("RotateAxis","RA","Unit rotate axis.",GH_ParamAccess.item);
            pManager.AddNumberParameter("Angle", "A", "Panel rotate angle in radius", GH_ParamAccess.item);

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("TransformedUnit", "TU", "Transformed unit.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Brep> unit = new List<Brep>();
            Curve axis = null;
            double angle = 0.0;

            if (!DA.GetDataList(0, unit)) return;
            if (!DA.GetData(1, ref axis)) return;
            if (!DA.GetData(2, ref angle)) return;

            var startPt = axis.PointAtStart;
            var endPt = axis.PointAtEnd;
            var axisLine = new Line(startPt, endPt);
            var breps = RotateUnit(unit, angle, axisLine);


            //Output
            DA.SetDataList(0, breps);
        }

        public List<Brep> RotateUnit(List<Brep> unit, double angle, Line axis)
        {
            //旋转反射板单元
            var breps = new List<Brep>();
            var center = new Point3d((axis.FromX + axis.ToX) / 2, axis.FromY, axis.FromZ);
            var vec = new Vector3d(1, 0, 0);//旋转轴向量
            foreach (Brep brep in unit)
            {
                brep.Rotate(-angle, vec, center);
                breps.Add(brep);
            }
            return breps;
        }
        protected override System.Drawing.Bitmap Icon => Properties.Resources.transform_unit;


        public override Guid ComponentGuid
        {
            get { return new Guid("1ED13388-769F-43FA-A073-EBB576929C83"); }
        }
    }
}