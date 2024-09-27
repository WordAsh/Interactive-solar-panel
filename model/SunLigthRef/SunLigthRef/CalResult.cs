using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SunLigthRef
{
    public class CalResult : GH_Component
    {
        public CalResult()
          : base("CalResult", "CR",
              "Calculate the reflected region and total reflected energy value.",
              "SunLightRef", "Primitive")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddRectangleParameter("Panels", "P", "Reflected panels.", GH_ParamAccess.list);
            pManager.AddLineParameter("InRays","IR", "The incident light path.",GH_ParamAccess.list);
            pManager.AddNumberParameter("Params", "P", "The cosine product of angle of incidence and the angle of reflection.", GH_ParamAccess.list);
            pManager.AddRectangleParameter("SunPanel","SP","The region of sun panel.",GH_ParamAccess.item);
        }
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Region","R","Reflected regions on sun panel.",GH_ParamAccess.list);
            pManager.AddNumberParameter("Energy","E","Total reflected energy.",GH_ParamAccess.item);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Rectangle3d> panels = new List<Rectangle3d>();
            List<Line> rays = new List<Line>();
            List<double> parameters = new List<double>();
            Rectangle3d border = new Rectangle3d();

            if (!DA.GetDataList(0, panels)) return;
            if(!DA.GetDataList(1, rays)) return;
            if(!DA.GetDataList(2, parameters)) return;
            if(!DA.GetData(3, ref border)) return;

            var plane = border.Plane; //Calculate the plane where the reflector resides
            var crvs = new List<Curve>();
            var areaProjected = new List<double>(); //Calculated projected total area

            var rec1 = GetRegionOnPlane(plane, rays.Take(4).ToList());
            var rec2 = GetRegionOnPlane(plane, rays.Skip(4).ToList());

            areaProjected.Add(rec1.Area);
            areaProjected.Add(rec2.Area);

            crvs.Add(rec1.ToNurbsCurve());
            crvs.Add(rec2.ToNurbsCurve());//Get initial region

            for (int i = 0; i < crvs.Count; i++)
            {
                if (IsRegionValid(crvs[i], border, plane))
                    continue;
                else
                {
                    Curve curve;
                    CreateCommonRegion(crvs[i], border, out curve);
                    crvs[i] = curve;
                }
            }

            var energy = ComputeEnergy(parameters, crvs, areaProjected);

            //Output
            DA.SetDataList(0,crvs);
            DA.SetData(1, energy);
        }


        public Rectangle3d GetRegionOnPlane(Plane plane, List<Line> lines)
        {
            //Create reflection area
            var pts = new List<Point3d>();
            foreach (var line in lines)
            {
                double t;
                Rhino.Geometry.Intersect.Intersection.LinePlane(line, plane, out t);
                pts.Add(line.PointAt(t));
            }
            return new Rectangle3d(plane, pts[0], pts[2]);

        }

        public bool IsRegionValid(Curve crv, Rectangle3d border, Plane plane)
        {
            //Check that the projection area is completely inside the PV panel
            var result = Curve.PlanarClosedCurveRelationship(crv, border.ToNurbsCurve(), plane, 0.01);
            if (result == RegionContainment.AInsideB) return true;
            return false;
        }

        public void CreateCommonRegion(Curve crv, Rectangle3d border, out Curve curve)
        {
            //The projection area intersects the border and returns the intersection area
            var crvs = new List<Curve>();
            crvs.Add(crv);
            crvs.Add(border.ToNurbsCurve());
            curve = Curve.CreateBooleanIntersection(crv, border.ToNurbsCurve(), 0.01)[0];

        }

        public double ComputeEnergy(List<double> param, List<Curve> crvs, List<double> areas)
        {
            //Calculate the energy obtained by photovoltaic panels: energy = cos(alpha1) * cos(alpha2) * (S_accept / S_project)
            double sum = 0;
            for (int i = 0; i < crvs.Count; i++)
            {
                var area = AreaMassProperties.Compute(crvs[i]).Area;
                sum += param[i] * area / areas[i];
            }
            return sum;
        }
        protected override System.Drawing.Bitmap Icon => Properties.Resources.cal_reflect;

        public override Guid ComponentGuid
        {
            get { return new Guid("9B5C4245-E180-479B-9ED4-D1DC9105EBC1"); }
        }
    }
}