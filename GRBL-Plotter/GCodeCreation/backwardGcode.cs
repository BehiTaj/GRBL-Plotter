using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GRBL_Plotter.GCodeCreation
{
    public class BackwardGCode
    {
        public List<string> createBackwardList(string[] gcode)
        {
            gcodeByLine preLine = new gcodeByLine(); //previus line    
            modalGroup preModalGroup = new modalGroup();

            gcodeByLine curLine = new gcodeByLine(); //current line
            modalGroup curModalGroup = new modalGroup();


            string tmp = "";
            bool isFirstGcodeLine = true;
            List<string> result = new List<string>();

            for (int i = gcode.Length - 1; i >= 0; i--)
            {
                if (gcode[i].Contains("G10 L20"))
                {
                    curLine.parseLine(i, gcode[i], ref curModalGroup, false);
                }
                else
                {
                    curLine.parseLine(i, gcode[i], ref curModalGroup);
                }
                tmp = "";

                tmp += "N" + curLine.gcodeNUmber + " ";
                if (isFirstGcodeLine) //is it first gcode Line?
                {
                    isFirstGcodeLine = false;
                    tmp += "G10 L20" + returnCoordsVal(curLine);
                }
                else
                {
                    if (preModalGroup.motionMode == 1 || preModalGroup.motionMode == 0)
                    {
                        tmp += string.Format("G{0}{1}", preModalGroup.motionMode, returnCoordsVal(curLine));
                    }
                    else
                    {
                        tmp += arcMaker(curLine, preLine, preModalGroup);
                    }
                }

                result.Add(tmp);
                tmp += "\r\n";
                preLine = new gcodeByLine(curLine);
                preModalGroup = new modalGroup(curModalGroup);
            }

            //      +++  Test  +++
            //SaveFileDialog save = new SaveFileDialog();
            //if (save.ShowDialog() == DialogResult.OK)
            //{
            //    File.WriteAllLines(save.FileName, result);
            //}

            return result;
        }

        public string[] initBackward(string[] gcodes, int lineNr, double holdedLocationX, double holdedLocationY, double holdLocationZ)
        {

            gcodes.ToList<string>();
            List<string> result = new List<string>();

            bool isFind = false;
            int index = 0;

            for (int i = 0; i < gcodes.Length; i++)
            {
                if (gcodes[i].StartsWith("N" + lineNr))
                {
                    isFind = true;
                    index = i;
                    break;
                }
            }

            if (!isFind)
            {
                MessageBox.Show("Problem in backward !!!");
                return null;
            }
            else
            {
                gcodeByLine nextLine = new gcodeByLine(); //nextLine in backward or preLine in forward
                modalGroup nextLineModalGroup = new modalGroup();
                nextLine.parseLine(0, gcodes[index + 1], ref nextLineModalGroup);

                gcodeByLine curLine = new gcodeByLine(); //nextLine in backward or preLine in forward
                modalGroup curLineModalGroup = new modalGroup();
                curLine.parseLine(0, gcodes[index], ref curLineModalGroup);

                if (nextLineModalGroup.containsG2G3)
                {
                    string newGcode = "";

                    double motionMode = 3;
                    if (nextLineModalGroup.motionMode == 2)
                    {
                        motionMode = 2;
                    }
                    newGcode = string.Format("G1 X{4} Y{5} F100\r\nN{3} G{0}{1}{2}", motionMode, returnCoordsVal(nextLine), returnIJ2(curLine, nextLine,
                        (double)holdedLocationX, (double)holdedLocationY), lineNr - 1, holdedLocationX, holdedLocationY);
                    gcodes[index + 1] = newGcode;
                    gcodes = gcodes.Skip(1).ToArray();
                }
                else
                {
                    gcodes[index + 1] += "F100";
                    gcodes = gcodes.Skip(1).ToArray();
                }

                return gcodes.Skip(index).ToArray();
            }
        }




        #region [ returnCoordsVal ]
        /// <summary>
        /// get a parsed line and return a string that contains value of line coords
        /// example: return " X10 Y20 Z10"
        /// </summary>
        /// <param name="values">parsedLine</param>
        /// <returns></returns>
        private string returnCoordsVal(gcodeByLine values)
        {
            string result = "";
            if (values.x != null)
            {
                result += " X" + values.x;
            }
            if (values.y != null)
            {
                result += " Y" + values.y;
            }
            if (values.z != null)
            {
                result += " Z" + values.z;
            }
            if (values.a != null)
            {
                result += " A" + values.a;
            }
            if (values.b != null)
            {
                result += " B" + values.b;
            }
            if (values.c != null)
            {
                result += " C" + values.c;
            }
            return result;
        }
        #endregion

        #region [ returnIJ ]
        private string returnIJ(gcodeByLine preCoords, gcodeByLine curCoords)
        {
            double cX = (double)curCoords.x + (double)preCoords.i;
            double cy = (double)curCoords.y + (double)preCoords.j;

            double I = cX - (double)preCoords.x;
            double J = cy - (double)preCoords.y;

            return string.Format(" I{0:0.0000} J{1:0.0000}", I, J);
        }

        private string returnIJ(gcodeByLine preCoords, double X, double Y)
        {
            double cX = X + (double)preCoords.i;
            double cy = Y + (double)preCoords.j;

            double I = cX - (double)preCoords.x;
            double J = cy - (double)preCoords.y;

            return string.Format(" I{0:0.0000} J{1:0.0000}", I, J);
        }

        private string returnIJ2(gcodeByLine curCoords, gcodeByLine preCoords, double X, double Y)
        {
            double cX = (double)curCoords.x + (double)preCoords.i;
            double cy = (double)curCoords.y + (double)preCoords.j;

            double I = cX - X;
            double J = cy - Y;

            return string.Format(" I{0:0.0000} J{1:0.0000}", I, J);
        }
        #endregion


        #region [ arcMaker ]
        private string arcMaker(gcodeByLine curCoords, gcodeByLine preCoords, modalGroup preModalGroup)
        {
            string result = "";

            int motionMode = 2;
            if (preModalGroup.motionMode == 2)
            {
                motionMode = 3;
            }

            result = string.Format("G{0}{1}{2}", motionMode, returnCoordsVal(curCoords), returnIJ(preCoords, curCoords));

            return result;
        }
        #endregion
    }
}
