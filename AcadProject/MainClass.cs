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
using AcadProjectLineUtils;

namespace AcadProject
{

    public class Commands
    {

        //Collections of LINEs/POLYLINEs/DBText
        List<Line> mLineCollection = new List<Line>();
        List<Polyline> mPolylineCollection = new List<Polyline>();
        List<DBText> mDBTextCollection = new List<DBText>();

        //Collections of LAYERs
        List<String> mLayerCollection = new List<String>();


        //LINE UTILS OBJECTs
        LineUtils mLineUtilObject = new LineUtils();


        [CommandMethod("DST", CommandFlags.UsePickSet)]
        public void SelectAllLines()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            var ed = doc.Editor;

            //CLEAR DATA
            mDBTextCollection.Clear();
            mLayerCollection.Clear();
            mPolylineCollection.Clear();
            mLineCollection.Clear();


            PromptSelectionOptions pso = new PromptSelectionOptions();

            //Message to user for selction
            pso.MessageForAdding = "\nSelect area contains objects: ";
            //Remove duplicated selection
            pso.AllowDuplicates = false;

            // Perform our restricted selection
            PromptSelectionResult psr = ed.GetSelection(pso);

            if (psr.Status != PromptStatus.OK)
                return;

            //Dump debug information to txt file
            var sb = new StringBuilder();

            //Open a transaction
            Transaction tr = doc.TransactionManager.StartTransaction();
            foreach (ObjectId acSSObj in psr.Value.GetObjectIds())
            {
                //get properties
                Entity ent = (Entity)tr.GetObject(acSSObj, OpenMode.ForRead);

                //CHECKING OBJECT TYPE
                switch (ent.GetType().ToString())
                {

                    case "Autodesk.AutoCAD.DatabaseServices.Line":
                        Line ln = tr.GetObject(acSSObj, OpenMode.ForRead) as Line;
                        mLineCollection.Add(ln);
                        break;

                    case "Autodesk.AutoCAD.DatabaseServices.Polyline":
                        Polyline pln = tr.GetObject(acSSObj, OpenMode.ForRead) as Polyline;
                        mPolylineCollection.Add(pln);
                        break;

                    case "Autodesk.AutoCAD.DatabaseServices.DBText":
                        DBText dbt = tr.GetObject(acSSObj, OpenMode.ForRead) as DBText;
                        mDBTextCollection.Add(dbt);
                        break;

                    default:
                        ed.WriteMessage("\nNo handle");
                        ed.WriteMessage("\nType:         " + ent.GetType().ToString());
                        break;
                }

                ent.Dispose();
            }
            //Apply changes and close transaction
            tr.Commit();

            mLineCollection.AddRange(mLineUtilObject.convertPolyLineToLine(mPolylineCollection, ed));
            sb.Append("Number of line : " + mLineCollection.Count + "\n");
            for (int i = 0; i < mLineCollection.Count; i++)
            {
                sb.Append("\nLine " + (i + 1) + " : " + mLineCollection[i].StartPoint.X + "  " + mLineCollection[i].StartPoint.Y);
                sb.Append("  " + mLineCollection[i].EndPoint.X + "  " + mLineCollection[i].EndPoint.Y + "\n");

            }
            mLineUtilObject.dump(sb.ToString(), ed);
        }

    }
}