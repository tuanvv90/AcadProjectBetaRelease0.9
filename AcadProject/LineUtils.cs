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
                    if (result.Count > 0 && isCheckLineDuplicate(result, new Line(mPolylineCollection[i].GetPoint3dAt(vid), mPolylineCollection[i].GetPoint3dAt(vid + 1))))
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
            if (line.StartPoint.X == line.EndPoint.X)
                return true;
            return false;
        }

        /*
         * Dump debug information to txt file
         */
        public void dump(String dumpData, Editor ed)
        {
            var txt = "D:/linedata.txt";
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
