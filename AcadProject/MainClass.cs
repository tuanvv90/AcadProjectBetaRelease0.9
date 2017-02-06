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
using AcadProjectExtractData;

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

        //EXTRACT DATA OBJECT//Make it as global variable
        ExtractData extractData = new ExtractData();

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

            //[[---------------------------------------------------------
            //Get Zero point
            PromptPointResult pPtRes;
            PromptPointOptions pPtOpts = new PromptPointOptions("");

            // Prompt for the start point
            pPtOpts.Message = "\nPick Zero point : ";
            pPtRes = doc.Editor.GetPoint(pPtOpts);
            Point3d ptZero = pPtRes.Value;
            extractData.zeroPoint.setXY(ptZero.X, ptZero.Y); 

            // Exit if the user presses ESC or cancels the command
            if (pPtRes.Status != PromptStatus.OK) return;
            //---------------------------------------------------------]]


            //[[----------------------------------------------------------
            //Get comparison level in double
            PromptDoubleOptions pMssOpts = new PromptDoubleOptions("");
            pMssOpts.Message = "\nEnter MSS :  ";

            // Restrict input to positive and non-negative values
            pMssOpts.AllowZero = false;
            pMssOpts.AllowNegative = false;

            // Define the valid keywords and allow Enter
            pMssOpts.Keywords.Add("Big");
            pMssOpts.Keywords.Add("Small");
            pMssOpts.Keywords.Add("Regular");
            pMssOpts.Keywords.Default = "Regular";
            pMssOpts.AllowNone = true;

            // Get the MSS value entered by the user
            PromptDoubleResult pMssRes = doc.Editor.GetDouble(pMssOpts);
            extractData.setMss(pMssRes.Value);
            
            if (pMssRes.Status != PromptStatus.OK)
                return;

            //----------------------------------------------------------]]


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


            List<Line> lines = mLineUtilObject.cuttingLineIfCrossed(mLineCollection);
            int numberOfPoint;
            //ExtractData extractData = new ExtractData();
            double[] arrayX = extractData.getArrayX();
            double[] arrayY = extractData.getArrayY();
            bool[,] isLineMatrix = extractData.getIsLineMatrix();

            numberOfPoint = mLineUtilObject.convertListLineToArray(lines, arrayX, arrayY, isLineMatrix);
            //numberOfPoint = mLineUtilObject.convertListLineToArray(mLineCollection, arrayX, arrayY, isLineMatrix);
            ed.WriteMessage("number of Point: {0}", numberOfPoint);
            extractData.setNumberOfPoint(numberOfPoint);

            sb.Append("Number of line : " + mLineCollection.Count + "\n");
            for (int i = 0; i < mLineCollection.Count; i++)
            {
                sb.Append("\nLine " + (i + 1) + " : " + mLineCollection[i].StartPoint.X + "  " + mLineCollection[i].StartPoint.Y);
                sb.Append("  " + mLineCollection[i].EndPoint.X + "  " + mLineCollection[i].EndPoint.Y + "\n");

            }
            string fileName = "D:/linedata.txt";
            mLineUtilObject.dump(sb.ToString(), ed,fileName);


            sb = new StringBuilder();
            sb.Append("Number of line : " + lines.Count + "\n");
            for (int i = 0; i < lines.Count; i++)
            {
                sb.Append("\nLine " + (i + 1) + " : " + lines[i].StartPoint.X + "  " + lines[i].StartPoint.Y);
                sb.Append("  " + lines[i].EndPoint.X + "  " + lines[i].EndPoint.Y + "\n");

            }
            fileName = "D:/lines.txt";
            mLineUtilObject.dump(sb.ToString(), ed, fileName);

            sb = new StringBuilder();
            sb.Append("Number of line : " + lines.Count + "\n");
            for (int i = 0; i < numberOfPoint; i++)
            {
                sb.Append("\nPoint " + (i + 1) + " : " + arrayX[i] + "  " + arrayY[i]);

            }
            fileName = "D:/point.txt";
            mLineUtilObject.dump(sb.ToString(), ed, fileName);

            fileName = "D:/extractInput.txt";
            mLineUtilObject.dump(extractData.getStringInput(), ed, fileName);

            extractData.processExtractData();
            fileName = "D:/extractOutput.txt";
            mLineUtilObject.dump(extractData.getStringResult(), ed, fileName);

        }

    }
}