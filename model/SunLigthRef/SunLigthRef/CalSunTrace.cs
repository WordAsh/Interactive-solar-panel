using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SunLigthRef
{
    public class CalSunTrace : GH_Component
    {
        public CalSunTrace()
          : base("CalSunTrace", "CST",
              "Show the incident light and reflected light",
              "SunLightRef", "Primitive")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddRectangleParameter("Panels","P","Reflected panels.",GH_ParamAccess.list);
            pManager.AddIntegerParameter("Altitude","A","Solar altitude.",GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("InRays","IR","The incident light path.",GH_ParamAccess.list);
            pManager.AddLineParameter("ReflectedRays","RR","The reflected light path.",GH_ParamAccess.list);
            pManager.AddNumberParameter("Params","P","The cosine product of angle of incidence and the angle of reflection.",GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Rectangle3d> panels = new List<Rectangle3d>();
            int angle = 0;

            if(!DA.GetDataList(0, panels)) return;
            if(!DA.GetData(1, ref angle)) return;


            double[] param = new double[2];//太阳光入射角与反射光入射角余弦积
            var rays1 = new List<Line>();//反射光线
            var rays2 = new List<Line>();//入射光线

            for (int i = 0; i < panels.Count; i++)
            {
                Vector3d inVec;
                Vector3d outVec;
                GenerateInvecOutvec(angle, panels[i], out inVec, out outVec);

                inVec.Reverse();
                var alpha1 = Vector3d.VectorAngle(inVec, outVec, Plane.WorldYZ) / 2;//太阳光入射角
                var alpha2 = ComputeAngle(outVec);//反射光线入射角
                param[i] = Math.Cos(alpha1) * Math.Cos(alpha2);


                var lines1 = GenerateRay(panels[i], outVec);//生成反射光线
                var lines2 = GenerateRay(panels[i], inVec);//生成入射光线
                rays1.AddRange(lines1);
                rays2.AddRange(lines2);
            }

            //Output
            DA.SetDataList(0,rays2);
            DA.SetDataList(1,rays1);
            DA.SetDataList(2,param);
        }

        public void GenerateInvecOutvec(double angle, Rectangle3d rec, out Vector3d inVec, out Vector3d outVec)
        {
            //根据给定太阳高度角，生成入射向量，反射向量
            inVec = new Vector3d(0, 1, 0);
            inVec.Rotate(-Math.PI * angle / 180, new Vector3d(1, 0, 0));//入射向量
            var norm = rec.Plane.Normal;
            if (norm * inVec > 0)
            {
                norm.Reverse();
            }
            norm.Unitize();
            outVec = inVec - 2 * (inVec * norm) * norm;//反射向量

        }
        public List<Line> GenerateRay(Rectangle3d rec, Vector3d vec)
        {
            //生成指定向量方向的射线

            var rays = new List<Line>();
            for (int i = 0; i < 4; i++)
            {
                var pt = rec.Corner(i);
                var line = new Line(pt, vec, 10000);//10为设定射线长度
                rays.Add(line);
            }
            return rays;

        }
        public double ComputeAngle(Vector3d outVec)
        {
            //计算反射光线与光伏板的入射角
            var norm = new Vector3d(0, -1, 0);
            var outoutVec = outVec - 2 * (outVec * norm) * norm;//光伏板上的反射向量
            outVec.Reverse();
            return Vector3d.VectorAngle(outVec, outoutVec, Plane.WorldYZ) / 2;
        }
        protected override System.Drawing.Bitmap Icon => Properties.Resources.light_trace;


        public override Guid ComponentGuid
        {
            get { return new Guid("3C6A9913-DF64-44E8-A162-7DB061B26C52"); }
        }
    }
}