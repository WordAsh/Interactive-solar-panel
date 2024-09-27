using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SunLigthRef
{
    public class CalFirstPanel : GH_Component
    {
        public CalFirstPanel()
          : base("CalFirstPanel", "CFP",
              "Get the first panel's information and second panel's rotate axis.",
              "SunLightRef", "Primitive")
        {
        }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("RotateAxis","RA","First panel's rotate axis.",GH_ParamAccess.item);
            pManager.AddNumberParameter("T1","T1","Pseudo t parameter of rod1.", GH_ParamAccess.item);
            pManager.AddNumberParameter("T2", "T2", "This parm controls the intersection position of rod and panel", GH_ParamAccess.item);
        }
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("RodLength","RL","Real length of first rod",GH_ParamAccess.item);
            pManager.AddPointParameter("Rod1FixedPt", "RFP", "The fixed point of rod1", GH_ParamAccess.item);
            pManager.AddRectangleParameter("Panel","P","The first reflect panel rectangle.",GH_ParamAccess.item);
            pManager.AddPointParameter("JointPt","JP","The real joint point of rod and panel.",GH_ParamAccess.item);
            pManager.AddLineParameter("RotateAxis","RA","Rotate axis of second reflect panel.",GH_ParamAccess.item);
            pManager.AddLineParameter("Rod","R","First rod",GH_ParamAccess.item);
            pManager.AddNumberParameter("Angle","A","First panel's rotate angle in radius",GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Line rotateAxis = new Line();
            double t1 = 0.0;
            double t2 = 0.0;

            if (!DA.GetData(0, ref rotateAxis)) return;
            if(!DA.GetData(1,ref t1)) return;
            if(!DA.GetData(2, ref t2)) return;

            var axisMidPt = new Point3d((rotateAxis.FromX + rotateAxis.ToX) / 2, rotateAxis.FromY, rotateAxis.FromZ);//旋转轴的中点
            var rodFixedPt = GetRodFixedPt(vertDist, rotateAxis, t2, rodLengthInterval, axisMidPt);
            var rodLength = rodLengthInterval.Min + (rodLengthInterval.Max - rodLengthInterval.Min) * t1;
            var pseudoJointPt = GetJointPoint(rotateAxis, rodLength, rodFixedPt, t2, axisMidPt);//伪杆板交点

            var vec = pseudoJointPt - axisMidPt;
            vec.Unitize();//反射板方向向量

            var panelEdgeMidPt = GetRealPanelEdgeMidPt(axisMidPt, vec);//反射板边缘中点
            var panel = GetPanel(panelEdgeMidPt, vec, panelWidth, panelLength);//反射板1

            var angle = ComputeRotateAngleRadians(panel);//反射板1旋转角度
            var realJointPt = GetRealJointPt(panel, pseudoJointPt);//铰接点

            var axis2 = GetSecondAxis(angle, rotateAxis, axisMidPt);//第二个旋转轴
            var rod1 = new Line(rodFixedPt, realJointPt);//伸缩杆1
            var rod1Length = Math.Round(rod1.Length, 1); //伸缩杆1长度


            DA.SetData(0, rod1Length);
            DA.SetData(1, rodFixedPt);
            DA.SetData(2, panel);
            DA.SetData(3,realJointPt);
            DA.SetData(4,axis2);
            DA.SetData(5,rod1);
            DA.SetData(6,angle);
        }


        Interval rodLengthInterval = new Interval(226, 320);//(伪)伸缩杆1长度区间,比真实区间要大，真实(205，305)
        const double panelLength = 800;//反射板长度
        const double panelWidth = 1500; //反射板宽度
        const double axisToPanel = 13.4;//旋转轴距离反射板边缘垂直距离
        const double panelThickness = 1; //反射板厚度
        const double barThickness = 20;//型材厚度
        const double vertDist = 145; //杆1固定点距旋转轴垂直距离
        const double jointAxisToPanel = 25;//大杆板铰接点旋转轴距离反射板边缘垂直距离


        public Point3d GetRodFixedPt(double vertDist, Line axis, double t, Interval rodLengthInterval, Point3d axisMidPt)
        {
            //根据杆板交点位置参数以及杆距离旋转轴的垂直距离，求杆的固定端点
            //t为杆板位置参数

            double rodBaseLength = rodLengthInterval.Min; //(伪)伸缩杆的基础长度
            var projected_length = Math.Sqrt(Math.Pow(rodBaseLength, 2) - Math.Pow(vertDist, 2));

            if (projected_length < panelLength * t + axisToPanel)
            {
                var rodFixedPt = new Point3d(axisMidPt.X, axisMidPt.Y - (panelLength * t + axisToPanel - projected_length), axisMidPt.Z - vertDist);
                return rodFixedPt;
            }
            else
            {
                var rodFixedPt = new Point3d(axisMidPt.X, axisMidPt.Y + (projected_length - panelLength * t - axisToPanel), axisMidPt.Z - vertDist);
                return rodFixedPt;
            }
        }


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

        public Line GetSecondAxis(double angle, Line originalAxis, Point3d axisMidPt)
        {
            //得到第二个反射板的旋转轴
            var trans1 = Transform.Translation(new Vector3d(0, -panelLength - axisToPanel * 2, 0));
            originalAxis.Transform(trans1);//第二个板旋转轴水平位置
            var trans2 = Transform.Rotation(-angle, new Vector3d(1, 0, 0), axisMidPt);
            originalAxis.Transform(trans2);//旋转
            return originalAxis;
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.cal_panelA;
        public override Guid ComponentGuid
        {
            get { return new Guid("6153A33A-6E4F-4D7D-9712-2D53B7232D67"); }
        }
    }
}