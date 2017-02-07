using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AcadProjectLayerUtils
{
    public class PointUtils
    {
        public double X, Y;
        public double High, Distance;
        public PointUtils() 
        {
            High = Distance = X = Y = 0.0;
        }

        public void setXY(double X, double Y) 
        {
            this.X = X;
            this.Y = Y;
        }

        public void setHigh(double High)
        {
            this.High = High;
        }

        public void setDistance(double Distance) 
        {
            this.Distance = Distance;
        }

        public double getX() {
            return this.X;
        }

        public double getY() {
            return this.Y;
        }

        public double getHigh() {
            return this.High;
        }

        public double getDistance() {
            return this.Distance;
        }
    }

    public class LayerUtils
    {
        public List<PointUtils> listOfPoint;
        public LayerUtils()
        {
            listOfPoint = new List<PointUtils>();
        }

        public void addPointToLayer(PointUtils addPoint) 
        {
            listOfPoint.Add(addPoint);
        }

        public int numberOfPoint() 
        {
            return listOfPoint.Count;
        }

        public PointUtils getPointAt(int index) 
        {
            return listOfPoint[index];
        }

        public StringBuilder toStringData() 
        {
            StringBuilder layerDataSb = new StringBuilder();
            for (int i = 0; i < listOfPoint.Count; i++) 
            {
                layerDataSb.Append("\n" + listOfPoint[i].getDistance() + " , +" + listOfPoint[i].getHigh());
            }
            return layerDataSb;
        }
    }
}
