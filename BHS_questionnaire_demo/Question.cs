using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Drawing;



namespace BHS_questionnaire_demo
{
    abstract class Question
    {

        //base class of Question types

        //fields

        private int widgetWidth;
        private int widgetHeight;
        private int widgetXpos;
        private int widgetYpos;

        //big message box
        private Form bigMessageBox;

        //reference to the form that the main controls are placed on
        private Form form;

        //global storage which is visible to all questions
        private GlobalStore gs;

        private GlobalStore specialDataStore;

        private QuestionManager qm;

        private List<IfNode> ifNodeList;

        private int numTimesShown = 0;



        //properties
        public string Code { get; set; }
        public string Val { get; set; }
        public string ToCode { get; set; }
        public string FromCode { get; set; }
        public string Section { get; set; }
        public string IfSettingVal { get; set; }
        public string IfSettingKey { get; set; }
        public string OnErrorQuestionCompare { get; set; }
        public bool RecordThisQ { get; set; }    //true if we want to record sound for this question
        public string Validation { get; set; }
        public string Process { get; set; }
        public bool MultiLine { get; set; }
        public int LabelToBoxGap { get; set; }
        public int SelectLength { get; set; }
        public bool CheckPreviousDontKnow { get; set; }
        public string NextCode { get; set; }    //used to store the runtime generated next qCode
        public string MessageOnSecondFail { get; set; }
        public string PopulateFrom { get; set; } //used to prepopulate based on a previous Q
        public string ConfigKey { get; set; }
        public string IfSettingSpecial { get; set; }
        public bool NoAnswerDontKnowNotApplicable { get; set; } //true if we want to have the 3 std skip buttons



        //properties
        public string SetKey { get; set; }

        //has the user entered data for this question ?
        public bool PageSeen { get; set; }

        public string processedData { get; set; }





        //constructor

        public Question(Form form, Form bigMessageBox, GlobalStore gs, GlobalStore specialDataStore, QuestionManager qm)
        {

            this.form = form;
            this.bigMessageBox = bigMessageBox;
            this.PageSeen = false;
            this.gs = gs;
            this.specialDataStore = specialDataStore;
            this.qm = qm;
            processedData = null;

            RecordThisQ = false;

            ifNodeList = new List<IfNode>();



        }


        //methods

        public void addIfNode(IfNode node)
        {
            ifNodeList.Add(node);


        }


        public void incrementNumTimesShown()
        {
            //number of times the question has been displayed.
            numTimesShown++;


        }

        public int getNumTimesShown()
        {
            return numTimesShown;


        }


        public void audioRecording()
        {
            //are we currently recoding sound?
            //get state of the recorder

            string recorderState = qm.getRecorderState();

            //are we currently recording?
            if (recorderState == "on")
            {
                //yes

                //do we want recording for this question?

                if (!RecordThisQ)
                {
                    //no

                    //pause recording.
                    qm.pauseRecording();

                }


            }


            else if (recorderState == "off")
            {

                //do we want recording for this question?

                if (RecordThisQ)
                {
                    //yes

                    //start recording.
                    qm.startRecording();

                }



            }


            else if (recorderState == "paused")
            {

                //do we want recording for this question?

                if (RecordThisQ)
                {
                    //yes

                    //resume recording.
                    qm.resumeRecording();

                }




            }
            else
            {
                //state unknown
                throw new Exception();



            }







        }

        public bool IfSettingsOK()
        {

            //check IfSettingKey and IfSettingVal to make sure we can go ahead with this question

            if ((IfSettingKey != null) && (IfSettingVal != null))
            {

                //get the value from the global store

                string globalSettingForKey = gs.Get(IfSettingKey);

                if (globalSettingForKey == null)
                {
                    //global key was not set: skip test
                    return true;

                }
                else
                {
                    //does the global value == the required value ?

                    if (globalSettingForKey == IfSettingVal)
                    {
                        //yes
                        // OK, we can continue
                        return true;



                    }
                    else
                    {
                        //no we can't continue
                        return false;



                    }


                }






            }

            else if (IfSettingSpecial != null)
            {
                //custom setting

                if (IfSettingSpecial == "HepcFS")
                {

                    //show only if 
                    // CASE 1: SEX = 1 and PRGNT = 2 or PRGNT =888 and HCVT = 1 and STCST9 = 1
                    // CASE 2: SEX = 2 and HCVT = 1 and STCST9 = 1

                    string sex = gs.Get("SEX");
                    string prgnt = gs.Get("PRGNT");
                    string hcvt = gs.Get("HCVT");
                    string stcst9 = gs.Get("STCST9");

                    
                    if ((sex != null) && (prgnt != null) && (hcvt != null) && (stcst9 != null) && (sex =="2" ) && ((prgnt == "888") || (prgnt == "2")) && (hcvt == "1" || hcvt=="3" || hcvt=="4") && (stcst9 == "1"))
                    {


                        return true;

                    }
                    else if ((sex != null) && (hcvt != null) && (stcst9 != null) && (sex == "1") && (hcvt == "1" || hcvt == "3" || hcvt == "4") && (stcst9 == "1"))
                    {

                        return true;

                    }

                    else
                    {

                        return false;
                    }



                }
                else
                {

                    throw new Exception("unknown IfSettingSpecial");

                }



            }




            else
            {

                //do we have the more complex IfSettingOP type system?

                if (ifNodeList.Count == 0)
                {
                    //null settings, i.e. skip the test
                    return true;

                }
                else
                {

                    //we have to determine what
                    return processIfNodes();






                }








            }




        }

