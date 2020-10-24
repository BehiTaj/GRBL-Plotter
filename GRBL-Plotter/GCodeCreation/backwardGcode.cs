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
        public string[] backwardGcode;  //Finaly backward list





        public void createBackwardList(string[] gcode)
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


                if (isFirstGcodeLine) //is it first gcode Line?
                {
                    isFirstGcodeLine = false;
                    tmp += "G10 L20" + returnCoordsVal(curLine);
                }
                else
                {
                    if (preModalGroup.motionMode == 1 || preModalGroup.motionMode == 0)
                    {
                        tmp += string.Format("G{0} {1}", preModalGroup.motionMode, returnCoordsVal(curLine));
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
            SaveFileDialog save = new SaveFileDialog();
            if (save.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllLines(save.FileName, result);
                MessageBox.Show("saved");
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


        private string returnIJ(gcodeByLine preCoords, gcodeByLine curCoords)
        {
            double cX = (double)curCoords.x + (double)preCoords.i;
            double cy = (double)curCoords.y + (double)preCoords.j;

            double I = cX - (double)preCoords.x;
            double J = cy - (double)preCoords.y;

            return string.Format(" I{0:0.0000} J{1:0.0000}", I, J);
        }


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
    }
}
