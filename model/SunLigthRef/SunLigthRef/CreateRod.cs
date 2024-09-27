using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SunLigthRef
{
    public class CreateRod : GH_Component
    {
        public CreateRod()
          : base("CreateRod", "CR",
              "Create two rods",
              "SunLightRef", "Primitive")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("RodLine1","RL","First rod line.",GH_ParamAccess.item);
            pManager.AddLineParameter("RodLine2", "RL", "Second rod line.", GH_ParamAccess.item);
        }
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Rod1","R","Rod1",GH_ParamAccess.list);
            pManager.AddBrepParameter("Rod2", "R", "Rod2", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Line rod1 = new Line();
            Line rod2 = new Line();

            if (!DA.GetData(0, ref rod1)) return;
            if(!DA.GetData(1, ref rod2)) return;

            var rod1Pipe = CreateRods(rod1, rod1BaseLength);
            var rod2Pipe = CreateRods(rod2, rod2BaseLength);


            DA.SetDataList(0,rod1Pipe);
            DA.SetDataList(1,rod2Pipe);
        }

        const double rod1BaseLength = 205;//伸缩杆1基础长度
        const double rod2BaseLength = 455;//伸缩杆2基础长度
        const double distance1 = 25; //伸缩杆基础长度中细杆长度部分
        const double distance2 = 14; //伸缩杆粗杆底部长度
        const double radius1 = 18; //伸缩杆粗杆半径
        const double radius2 = 10;//伸缩杆细杆半径

        public List<Brep> CreateRods(Line rod, double rodBaseLength)
        {
            //创建伸缩杆
            var breps = new List<Brep>();
            var startPt = rod.From;
            var vec = rod.Direction;
            vec.Unitize();

            var breakPt1 = startPt + vec * (rodBaseLength - distance1);//伸缩杆粗杆与细杆分割点
            var breakPt2 = startPt + vec * distance2; //伸缩杆粗杆底部出头部分

            var seg1 = new Line(breakPt1, breakPt2);//粗杆部分
            var seg2 = new Line(breakPt1, rod.To);//细杆部分
            var seg3 = new Line(breakPt2, startPt);//出头部分

            var brep1 = Brep.CreatePipe(seg1.ToNurbsCurve(), radius1, true, PipeCapMode.Flat, false, 0.1, 0.01).ToList();
            var brep2 = Brep.CreatePipe(seg2.ToNurbsCurve(), radius2, true, PipeCapMode.Flat, false, 0.1, 0.01).ToList();
            var brep3 = Brep.CreatePipe(seg3.ToNurbsCurve(), radius2, true, PipeCapMode.Flat, false, 0.1, 0.01).ToList();

            breps.AddRange(brep1);
            breps.AddRange(brep2);
            breps.AddRange(brep3);
            return breps;

        }
        protected override System.Drawing.Bitmap Icon => Properties.Resources.create_rod;

        public override Guid ComponentGuid
        {
            get { return new Guid("D63915EA-4E7E-4149-B91D-6263C6F3A899"); }
        }
    }
}