        private bool processIfNodes()
        {
            //for each node in the list and the operator, compare with each other member of the list in turn

            //get the first node
            IfNode currentNode = ifNodeList[0];

            //is this node valid, i.e. is the condition true?
            bool resultRunning = processThisIfNode(currentNode);

            for (int i = 1; i < ifNodeList.Count; i++)
            {
                IfNode thisNode = ifNodeList[i];

                bool resultThis = processThisIfNode(thisNode);

                //combine the current and the running result to generate a new running result
                string op = thisNode.Operator;

                if (op == "OR")
                {
                    resultRunning = resultRunning || resultThis;

                }
                else
                {
                    //AND
                    resultRunning = resultRunning && resultThis;


                }



            }

            return resultRunning;






        }

        private bool processThisIfNode(IfNode node)
        {
            string storedValue = gs.Get(node.Key);

            //is there any value?
            if (storedValue == null)
            {
                return false;

            }

            //does the value match the one we need?

            if (storedValue == node.Val)
            {
                return true;

            }
            else
            {
                return false;

            }





        }


        public void setStartQuestion(string startQcode)
        {
            //sets the question to start at when we next edit this questionaire
            gs.Add("START_QUESTION_CODE", startQcode);




        }





        public GlobalStore getGS()
        {

            return gs;


        }

        public QuestionManager getQM()
        {
            return qm;

        }


        public GlobalStore getSD()
        {
            return specialDataStore;


        }



        protected Form getForm()
        {
            return form;
        }

        protected Form getBigMessageBox()
        {
            return bigMessageBox;

        }


        public int getWidgetWidth()
        {

            return widgetWidth;

        }

        public void setWidgetWidth(string width)
        {

            widgetWidth = Convert.ToInt32(width);

        }

        public int getWidgetHeight()
        {

            return widgetHeight;


        }

        public void setWidgetHeight(string height)
        {

            widgetHeight = Convert.ToInt32(height);
        }

        public int getWidgetXpos()
        {

            return widgetXpos;
        }

        public void setWidgetXpos(string xpos)
        {

            widgetXpos = Convert.ToInt32(xpos);


        }

        public void setWidgetYpos(string ypos)
        {

            widgetYpos = Convert.ToInt32(ypos);

        }

        public int getWidgetYpos()
        {

            return widgetYpos;
        }



        //methods
        //convert a string to an int
        protected int strToInt(string str)
        {

            int numVal;

            numVal = Convert.ToInt32(str);

            return numVal;



        }

        public void showSection()
        {
            //display section on the form
            //qm.getSectionLabel().Text = Section;
            qm.getSectionLabel().Text = Section + " : " + Code;




        }

        protected bool testNullEntry(string userData)
        {

            //returns true if not null, empty or white-space


            if (string.IsNullOrWhiteSpace(userData))
            {

                return false;
            }
            else
            {

                return true;
            }


        }




        //called only in child classes
        //public abstract bool isSectionExit(Dictionary<string, string> questionToSectionMap);


        public abstract void configureControls(UserDirection direction);

        public abstract string processUserData();

        public abstract void removeControls();

        public abstract void save(TextWriter dhProcessedData, TextWriter dhUserData);

        public void saveFinalData(TextWriter dataOut)
        {
            //same as save except we filter out Questions which are duplicates etc



            //save the data stored in this object
            if (processedData != null)
            {


                //is this a skip setting?
                // if yes, convert to the numeric code

                string dOut;

                if (processedData == "No Answer")
                {
                    //dOut = "777";
                    dOut = GlobalConstants.NoAnswer;


                }
                else if (processedData == "Don't Know")
                {
                    //dOut = "888";
                    dOut = GlobalConstants.DontKnow;

                }

                else if (processedData == "Not Applicable")
                {
                    dOut = GlobalConstants.NotApplicable;

                    //dOut = "999";
                }
                else
                {
                    dOut = processedData;

                }



                //is this a duplicate code e.g. PLSAVG has a duplicate PLSAVG-2 ?
                // Match match = Regex.Match(Code, @".+-\d+$");

                // if (!match.Success)
                // {

                //did not match the duplicate pattern, so save

                //dataOut.WriteLine(Code + "\t" + processedData);
                dataOut.WriteLine(Code + "\t" + dOut);



                // }

            }




        }


        public void saveNextCode(TextWriter nextCodeOut)
        {
            if (NextCode != null)
            {

                nextCodeOut.WriteLine(Code + "\t" + NextCode);

            }
            else
            {
                //MessageBox.Show("nextcode was null for code:" + Code);

            }



        }





        public void save(TextWriter dataOut)
        {

            //save the data stored in this object
            if (processedData != null)
            {
                dataOut.WriteLine(Code + "\t" + processedData);


            }




        }

