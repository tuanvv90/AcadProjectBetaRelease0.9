using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Windows;
using System.IO;
using System.Text;
using System;

namespace AcadProjectLineUtils
{
    public class LineUtils
    {
        //Delta data
        public double DELTA_POINT = 0.0;


        // Convert List Line to array 
        public int convertListLineToArray(List<Line> lines, double []arrayX, double []arrayY, bool [,]isLine)
        {
            double x, y, cx, cy;
            int numberOfPoint = 0, j;
            for (int i = 0; i < lines.Count; i++)
            {
                x = lines[i].StartPoint.X;
                y = lines[i].StartPoint.Y;
                // Check have this point
                
                for(j = 0; j < numberOfPoint; j++)
                {   
                    if(x == arrayX[j] && y == arrayY[j])
                    {
                        break;
                    }
                }
                if (j == numberOfPoint)
                {
                    arrayX[numberOfPoint] = x;
                    arrayY[numberOfPoint] = y;
                    numberOfPoint++;
                }
                for (int k = 0; k < numberOfPoint; k++)
                {
                    if (arrayX[k] == lines[i].EndPoint.X && arrayY[k] == lines[i].EndPoint.Y)
                    {
                        isLine[j, k] = true;
                        isLine[k, j] = true;
                    }
                }

                x = lines[i].EndPoint.X;
                y = lines[i].EndPoint.Y;
                // Check have this point

                for (j = 0; j < numberOfPoint; j++)
                {
                    if (x == arrayX[j] && y == arrayY[j])
                    {
                        break;
                    }
                }
                if (j == numberOfPoint)
                {
                    arrayX[numberOfPoint] = x;
                    arrayY[numberOfPoint] = y;
                    numberOfPoint++;
                }
                for (int k = 0; k < numberOfPoint; k++)
                {
                    if (arrayX[k] == lines[i].StartPoint.X && arrayY[k] == lines[i].StartPoint.Y)
                    {
                        isLine[j, k] = true;
                        isLine[k, j] = true;
                    }
                }

            }

            return numberOfPoint;
        }

        // If a Line crossed by other line, devide this line to 2 line 
        public List<Line> cuttingLineIfCrossed(List<Line> lines)
        {
            List<Line> resultLines = new List<Line>();
            for (int i = 0; i < lines.Count; i++)
            {
                Line currentLine = lines[i];
                // Start Point
                double cstartX = currentLine.StartPoint.X;
                double cstartY = currentLine.StartPoint.Y;
                // End Point
                double cendX = currentLine.EndPoint.X;
                double cendY = currentLine.EndPoint.Y;

                double crossedX = 0, crossedY = 0;
                bool isCrossed = false;

                for (int j = 0; j < lines.Count; j++)
                {
                    Line otherLine = lines[j];
                    // start point
                    double ostartX = otherLine.StartPoint.X;
                    double ostartY = otherLine.StartPoint.Y;
                    // end point
                    double oendX = otherLine.EndPoint.X;
                    double oendY = otherLine.EndPoint.Y;

                    double startDx, endDx, startDy, endDy;

                    if (i != j)
                    {
                        // Consider start point
                        startDx = cstartX - ostartX; endDx = ostartX - cendX;
                        startDy = cstartY - ostartY; endDy = ostartY - cendY;

                        if (startDx == 0 && endDx == 0)
                        {
                            if(startDy * endDy > 0)
                            {
                                isCrossed = true; crossedX = ostartX; crossedY = ostartY; break;
                            }
                        }

                        if (startDy == 0 && endDy == 0)
                        {
                            if (startDx * endDx > 0)
                            {
                                isCrossed = true; crossedX = ostartX; crossedY = ostartY; break;
                            }
                        }
                        if (startDx != 0 && endDx != 0 && startDy != 0 && endDy != 0)
                        {
                            double startTan = startDy / startDx;
                            double endTan = endDy / endDx;
                            if(Math.Round(startTan*10000) == Math.Round(endTan*10000) && startDx * endDx > 0) {
                                isCrossed = true; crossedX = ostartX; crossedY = ostartY; break;
                            }
                        }

                        // Consider end point
                        startDx = cstartX - oendX; endDx = oendX - cendX;
                        startDy = cstartY - oendY; endDy = oendY - cendY;

                        if (startDx == 0 && endDx == 0)
                        {
                            if (startDy * endDy > 0)
                            {
                                isCrossed = true; crossedX = oendX; crossedY = oendY; break;
                            }
                        }

                        if (startDy == 0 && endDy == 0)
                        {
                            if (startDx * endDx > 0)
                            {
                                isCrossed = true; crossedX = oendX; crossedY = oendY; break;
                            }
                        }
                        if (startDx != 0 && endDx != 0 && startDy != 0 && endDy != 0)
                        {
                            double startTan = startDy / startDx;
                            double endTan = endDy / endDx;
                            if (Math.Round(startTan * 10000) == Math.Round(endTan * 10000) && startDx * endDx > 0)
                            {
                                isCrossed = true; crossedX = oendX; crossedY = oendY; break;
                            }
                        }
                    }

                }

                if (isCrossed)
                {
                    Point3d point1 = new Point3d(cstartX, cstartY, 0);
                    Point3d point2 = new Point3d(crossedX, crossedY, 0);
                    Point3d point3 = new Point3d(cendX, cendY, 0);

                    Line line1 = new Line(point1, point2);
                    Line line2 = new Line(point2, point3);
                    resultLines.Add(line1);
                    resultLines.Add(line2);
                }
                else
                {
                    resultLines.Add(currentLine);
                }
            }
            return resultLines;
        }


