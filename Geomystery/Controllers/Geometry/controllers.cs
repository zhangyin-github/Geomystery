﻿using Geomystery.Models.Geometry;
using Geomystery.Views.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geomystery.Controllers.Geometry
{
    public class controllers
    {
        //逻辑坐标系
        public Coordinate coord { get; set; }

        //用来显示的坐标系
        public OutputCoordinate outCoord { get; set; }


        //public int MyProperty { get; set; }
    }
}