        public abstract void load(Dictionary<string, string> pDataDict, Dictionary<string, string> uDataDict);

        public void load(Dictionary<string, string> pDataDict)
        {

            //can I find this code in the dictionary
            if (pDataDict.ContainsKey(Code))
            {
                processedData = pDataDict[Code];

            }
            else
            {
                processedData = null;

            }



        }


        public void loadNextCode(Dictionary<string, string> ncDict)
        {

            //can I find this code in the dictionary
            if (ncDict.ContainsKey(Code))
            {
                NextCode = ncDict[Code];

            }
            else
            {
                NextCode = null;

            }



        }









        public void savePageSeen(TextWriter dataOut)
        {

            //save the data stored in this object
            if (PageSeen)
            {
                dataOut.WriteLine(Code);


            }


        }

        public void loadPageSeen(HashSet<string> seenPages)
        {

            //load data from file
            if (seenPages.Contains(Code))
            {

                PageSeen = true;


            }



        }


        protected string getPreviousData()
        {

            //return the processedData for the previous question
            if (OnErrorQuestionCompare == null)
            {
                throw new Exception();

            }


            string previousQuestionCode = OnErrorQuestionCompare;

            Question previousQuestion = qm.getQuestion(previousQuestionCode);

            string previousUserData = previousQuestion.processedData;

            return previousUserData;




        }






        protected virtual bool testCheckSameAsPrevious(string userData)
        {
            //is this the same as the previous userdata ?

            //get the previous question code
            string previousQuestionCode = FromCode;

            //get the previous question object

            Question previousQuestion = qm.getQuestion(previousQuestionCode);

            string previousUserData = previousQuestion.processedData;

            //are these the same ?

            if (userData == previousUserData)
            {
                return true;

            }
            else
            {

                //check the special case where the previous value was null

                if (string.IsNullOrEmpty(userData) && (previousUserData == "No User Entry"))
                {
                    return true;


                }
                else
                {

                    return false;

                }




            }


        }


        public bool TestSameAsParticipantID(string userData)
        {

            string idno = getGS().Get("IDNO");

            if (idno == null)
            {

                Form3 warningBox = getQM().getWarningBox();

                warningBox.setLabel("Warning: Can't check barcode against participant ID, as IDNO was not entered earlier");
                warningBox.ShowDialog();

                return true;



            }
            else
            {

                if (idno == userData)
                {
                    return true;

                }
                else
                {
                    return false;


                }



            }






        }






        public bool TestNumeric(string data)
        {

            try
            {
                Convert.ToDecimal(data);
                return true;



            }
            catch
            {
                return false;

            }



        }

        public bool TestNumericNotZero(string data)
        {

            try
            {
                decimal num = Convert.ToDecimal(data);

                if (num > 0)
                {
                    return true;

                }
                else
                {
                    return false;


                }



            }
            catch
            {
                return false;

            }



        }


        public string getSkipSetting()
        {
            return getQM().getMainForm().getSkipSetting();


        }

        public void clearControlButtons()
        {
            //deselect all control buttons (no answer, etc)
            getQM().getMainForm().clearControlButtons();




        }

        public void button_click(object sender, EventArgs e)
        {
            //a radiobutton has been clicked: deselect any of the control buttons (no answer, etc) that might have been selected
            clearControlButtons();



        }


        public void setSkipSetting()
        {
            if (processedData != null)
            {
                if (processedData == "No Answer" || processedData == "Don't Know" || processedData == "Not Applicable")
                {

                    //set the radiobutton on the main form

                    getQM().getMainForm().setSkipSetting(processedData);





                }
                else
                {
                    //normal answer
                    //deselect all the skip controls
                    getQM().getMainForm().clearControlButtons();

                }


            }


            else
            {

                //de-select skip buttons
                getQM().getMainForm().clearControlButtons();


            }



        }


        public void setFontSize(params Control[] controls)
        {

            //varargs function, takes any number of Controls

            float fontSize = 18;

            foreach (Control control in controls)
            {
                control.Font = new Font(control.Font.Name, fontSize, control.Font.Style, control.Font.Unit);

            }





        }

        public decimal CalcAverageOf2(String val1, String val2)
        {

            decimal reading1AsDec = Convert.ToDecimal(val1);
            decimal reading2AsDec = Convert.ToDecimal(val2);


            decimal average = (reading1AsDec + reading2AsDec) / 2;

            return average;




        }


        public virtual bool isFormExit()
        {
            //will this exit the form
            return false;




        }




        public virtual bool isSectionExit(Dictionary<string, string> questionToSectionMap)
        {

            //is this question a valid section exit, i.e. 
            //we must have a non-null processedData AND must point to a question outside of this section

            if (processedData == null)
            {

                //question was not answered:
                return false;



            }
            else
            {
                //this question has been answered, but does the ToCode point to a different section
                string toCodeSection = questionToSectionMap[ToCode];

                if (toCodeSection == Section)
                {
                    //next question is in this section
                    return false;

                }
                else
                {
                    //next question is in another section -> exit question
                    return true;


                }


            }




        }



    }
}
