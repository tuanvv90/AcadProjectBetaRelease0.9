using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AcadProjectLayerUtils;

namespace AcadProjectExtractData
{
    class ExtractData
    {
        static int MAX_POINT = 10000;
        static int MAX_SET = 1000;
        static double EPSILON = 0.004;
        static int VISTED = 1;
        static int UNVISITED = 0;
        static bool CONNECTED = true;
        static bool UNCONNECTED = false;


        // input
        int numberOfPoint;
        double[] x = new double[MAX_POINT];
        double[] y = new double[MAX_POINT];
        bool[,] isLineArray = new bool[MAX_POINT, MAX_POINT];

        // temp data
        int[] visited = new int[MAX_POINT];
        int rsCurrentSetIndex;
        int rsCurrentPointIndex;

        // extracted ouput
        double[,] resultX = new double[MAX_SET, MAX_POINT];
        double[,] resultY = new double[MAX_SET, MAX_POINT];
        //double[,] resultX = new double[20, 20];
        //double[,] resultY = new double[20, 20];
        int[] numberPointOfSet = new int[MAX_SET];
        int numberSet;

        //List of Layer
        public List<LayerUtils> mListLayer = new List<LayerUtils>();
        public PointUtils zeroPoint = new PointUtils();
        public PointUtils centerPoint = new PointUtils();
        public double MSS = 95.00;

        public ExtractData()
        {
            for(int i = 0; i< MAX_POINT; i++)
            {
                for(int j = 0; j<MAX_POINT; j++)
                {
                    isLineArray[i, j] = UNCONNECTED;
                }
            }
        }

        public void setNumberOfPoint(int nPoints)
        {
            numberOfPoint = nPoints;
        }

        public double[] getArrayX()
        {
            return x;
        }

        public double[] getArrayY()
        {
            return y;
        }

        public bool[,] getIsLineMatrix()
        {
            return isLineArray;
        }

        public string getStringResult()
        {
            string str = "";

            for (int i = 0; i < numberSet; i++)
            {
                //Create new layer then add points to it
                LayerUtils addLayer = new LayerUtils();

                //Console.Write("Layer {0} : \n", i);
                str += "Layer " + i + " : \n";
                for (int j = 0; j < numberPointOfSet[i]; j++)
                {
                    //Console.Write("x = {0}\ty = {1}\n", resultX[i, j], resultY[i, j]);
                    str += "x = "+ resultX[i, j] + "\ty = "+ resultY[i, j]+ "\n";
                    PointUtils addPoint = new PointUtils();
                    addPoint.setXY(resultX[i, j], resultY[i, j]);
                    addLayer.addPointToLayer(addPoint);
                }
                mListLayer.Add(addLayer);

                //Console.Write("\n");
                str += "\n";
            }

            calculateHighDistance(mListLayer, zeroPoint);
            for (int i = 0; i < mListLayer.Count; i++) 
            {
                str += "\nDistance/High of Layer : " + i;
                str += mListLayer[i].toStringData().ToString();
            }
            return str;
        }

        public void calculateHighDistance(List<LayerUtils> listLayer, PointUtils zeroPoint) 
        {
            for (int i = 0; i < listLayer.Count; i++)
            {
                for (int j = 0; j < listLayer[i].numberOfPoint(); j++)
                {
                    int centerIndex = listLayer[i].numberOfPoint() / 2;
                    
                    double distance = Math.Round(Math.Abs(listLayer[i].getPointAt(j).getX() - listLayer[i].getPointAt(centerIndex).getX()), 2);
                    double high = Math.Round(Math.Abs(listLayer[i].getPointAt(j).getY() - zeroPoint.getY()), 2) + MSS;
                    
                    listLayer[i].getPointAt(j).setDistance(distance);
                    listLayer[i].getPointAt(j).setHigh(high);
                }
            }
        }

        public double calculateDistance(PointUtils point, PointUtils centerPoint)
        {
            return Math.Abs(point.getX() - centerPoint.getX());
        }

        public string getStringInput()
        {
            string str = "";

            for (int i = 0; i < numberOfPoint; i++)
            {
                str += "\nPoint " + (i + 1) + " : " + x[i] + "  " + y[i];
            }
            str += "\n\n";
            for (int i = 0; i<numberOfPoint; i++)
            {
                for(int j = 0; j<numberOfPoint; j++)
                {
                    if (isLineArray[i, j])
                    {
                        str += "true\t";
                    }
                    else
                    {
                        str += "false\t";
                    }
                }
                str += "\n\n";
            }
            return str;
        }

        public void printOutputResult()
        {
            for (int i = 0; i < numberSet; i++)
            {
                Console.Write("Layer {0} : \n", i);
                for (int j = 0; j < numberPointOfSet[i]; j++)
                {
                    Console.Write("x = {0}\ty = {1}\n", resultX[i, j], resultY[i, j]);
                }
                Console.Write("\n");
            }
        }

        public void processExtractData()
        {
            initTempData();
            //initInputData();

            while (true)
            {
                findSetFromAPoint();
                if (checkAllVisited())
                {
                    return;
                }
            }
        }

