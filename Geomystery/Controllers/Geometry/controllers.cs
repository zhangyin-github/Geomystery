﻿using Geomystery.Models.Geometry;
using Geomystery.Views.Geometry;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Input;

namespace Geomystery.Controllers.Geometry
{
    /// <summary>
    /// 模型控制器，提供点击pointer_pressed与释放pointer_released接口
    /// </summary>
    public class Controllers
    {
        /// <summary>
        /// 正在执行的记录器
        /// </summary>
        public DFA runningDFA { get; set; }

        /// <summary>
        /// 历史记录
        /// </summary>
        public List<DFA> historyDfaList { get; set; }

        /// <summary>
        /// 重做历史记录，记录用户撤销了多少
        /// </summary>
        public List<DFA> redoDfaList { get; set; }

        /// <summary>
        /// 是否经过初始化
        /// </summary>
        public bool isIniialized { get; set; }

        /// <summary>
        /// 受控制的逻辑坐标系
        /// </summary>
        public Coordinate coordinate { get; set; }

        /// <summary>
        /// 用来显示的坐标系列表，将会分别在在canvas中显示
        /// </summary>
        public List<OutputCoordinate> outputCoordinates { get; set; }

        /// <summary>
        /// 指针按下点
        /// </summary>
        public Point pressedPoint { get; set; }

        /// <summary>
        /// 指针弹起点
        /// </summary>
        public Point releasedPoint { get; set; }

        /// <summary>
        /// 输出坐标系个数
        /// </summary>
        /// <param name="outputNumber">输出坐标系个数</param>
        public Controllers(int outputNumber)
        {
            isIniialized = false;
            OutputPoint.scopeLength = 6;               //10 dip
            Initialized(outputNumber);
        }


        /// <summary>
        /// 初始化坐标系
        /// </summary>
        /// <param name="outputNumber">有多少个显示器（canvas）</param>
        /// <returns>1经过初始化 0初始化失败</returns>
        public int Initialized(int outputNumber)
        {
            if (isIniialized) return 1;
            if(outputNumber>0)
            {
                coordinate = new Coordinate();
                outputCoordinates = new List<OutputCoordinate>();
                for(int i = 0; i < outputNumber; i++)
                {
                    outputCoordinates.Add(new OutputCoordinate(coordinate));
                }
                coordinate.outputCoordinates = outputCoordinates;

                historyDfaList = new List<DFA>();
                redoDfaList = new List<DFA>();

                return 1;
            }
            return 0;
        }

        /// <summary>
        /// 关卡的预初始化
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public int PreInitialized(Controllers controller)
        {
            this.coordinate = controller.coordinate;
            if (outputCoordinates != null)
            {
                outputCoordinates.Clear();
            }
            outputCoordinates = new List<OutputCoordinate>();
            outputCoordinates.Add(controller.outputCoordinates[0]);
            return 1;
        }

