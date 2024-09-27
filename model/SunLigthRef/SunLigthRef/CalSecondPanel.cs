using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SunLigthRef
{
    public class CalSecondPanel : GH_Component
    {
        public CalSecondPanel()
          : base("CalSecondPanel", "CSP",
              "Get the second panel's information.",
              "SunLightRef", "Primitive")
        {
        }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Rod1FixedPt","RFP","First rod's fixed point.",GH_ParamAccess.item);
            pManager.AddLineParameter("RotateAxis","RA","Second panel's rotate axis.",GH_ParamAccess.item);
            pManager.AddNumberParameter("T1", "T1", "Pseudo t parameter of rod2.", GH_ParamAccess.item);
            pManager.AddNumberParameter("T2", "T2", "This parm controls the intersection position of rod and panel", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("RodLength", "RL", "Real length of second rod", GH_ParamAccess.item);
            pManager.AddPointParameter("Rod2FixedPt", "RSP", "The fixed point of rod2", GH_ParamAccess.item);
            pManager.AddRectangleParameter("Panel", "P", "The second reflect panel rectangle.", GH_ParamAccess.item);
            pManager.AddPointParameter("JointPt", "JP", "The real joint point of rod and panel.", GH_ParamAccess.item);
            pManager.AddLineParameter("Rod", "R", "Second rod", GH_ParamAccess.item);
            pManager.AddNumberParameter("Angle", "A", "Second panel's rotate angle in radius", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Point3d rod1FixedPt = new Point3d();
            Line rotateAxis = new Line();
            double t1 = 0.0;
            double t2 = 0.0;

            if (!DA.GetData(0, ref rod1FixedPt)) return;
            if(!DA.GetData(1, ref rotateAxis)) return;
            if(!DA.GetData(2, ref t1)) return;
            if(!DA.GetData(3, ref t2)) return;

            var axisMidPt = new Point3d((rotateAxis.FromX + rotateAxis.ToX) / 2, rotateAxis.FromY, rotateAxis.FromZ);//旋转轴的中点
            var rodFixedPt = new Point3d(rod1FixedPt.X, rod1FixedPt.Y - dist, rod1FixedPt.Z);
            var rodLength = rodLengthInterval.Min + (rodLengthInterval.Max - rodLengthInterval.Min) * t1;
            var pseudoJointPt = GetJointPoint(rotateAxis, rodLength, rodFixedPt, t2, axisMidPt);//伪杆板交点

            var vec = pseudoJointPt - axisMidPt;
            vec.Unitize();//反射板方向向量

            var panelEdgeMidPt = GetRealPanelEdgeMidPt(axisMidPt, vec);//反射板边缘中点
            var panel = GetPanel(panelEdgeMidPt, vec, panelWidth, panelLength);//反射板2

            var angle = ComputeRotateAngleRadians(panel);//反射板2旋转角度
            var realJointPt = GetRealJointPt(panel, pseudoJointPt);//铰接点
            var rod2 = new Line(rodFixedPt, realJointPt);//伸缩杆2
            var rod2Length = Math.Round(rod2.Length, 1);//伸缩杆2长度


            DA.SetData(0, rod2Length);
            DA.SetData(1, rodFixedPt);
            DA.SetData(2, panel);
            DA.SetData(3, realJointPt);
            DA.SetData(4,rod2);
            DA.SetData(5, angle);
        }


        Interval rodLengthInterval = new Interval(465, 800);//(伪)伸缩杆2长度区间,真实(455，755)
        const double panelLength = 800;//反射板长度
        const double panelWidth = 1500; //反射板宽度
        const double axisToPanel = 13.4;//旋转轴距离反射板边缘垂直距离
        const double panelThickness = 1; //反射板厚度
        const double barThickness = 20;//型材厚度
        const double dist = 650;//两伸缩杆固定点水平距离
        const double jointAxisToPanel = 25;//大杆板铰接点旋转轴距离反射板边缘垂直距离


        public Point3d GetJointPoint(Line axis, double rodLength, Point3d rodFixedPoint, double t, Point3d axisMidPt)
        {
            //根据反射板的旋转轴，得到反射板与伸缩杆的交叉点
            //t值控制交叉点相对于板中线的位置，反射板的固定尺寸为1500*800
            var rPanel = panelLength * t + axisToPanel;
            var intersect = GetIntersectPoint(axisMidPt, rodFixedPoint, rPanel, rodLength);
            return intersect;
        }

        public Point3d GetIntersectPoint(Point3d pt1, Point3d pt2, double r1, double r2)
        {
            //以两个点为圆心，以给定距离为半径画圆，求交叉点
            var c1 = new Circle(Plane.WorldYZ, pt1, r1);
            var c2 = new Circle(Plane.WorldYZ, pt2, r2);
            Point3d pt_inter1;
            Point3d pt_inter2;
            var result = Rhino.Geometry.Intersect.Intersection.CircleCircle(c1, c2, out pt_inter1, out pt_inter2);

            //排序点,获取所需相交点
            List<Point3d> ptList = new List<Point3d>();
            ptList.Add(pt_inter1);
            ptList.Add(pt_inter2);
            ptList = ptList.OrderBy(pt => pt.Y).ToList();
            var pt_intersect = ptList[0];
            return pt_intersect;
        }

        public Point3d GetRealPanelEdgeMidPt(Point3d axisMidPt, Vector3d vector)
        {

            //得到真正反射板的边缘中点
            //vec为反射板单位方向向量
            var vec = new Vector3d(vector);
            var trans1 = Transform.Translation(vec * axisToPanel);
            axisMidPt.Transform(trans1);
            vec.Rotate(-Math.PI / 2, new Vector3d(1, 0, 0));
            var trans2 = Transform.Translation(vec * (barThickness / 2 + panelThickness));
            axisMidPt.Transform(trans2);
            return axisMidPt;
        }

        public Rectangle3d GetPanel(Point3d pt, Vector3d vec, double width, double length)
        {
            //创建反射板
            var newVec = vec * (length / 2);
            var newPt = pt + newVec;//反射板中心点

            var vec1 = new Vector3d(vec);
            var vec2 = new Vector3d(vec);
            vec.Rotate(Math.PI / 2, new Vector3d(1, 0, 0));

            vec1.Rotate(Math.PI / 2, vec);
            var plane = new Plane(newPt, vec1, vec2);//构造反射板平面

            var wInterval = new Interval(-width / 2, width / 2);
            var lInterval = new Interval(-length / 2, length / 2);
            var rec = new Rectangle3d(plane, wInterval, lInterval);
            return rec;
        }

        public double ComputeRotateAngleRadians(Rectangle3d panel)
        {
            //计算旋转角度,与水平方向夹角
            var plane = panel.Plane;
            var norm = plane.Normal;
            var angle = Vector3d.VectorAngle(norm, new Vector3d(0, 1, 0));
            return Math.PI / 2 - angle;
        }

        public Point3d GetRealJointPt(Rectangle3d panel, Point3d pseudoPt)
        {
            //根据panel平面法线向量，得到真正的杆板铰接点
            var realJointPtToPseudoJointPtDist = jointAxisToPanel + barThickness / 2;

            var plane = panel.Plane;
            var norm = plane.Normal;

            var vec = norm * realJointPtToPseudoJointPtDist;
            vec.Reverse();


            //    var vec = new Vector3d(0, 0, realJointPtToPseudoJointPtDist);
            //    vec.Rotate(angle, new Vector3d(1, 0, 0));
            //    vec.Reverse();
            var trans = Transform.Translation(vec);
            pseudoPt.Transform(trans);

            return pseudoPt;
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.cal_panelB;


        public override Guid ComponentGuid
        {
            get { return new Guid("1013B19F-33B1-4E64-96EB-41A1B4914E44"); }
        }
    }
}