        private void initInputData()
        {
            x[0] = 6.00; y[0] = 98.25; isLineArray[0, 1] = CONNECTED; isLineArray[1, 0] = CONNECTED; isLineArray[5, 0] = CONNECTED; isLineArray[0, 5] = CONNECTED;
            x[1] = 3.50; y[1] = 98.35; isLineArray[1, 2] = CONNECTED; isLineArray[2, 1] = CONNECTED;
            x[2] = 0.00; y[2] = 98.47; isLineArray[2, 3] = CONNECTED; isLineArray[3, 2] = CONNECTED;
            x[3] = -3.50; y[3] = 98.59; isLineArray[3, 4] = CONNECTED; isLineArray[4, 3] = CONNECTED;
            x[4] = -6.00; y[4] = 98.49; isLineArray[9, 4] = CONNECTED; isLineArray[4, 9] = CONNECTED;

            x[5] = 6.05; y[5] = 98.20; isLineArray[10, 5] = CONNECTED; isLineArray[5, 10] = CONNECTED; isLineArray[6, 5] = CONNECTED; isLineArray[5, 6] = CONNECTED;
            x[6] = 3.50; y[6] = 98.30; isLineArray[6, 7] = CONNECTED; isLineArray[7, 6] = CONNECTED;
            x[7] = 0.00; y[7] = 98.42; isLineArray[7, 8] = CONNECTED; isLineArray[8, 7] = CONNECTED;
            x[8] = -3.50; y[8] = 98.54; isLineArray[9, 8] = CONNECTED; isLineArray[8, 9] = CONNECTED;
            x[9] = -6.05; y[9] = 98.44; isLineArray[15, 9] = CONNECTED; isLineArray[9, 15] = CONNECTED;

            x[10] = 6.16; y[10] = 98.09; isLineArray[11, 10] = CONNECTED; isLineArray[10, 11] = CONNECTED;
            x[11] = 6.36; y[11] = 98.09; isLineArray[17, 11] = CONNECTED; isLineArray[11, 17] = CONNECTED; isLineArray[12, 11] = CONNECTED; isLineArray[11, 12] = CONNECTED;
            x[12] = 3.50; y[12] = 98.20; isLineArray[12, 13] = CONNECTED; isLineArray[13, 12] = CONNECTED;
            x[13] = 0.00; y[13] = 98.32; isLineArray[14, 13] = CONNECTED; isLineArray[13, 14] = CONNECTED;
            x[14] = -3.50; y[14] = 98.44; isLineArray[14, 16] = CONNECTED; isLineArray[16, 14] = CONNECTED;
            x[15] = -6.16; y[15] = 98.34; isLineArray[16, 15] = CONNECTED; isLineArray[15, 16] = CONNECTED;
            x[16] = -6.36; y[16] = 98.33; isLineArray[22, 16] = CONNECTED; isLineArray[16, 22] = CONNECTED;

            x[17] = 6.67; y[17] = 97.77; isLineArray[18, 17] = CONNECTED; isLineArray[17, 18] = CONNECTED;
            x[18] = 6.82; y[18] = 97.77; isLineArray[24, 18] = CONNECTED; isLineArray[18, 24] = CONNECTED; isLineArray[18, 19] = CONNECTED; isLineArray[19, 18] = CONNECTED;
            x[19] = 3.50; y[19] = 97.90; isLineArray[19, 20] = CONNECTED; isLineArray[20, 19] = CONNECTED;
            x[20] = 0.00; y[20] = 98.02; isLineArray[20, 21] = CONNECTED; isLineArray[21, 20] = CONNECTED;
            x[21] = -3.50; y[21] = 98.14; isLineArray[21, 23] = CONNECTED; isLineArray[23, 21] = CONNECTED;
            x[22] = -6.67; y[22] = 98.02; isLineArray[22, 23] = CONNECTED; isLineArray[23, 22] = CONNECTED;
            x[23] = -6.82; y[23] = 98.01; isLineArray[28, 23] = CONNECTED; isLineArray[23, 28] = CONNECTED;

            x[24] = 7.38; y[24] = 97.20; isLineArray[29, 24] = CONNECTED; isLineArray[24, 29] = CONNECTED; isLineArray[24, 25] = CONNECTED; isLineArray[25, 24] = CONNECTED;
            x[25] = 3.50; y[25] = 97.36; isLineArray[25, 26] = CONNECTED; isLineArray[26, 25] = CONNECTED;
            x[26] = 0.00; y[26] = 97.48; isLineArray[26, 27] = CONNECTED; isLineArray[27, 26] = CONNECTED;
            x[27] = -3.50; y[27] = 97.60; isLineArray[27, 28] = CONNECTED; isLineArray[28, 27] = CONNECTED;
            x[28] = -7.38; y[28] = 97.45; isLineArray[28, 33] = CONNECTED; isLineArray[33, 28] = CONNECTED;

            x[29] = 7.69; y[29] = 96.89; isLineArray[29, 30] = CONNECTED; isLineArray[30, 29] = CONNECTED;
            x[30] = 3.50; y[30] = 97.06; isLineArray[31, 30] = CONNECTED; isLineArray[30, 31] = CONNECTED;
            x[31] = 0.00; y[31] = 97.18; isLineArray[31, 32] = CONNECTED; isLineArray[32, 31] = CONNECTED;
            x[32] = -3.50; y[32] = 97.30; isLineArray[32, 333] = CONNECTED; isLineArray[33, 32] = CONNECTED;
            x[33] = -7.69; y[33] = 97.14;
            numberOfPoint = 34;
        }



