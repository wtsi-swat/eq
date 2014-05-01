using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;

namespace BHS_questionnaire_demo
{
    class QuestionRank : Question, IoptionList
    {

         private Label label; 


        private List<Option> optionList;


        private List<RankBox> rankBoxList;
        private List<string> processedDataList;
        private List<string> rankList;      //populated from XML






        
        //constructor
        public QuestionRank(Form form, Form bigMessageBox, GlobalStore gs, GlobalStore specialDataStore, QuestionManager qm)
            : base(form, bigMessageBox, gs, specialDataStore, qm)
        {
            //init the optionslist
            optionList = new List<Option>();

            rankBoxList = new List<RankBox>();
            processedDataList = new List<string>();
            rankList = new List<string>();



        }

        //methods

        public void addRank(string rank)
        {

            rankList.Add(rank);

        }




        public override void save(TextWriter dhProcessedData, TextWriter dhUserData)
        {

            //call the base method save to save the processed data
            save(dhProcessedData);





        }
        public override void load(Dictionary<string, string> pDataDict, Dictionary<string, string> uDataDict)
        {

            //MessageBox.Show("loading QuestionRank");
            
            //load the processed data via the base class
            load(pDataDict);

            processedDataList.Clear();

            //parse the 3 options
            if (processedData != null)
            {


                string[] dList = processedData.Split(':');

                foreach (string d in dList)
                {
                    processedDataList.Add(d);



                }

            }
            


        }







        public override void removeControls()
        {
            

            getQM().getPanel().Controls.Remove(label);
            
            label.Dispose();

            foreach (RankBox rbox in rankBoxList)
            {
                rbox.removeControls();

            }

            rankBoxList.Clear();

            


        }

        public void addOption(Option op)
        {
            optionList.Add(op);



        }

        public bool testQuestionRefs(Dictionary<string, Question> questionHash)
        {

            return Utils.testQuestionRefs(questionHash, optionList);




        }

        public override void configureControls(UserDirection direction)
        {

            //turn the skip controls on again
            getQM().getMainForm().setSkipControlsVisible();
            
            //create a label 
            label = new Label();

            //the question Text shown to the user
            label.Text = Val;
            label.ForeColor = GlobalColours.mainTextColour;

            int labelXpos = getWidgetXpos();
            int labelYpos = getWidgetYpos();

            int widgetHeight= getWidgetHeight();

            //position of the Label
            label.Location = new Point(labelXpos, labelYpos);
            label.Size = new Size(getWidgetWidth(), getWidgetHeight());

            int rankBoxYpos = labelYpos + widgetHeight + 20;


            //build RankBoxList
            RankBox rb = null;

            int rankXpos = labelXpos;
            int rankYpos = widgetHeight;
            int rankWidth = getWidgetWidth();
            


            foreach (string text in rankList)
            {
                
                
                
                rb = new RankBox(text, this, rankXpos, rankBoxYpos, rankWidth, SelectLength, getQM());
                rankBoxList.Add(rb);

                //MessageBox.Show("adding text:" + text);

                rankBoxYpos += 80;



            }



            //set font size
            setFontSize(label);


            //set the previously selected data if any
            if (processedData != null)
            {

                for (int i = 0; i < rankBoxList.Count; i++)
                {
                    rankBoxList[i].setProcessedData(processedDataList[i]);



                }


            }


            
            //add items to combobox
            foreach (Option op in optionList)
            {

                foreach (RankBox rbox in rankBoxList)
                {
                    rbox.AddOption(op);

                    if (processedData != null)
                    {
                        //if this option contains the selected value, set as the selected option
                        if (op.getValue() == rbox.getProcessedData())
                        {
                            rbox.setSelectedOption(op);


                        }
                        
                       
                    }
                    
                }
                
               
            }




            //add to the form
            getQM().getPanel().Controls.Add(label);
            
            setSkipSetting();

            //start audio recording if enabled
            audioRecording();


           




        }

        public override string processUserData()
        {

            //we have seen this page
            PageSeen = true;

            //did the user skip this question ?
            string skipSetting = getSkipSetting();
            if (skipSetting != null)
            {
               

                processedData = skipSetting;

                return ToCode;




            }



            //get the selected date
            //are any of these null?

            foreach (RankBox rbox in rankBoxList)
            {

                if (rbox.getSelectedOption() == null)
                {

                    ((Form2)getBigMessageBox()).setLabel("Error: You must choose something for each");
                    getBigMessageBox().ShowDialog();

                    return Code;

                }

            }


            //validation: do the selected options all need to be different or not?
            if (Validation == "OptionsAllDifferent")
            {

                HashSet<Option> optionSet = new HashSet<Option>();

                foreach (RankBox rbox in rankBoxList)
                {
                    optionSet.Add(rbox.getSelectedOption());


                }

                //if all options are different, then the number in the set will == the number of ranks

                if (optionSet.Count != rankBoxList.Count)
                {

                    ((Form2)getBigMessageBox()).setLabel("Error: Each choice must be different");
                    getBigMessageBox().ShowDialog();

                    return Code;

                }




            }




            //convert to string
            StringBuilder sb = new StringBuilder();

            RankBox lastRank = rankBoxList.Last();
            string optionToCode = null;

            processedDataList.Clear();

            string thisVal;
            
            foreach (RankBox rbox in rankBoxList)
            {

                thisVal = rbox.getSelectedOption().getValue();
                processedDataList.Add(thisVal);
                
                if (rbox == lastRank)
                {

                    sb.Append(thisVal);
                }
                else
                {
                    sb.Append(thisVal).Append(":");

                }

                //does this option have a ToCode?
                string rboxToCode= rbox.getSelectedOption().ToCode;
                if (rboxToCode != null)
                {

                    optionToCode = rboxToCode;
                }


                
            }

            processedData = sb.ToString();


            //did any of the selected options have a ToCode?

            if (optionToCode == null)
            {
                return ToCode;
            }
            else
            {
                return optionToCode;


            }




        }











    }
}