        /// <summary>
        /// 获取鼠标在屏幕当前点附近的逻辑坐标点
        /// </summary>
        /// <param name="point">屏幕上的点</param>
        /// <returns>逻辑点列表</returns>
        public List<Models.Geometry.Geometry> GetSurroundings(Vector2 point)
        {
            List<Models.Geometry.Geometry> result = null;
            List<OutputGeometry> outGeometry = outputCoordinates[0].geometryList;
            result = new List<Models.Geometry.Geometry>();
            float minLength = -1;
            float currentLength = 0;
            if (outGeometry == null) return null;
            for (int i = 0; i < outGeometry.Count; i++)
            {
                if(outGeometry[i].isVisible == true)
                {
                    if(outGeometry[i] is OutputPoint)
                    {
                        var pCurrent = outGeometry[i] as OutputPoint;
                        if ((currentLength = (pCurrent.viewPoint - point).Length()) < OutputPoint.scopeLength)            //点击的点在屏幕上某个点的圆圈内
                        {
                            if(minLength == -1)
                            {
                                minLength = currentLength;
                                result.Add(pCurrent.point);
                            }
                            else if(currentLength < minLength)
                            {
                                minLength = currentLength;
                                result.Clear();
                                result.Add(pCurrent.point);
                            }
                        }
                    }
                    else if(outGeometry[i] is OutputLine)
                    {
                        var lCurrent = outGeometry[i] as OutputLine;
                        var perpendicularFoot = new Vector2();
                        if ( (currentLength = OutputCoordinate.DistanceOfPointAndLine(lCurrent.p1,lCurrent.p2 - lCurrent.p1, point,ref perpendicularFoot)) < OutputPoint.scopeLength)
                        {
                            if (minLength == -1)
                            {
                                minLength = currentLength;
                                result.Add(lCurrent.line);
                            }
                            else if (currentLength < minLength)
                            {
                                minLength = currentLength;
                                result.Clear();
                                result.Add(lCurrent.line);
                            }
                        }
                    }
                    else if (outGeometry[i] is OutputCircle)
                    {
                        var cCurrent = outGeometry[i] as OutputCircle;
                        if( (currentLength = Math.Abs(cCurrent.radius - (point-cCurrent.center).Length())) < OutputPoint.scopeLength)
                        {
                            if (minLength == -1)
                            {
                                minLength = currentLength;
                                result.Add(cCurrent.circle);
                            }
                            else if (currentLength < minLength)
                            {
                                minLength = currentLength;
                                result.Clear();
                                result.Add(cCurrent.circle);
                            }
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 指针被按下
        /// </summary>
        /// <param name="userTool">当前选择的工具</param>
        /// <param name="sender">CanvasAnimatedControl的一个实例canvas1</param>
        /// <param name="e">点击时的信息，包括坐标、滑轮和按键等</param>
        public void PointerPressed(UserTool userTool, object sender, PointerRoutedEventArgs e)
        {
            pressedPoint = e.GetCurrentPoint((CanvasAnimatedControl)sender).Position;
            Vector2 vector2 = new Vector2();
            vector2.X = (float)pressedPoint.X;
            vector2.Y = (float)pressedPoint.Y;

            //var surroundinsGeometryList = GetSurroundings(vector2);
            Surroundings surroundings = outputCoordinates[0].GetSurroundings(vector2);               //周围的点直线圆
            Surroundings scross = outputCoordinates[0].GetCross(surroundings);                       //周围所有可能的交点

            if (runningDFA == null)                                             //可以使用新工具
            {
                runningDFA = new DFA(userTool, 0, coordinate);                      //创建当前工具的记录器
            }
            else if (runningDFA.userTool.toolName != userTool.toolName || runningDFA.state == -2)             //用户更换工具
            {
                runningDFA.Undo();                          //撤销上一个工具
                runningDFA = null;
                runningDFA = new DFA(userTool, 0, coordinate);
            }

            if (userTool.toolName == "选择工具")
            {
                if (surroundings.surroundingPoint.Count > 0)
                {
                    coordinate.ToSelectGeometry(surroundings.surroundingPoint[0].geometry);
                    Debug.WriteLine(surroundings.surroundingPoint[0].geometry.ToString());
                }
                else if(surroundings.surroundingLine.Count > 0)
                {
                    coordinate.ToSelectGeometry(surroundings.surroundingLine[0].geometry);
                    Debug.WriteLine(surroundings.surroundingLine[0].geometry.ToString());
                }
                else if(surroundings.surroundingCircle.Count > 0)
                {
                    coordinate.ToSelectGeometry(surroundings.surroundingCircle[0].geometry);
                    Debug.WriteLine(surroundings.surroundingCircle[0].geometry.ToString());
                }
                else
                { 
                    coordinate.ClearSelectedGeometry();
                }
            }
            else if(userTool.toolName == "交点工具")
            {
                if (surroundings.surroundingLine.Count > 0)
                {
                    if(surroundings.surroundingCircle.Count > 0)                    //选定一个
                    {
                        if(surroundings.surroundingLine[0].distance <= surroundings.surroundingCircle[0].distance)
                        {
                            coordinate.ToSelectGeometry(surroundings.surroundingLine[0].geometry);
                            runningDFA.UserSelectGeomerty(surroundings.surroundingLine[0].geometry, false);
                        }
                        else
                        {
                            coordinate.ToSelectGeometry(surroundings.surroundingCircle[0].geometry);
                            runningDFA.UserSelectGeomerty(surroundings.surroundingCircle[0].geometry, false);
                        }
                    }
                    else
                    {
                        coordinate.ToSelectGeometry(surroundings.surroundingLine[0].geometry);
                        runningDFA.UserSelectGeomerty(surroundings.surroundingLine[0].geometry, false);
                    }
                }
                else if(surroundings.surroundingCircle.Count > 0)
                {
                    coordinate.ToSelectGeometry(surroundings.surroundingCircle[0].geometry);
                    runningDFA.UserSelectGeomerty(surroundings.surroundingCircle[0].geometry, false);
                }

                if(runningDFA.state == 2)
                {
                    List<Point2> plist = ((IPointSet)(runningDFA.needList[0].selectStack[0].selectedGeometry)).Intersection((IPointSet)runningDFA.needList[0].selectStack[1].selectedGeometry);
                    for (int i = 0; i < plist.Count; i++)
                    {
                        coordinate.AddPoint(plist[i]);
                    }
                    historyDfaList.Add(runningDFA);
                    redoDfaList.Clear();
                    runningDFA = null;
                }
            }
            else if (userTool.toolName == "点工具")
            {
                if(surroundings.surroundingPoint.Count > 0)
                {
                    coordinate.ToSelectGeometry(surroundings.surroundingPoint[0].geometry);
                }
                else
                {
                    Point2 newPoint = outputCoordinates[0].ToPoint2(vector2);
                    coordinate.AddPoint(newPoint);
                }
            }
            else if(userTool.toolName == "直线工具")
            {
                if (surroundings.surroundingPoint.Count > 0)
                {
                    coordinate.ToSelectGeometry(surroundings.surroundingPoint[0].geometry);
                    runningDFA.UserSelectGeomerty(surroundings.surroundingPoint[0].geometry, false);
                }
                else
                {
                    Point2 newPoint = outputCoordinates[0].ToPoint2(vector2);
                    coordinate.AddPoint(newPoint);
                    runningDFA.UserSelectGeomerty(newPoint, true);
                }

                if(runningDFA.state == 2)
                {
                    Line line = new Line();

                    line.p1 = runningDFA.needList[0].selectStack[0].selectedGeometry as Point2;
                    line.p2 = runningDFA.needList[0].selectStack[1].selectedGeometry as Point2;

                    line.type = LineType.Straight;
                    line.lineRely = LineRely.Normal;

                    coordinate.AddLine(line);

                    runningDFA.needList[0].selectStack.Clear();
                    runningDFA.needList.Clear();
                    runningDFA = null;
                }
                else if(runningDFA.state == -2)
                {
                    if(runningDFA.needList[0].selectStack.Count == 1)
                    {
                        if (runningDFA.needList[0].selectStack[0].IsNew)
                        {
                            coordinate.RemovePoint(runningDFA.needList[0].selectStack[0].selectedGeometry as Point2);
                        }

                    }
                    runningDFA.needList[0].selectStack.Clear();
                    runningDFA.needList.Clear();
                    runningDFA = null;
                }
            }
            else if (userTool.toolName == "圆工具")
            {
                if (surroundings.surroundingPoint.Count > 0)
                {
                    coordinate.ToSelectGeometry(surroundings.surroundingPoint[0].geometry);
                    runningDFA.UserSelectGeomerty(surroundings.surroundingPoint[0].geometry, false);
                }
                else
                {
                    Point2 newPoint = outputCoordinates[0].ToPoint2(vector2);
                    coordinate.AddPoint(newPoint);
                    runningDFA.UserSelectGeomerty(newPoint, true);
                }

                if (runningDFA.state == 2)
                {
                    Circle circle = new Circle();

                    circle.center = runningDFA.needList[0].selectStack[0].selectedGeometry as Point2;
                    circle.radius = runningDFA.needList[0].selectStack[1].selectedGeometry as Point2;

                    coordinate.AddCircle(circle);

                    runningDFA.needList[0].selectStack.Clear();
                    runningDFA.needList.Clear();
                    runningDFA = null;
                }
                else if (runningDFA.state == -2)
                {
                    //if (dfa.needList[0].selectStack.Count == 0) ;
                    for(int i = 0; i < runningDFA.needList[0].selectStack.Count; i++)
                    {
                        SelectedGeometryStackItem selectedGeometryStackItem = runningDFA.needList[0].selectStack[i];
                        if (selectedGeometryStackItem.IsNew)
                        {
                            coordinate.RemovePoint(selectedGeometryStackItem.selectedGeometry as Point2);
                        }
                    }
                    runningDFA.needList[0].selectStack.Clear();
                    runningDFA.needList.Clear();
                    runningDFA = null;
                }
            }

        }


        /// <summary>
        /// 指针弹起
        /// </summary>
        /// <param name="userTool">当前选择的工具</param>
        /// <param name="sender">CanvasAnimatedControl的一个实例canvas1</param>
        /// <param name="e">点击时的信息，包括坐标、滑轮和按键等</param>
        public void PointerReleased(UserTool userTool, object sender, PointerRoutedEventArgs e)
        {
            releasedPoint = e.GetCurrentPoint((CanvasAnimatedControl)sender).Position;
        }

        /// <summary>
        /// 指针移动
        /// </summary>
        /// <param name="userTool">当前选择的工具</param>
        /// <param name="sender">CanvasAnimatedControl的一个实例canvas1</param>
        /// <param name="e">点击时的信息，包括坐标、滑轮和按键等</param>
        public void PointerMoved(UserTool userTool, object sender, PointerRoutedEventArgs e)
        {
            Point currentPoint = e.GetCurrentPoint((CanvasAnimatedControl)sender).Position;

        }


        
    }
}