        private bool checkAllVisited()
        {
            for (int i = 0; i < numberOfPoint; i++)
            {
                if (visited[i] != VISTED)
                {
                    return false;
                }
            }
            return true;
        }

        private void initTempData()
        {
            for (int i = 0; i < numberOfPoint; i++)
            {
                visited[i] = UNVISITED;
                //for (int j = 0; j < numberOfPoint; j++)
                //{
                //    isLineArray[i, j] = UNCONNECTED;
                //}
            }
            rsCurrentSetIndex = 0;
            rsCurrentPointIndex = 0;
            numberSet = 0;
        }

        private void findSetFromAPoint()
        {

            int mostLeftIndex = getIndexMostLeftPoint();
            int mostRightIndex = getIndexMostRightPoint();
            int currenIndex = mostLeftIndex;


            resultX[rsCurrentSetIndex, rsCurrentPointIndex] = x[currenIndex];
            resultY[rsCurrentSetIndex, rsCurrentPointIndex] = y[currenIndex];
            rsCurrentPointIndex++;

            while (true)
            {


                // Find next true point connected with current point
                int nextIndex1 = -1, nextIndex2 = -1;
                int nextIndex = -1;
                for (int i = 0; i < numberOfPoint; i++)
                {
                    if (isLineArray[currenIndex, i] && visited[i] == UNVISITED)
                    {
                        if (nextIndex1 == -1) nextIndex1 = i;
                        else if (nextIndex2 == -1) nextIndex2 = i;
                    }
                }


                // For debug
                //if (rsCurrentSetIndex == 3 && rsCurrentPointIndex == 6)
                //{
                //    int h = 0;
                //}

                // Consider next poit belong to current set
                if (nextIndex2 == -1 && nextIndex1 != -1)
                {
                    nextIndex = nextIndex1;

                    resultX[rsCurrentSetIndex, rsCurrentPointIndex] = x[nextIndex];
                    resultY[rsCurrentSetIndex, rsCurrentPointIndex] = y[nextIndex];
                    visited[currenIndex] = VISTED;
                    rsCurrentPointIndex++;

                    if (nextIndex == mostRightIndex)
                    {
                        if (rsCurrentSetIndex != 0)
                        {
                            visited[nextIndex] = VISTED;
                        }

                        break;
                    }

                }
                else if (nextIndex2 == -1 && nextIndex1 == -1)
                {
                    break;
                }
                else if (nextIndex1 != -1 && nextIndex2 != -1)
                {
                    double d1x = x[nextIndex1] - x[currenIndex], d2x = x[nextIndex2] - x[currenIndex];
                    double d1y = y[nextIndex1] - y[currenIndex], d2y = y[nextIndex2] - y[currenIndex];

                    nextIndex = d1x > d2x ? nextIndex1 : nextIndex2;
                    double considerDy = d1x > d2x ? d2y : d1y;

                    resultX[rsCurrentSetIndex, rsCurrentPointIndex] = x[nextIndex];
                    resultY[rsCurrentSetIndex, rsCurrentPointIndex] = y[nextIndex];
                    rsCurrentPointIndex++;

                    if (nextIndex == mostRightIndex)
                    {
                        //visited[currenIndex] = VISTED;

                        if (Math.Abs(considerDy) < 0.03)
                        {
                            visited[currenIndex] = VISTED;
                        }
                        visited[nextIndex] = VISTED;
                        break;
                    }
                    else
                    {
                        double tan1 = d1y / d1x;
                        double tan2 = d2y / d2x;
                        if (Math.Abs(tan1 - tan2) < 0.1)
                        {
                            visited[currenIndex] = VISTED;
                        }
                    }
                }
                currenIndex = nextIndex;

            }

            numberPointOfSet[rsCurrentSetIndex] = rsCurrentPointIndex;
            numberSet++;
            rsCurrentSetIndex++;
            rsCurrentPointIndex = 0;

        }

        private int getIndexMostLeftPoint()
        {
            int index = 0;
            for (int i = 1; i < numberOfPoint; i++)
            {
                if (x[i] < x[index] && visited[i] != 1)
                {
                    index = i;
                }
            }
            return index;
        }

        private int getIndexMostRightPoint()
        {
            int index = 0;
            for (int i = 1; i < numberOfPoint; i++)
            {
                if (x[i] > x[index] && visited[i] != 1)
                {
                    index = i;
                }
            }
            return index;
        }
    }
}
