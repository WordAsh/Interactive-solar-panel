using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SunLigthRef
{
    public class GetJoint : GH_Component
    {
        public GetJoint()
          : base("GetJoint", "GJ",
              "Get joint ready to transform.",
              "SunLightRef", "Primitive")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("OriginJoint","OJ","Original joint.",GH_ParamAccess.list);
            pManager.AddNumberParameter("T", "T", "This parm controls the intersection position of rod and panel", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("MovedJoint", "MJ", "Ready joint.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Brep> joint = new List<Brep>();
            double t = 0.0;

            if(!DA.GetDataList(0, joint)) return;
            if (!DA.GetData(1, ref t)) return;

            var joint1 = MoveJoints(joint, t);


            //Output
            DA.SetDataList(0,joint1);
        }


        const double panelLength = 800;//反射板长度

        public List<Brep> MoveJoints(List<Brep> breps, double t)
        {
            //根据t值，移动铰接点
            var brepMoved = new List<Brep>();
            var distance = panelLength * t;

            var vec = new Vector3d(0, -distance, 0);
            var trans = Transform.Translation(vec);
            foreach (var brep in breps)
            {
                brep.Transform(trans);
                brepMoved.Add(brep);
            }
            return brepMoved;
        }
        protected override System.Drawing.Bitmap Icon => Properties.Resources.joint_ready;

        public override Guid ComponentGuid
        {
            get { return new Guid("6FFAA85C-338A-4871-A3C3-6A308E86BD85"); }
        }
    }
}