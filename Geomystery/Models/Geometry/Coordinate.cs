﻿using Geomystery.Views.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geomystery.Models.Geometry
{
    /// <summary>
    /// 逻辑坐标系，一张无限大的白纸，可以把其中的一个矩形区域（OutputCoordinate）“投射”到屏幕上
    /// </summary>
    public class Coordinate
    {
        /// <summary>
        /// 曾经创建的几何体计数，用来给新创建的几何体分配ID.
        /// </summary>
        public int GeometryCount { get; set; }

        /// <summary>
        /// 坐标系中所有的点
        /// </summary>
        public List<Point2> pointList { get; set; }

        /// <summary>
        /// 坐标系中的所有的点集（实现点集IPointSet的类有直线Line和圆Circle）
        /// </summary>
        public List<IPointSet> pointSetList { get; set; }

        /// <summary>
        /// 坐标系中的所有多边形，之所以单独列出就是因为多边形依赖于一个点的序列
        /// </summary>
        public List<Polygon> polygonList { get; set; }

        /// <summary>
        /// 被选择的几何实体，
        /// 这是一个双重绑定，相应几何元素（点线圆）的isSelected也会变成true
        /// </summary>
        public List<Geometry> selectedGeometrys { get; set; }


        /// <summary>
        /// 显示坐标系序列用来记录有多少个显示坐标系依赖于此坐标系
        /// </summary>
        public List<OutputCoordinate> outputCoordinates { get; set; }

        /// <summary>
        /// 逻辑坐标系构造函数
        /// </summary>
        public Coordinate()
        {
            init();
        }

        /// <summary>
        /// 初始化函数，关于使用构造函数还是额外的初始化函数有待于探讨
        /// </summary>
        public void init()
        {
            pointList = new List<Point2>();
            pointSetList = new List<IPointSet>();
            polygonList = new List<Polygon>();

            selectedGeometrys = new List<Geometry>();

            GeometryCount = 1;
        }

        /// <summary>
        /// 将一个点添加到List<Point2> pointList， 
        /// 需要注意，判断两个点是同一个点的条件就是点的坐标，当用户想添加同一个点的时候，pointList并不会真正Add这个点，
        /// 甚至两个点十分接近，这取决于点的显示半径，或者是点的“感应半径”
        /// 在这个半径之中，用户的点击视为选择已有的点而不是添加新的点。
        /// </summary>
        /// <param name="p">返回1，添加操作，返回0，选择操作</param>
        /// <returns></returns>
        public int AddPoint(Point2 point)
        {
            point.id = GeometryCount;
            point.coord = this;                     //点在坐标系中
            GeometryCount++;

            pointList.Add(point);                   //坐标系中有一个点
            for(int i = 0; i < outputCoordinates.Count;i++)
            {
                outputCoordinates[i].AddPoint(point);
            }
            this.ClearSelectedGeometry();
            this.ToSelectGeometry(point);
            return 0;
        }

        /// <summary>
        /// 将一条线添加到List<IPointSet> pointSetList，
        /// 当用户定义line的两个点p1和p2都在已经定义的某条直线上时，pointSetList并不会真正Add这条直线
        /// 注：可以当前直线被选中，返回被选中直线的负数编号（例如已经存在直线在pointSetList中是【0】号，返回0，是【4】号，返回-1）
        /// </summary>
        /// <param name="line">返回1，添加成功，返回非正整数，这个整数的绝对值是已经存在的“点集”在pointSetList中的编号</param>
        /// <returns></returns>
        public int AddLine(Line line)
        {

            return 0;
        }

        /// <summary>
        /// 将一个圆添加到List<IPointSet> pointSetList，
        /// 当用户的选择的圆心是一个圆的圆心，且半径定义点点在这个圆的圆上，pointSetList并不会真正Add这个圆
        /// 注：与直线类似，可以考虑用负数代替这个被选中的圆在pointSetList的编号
        /// </summary>
        /// <param name="circle">返回1，添加成功，返回非正整数，这个整数的绝对值是已经存在的“点集”在pointSetList中的编号</param>
        /// <returns></returns>
        public int AddCircle(Circle circle)
        {

            return 0;
        }

        /// <summary>
        /// 添加多边形，等待后续操作
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public int AddPolygon(Polygon polygon)
        {

            return 0;
        }

        /// <summary>
        /// 取消所有选中
        /// </summary>
        public void ClearSelectedGeometry()
        {
            foreach (Models.Geometry.Geometry selectedGeometry in this.selectedGeometrys)
            {
                    selectedGeometry.isSelected = false;
            }
            this.selectedGeometrys.Clear();
        }

        /// <summary>
        /// 该几何元素未被选中，选中它并取消选中其他的，或者仅有该元素被选中时取消选中此几何元素
        /// </summary>
        /// <param name="geometry">坐标系中的几何元素的引用</param>
        public void ToSelectGeometry(Geometry geometry)
        {
            if(geometry.coord == this)              //坐标系中的元素
            {
                if (!geometry.isSelected)           //元素未被选中
                {
                    ClearSelectedGeometry();                //取消选中其他任何元素
                    this.selectedGeometrys.Add(geometry);
                    geometry.isSelected = true;             //选中当前元素
                }
                else
                {
                    if(this.selectedGeometrys.Count == 1)       //只有当全元素被选中
                    {
                        ClearSelectedGeometry();                //取消选中其他任何元素
                    }
                    else if(this.selectedGeometrys.Count > 1)
                    {
                        ClearSelectedGeometry();                //取消选中其他任何元素
                        this.selectedGeometrys.Add(geometry);
                        geometry.isSelected = true;             //只选中当前元素
                    }
                }
            }
            
        }

        /// <summary>
        /// （通过坐标系中元素的id号来选择和取消选择）该几何元素未被选中，选中它并取消选中其他的，或者仅有该元素被选中时取消选中此几何元素
        /// </summary>
        /// <param name="id"></param>
        public void ToSelectGeometry(int id)
        {
            foreach (Models.Geometry.Geometry selectedPoint in this.pointList)
            {
                if(selectedPoint.id == id)
                {
                    ToSelectGeometry(selectedPoint);
                    return;
                }
            }
            foreach (Models.Geometry.Geometry selectedPointSet in this.pointSetList)
            {
                if (selectedPointSet.id == id)
                {
                    ToSelectGeometry(selectedPointSet);
                    return;
                }
            }
            foreach (Models.Geometry.Geometry selectedPolygon in this.polygonList)
            {
                if (selectedPolygon.id == id)
                {
                    ToSelectGeometry(selectedPolygon);
                    return;
                }
            }
        }

        /// <summary>
        /// 增加选择某个元素，或者取消选择某个几何元素，保持其他元素的选择状态
        /// </summary>
        /// <param name="geometry">几何元素引用</param>
        public void ToMultiSelectGeometry(Geometry geometry)
        {
            if(!geometry.isSelected)
            {
                geometry.isSelected = true;
                this.selectedGeometrys.Add(geometry);
            }
            else
            {
                geometry.isSelected = false;
                this.selectedGeometrys.Remove(geometry);
            }
        }

        /// <summary>
        /// （按照ID）增加选择某个元素，或者取消选择某个几何元素，保持其他元素的选择状态
        /// </summary>
        /// <param name="id">元素的id</param>
        public void ToMultiSelectGeometry(int id)
        {
            foreach (Models.Geometry.Geometry selectedPoint in this.pointList)
            {
                if (selectedPoint.id == id)
                {
                    ToMultiSelectGeometry(selectedPoint);
                    return;
                }
            }
            foreach (Models.Geometry.Geometry selectedPointSet in this.pointSetList)
            {
                if (selectedPointSet.id == id)
                {
                    ToMultiSelectGeometry(selectedPointSet);
                    return;
                }
            }
            foreach (Models.Geometry.Geometry selectedPolygon in this.polygonList)
            {
                if (selectedPolygon.id == id)
                {
                    ToMultiSelectGeometry(selectedPolygon);
                    return;
                }
            }
        }
    }
}