        /* Convert polyline to lines
         * and remove duplicated line
         */
        public List<Line> convertPolyLineToLine(List<Polyline> mPolylineCollection, Editor ed)
        {
            List<Line> result = new List<Line>();
            for (int i = 0; i < mPolylineCollection.Count; i++)
            {
                int nov = mPolylineCollection[i].NumberOfVertices;
                for (int vid = 0; vid < nov - 1; vid++)
                {
                    //if (result.Count > 0 && isCheckLineDuplicate(result, new Line(mPolylineCollection[i].GetPoint3dAt(vid), mPolylineCollection[i].GetPoint3dAt(vid + 1))))
                    if (isLineBeRemoved(new Line(mPolylineCollection[i].GetPoint3dAt(vid), mPolylineCollection[i].GetPoint3dAt(vid + 1))))
                        ed.WriteMessage("Line is duplicated or should be removed!! Ignore it\n");
                    else
                        result.Add(new Line(mPolylineCollection[i].GetPoint3dAt(vid), mPolylineCollection[i].GetPoint3dAt(vid + 1)));

                }
            }
            return result;
        }

        /*
         * Check duplicated line
         * A line already in collection or not
         * Return true/false
         */
        public bool isCheckLineDuplicate(List<Line> mLineCollection, Line line)
        {
            for (int i = 0; i < mLineCollection.Count; i++)
            {
                if (isLineBeRemoved(line) || line.Equals(mLineCollection[i]))
                    return true;
            }
            return false;
        }

        /*
         * Remove vertical line
         * or Line has angle equals 90
         */
        public bool isLineBeRemoved(Line line) 
        {
            //if (Math.Abs(line.StartPoint.X - line.EndPoint.X) < 0.01)
            if(line.StartPoint.X == line.EndPoint.X)
                return true;
            return false;
        }

        /*
         * Dump debug information to txt file
         */
        public void dump(String dumpData, Editor ed, string fileName)
        {
            var txt = fileName;
            //Write content to TXT file
            if (!String.IsNullOrEmpty(dumpData))
            {
                try
                {
                    // Write the contents to the selected TXT file

                    using (
                      var sw = new StreamWriter(txt, false, Encoding.UTF8)
                    )
                    {
                        sw.WriteLine(dumpData);
                    }
                }
                catch (System.IO.IOException)
                {
                    // We might have an exception, if the TXT is open in
                    // Excel, for instance... could also show a messagebox
                    ed.WriteMessage("\nUnable to write to file.");
                }
            }

        }
    }
}
