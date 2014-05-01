using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Resources;
using System.Text.RegularExpressions;






namespace BHS_questionnaire_demo
{
    class QuestionManager
    {
        //class that contains a hash of questions

        //fields

        //hash of Question objects, where the key is the code
        private Dictionary<string, Question> questionHash;

        //the question that we are currently showing
        private Question currentQuestion;

        //the code for the next question to process
        private string nextCode;

        private Form bigMessageBox;

        //stack to keep track of which questions have been visited
        //the stack contains the question code
        private Stack<string> qStack;


        //global-store: a data repository that all questions can access
        private GlobalStore gs;

        //special questions: for questions that generate more than 1 answer: that can be stored here
        private GlobalStore specialDataStore;

        private Panel mainPanel;

        private Label sectionLabel;

        private Form3 warningMessageBox;
        private AdviceForm adviceBox;

        private ConfirmForm confirmBox;

        private Form1 mainForm;

        private CompletenessForm cf;

        private string language;



        private string fileNameProcessedData = null;
        private string fileNameUserData = null;
        private string fileNameStack = null;
        private string fileNameGlobalStore = null;
        private string fileNameSpecialStore = null;
        private string fileNamePageSeen = null;
        private string fileNameNextCode = null;

        //final data output
        private string fileNameFinalData = null;

        private Label latitudeLabel;
        private Label longitudeLabel;

        //if this is set to a specific country, limit possible longitude/latitude to that country, i.e. anything else is an error.
        private string gpsCountry;

        //used to check allowed limits of gps readings
        private GPSCountryManager countryManager = null;

        //map of section name to list of question-codes
        private Dictionary<string, List<string>> sectionToCodeList = null;

        //map of question code to section name
        private Dictionary<string, string> questionCodeToSection = null;



        //userID

        private string userID;


        //userDir: where to store files
        private string userDir;


        //can we skip this question ?
        public bool SkipThisQuestion { get; set; }

        private string baseDir; //dir that contains all files associated with EQ

        private AudioRecorder recorder;

        private Timer timer;    //check periodically disc usage

        private string userConfigFileName;  //name of the participant's config file



        //constructor
        public QuestionManager(Form bigMessageBox, string userDir, Panel mainPanel, Label sectionLabel, string userID, Label latitudeLabel, Label longitudeLabel, AdviceForm adviceBox, ConfirmForm confirmBox, Form1 mainForm, string gpsCountry, string language, string baseDir, string userConfigFileName)
        {

            this.adviceBox = adviceBox;
            this.confirmBox = confirmBox;
            this.mainPanel = mainPanel;
            this.sectionLabel = sectionLabel;
            this.userID = userID;
            this.mainForm = mainForm;
            this.gpsCountry = gpsCountry;
            this.baseDir = baseDir;
            this.userConfigFileName = userConfigFileName;



            if (gpsCountry != null)
            {
                countryManager = new GPSCountryManager(gpsCountry);

            }



            questionHash = new Dictionary<string, Question>();

            this.bigMessageBox = bigMessageBox;

            qStack = new Stack<string>();

            gs = new GlobalStore();

            specialDataStore = new GlobalStore();


            //userID = "testUser12345";


            this.userDir = userDir;


            //set up the filenames for data saving

            this.fileNameProcessedData = userDir + @"\state_data_processed.txt";
            this.fileNameUserData = userDir + @"\state_data_userdata.txt";
            this.fileNameStack = userDir + @"\state_data_stack.txt";
            this.fileNameGlobalStore = userDir + @"\state_data_global.txt";
            this.fileNameSpecialStore = userDir + @"\state_data_special.txt";
            this.fileNamePageSeen = userDir + @"\state_data_page_seen.txt";

            this.fileNameFinalData = userDir + @"\final_data_" + userID + ".txt";
            this.fileNameNextCode = userDir + @"\nextcode_data.txt";






            //labels which contain GPS data
            this.latitudeLabel = latitudeLabel;
            this.longitudeLabel = longitudeLabel;

            this.language = language;

            this.recorder = new AudioRecorder(this);
            timer = new Timer();

            timer.Tick += new EventHandler(timer_Tick); // Everytime timer ticks, timer_Tick will be called
            timer.Interval = 60000;              // Timer will tick every 60 seconds
            timer.Enabled = true;                       // Enable the timer
            timer.Start();                              // Start the timer




        }









        //constructor used by CompletenessForm
        public QuestionManager(string userDir, string userID, CompletenessForm cf)
        {


            this.userID = userID;

            this.cf = cf;


            questionHash = new Dictionary<string, Question>();



            qStack = new Stack<string>();

            gs = new GlobalStore();

            specialDataStore = new GlobalStore();


            this.userDir = userDir;


            //set up the filenames for data saving

            this.fileNameProcessedData = userDir + @"\state_data_processed" + ".txt";
            this.fileNameUserData = userDir + @"\state_data_userdata" + ".txt";
            this.fileNameStack = userDir + @"\state_data_stack" + ".txt";
            this.fileNameGlobalStore = userDir + @"\state_data_global" + ".txt";
            this.fileNameSpecialStore = userDir + @"\state_data_special" + ".txt";
            this.fileNamePageSeen = userDir + @"\state_data_page_seen" + ".txt";

            this.fileNameFinalData = userDir + @"\final_data_" + userID + ".txt";


        }

        //methods

        public string getSpecialStartQuestion()
        {
            string startQcode = gs.Get("START_QUESTION_CODE");

            //reset to null
            gs.Add("START_QUESTION_CODE", "NULL");

            if (startQcode == null || startQcode == "NULL")
            {
                return null;

            }
            else
            {

                return startQcode;

            }



        }




        public void checkCompleteness(Qconfig config, bool showResult)
        {

            //check each section is complete

            string firstQcode;
            Question thisQ;
            HashSet<Question> qSet = new HashSet<Question>();
            string debug = "";



            foreach (Section section in config.getSectionList())
            {

                //create a set, so we know if any questiona are re-visited, i.e. loops
                //clear the set initially.
                qSet.Clear();



                firstQcode = section.getFirstQuestion();
                thisQ = questionHash[firstQcode];

                bool result = pathScan(thisQ, section.getSectionTitle(), qSet);

                //save in the config
                section.setCompleteness(result);

                debug += section.getSectionTitle() + ":" + result + "\n";




            }

            //MessageBox.Show(debug);

            //whole-form scan: not section by section. This will find completed sections due to abort in consents section
            pathScan2(questionHash["START"], config, qSet);





            //display window
            if (showResult)
            {

                Dictionary<string, bool> exportDict = new Dictionary<string, bool>();


                foreach (Section section in config.getSectionList())
                {

                    exportDict[section.getSectionTitle()] = section.getCompleteness();


                    //showInPanel(section.getCompleteness(), section.getSectionTitle(), null);


                }


                Utils.displayCompletionWindow(exportDict);






            }





            //save to the user's config file
            saveCompletenessToConfig(config);




        }


        private void saveCompletenessToConfig(Qconfig config)
        {

            Dictionary<string, string> dataDict = new Dictionary<string, string>();
            string key;
            string val;



            foreach (Section section in config.getSectionList())
            {

                key = "section_complete^" + section.getSectionTitle();
                val = section.getCompleteness().ToString();

                dataDict[key] = val;




            }

            Utils.updateUserConfigFile(dataDict, userConfigFileName);




        }



        public void close()
        {

            //stop any timers etc
            timer.Stop();
            timer.Dispose();




        }

        private void timer_Tick(object sender, EventArgs e)
        {
            //do we have enough spare disk space

            long MIN_SPACE_BYTES = 1048576 * 50;      //50 Mbytes
            // long MIN_SPACE_BYTES = 500000000000;

            //what drivename as we using?
            string pathRoot = Path.GetPathRoot(baseDir);

            try
            {
                DriveInfo di = new DriveInfo(pathRoot);

                long freeSpace = di.AvailableFreeSpace;

                //MessageBox.Show("free disk space:" + freeSpace);


                //do we have too little free space?

                if (freeSpace < MIN_SPACE_BYTES)
                {

                    recorder.DiskSpaceOK = false;
                    recorder.stopRecording();
                    timer.Stop();




                    ((Form2)bigMessageBox).setLabel("Warning: Disk space is running low: Audio recording will be stopped");
                    bigMessageBox.ShowDialog();

                }
                else
                {

                    recorder.DiskSpaceOK = true;

                }



            }
            catch
            {
                ((Form2)bigMessageBox).setLabel("Error: could not monitor free disk space");
                bigMessageBox.ShowDialog();


            }





        }

        public string getRecorderState()
        {

            return recorder.getRecorderState();



        }

        public void pauseRecording()
        {

            recorder.pauseRecording();


        }

        public void startRecording()
        {

            recorder.startRecording();


        }

        public void resumeRecording()
        {

            recorder.resumeRecording();

        }

        public void stopRecording()
        {

            recorder.stopRecording();


        }




        public string getUserDir()
        {

            return userDir;

        }

        public string getBaseDir()
        {

            return baseDir;

        }

        public void showDebug()
        {

            String all = "";


            //get each object in the hash
            foreach (KeyValuePair<string, Question> kv in questionHash)
            {

                string key = kv.Key;
                Question val = kv.Value;

                all += (key + "\n");

                /*

                if (val is QuestionText)
                {
                    string show = key + ":" + ((QuestionText)val).LabelToBoxGap + "\n";

                    all += show;




                }
                 */



            }

            MessageBox.Show(all);




        }

        public void mapSectionToQuestion()
        {

            //map each section-name to a list of question-codes
            sectionToCodeList = new Dictionary<string, List<string>>();

            questionCodeToSection = new Dictionary<string, string>();


            string sectionName;
            string questionCode;
            Question thisQuestion;
            List<string> qList;

            //get each Question from the hash
            foreach (KeyValuePair<string, Question> kv in questionHash)
            {

                //get the section-name
                questionCode = kv.Key;
                thisQuestion = kv.Value;

                sectionName = thisQuestion.Section;

                //map question to section
                questionCodeToSection[questionCode] = sectionName;

                //get the list of questions-codes for that section
                //qList = sectionToCodeList[sectionName];

                if (sectionToCodeList.TryGetValue(sectionName, out qList))
                {
                    //there is a list already -> add the code
                    qList.Add(questionCode);

                }
                else
                {
                    //not a list already: make one.
                    qList = new List<string>();
                    qList.Add(questionCode);
                    sectionToCodeList[sectionName] = qList;



                }





            }





        }




        private void setFontSize(params Control[] controls)
        {

            //varargs function, takes any number of Controls

            float fontSize = 18;

            foreach (Control control in controls)
            {
                control.Font = new Font(control.Font.Name, fontSize, control.Font.Style, control.Font.Unit);

            }





        }

        /*
        public bool testEachSectionForCompletion(Qconfig config)
        {

            string firstQcode;
            Question thisQ;

            
            foreach (Section section in config.getSectionList())
            {

                firstQcode = section.getFirstQuestion();
                thisQ = questionHash[firstQcode];

                pathScan(thisQ, section.getSectionTitle);







            }



        }
         */


        private void pathScanSave(Question thisQ, TextWriter fdh, HashSet<Question> qSet, string thisSectionName)
        {

            //have we already seen this question?
            if (qSet.Contains(thisQ))
            {
                //yes. We have a loop : end scan
                return;



            }
            else
            {
                //add it to the set
                qSet.Add(thisQ);


            }

            //have we reached the last question?
            if (thisQ.Code == "EXIT")
            {
                return;


            }

            //save data
            thisQ.saveFinalData(fdh);



            //does the path continue: check the nextcode
            string nextCode = thisQ.NextCode;

            if (nextCode == null)
            {
                //path ended
                return;


            }



            //get the next question
            Question nextQ = questionHash[nextCode];

            //have we reached the next section?
            if (nextQ.Section != thisSectionName)
            {
                //yes
                return;


            }




            //continue the scan
            pathScanSave(nextQ, fdh, qSet, thisSectionName);



        }





        private void pathScan2(Question thisQ, Qconfig config, HashSet<Question> qSet)
        {

            //have we already seen this question?
            if (qSet.Contains(thisQ))
            {
                //yes. We have a loop : end scan
                return;



            }
            else
            {
                //add it to the set
                qSet.Add(thisQ);


            }

            //have we reached the last question?
            if (thisQ.Code == "EXIT")
            {
                return;


            }


            //does the path continue: check the nextcode
            string nextCode = thisQ.NextCode;

            if (nextCode == null)
            {
                //path ended
                return;


            }


            //get the next question
            Question nextQ = questionHash[nextCode];

            //have we changed to a new section
            if (thisQ.Section != nextQ.Section)
            {
                //set each section from this to the new one (but not including it) as complete
                config.setSectionsComplete(thisQ.Section, nextQ.Section);



            }

            //continue the scan
            pathScan2(nextQ, config, qSet);



        }



        private bool pathScan(Question thisQ, string thisSectionName, HashSet<Question> qSet)
        {



            //have we already seen this question?
            if (qSet.Contains(thisQ))
            {
                //yes. We have a loop : section not complete
                return false;


            }
            else
            {
                //add it to the set
                qSet.Add(thisQ);


            }

            //have we reached the last question?
            if (thisQ.Code == "EXIT")
            {
                return true;


            }

            //have we reached the next section?
            if (thisQ.Section != thisSectionName)
            {
                //yes
                return true;

            }


            //does the path continue: check the nextcode
            string nextCode = thisQ.NextCode;

            if (nextCode == null)
            {
                //path ended
                return false;


            }
            else
            {
                //continue the path-scan

                Question nextQ = questionHash[nextCode];
                return pathScan(nextQ, thisSectionName, qSet);


            }





        }


        /*
        public bool testEachSectionForCompletion()
        {
            //is each section complete or not?

            //whole form complete ?
            bool isComplete = true;

            //did the user select an abort (i.e. a consent which effectively terminates the form)?
            bool abort = false;

            string abortMessage = "consent denied";


            //each section:
            List<string> sectionList = Utils.getSectionList();

            foreach (string section in sectionList)
            {
                //test section for completeness
                


                if (abort && (section != "Final Comments"))
                {

                    //mark all sections are complete with abort message
                    showInPanel(true, section, abortMessage);



                }


                //final section is treated differently
                else if (section == "Final Comments")
                {
                    //finished if the question 'SPINE' has been answered
                    //Question spineQuestion = questionHash["SPINE"];

                    Question iCommentsQuestion = questionHash["INTERVIEWERCOMMENTS"];

                   // if (spineQuestion.processedData != null)

                    if(iCommentsQuestion.PageSeen)

                    {

                        //MessageBox.Show("section " + section + " is complete ");
                        showInPanel(true, section, null);
                        


                    }
                    else
                    {
                        showInPanel(false, section, null);

                        isComplete = false;


                    }
                    


                }


                    //advice treated differently
                else if (section == "Advice")
                {

                    //complete if ADVICE_BMI  was answered
                    Question adviceBMIQuestion = questionHash["ADVICE_BMI"];

                    if (adviceBMIQuestion.processedData != null)
                    {

                        //MessageBox.Show("section " + section + " is complete ");
                        showInPanel(true, section, null);
                        


                    }
                    else
                    {
                        showInPanel(false, section, null);

                        isComplete = false;


                    }





                }

                else if (section == "Consents")
                {
                    //are any of these fatal, i.e. does the participant want to abort the survey ?

                    List<string> qCodesThisSection = sectionToCodeList[section];

                    bool sectionComplete = false;

                    foreach (string qCode in qCodesThisSection)
                    {

                        //is this question a valid exit question ?
                        Question thisQuestion = questionHash[qCode];

                        if (thisQuestion.isFormExit())
                        {
                            //form is complete: user has aborted
                            abort = true;

                            sectionComplete = true;

                            showInPanel(true, section, abortMessage);

                            break;

                           

                        }
                        else if (thisQuestion.isSectionExit(questionCodeToSection))
                        {
                            //section is complete
                            //MessageBox.Show("section " + section + " is complete with exit at " + qCode);
                            showInPanel(true, section, null);

                            sectionComplete = true;

                            //skip any remaining questions in this section
                            break;




                        }


                    }

                    if (!sectionComplete)
                    {
                        
                        //there is a problem with declining blood samples, i.e. it will skip the last few questions,
                        //the last question in the section in this case is CON13
                        

                        string bloodConsent = gs.Get("CON5");

                        if ((bloodConsent == null) || (bloodConsent == "1"))
                        {
                            showInPanel(false, section, null);

                            isComplete = false;


                        }
                        else
                        {
                            //blood consent declined
                            //are we at the end of the section given that blood consent was declined ?
                            Question con13 = questionHash["CON13"];

                            if (con13.processedData == null)
                            {

                                //not at the end
                                showInPanel(false, section, null);

                                isComplete = false;


                            }
                            else
                            {
                                //yes, at the end

                                showInPanel(true, section, null);

                                


                            }



                        }
                        
                        
                        
                        
                        
                        

                    }





                }

                else if (section == "Blood Sample")
                {
                    List<string> qCodesThisSection = sectionToCodeList[section];

                    bool sectionComplete = false;

                    foreach (string qCode in qCodesThisSection)
                    {

                        //is this question a valid exit question ?
                        Question thisQuestion = questionHash[qCode];

                        if (thisQuestion.isSectionExit(questionCodeToSection))
                        {
                            //section is complete
                            //MessageBox.Show("section " + section + " is complete with exit at " + qCode);
                            showInPanel(true, section, null);

                            sectionComplete = true;


                            //skip any remaining questions in this section
                            break;




                        }


                    }
                    if (!sectionComplete)
                    {



                        //need to check if blood samples were consented to.
                        string bloodConsent = gs.Get("CON5");

                        if (bloodConsent == null)
                        {
                            //consent 5 has not been set
                            //MessageBox.Show("section " + section + " is NOT complete");
                            showInPanel(false, section, null);
                            isComplete = false;

                        }
                        else if (bloodConsent == "1")
                        {
                            //they DID consent to bloods
                            //MessageBox.Show("section " + section + " is NOT complete");
                            showInPanel(false, section, null);
                            isComplete = false;

                        }
                        else if (bloodConsent == "2")
                        {
                            //they did NOT consent to bloods
                            //MessageBox.Show("section " + section + " is complete (No Consent for Blood samples)");
                            showInPanel(true, section, "No Consent for Blood samples");

                        }
                        else
                        {
                            throw new Exception();

                        }



                    }
                }

                    //all other sections:
                else
                {

                    List<string> qCodesThisSection = sectionToCodeList[section];

                    bool sectionComplete = false;

                    foreach (string qCode in qCodesThisSection)
                    {

                        //is this question a valid exit question ?
                        Question thisQuestion = questionHash[qCode];

                        if (thisQuestion.isSectionExit(questionCodeToSection))
                        {
                            
                            //a special case: in Demographics, its possible to skip to GPS, ie. bypass the first few Qs, then complete
                            //the section, so we need to test that HSA (Q before GPS) was also anwered
                            if ((section == "Demographic Information") && (questionHash["HSA"].processedData == null))
                            {
                                sectionComplete = false;
                                break;


                            }
                            else
                            {

                                //section is complete
                                //MessageBox.Show("section " + section + " is complete with exit at " + qCode);
                                showInPanel(true, section, null);

                                sectionComplete = true;


                                //skip any remaining questions in this section
                                break;



                            }
                            
                            

                        }


                    }

                    if (! sectionComplete)
                    {
                        showInPanel(false, section, null);

                        isComplete = false;

                    }


                }



            }

            //return true if the form is complete else false
            return isComplete;


        }
         */




        public GPSCountryManager getGPSCountryManager()
        {

            return countryManager;

        }


        public string getLatitude()
        {
            return latitudeLabel.Text;



        }

        public string getLongitude()
        {

            return longitudeLabel.Text;

        }


        public AdviceForm getAdviceBox()
        {

            return adviceBox;

        }

        public ConfirmForm getConfirmBox()
        {
            return confirmBox;

        }

        public Form1 getMainForm()
        {
            return mainForm;

        }


        public string getUserID()
        {
            return userID;


        }

        public void setWarningBox(Form3 warningMessageBox)
        {
            this.warningMessageBox = warningMessageBox;




        }

        public Form3 getWarningBox()
        {
            return warningMessageBox;


        }

        public Label getSectionLabel()
        {

            return sectionLabel;

        }


        public Panel getPanel()
        {

            return mainPanel;


        }

        public void save(bool finalSave, Qconfig config)
        {
            //save all the current data to a file
            //open a file:

            //open data files
            System.IO.TextWriter dhProcessedData = null;
            System.IO.TextWriter dhUserData = null;
            System.IO.TextWriter dhStack = null;
            System.IO.TextWriter dhGlobalStore = null;
            System.IO.TextWriter dhSpecialStore = null;

            System.IO.TextWriter dhPageSeen = null;

            System.IO.TextWriter dhFinalData = null;
            System.IO.TextWriter dhNextCode = null;


            try
            {

                dhProcessedData = new System.IO.StreamWriter(fileNameProcessedData);
                dhUserData = new System.IO.StreamWriter(fileNameUserData);
                dhStack = new System.IO.StreamWriter(fileNameStack);
                dhGlobalStore = new System.IO.StreamWriter(fileNameGlobalStore);
                dhSpecialStore = new System.IO.StreamWriter(fileNameSpecialStore);

                dhPageSeen = new System.IO.StreamWriter(fileNamePageSeen);

                dhFinalData = new System.IO.StreamWriter(fileNameFinalData);

                dhNextCode = new System.IO.StreamWriter(fileNameNextCode);



                //ask each question object to write its data to the files for userdata and processed-data

                foreach (KeyValuePair<string, Question> kvp in questionHash)
                {


                    Question thisQuestion = kvp.Value;
                    thisQuestion.save(dhProcessedData, dhUserData);

                    //page seen
                    thisQuestion.savePageSeen(dhPageSeen);

                    //final data: don't save this way for finalsave
                    //use a pathscan to remove orphan questions

                    if (!finalSave)
                    {
                        thisQuestion.saveFinalData(dhFinalData);

                    }


                    //nextcode
                    thisQuestion.saveNextCode(dhNextCode);




                }

                //write the items in the special data store
                specialDataStore.save(dhSpecialStore);
                //also save to final data
                specialDataStore.save(dhFinalData);


                //write the items in the global store
                gs.save(dhGlobalStore);

                //write the items in the Question Stack
                List<string> stackData = qStack.ToList();

                foreach (string item in stackData)
                {
                    dhStack.WriteLine(item);


                }

                if (finalSave)
                {
                    //when we exit the form: remove orphan questions from the finalData
                    HashSet<Question> qSet = new HashSet<Question>();
                    string firstQcode;
                    Question thisQ;

                    foreach (Section section in config.getSectionList())
                    {

                        //create a set, so we know if any questiona are re-visited, i.e. loops
                        //clear the set initially.
                        qSet.Clear();

                        firstQcode = section.getFirstQuestion();
                        thisQ = questionHash[firstQcode];

                        pathScanSave(thisQ, dhFinalData, qSet, section.getSectionTitle());




                    }



                }





            }
            finally
            {

                if (dhProcessedData != null)
                {

                    dhProcessedData.Close();

                }

                if (dhUserData != null)
                {

                    dhUserData.Close();
                }

                if (dhStack != null)
                {
                    dhStack.Close();

                }

                if (dhGlobalStore != null)
                {
                    dhGlobalStore.Close();

                }

                if (dhSpecialStore != null)
                {

                    dhSpecialStore.Close();

                }

                if (dhPageSeen != null)
                {

                    dhPageSeen.Close();

                }

                if (dhFinalData != null)
                {
                    dhFinalData.Close();

                }

                if (dhNextCode != null)
                {
                    dhNextCode.Close();

                }



            }







        }



        public void postProcessFinalData()
        {

            //make sure there is a result for all questions, i.e. enter 555 as the result of any skipped questions
            //read the current values from the final-data file


            //StreamReader dhFinalData = null;

            //set of all question-codes that have been answered
            HashSet<string> qCodes = new HashSet<string>();


            using (StreamReader dhFinalData = new StreamReader(fileNameFinalData))
            {

                Char[] delim = new Char[] { '\t' };

                while (dhFinalData.EndOfStream == false)
                {
                    string line = dhFinalData.ReadLine();

                    //using tab as delim
                    string[] parts = line.Split(delim);

                    //save qCode
                    qCodes.Add(parts[0]);



                }

            }




            //save
            //System.IO.TextWriter dhFinalDataOut = null;
            //Note: must append data, hense the "true" param
            using (TextWriter dhFinalDataOut = new System.IO.StreamWriter(fileNameFinalData, true))
            {

                //list of all questions
                foreach (string qCode in questionHash.Keys)
                {
                    //has this question been answered?

                    if (!qCodes.Contains(qCode))
                    {
                        //No

                        //dhFinalDataOut.WriteLine(qCode + "\t555");
                        dhFinalDataOut.WriteLine(qCode + "\t" + GlobalConstants.Skipped);



                    }



                }

                //BMI (not in qHash): has special code 444 if skipped
                if (!qCodes.Contains("BMI"))
                {

                    //dhFinalDataOut.WriteLine("BMI\t444");
                    dhFinalDataOut.WriteLine("BMI\t" + GlobalConstants.SkippedBMI);


                }




            }



        }





        public void load()
        {
            //load in previous data from files
            //open a file:

            //open data files

            StreamReader dhProcessedData = null;
            StreamReader dhUserData = null;
            StreamReader dhStack = null;
            StreamReader dhGlobalStore = null;
            StreamReader dhSpecialStore = null;
            StreamReader dhPageSeen = null;
            StreamReader dhNextCode = null;


            try
            {

                dhProcessedData = new StreamReader(fileNameProcessedData);
                dhUserData = new StreamReader(fileNameUserData);
                dhStack = new StreamReader(fileNameStack);
                dhGlobalStore = new StreamReader(fileNameGlobalStore);
                dhSpecialStore = new StreamReader(fileNameSpecialStore);
                dhPageSeen = new StreamReader(fileNamePageSeen);
                dhNextCode = new StreamReader(fileNameNextCode);


                //read nextcode into a dict.

                Dictionary<string, string> nextCodeDict = new Dictionary<string, string>();
                Char[] delim = new Char[] { '\t' };

                while (dhNextCode.EndOfStream == false)
                {
                    string line = dhNextCode.ReadLine();

                    //using tab as delim
                    string[] parts = line.Split(delim);

                    nextCodeDict[parts[0]] = parts[1];



                }




                //read the processed data into a Dictionary
                Dictionary<string, string> pDataDict = new Dictionary<string, string>();
                // Char[] delim = new Char[] { '\t' };

                while (dhProcessedData.EndOfStream == false)
                {
                    string line = dhProcessedData.ReadLine();

                    //using tab as delim
                    string[] parts = line.Split(delim);

                    pDataDict[parts[0]] = parts[1];



                }




                //read the user data into a dictionary

                Dictionary<string, string> uDataDict = new Dictionary<string, string>();


                while (dhUserData.EndOfStream == false)
                {
                    string line = dhUserData.ReadLine();

                    //using tab as delim
                    string[] parts = line.Split(delim);

                    int arrLength = parts.Length;

                    //the data itself may be in several parts.
                    if (arrLength == 2)
                    {
                        //normal case
                        uDataDict[parts[0]] = parts[1];


                    }
                    else
                    {
                        //when the data is split into multiple parts

                        //create an array without the first element of parts
                        string[] subList = new string[arrLength - 1];

                        for (int i = 1; i < arrLength; i++)
                        {

                            subList[i - 1] = parts[i];

                        }

                        string partsJoined = string.Join("\t", subList);

                        uDataDict[parts[0]] = partsJoined;


                    }


                }


                //load the seen pages
                HashSet<string> seenPages = new HashSet<string>();

                while (dhPageSeen.EndOfStream == false)
                {
                    string line = dhPageSeen.ReadLine();


                    //add to the set of seen pages
                    seenPages.Add(line);




                }



                //read the userdata and processed data back 

                foreach (KeyValuePair<string, Question> kvp in questionHash)
                {


                    Question thisQuestion = kvp.Value;
                    thisQuestion.load(pDataDict, uDataDict);
                    thisQuestion.loadPageSeen(seenPages);
                    thisQuestion.loadNextCode(nextCodeDict);



                }

                //load the items in the special data store
                specialDataStore.load(dhSpecialStore);



                //load the items in the global store
                gs.load(dhGlobalStore);



                //read the stack data from the file and push onto the stack

                //create a temp list to get the contents of the file
                List<string> fromFileToStack = new List<string>();

                while (dhStack.EndOfStream == false)
                {
                    string line = dhStack.ReadLine();
                    fromFileToStack.Add(line);



                }

                //push list onto the stack in reverse order
                fromFileToStack.Reverse();

                foreach (string code in fromFileToStack)
                {

                    qStack.Push(code);
                }



            }
            finally
            {

                //close the files
                if (dhProcessedData != null)
                {

                    dhProcessedData.Close();

                }

                if (dhUserData != null)
                {

                    dhUserData.Close();
                }

                if (dhStack != null)
                {
                    dhStack.Close();

                }

                if (dhGlobalStore != null)
                {
                    dhGlobalStore.Close();

                }

                if (dhSpecialStore != null)
                {

                    dhSpecialStore.Close();

                }

                if (dhPageSeen != null)
                {

                    dhPageSeen.Close();

                }

                if (dhNextCode != null)
                {

                    dhNextCode.Close();

                }




            }





        }



        public bool isLastPage()
        {

            //is this the last page of the questionnaire ?

            if (currentQuestion.Code == "EXIT")
            {
                return true;

            }
            else
            {
                return false;

            }



        }

        public bool isFirstPage()
        {

            //is this the first page of the questionnaire ?

            if (currentQuestion.Code == "START")
            {
                return true;

            }
            else
            {
                return false;

            }



        }

        //old version
        /*
        private string getTitle(XPathNodeIterator node, string tagName)
        {

            XPathNodeIterator textNode;
            string lang = null;
            string title = null;

            textNode = node.Current.Select(tagName);
            while (textNode.MoveNext())
            {
                //get language
                lang = textNode.Current.SelectSingleNode("@lang").Value;

                //get title
                title = textNode.Current.Value;

                //MessageBox.Show(lang + ": " + title);


                //is this the selected language
                if (lang == language)
                {

                    //yes
                    return title;



                }

            }

            //there is nothing available in the selected language
            //use any language if possible

            if (title == null)
            {
                throw new Exception("Title missing for:" + node.Current.Name);
            }
            else
            {
                return title;


            }


        }
         */



        //new version
        private string getTitle(XPathNodeIterator node, string tagName)
        {

            XPathNodeIterator textNode;
            string lang = null;
            string title = null;

            textNode = node.Current.Select(tagName);
            while (textNode.MoveNext())
            {
                //get language
                lang = textNode.Current.SelectSingleNode("@lang").Value;

                //get title
                title = textNode.Current.Value;

                //MessageBox.Show(lang + ": " + title);


                //is this the selected language
                if (lang == language)
                {

                    //yes
                    return processVarsInTitle(title);



                }

            }

            //there is nothing available in the selected language
            //use any language if possible

            if (title == null)
            {
                throw new Exception("Title missing for:" + node.Current.Name);
            }
            else
            {
                return processVarsInTitle(title);


            }



        }


        private string processVarsInTitle(string title)
        {

            //the title may contain vars in the form ${...}
            //these should be replaced by the relevant text from the config

            string[] parts = title.Split();
            StringBuilder sb = new StringBuilder();
            bool first = true;

            Regex rx = new Regex(@"\{\{(.+)\}\}");
            string key;
            string word;

            foreach (string part in parts)
            {

                //is this a var
                Match match = rx.Match(part);

                if (match.Success)
                {

                    key = match.Groups[1].Value;

                    //lookup the key to get the current valle from the config
                    word = mainForm.config.getGlobalName(key);



                }
                else
                {

                    word = part;


                }




                if (first)
                {

                    sb.Append(word);
                    first = false;
                }
                else
                {

                    sb.Append(" ").Append(word);

                }



            }

            return sb.ToString();




        }





        public void processUserData()
        {

            //process the data entered by the user and fetch the next code to process
            nextCode = currentQuestion.processUserData();


        }

        public void removeControls()
        {
            //delete form controls for the current question
            currentQuestion.removeControls();

        }


        public void configureControls(UserDirection direction)
        {



            //is this an automatic question, i.e. no UI ?
            if (currentQuestion is QuestionAutomatic)
            {
                //automatic Question: no controls to configure



                //fetch the next Question or the previous question
                if (direction == UserDirection.forward)
                {
                    //process
                    processUserData();

                    save(false, null);

                    setNextQuestion();

                }
                else
                {
                    setPreviousQuestion();

                }



                //recursive call
                configureControls(direction);





            }
            else if (!currentQuestion.IfSettingsOK())
            {

                //skip this question as permission was not granted to show it, e.g. the user denied permission
                //fetch the next Question or the previous question
                if (direction == UserDirection.forward)
                {

                    //get the default next code from the current question
                    nextCode = currentQuestion.ToCode;

                    setNextQuestion();

                }
                else
                {
                    setPreviousQuestion();

                }



                //recursive call
                configureControls(direction);



            }

            else
            {
                //normal Question

                //configure the controls for the current Question
                currentQuestion.showSection();


                currentQuestion.configureControls(direction);

                currentQuestion.incrementNumTimesShown();




            }






        }

        public string getCodeAtTopOfStack()
        {

            //return the top question on the stack
            return qStack.Pop();



        }




        public void setCurrentQuestion(string code)
        {
            //set the question pointer to the question object for this code
            //it is possible that the code is not valid, so check.
            currentQuestion = questionHash[code];

            //add the code to the question stack
            qStack.Push(code);




        }

        public void setNextQuestion()
        {

            //save the returned qCode in the object, which can be used later for path-scanning (section completeness)
            currentQuestion.NextCode = nextCode;


            //save a ref to the current question
            Question tempQuestion = currentQuestion;


            //advance the question pointer to the next question in the survey
            try
            {
                currentQuestion = questionHash[nextCode];

            }
            catch
            {

                MessageBox.Show("error fetching qCode:" + nextCode);
                showDebug();



            }



            //push the new Question onto the stack ONLY if it is not the same as the previous question
            if (currentQuestion != tempQuestion)
            {
                //add the code to the question stack
                qStack.Push(nextCode);


            }
            else
            {
                //notification that we are displaying the same Q again.
                FinishedForm ff = new FinishedForm();
                ff.setLabel("Repeating Previous Question");
                ff.ShowDialog();


            }



        }

        public void setNextQuestion(string qCode)
        {
            //used to jump to a specific section

            //advance the question pointer to the next question in the survey
            currentQuestion = questionHash[qCode];

            //add the code to the question stack
            qStack.Push(qCode);



        }

        public void setPreviousQuestion()
        {
            //pop the current question off the stack
            qStack.Pop();

            //peek at the new top of the stack to see what the previous code is
            currentQuestion = questionHash[qStack.Peek()];


        }




        public Question getQuestion(string qCode)
        {
            //get the Question object with this code.

            return questionHash[qCode];



        }

        //parse the XML config file
        //NEW VERSION:
        public void ParseConfigXML(string configFileName, Form form)
        {
            XPathNavigator nav;
            XPathNavigator nav2;
            XPathDocument docNav;
            XPathNodeIterator nodeItr;
            XPathNodeIterator rankItr;



            QuestionText qt;
            QuestionRadio qr;
            QuestionLabel ql;
            QuestionDatePickerSelect qdp;
            QuestionTextRadio qtr;
            QuestionTextOneCheck qtc;
            QuestionTextDouble qtd;
            QuestionGetTime qgt;
            QuestionTextTriple qtt;
            QuestionAutomatic qauto;

            QuestionTextDuplicate qtdup;
            QuestionTextRadioButton qtrb;
            QuestionSelect qs;
            QuestionYear qy;
            QuestionRadioDynamicLabel qrd;
            QuestionTextDate qtdt;
            QuestionSelectText qst;
            QuestionRank qrk;


            Option op;


            // Open the XML.
            docNav = new XPathDocument(configFileName);




            // Create a navigator to query with XPath.
            nav = docNav.CreateNavigator();

            //select datepicker nodes
            nodeItr = nav.Select("//QuestionDate");

            while (nodeItr.MoveNext())
            {

                qdp = new QuestionDatePickerSelect(form, bigMessageBox, gs, specialDataStore, this);

                populateQuestion(qdp, nodeItr);


            }


            nodeItr = nav.Select("//QuestionYear");
            while (nodeItr.MoveNext())
            {

                qy = new QuestionYear(form, bigMessageBox, gs, specialDataStore, this);

                populateQuestion(qy, nodeItr);

            }


            nodeItr = nav.Select("//QuestionTextDuplicate");
            while (nodeItr.MoveNext())
            {

                //a new QuestionText object
                qtdup = new QuestionTextDuplicate(form, bigMessageBox, gs, specialDataStore, this);
                populateQuestion(qtdup, nodeItr);

            }

            nodeItr = nav.Select("//QuestionTextBox");
            while (nodeItr.MoveNext())
            {

                //a new QuestionText object
                qt = new QuestionText(form, bigMessageBox, gs, specialDataStore, this);
                populateQuestion(qt, nodeItr);

            }

            nodeItr = nav.Select("//QuestionTextDate");

            while (nodeItr.MoveNext())
            {

                //a new QuestionText object
                qtdt = new QuestionTextDate(form, bigMessageBox, gs, specialDataStore, this);
                populateQuestion(qtdt, nodeItr);

            }

            nodeItr = nav.Select("//QuestionAutomatic");
            while (nodeItr.MoveNext())
            {

                //a new QuestionText object
                qauto = new QuestionAutomatic(form, bigMessageBox, gs, specialDataStore, this);
                populateQuestion(qauto, nodeItr);


                //advice-underweight

                nav2 = nodeItr.Current.SelectSingleNode("child::AdviceText[@name='underweight']");
                if (nav2 != null)
                {
                    qauto.UnderWeightAdvice = nav2.Value;

                }


                //advice overweight

                nav2 = nodeItr.Current.SelectSingleNode("child::AdviceText[@name='overweight']");
                if (nav2 != null)
                {
                    qauto.OverWeightAdvice = nav2.Value;

                }

                //advice hypertensive
                nav2 = nodeItr.Current.SelectSingleNode("child::AdviceText[@name='hypertensive']");
                if (nav2 != null)
                {
                    qauto.HypertensiveAdvice = nav2.Value;

                }




            }

            nodeItr = nav.Select("//QuestionTextTriple");
            while (nodeItr.MoveNext())
            {

                //a new QuestionText object
                qtt = new QuestionTextTriple(form, bigMessageBox, gs, specialDataStore, this);

                populateQuestion(qtt, nodeItr);

            }

            nodeItr = nav.Select("//QuestionGetTime");
            while (nodeItr.MoveNext())
            {

                //a new QuestionText object
                qgt = new QuestionGetTime(form, bigMessageBox, gs, specialDataStore, this);
                populateQuestion(qgt, nodeItr);


            }


            nodeItr = nav.Select("//QuestionTextDouble");
            while (nodeItr.MoveNext())
            {

                //a new QuestionText object
                qtd = new QuestionTextDouble(form, bigMessageBox, gs, specialDataStore, this);
                populateQuestion(qtd, nodeItr);

            }


            nodeItr = nav.Select("//QuestionRadio");

            while (nodeItr.MoveNext())
            {


                qr = new QuestionRadio(form, bigMessageBox, gs, specialDataStore, this);
                populateQuestion(qr, nodeItr);

                //add options
                populateOptions(qr, nodeItr, null);


            }


            nodeItr = nav.Select("//QuestionRadioDynamicLabel");

            while (nodeItr.MoveNext())
            {
                qrd = new QuestionRadioDynamicLabel(form, bigMessageBox, gs, specialDataStore, this);
                populateQuestion(qrd, nodeItr);
                //add options
                populateOptions(qrd, nodeItr, null);


                nav2 = nodeItr.Current.SelectSingleNode("child::LabelGeneratorFunc");
                if (nav2 == null)
                {
                    MessageBox.Show("Error in XML for QuestionRadioDynamicLabel: missing LabelGeneratorFunc");
                    throw new Exception();

                }
                else
                {
                    qrd.LabelToGroupBoxGap = nav2.Value;

                }




            }


            nodeItr = nav.Select("//QuestionSelect");

            while (nodeItr.MoveNext())
            {


                qs = new QuestionSelect(form, bigMessageBox, gs, specialDataStore, this);
                populateQuestion(qs, nodeItr);

                //add options

                //are we using options from the global config?
                string configKey = qs.ConfigKey;

                populateOptions(qs, nodeItr, configKey);



            }

            /*
            nodeItr = nav.Select("//QuestionSelectVarOptions");

            while (nodeItr.MoveNext())
            {


                qs = new QuestionSelectVarOptions(form, bigMessageBox, gs, specialDataStore, this);
                populateQuestion(qs, nodeItr);





            }
             */



            nodeItr = nav.Select("//QuestionRank");

            while (nodeItr.MoveNext())
            {

                qrk = new QuestionRank(form, bigMessageBox, gs, specialDataStore, this);
                populateQuestion(qrk, nodeItr);


                //are we using options from the global config?
                string configKey = qrk.ConfigKey;

                populateOptions(qrk, nodeItr, configKey);


                //populateOptions(qrk, nodeItr, null);

                rankItr = nodeItr.Current.Select("Rank");
                string rankLabel;

                while (rankItr.MoveNext())
                {


                    rankLabel = getTitle(rankItr, "Title");
                    qrk.addRank(rankLabel);



                }



            }


            nodeItr = nav.Select("//QuestionLabel");

            while (nodeItr.MoveNext())
            {

                ql = new QuestionLabel(form, bigMessageBox, gs, specialDataStore, this);
                populateQuestion(ql, nodeItr);

            }


            nodeItr = nav.Select("//QuestionTextRadio");
            while (nodeItr.MoveNext())
            {

                //a new QuestionText object
                qtr = new QuestionTextRadio(form, bigMessageBox, gs, specialDataStore, this);
                populateQuestion(qtr, nodeItr);
                populateOptions(qtr, nodeItr, null);

                qtr.RadioLabel = getTitle(nodeItr, "Subtitle");

            }



            nodeItr = nav.Select("//QuestionSelectText");
            while (nodeItr.MoveNext())
            {

                //a new QuestionText object
                qst = new QuestionSelectText(form, bigMessageBox, gs, specialDataStore, this);
                populateQuestion(qst, nodeItr);
                populateOptions(qst, nodeItr, null);
                qst.TextLabel = getTitle(nodeItr, "Subtitle");


            }

            nodeItr = nav.Select("//QuestionTextRadioButton");
            while (nodeItr.MoveNext())
            {

                //a new QuestionText object
                qtrb = new QuestionTextRadioButton(form, bigMessageBox, gs, specialDataStore, this);
                populateQuestion(qtrb, nodeItr);
                populateOptions(qtrb, nodeItr, null);

                qtrb.RadioLabel = getTitle(nodeItr, "Subtitle");

                nav2 = nodeItr.Current.SelectSingleNode("child::ValidationButton");
                if (nav2 == null)
                {
                    MessageBox.Show("Error in XML for QuestionTextRadioButton: missing ValidationButton");
                    throw new Exception();

                }
                else
                {
                    qtrb.setValidationButton(nav2.Value);

                }




            }




        }


        private void populateOptions(IoptionList q, XPathNodeIterator nodeItr, string configKey)
        {
            XPathNodeIterator optionItr;
            XPathNavigator nav;
            Option op = null;

            //Note: some questions may have a combination of options from the XML and options form the conf file


            //in some special cases, we get the options from the global config rather than the XML
            //e.g. if the options are country-specific
            if (configKey != null)
            {
                Qconfig conf = mainForm.config;

                //what was the selected country?
                string selectedCountryName = conf.selectedCountryName;

                List<Option> opsList = null;

                if (configKey == "langs")
                {
                    opsList = conf.countryMap[selectedCountryName].langs;


                }
                else if (configKey == "tribes")
                {

                    opsList = conf.countryMap[selectedCountryName].tribes;


                }
                else
                {

                    //assume this is a global configKey, not country-specific
                    opsList = conf.getGlobalOptionList(configKey);


                }


                foreach (Option option in opsList)
                {

                    q.addOption(option);


                }



            }




            optionItr = nodeItr.Current.Select("Option");

            if (optionItr == null)
            {

                //no options in XML
                return;


            }

            while (optionItr.MoveNext())
            {


                //value
                string optionValue = optionItr.Current.SelectSingleNode("child::Value").Value;

                //text
                string optionText = getTitle(optionItr, "Title");

                //get the child node: WidgetPos
                //optional, i.e. not used for Select type options

                string widgetPosWidth = null;
                string widgetPosHeight = null;
                string widgetPosX = null;
                string widgetPosY = null;



                //widget width
                nav = optionItr.Current.SelectSingleNode("child::WidgetPos/@width");
                if (nav != null)
                {

                    widgetPosWidth = nav.Value;


                }


                //widget height
                nav = optionItr.Current.SelectSingleNode("child::WidgetPos/@height");
                if (nav != null)
                {
                    widgetPosHeight = nav.Value;

                }


                //widget Xpos
                nav = optionItr.Current.SelectSingleNode("child::WidgetPos/@xpos");
                if (nav != null)
                {
                    widgetPosX = nav.Value;

                }


                //widget Ypos
                nav = optionItr.Current.SelectSingleNode("child::WidgetPos/@ypos");
                if (nav != null)
                {
                    widgetPosY = nav.Value;

                }




                //toCode: optional
                nav = optionItr.Current.SelectSingleNode("child::ToCode");
                string optionToCode = null;

                if (nav != null)
                {
                    optionToCode = nav.Value;

                }


                //special process label
                nav = optionItr.Current.SelectSingleNode("child::ToCodeProcess");
                string toCodeProcess = null;

                if (nav != null)
                {
                    toCodeProcess = nav.Value;

                }



                //toCodeSecond: optional
                nav = optionItr.Current.SelectSingleNode("child::ToCodeSecond");
                string toCodeSecond = null;

                if (nav != null)
                {
                    toCodeSecond = nav.Value;

                }




                //ToCodeAndError: optional
                nav = optionItr.Current.SelectSingleNode("child::ToCodeAndError");
                string optionToCodeErr = null;

                if (nav != null)
                {
                    optionToCodeErr = nav.Value;

                }



                op = new Option(optionValue, optionText);

                op.setWidgetWidth(widgetPosWidth);
                op.setWidgetHeight(widgetPosHeight);
                op.setWidgetXpos(widgetPosX);
                op.setWidgetYpos(widgetPosY);

                op.ToCode = optionToCode;
                op.ToCodeErr = optionToCodeErr;
                op.ToCodeSecond = toCodeSecond;
                op.ToCodeProcess = toCodeProcess;



                //is this option the default ?
                var defaultOption = optionItr.Current.SelectSingleNode("child::Default");

                if (defaultOption == null)
                {
                    op.Default = false;
                }
                else
                {
                    op.Default = true;

                }


                q.addOption(op);


            }







        }




        private void populateQuestion(Question q, XPathNodeIterator nodeItr)
        {

            XPathNavigator nav;

            //get the code from the current node
            string thisCode = null;
            nav = nodeItr.Current.SelectSingleNode("@code");
            if (nav == null)
            {
                //error
                MessageBox.Show("Error in XML for question: missing Code");
                throw new Exception();


            }
            else
            {
                thisCode = nav.Value;
                q.Code = thisCode;


            }



            //text of the question: skip for QuestionAutomatic
            if (!(q is QuestionAutomatic))
            {
                q.Val = getTitle(nodeItr, "Title");


                //widget width
                nav = nodeItr.Current.SelectSingleNode("child::WidgetPos/@width");
                if (nav == null)
                {
                    //error
                    MessageBox.Show("Error in XML for question code: " + thisCode + ": missing WidgetPos:width");
                    throw new Exception();

                }
                else
                {
                    q.setWidgetWidth(nav.Value);
                }

                //widget height
                nav = nodeItr.Current.SelectSingleNode("child::WidgetPos/@height");
                if (nav == null)
                {
                    //error
                    MessageBox.Show("Error in XML for question code: " + thisCode + ": missing WidgetPos:height");
                    throw new Exception();

                }
                else
                {
                    q.setWidgetHeight(nav.Value);
                }

                //widget Xpos
                nav = nodeItr.Current.SelectSingleNode("child::WidgetPos/@xpos");
                if (nav == null)
                {
                    //error
                    MessageBox.Show("Error in XML for question code: " + thisCode + ": missing WidgetPos:Xpos");
                    throw new Exception();

                }
                else
                {
                    q.setWidgetXpos(nav.Value);
                }

                //widget Ypos
                nav = nodeItr.Current.SelectSingleNode("child::WidgetPos/@ypos");
                if (nav == null)
                {
                    //error
                    MessageBox.Show("Error in XML for question code: " + thisCode + ": missing WidgetPos:Ypos");
                    throw new Exception();

                }
                else
                {
                    q.setWidgetYpos(nav.Value);
                }



                //avoid the NoAnswer/DontKnow/NotApplicable thing
                nav = nodeItr.Current.SelectSingleNode("child::OmitNoAnswerDontKnowNotApplicable");
                if (nav == null)
                {
                    //show the 3 skip buttons
                    q.NoAnswerDontKnowNotApplicable = true;

                }
                else
                {
                    //don't show the 3 skip buttons
                    q.NoAnswerDontKnowNotApplicable = false;


                }



            }


            //ToCode

            nav = nodeItr.Current.SelectSingleNode("child::ToCode");
            q.ToCode = null;
            if (nav == null)
            {
                //error
                MessageBox.Show("Error in XML for question code: " + thisCode + ": missing ToCode");
                throw new Exception();


            }
            else
            {
                q.ToCode = nav.Value;

            }


            //ConfigKey: only for QuestionSelect: used when the options come from the config not the XML



            nav = nodeItr.Current.SelectSingleNode("child::ConfigKey");
            q.ConfigKey = null;
            if (nav != null)
            {
                q.ConfigKey = nav.Value;
            }




            //section

            nav = nodeItr.Current.SelectSingleNode("child::Section");
            q.Section = null;
            if (nav == null)
            {
                //error
                MessageBox.Show("Error in XML for question code: " + thisCode + ": missing Section");
                throw new Exception();


            }
            else
            {
                q.Section = nav.Value;

            }



            //FromCode: optional
            nav = nodeItr.Current.SelectSingleNode("child::FromCode");
            q.FromCode = null;
            if (nav != null)
            {
                q.FromCode = nav.Value;

            }





            //message on second fail: optional
            nav = nodeItr.Current.SelectSingleNode("child::MessageOnSecondFail");
            q.MessageOnSecondFail = null;
            if (nav != null)
            {
                q.MessageOnSecondFail = nav.Value;

            }

            //populate from : optional
            nav = nodeItr.Current.SelectSingleNode("child::PopulateFrom");
            q.PopulateFrom = null;
            if (nav != null)
            {
                q.PopulateFrom = nav.Value;

            }



            //onerrorquestioncompare: optional
            nav = nodeItr.Current.SelectSingleNode("child::OnErrorQuestionCompare");
            q.OnErrorQuestionCompare = null;
            if (nav != null)
            {
                q.OnErrorQuestionCompare = nav.Value;

            }

            //setkey: optional
            nav = nodeItr.Current.SelectSingleNode("child::SetKey");
            q.SetKey = null;
            if (nav != null)
            {
                q.SetKey = nav.Value;

                //MessageBox.Show("setting key for:" + thisCode + " to:" + nav.Value);

            }

            //validation: optional
            nav = nodeItr.Current.SelectSingleNode("child::Validation");
            q.Validation = null;
            if (nav != null)
            {
                q.Validation = nav.Value;

                //MessageBox.Show("validation found:" + nav.Value);


            }


            //ifSettingSpecial: optional
            //for complex if settings
            nav = nodeItr.Current.SelectSingleNode("child::IfSettingSpecial");
            q.IfSettingSpecial = null;
            if (nav != null)
            {
                q.IfSettingSpecial = nav.Value;

                


            }




            //process: optional
            nav = nodeItr.Current.SelectSingleNode("child::Process");
            q.Process = null;
            if (nav != null)
            {
                q.Process = nav.Value;

            }




            //multiline (i.e. like a textarea):optional
            nav = nodeItr.Current.SelectSingleNode("child::MultiLine");
            q.MultiLine = false;

            if (nav != null)
            {
                q.MultiLine = true;

            }




            //ifSetting
            nav = nodeItr.Current.SelectSingleNode("child::IfSetting/@key");
            q.IfSettingKey = null;

            if (nav != null)
            {
                q.IfSettingKey = nav.Value;

            }

            nav = nodeItr.Current.SelectSingleNode("child::IfSetting/@val");
            q.IfSettingVal = null;

            if (nav != null)
            {
                q.IfSettingVal = nav.Value;

            }

            //ifSetting logical operators
            XPathNodeIterator ifSettingItr = nodeItr.Current.Select("IfSettingOP");
            string ifKey;
            string ifVal;
            string ifOp;

            IfNode ifNode = null;


            while (ifSettingItr.MoveNext())
            {
                //get key
                ifKey = ifSettingItr.Current.SelectSingleNode("@key").Value;

                //MessageBox.Show("ifkey:" + ifKey);



                //get value
                ifVal = ifSettingItr.Current.SelectSingleNode("@val").Value;


                //operator can be left out for the first element, as it is not used anyway

                //ifOp = ifSettingItr.Current.SelectSingleNode("@op").Value;

                ifNode = new IfNode();
                ifNode.Key = ifKey;
                ifNode.Val = ifVal;
                ifNode.Operator = null;



                nav = ifSettingItr.Current.SelectSingleNode("@op");
                if (nav != null)
                {
                    ifNode.Operator = nav.Value;


                }



                q.addIfNode(ifNode);






            }






            //SelectLength: for types that include combo boxes
            nav = nodeItr.Current.SelectSingleNode("child::SelectLength");
            q.SelectLength = 0;

            if (nav != null)
            {
                q.SelectLength = Convert.ToInt32(nav.Value);

            }



            //optional gap between the label and the textbox

            nav = nodeItr.Current.SelectSingleNode("child::LabelToBoxGap");
            q.LabelToBoxGap = 0;

            if (nav != null)
            {
                q.LabelToBoxGap = Convert.ToInt32(nav.Value);

            }


            //recording
            q.RecordThisQ = false;

            nav = nodeItr.Current.SelectSingleNode("child::Recording");
            q.RecordThisQ = false;

            if (nav != null)
            {
                string recState = nav.Value;

                if (recState == "ON")
                {
                    q.RecordThisQ = true;


                }

            }

            nav = nodeItr.Current.SelectSingleNode("child::CheckPreviousDontKnow");
            q.CheckPreviousDontKnow = false;
            if (nav != null)
            {
                q.CheckPreviousDontKnow = true;


            }









            //save the object in the hash

            try
            {
                questionHash.Add(thisCode, q);


            }

            catch
            {
                MessageBox.Show("Error duplicate question code for" + thisCode);

            }











        }


        public void testSectionRefs()
        {

            //check that each section in the config maps to a real question

            List<Section> sectionList = mainForm.config.getSectionList();

            string qCode;
            bool errorState = false;
            StringBuilder message = new StringBuilder();

            HashSet<string> sectionSet = new HashSet<string>();


            foreach (Section section in sectionList)
            {

                sectionSet.Add(section.getSectionTitle());
                
                qCode = section.getFirstQuestion();

                if (!questionHash.ContainsKey(qCode))
                {
                    errorState = true;
                    message.Append("Section: ").Append(section.getSectionTitle()).Append(" has invalid reference to Code:").Append(qCode).Append("\n");


                }



            }


            //check that each question contains a valid section
            string sectionName;

            foreach (Question question in questionHash.Values)
            {

                sectionName = question.Section;

                if (sectionName == null)
                {
                    errorState = true;
                    message.Append("Question code:").Append(question.Code).Append(" is missing a Section tag\n");
                   

                }
                else
                {
                    //is this a valid section.
                    if (! sectionSet.Contains(sectionName))
                    {

                        errorState = true;
                        message.Append("Question code:").Append(question.Code).Append(" has a section (").Append(sectionName).Append(") that is not present in the config\n");


                    }


                }


            }


            if (errorState)
            {

                MessageBox.Show("Section Errors\n" + message);



            }

        }











        public void testQuestionRefs()
        {
            //scan each question in hash, looking for invalid references

            string debug = "";
            string toCode;
            string code;

            bool errorState = false;
            bool opError = false;


            foreach (Question question in questionHash.Values)
            {

                code = question.Code;

                //ignore EXIT
                if (code == "EXIT")
                {

                    continue;

                }

                //get toCode
                toCode = question.ToCode;

                //this should be a key in the hash
                if (!questionHash.ContainsKey(toCode))
                {
                    errorState = true;
                    debug += ("Question:" + code + " is trying to reference non-existant code:" + toCode + "\n");


                }



                //if this question has options, check each option for a tocode
                if (question is IoptionList)
                {

                    opError = ((IoptionList)question).testQuestionRefs(questionHash);
                    if (!opError)
                    {
                        errorState = true;
                        debug += ("Question:" + code + " has an option that is trying to reference non-existant code\n");

                    }


                }


            }


            if (errorState)
            {

                MessageBox.Show("Reference Errors\n" + debug);



            }



        }






        //parse the XML config file
        //OLD VERSION
        /*
        public void ParseConfigXML(string configFileName, Form form)
        {
            XPathNavigator nav;
            XPathDocument docNav;
            XPathNodeIterator textBoxItr;
            XPathNodeIterator radioItr;
            XPathNodeIterator optionItr;
            XPathNodeIterator labelItr;
            XPathNodeIterator dateItr;
            XPathNodeIterator textRadioItr;
            XPathNodeIterator fromCodeNode;

            XPathNavigator gen;





            string thisCode;
            string thisQuestionText;
            string widgetPosWidth;
            string widgetPosHeight;
            string widgetPosX;
            string widgetPosY;
            string toCode;


            string validation;
            string process;
            string section;

            string setKey;
            string optionValue;
            string optionText;
            string optionToCode;
            string optionToCodeErr;
            string fromCode;

            string lang;
            string title;

            QuestionText qt;
            QuestionRadio qr;
            QuestionLabel ql;
            QuestionDatePickerSelect qdp;
            QuestionTextRadio qtr;
            QuestionTextOneCheck qtc;
            QuestionTextDouble qtd;
            QuestionGetTime qgt;
            QuestionTextTriple qtt;
            QuestionAutomatic qauto;
            
            QuestionTextDuplicate qtdup;
            QuestionTextRadioButton qtrb;
            QuestionSelect qs;
            QuestionYear qy;
            QuestionRadioDynamicLabel qrd;
            QuestionTextDate qtdt;
            QuestionSelectText qst;
            QuestionRank qrk;


            Option op;


            // Open the XML.
            docNav = new XPathDocument(configFileName);




            // Create a navigator to query with XPath.
            nav = docNav.CreateNavigator();

            //select datepicker nodes
            dateItr = nav.Select("//QuestionDate");

            while (dateItr.MoveNext())
            {

                qdp = new QuestionDatePickerSelect(form, bigMessageBox, gs, specialDataStore, this);

                //get the code from the current node
                thisCode = dateItr.Current.SelectSingleNode("@code").Value;
                qdp.Code = thisCode;


                //get the text of the question
                //thisQuestionText = dateItr.Current.SelectSingleNode("@val").Value;
                //qdp.Val = thisQuestionText;


                qdp.Val = getTitle(dateItr, "Title");


                //get the child node: WidgetPos
                widgetPosWidth = dateItr.Current.SelectSingleNode("child::WidgetPos/@width").Value;
                widgetPosHeight = dateItr.Current.SelectSingleNode("child::WidgetPos/@height").Value;
                widgetPosX = dateItr.Current.SelectSingleNode("child::WidgetPos/@xpos").Value;
                widgetPosY = dateItr.Current.SelectSingleNode("child::WidgetPos/@ypos").Value;

                qdp.setWidgetWidth(widgetPosWidth);
                qdp.setWidgetHeight(widgetPosHeight);
                qdp.setWidgetXpos(widgetPosX);
                qdp.setWidgetYpos(widgetPosY);

                //FromCode: optional
                gen = dateItr.Current.SelectSingleNode("child::FromCode");
                if (gen != null)
                {
                    fromCode = gen.Value;

                }
                else
                {
                    fromCode = null;
                }

                qdp.FromCode = fromCode;


                //ToCode
                toCode = dateItr.Current.SelectSingleNode("child::ToCode").Value;
                qdp.ToCode = toCode;

                setKey = dateItr.Current.SelectSingleNode("child::SetKey").Value;
                qdp.SetKey = setKey;

                //section
                section = dateItr.Current.SelectSingleNode("child::Section").Value;
                qdp.Section = section;

                XPathNavigator qCompare = dateItr.Current.SelectSingleNode("child::OnErrorQuestionCompare");
                if (qCompare != null)
                {
                    qdp.OnErrorQuestionCompare = qCompare.Value;

                }

                //validation
                XPathNavigator validationNode = dateItr.Current.SelectSingleNode("child::Validation");
                if (validationNode != null)
                {
                    qdp.setValidation(validationNode.Value);

                }


                questionHash.Add(thisCode, qdp);


            }


            dateItr = nav.Select("//QuestionYear");

            while (dateItr.MoveNext())
            {

                qy = new QuestionYear(form, bigMessageBox, gs, specialDataStore, this);

                //get the code from the current node
                thisCode = dateItr.Current.SelectSingleNode("@code").Value;
                qy.Code = thisCode;


                //get the text of the question
                thisQuestionText = dateItr.Current.SelectSingleNode("@val").Value;
                qy.Val = thisQuestionText;

                //get the child node: WidgetPos
                widgetPosWidth = dateItr.Current.SelectSingleNode("child::WidgetPos/@width").Value;
                widgetPosHeight = dateItr.Current.SelectSingleNode("child::WidgetPos/@height").Value;
                widgetPosX = dateItr.Current.SelectSingleNode("child::WidgetPos/@xpos").Value;
                widgetPosY = dateItr.Current.SelectSingleNode("child::WidgetPos/@ypos").Value;

                qy.setWidgetWidth(widgetPosWidth);
                qy.setWidgetHeight(widgetPosHeight);
                qy.setWidgetXpos(widgetPosX);
                qy.setWidgetYpos(widgetPosY);

                //FromCode: optional
                gen = dateItr.Current.SelectSingleNode("child::FromCode");
                if (gen != null)
                {
                    fromCode = gen.Value;

                }
                else
                {
                    fromCode = null;
                }

                qy.FromCode = fromCode;


                //ToCode
                toCode = dateItr.Current.SelectSingleNode("child::ToCode").Value;
                qy.ToCode = toCode;

                setKey = dateItr.Current.SelectSingleNode("child::SetKey").Value;
                qy.SetKey = setKey;

                //section
                section = dateItr.Current.SelectSingleNode("child::Section").Value;
                qy.Section = section;

                XPathNavigator qCompare = dateItr.Current.SelectSingleNode("child::OnErrorQuestionCompare");
                if (qCompare != null)
                {
                    qy.OnErrorQuestionCompare = qCompare.Value;

                }

                //validation
                XPathNavigator validationNode = dateItr.Current.SelectSingleNode("child::Validation");
                if (validationNode != null)
                {
                    qy.setValidation(validationNode.Value);

                }


                questionHash.Add(thisCode, qy);


            }




            // Select the TextBox nodes
            textBoxItr = nav.Select("//QuestionTextDuplicate");


            while (textBoxItr.MoveNext())
            {

                //a new QuestionText object
                qtdup = new QuestionTextDuplicate(form, bigMessageBox, gs, specialDataStore, this);



                //get the code from the current node
                thisCode = textBoxItr.Current.SelectSingleNode("@code").Value;
                qtdup.Code = thisCode;


                //get the text of the question
                thisQuestionText = textBoxItr.Current.SelectSingleNode("@val").Value;
                qtdup.Val = thisQuestionText;

                //get the child node: WidgetPos
                widgetPosWidth = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@width").Value;
                widgetPosHeight = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@height").Value;
                widgetPosX = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@xpos").Value;
                widgetPosY = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@ypos").Value;

                qtdup.setWidgetWidth(widgetPosWidth);
                qtdup.setWidgetHeight(widgetPosHeight);
                qtdup.setWidgetXpos(widgetPosX);
                qtdup.setWidgetYpos(widgetPosY);


                //ToCode
                toCode = textBoxItr.Current.SelectSingleNode("child::ToCode").Value;
                qtdup.ToCode = toCode;

                //FromCode: optional
                gen = textBoxItr.Current.SelectSingleNode("child::FromCode");
                if (gen != null)
                {
                    fromCode = gen.Value;

                }
                else
                {
                    fromCode = null;
                }

                qtdup.FromCode = fromCode;





                XPathNavigator qCompare = textBoxItr.Current.SelectSingleNode("child::OnErrorQuestionCompare");
                if (qCompare != null)
                {
                    qtdup.OnErrorQuestionCompare = qCompare.Value;

                }

                XPathNavigator qSetKey = textBoxItr.Current.SelectSingleNode("child::SetKey");
                if (qSetKey != null)
                {
                    qtdup.SetKey = qSetKey.Value;

                }




                //validation
                validation = textBoxItr.Current.SelectSingleNode("child::Validation").Value;
                qtdup.setValidation(validation);

                //process
                process = textBoxItr.Current.SelectSingleNode("child::Process").Value;
                qtdup.setProcess(process);

                //section
                section = textBoxItr.Current.SelectSingleNode("child::Section").Value;
                qtdup.Section = section;



                //ifSetting

                XPathNavigator ifSettingKeyNode = textBoxItr.Current.SelectSingleNode("child::IfSetting/@key");
                if (ifSettingKeyNode != null)
                {
                    qtdup.IfSettingKey = ifSettingKeyNode.Value;

                }
                else
                {
                    qtdup.IfSettingKey = null;

                }

                XPathNavigator ifSettingValNode = textBoxItr.Current.SelectSingleNode("child::IfSetting/@val");
                if (ifSettingValNode != null)
                {
                    qtdup.IfSettingVal = ifSettingValNode.Value;

                }
                else
                {
                    qtdup.IfSettingVal = null;

                }




                //save the object in the hash
                questionHash.Add(thisCode, qtdup);



            }



            // Select the TextBox nodes
            textBoxItr = nav.Select("//QuestionTextBox");


            while (textBoxItr.MoveNext())
            {

                //a new QuestionText object
                qt = new QuestionText(form, bigMessageBox, gs, specialDataStore, this);



                //get the code from the current node
                thisCode = textBoxItr.Current.SelectSingleNode("@code").Value;
                qt.Code = thisCode;


                //question title

                //<Title lang="English">[1] Interviewer ID Code</Title>
                //<Title lang="Luganda">[1] Namba y’abuuuza ebibuuzo</Title>

                qt.Val = getTitle(textBoxItr, "Title");



                //get the text of the question
                //thisQuestionText = textBoxItr.Current.SelectSingleNode("@val").Value;
                //qt.Val = thisQuestionText;

                //get the child node: WidgetPos
                widgetPosWidth = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@width").Value;
                widgetPosHeight = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@height").Value;
                widgetPosX = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@xpos").Value;
                widgetPosY = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@ypos").Value;

                qt.setWidgetWidth(widgetPosWidth);
                qt.setWidgetHeight(widgetPosHeight);
                qt.setWidgetXpos(widgetPosX);
                qt.setWidgetYpos(widgetPosY);


                //ToCode
                toCode = textBoxItr.Current.SelectSingleNode("child::ToCode").Value;
                qt.ToCode = toCode;

                //FromCode: optional
                gen = textBoxItr.Current.SelectSingleNode("child::FromCode");
                if (gen != null)
                {
                    fromCode = gen.Value;

                }
                else
                {
                    fromCode = null;
                }

                qt.FromCode = fromCode;



                //onError
                //onError = textBoxItr.Current.SelectSingleNode("child::OnError").Value;
                //qt.OnError = onError;



                XPathNavigator qCompare = textBoxItr.Current.SelectSingleNode("child::OnErrorQuestionCompare");
                if (qCompare != null)
                {
                    qt.OnErrorQuestionCompare = qCompare.Value;

                }

                XPathNavigator qSetKey = textBoxItr.Current.SelectSingleNode("child::SetKey");
                if (qSetKey != null)
                {
                    qt.SetKey = qSetKey.Value;

                }




                //validation
                validation = textBoxItr.Current.SelectSingleNode("child::Validation").Value;
                qt.setValidation(validation);

                //process
                process = textBoxItr.Current.SelectSingleNode("child::Process").Value;
                qt.setProcess(process);

                //section
                section = textBoxItr.Current.SelectSingleNode("child::Section").Value;
                qt.Section = section;


                //multiline (i.e. like a textarea)
                XPathNavigator multiLineNode = textBoxItr.Current.SelectSingleNode("child::MultiLine");
                if (multiLineNode != null)
                {
                    qt.HasTextArea = true;

                }
                else
                {
                    qt.HasTextArea = false;

                }





                //ifSetting

                XPathNavigator ifSettingKeyNode = textBoxItr.Current.SelectSingleNode("child::IfSetting/@key");
                if (ifSettingKeyNode != null)
                {
                    qt.IfSettingKey = ifSettingKeyNode.Value;

                }
                else
                {
                    qt.IfSettingKey = null;

                }

                XPathNavigator ifSettingValNode = textBoxItr.Current.SelectSingleNode("child::IfSetting/@val");
                if (ifSettingValNode != null)
                {
                    qt.IfSettingVal = ifSettingValNode.Value;

                }
                else
                {
                    qt.IfSettingVal = null;

                }

                //optional gap between the label and the textbox
                gen = textBoxItr.Current.SelectSingleNode("child::LabelToBoxGap");
                if (gen != null)
                {
                    qt.LabelToBoxGap = Convert.ToInt32(gen.Value);

                    //MessageBox.Show("value is" + qt.LabelToBoxGap + "for:" + thisCode);

                    

                }
                else
                {
                    qt.LabelToBoxGap=0;

                    //MessageBox.Show("value is" + qt.LabelToBoxGap + "for:" + thisCode);

                }


                //recording
                qt.RecordThisQ = false;

                XPathNavigator recNode = textBoxItr.Current.SelectSingleNode("child::Recording");
                if (recNode != null)
                {
                    string recState = recNode.Value;

                    if (recState == "ON")
                    {
                        qt.RecordThisQ = true;


                    }

                }



                //save the object in the hash
                questionHash.Add(thisCode, qt);







            }


            
            textBoxItr = nav.Select("//QuestionTextDate");


            while (textBoxItr.MoveNext())
            {

                //a new QuestionText object
                qtdt = new QuestionTextDate(form, bigMessageBox, gs, specialDataStore, this);



                //get the code from the current node
                thisCode = textBoxItr.Current.SelectSingleNode("@code").Value;
                qtdt.Code = thisCode;


                //get the text of the question
                thisQuestionText = textBoxItr.Current.SelectSingleNode("@val").Value;
                qtdt.Val = thisQuestionText;

                //get the child node: WidgetPos
                widgetPosWidth = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@width").Value;
                widgetPosHeight = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@height").Value;
                widgetPosX = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@xpos").Value;
                widgetPosY = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@ypos").Value;

                qtdt.setWidgetWidth(widgetPosWidth);
                qtdt.setWidgetHeight(widgetPosHeight);
                qtdt.setWidgetXpos(widgetPosX);
                qtdt.setWidgetYpos(widgetPosY);


                //ToCode
                toCode = textBoxItr.Current.SelectSingleNode("child::ToCode").Value;
                qtdt.ToCode = toCode;

                //FromCode: optional
                gen = textBoxItr.Current.SelectSingleNode("child::FromCode");
                if (gen != null)
                {
                    fromCode = gen.Value;

                }
                else
                {
                    fromCode = null;
                }

                qtdt.FromCode = fromCode;



                //onError
                //onError = textBoxItr.Current.SelectSingleNode("child::OnError").Value;
                //qt.OnError = onError;



                XPathNavigator qCompare = textBoxItr.Current.SelectSingleNode("child::OnErrorQuestionCompare");
                if (qCompare != null)
                {
                    qtdt.OnErrorQuestionCompare = qCompare.Value;

                }

                XPathNavigator qSetKey = textBoxItr.Current.SelectSingleNode("child::SetKey");
                if (qSetKey != null)
                {
                    qtdt.SetKey = qSetKey.Value;

                }




                //validation
                validation = textBoxItr.Current.SelectSingleNode("child::Validation").Value;
                qtdt.setValidation(validation);

                //process
                process = textBoxItr.Current.SelectSingleNode("child::Process").Value;
                qtdt.setProcess(process);

                //section
                section = textBoxItr.Current.SelectSingleNode("child::Section").Value;
                qtdt.Section = section;


                //ifSetting

                XPathNavigator ifSettingKeyNode = textBoxItr.Current.SelectSingleNode("child::IfSetting/@key");
                if (ifSettingKeyNode != null)
                {
                    qtdt.IfSettingKey = ifSettingKeyNode.Value;

                }
                else
                {
                    qtdt.IfSettingKey = null;

                }

                XPathNavigator ifSettingValNode = textBoxItr.Current.SelectSingleNode("child::IfSetting/@val");
                if (ifSettingValNode != null)
                {
                    qtdt.IfSettingVal = ifSettingValNode.Value;

                }
                else
                {
                    qtdt.IfSettingVal = null;

                }

                //optional gap between the label and the textbox
                gen = textBoxItr.Current.SelectSingleNode("child::LabelToBoxGap");
                if (gen != null)
                {
                    qtdt.LabelToBoxGap = Convert.ToInt32(gen.Value);

                    //MessageBox.Show("value is" + qt.LabelToBoxGap + "for:" + thisCode);



                }
                else
                {
                    qtdt.LabelToBoxGap = 0;

                    //MessageBox.Show("value is" + qt.LabelToBoxGap + "for:" + thisCode);

                }



                //save the object in the hash
                questionHash.Add(thisCode, qtdt);







            }




            // Select the Automatic nodes
            textBoxItr = nav.Select("//QuestionAutomatic");


            while (textBoxItr.MoveNext())
            {

                //a new QuestionText object
                qauto = new QuestionAutomatic(form, bigMessageBox, gs, specialDataStore, this);



                //get the code from the current node
                thisCode = textBoxItr.Current.SelectSingleNode("@code").Value;
                qauto.Code = thisCode;




                //ToCode
                toCode = textBoxItr.Current.SelectSingleNode("child::ToCode").Value;
                qauto.ToCode = toCode;



                //process
                process = textBoxItr.Current.SelectSingleNode("child::Process").Value;
                qauto.setProcess(process);

                //section
                section = textBoxItr.Current.SelectSingleNode("child::Section").Value;
                qauto.Section = section;

                //advice-underweight

                XPathNavigator uwNode = textBoxItr.Current.SelectSingleNode("child::AdviceText[@name='underweight']");
                if (uwNode != null)
                {
                    qauto.UnderWeightAdvice = uwNode.Value;

                }


                //advice overweight

                XPathNavigator owNode = textBoxItr.Current.SelectSingleNode("child::AdviceText[@name='overweight']");
                if (owNode != null)
                {
                    qauto.OverWeightAdvice = owNode.Value;

                }

                //advice hypertensive
                XPathNavigator htNode = textBoxItr.Current.SelectSingleNode("child::AdviceText[@name='hypertensive']");
                if (htNode != null)
                {
                    qauto.HypertensiveAdvice = htNode.Value;

                }


                //save the object in the hash
                questionHash.Add(thisCode, qauto);



            }

            // Select the TextBox nodes
            textBoxItr = nav.Select("//QuestionTextTriple");


            while (textBoxItr.MoveNext())
            {

                //a new QuestionText object
                qtt = new QuestionTextTriple(form, bigMessageBox, gs, specialDataStore, this);



                //get the code from the current node
                thisCode = textBoxItr.Current.SelectSingleNode("@code").Value;
                qtt.Code = thisCode;


                //get the text of the question
                thisQuestionText = textBoxItr.Current.SelectSingleNode("@val").Value;
                qtt.Val = thisQuestionText;

                //get the child node: WidgetPos
                widgetPosWidth = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@width").Value;
                widgetPosHeight = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@height").Value;
                widgetPosX = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@xpos").Value;
                widgetPosY = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@ypos").Value;

                qtt.setWidgetWidth(widgetPosWidth);
                qtt.setWidgetHeight(widgetPosHeight);
                qtt.setWidgetXpos(widgetPosX);
                qtt.setWidgetYpos(widgetPosY);


                //ToCode
                toCode = textBoxItr.Current.SelectSingleNode("child::ToCode").Value;
                qtt.ToCode = toCode;

                //FromCode: optional
                gen = textBoxItr.Current.SelectSingleNode("child::FromCode");
                if (gen != null)
                {
                    fromCode = gen.Value;

                }
                else
                {
                    fromCode = null;
                }

                qtt.FromCode = fromCode;



                //onError
                //onError = textBoxItr.Current.SelectSingleNode("child::OnError").Value;
                //qtt.OnError = onError;



                XPathNavigator qCompare = textBoxItr.Current.SelectSingleNode("child::OnErrorQuestionCompare");
                if (qCompare != null)
                {
                    qtt.OnErrorQuestionCompare = qCompare.Value;

                }

                XPathNavigator qSetKey = textBoxItr.Current.SelectSingleNode("child::SetKey");
                if (qSetKey != null)
                {
                    qtt.SetKey = qSetKey.Value;

                }




                //validation
                validation = textBoxItr.Current.SelectSingleNode("child::Validation").Value;
                qtt.setValidation(validation);

                //process
                process = textBoxItr.Current.SelectSingleNode("child::Process").Value;
                qtt.setProcess(process);

                //section
                section = textBoxItr.Current.SelectSingleNode("child::Section").Value;
                qtt.Section = section;

                //ifSetting
                XPathNavigator ifSettingKeyNode = textBoxItr.Current.SelectSingleNode("child::IfSetting/@key");
                if (ifSettingKeyNode != null)
                {
                    qtt.IfSettingKey = ifSettingKeyNode.Value;

                }
                else
                {
                    qtt.IfSettingKey = null;

                }

                XPathNavigator ifSettingValNode = textBoxItr.Current.SelectSingleNode("child::IfSetting/@val");
                if (ifSettingValNode != null)
                {
                    qtt.IfSettingVal = ifSettingValNode.Value;

                }
                else
                {
                    qtt.IfSettingVal = null;

                }





                //save the object in the hash
                questionHash.Add(thisCode, qtt);







            }

            // Select the TextBox nodes
            textBoxItr = nav.Select("//QuestionGetTime");


            while (textBoxItr.MoveNext())
            {

                //a new QuestionText object
                qgt = new QuestionGetTime(form, bigMessageBox, gs, specialDataStore, this);



                //get the code from the current node
                thisCode = textBoxItr.Current.SelectSingleNode("@code").Value;
                qgt.Code = thisCode;


                //get the text of the question
                thisQuestionText = textBoxItr.Current.SelectSingleNode("@val").Value;
                qgt.Val = thisQuestionText;

                //get the child node: WidgetPos
                widgetPosWidth = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@width").Value;
                widgetPosHeight = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@height").Value;
                widgetPosX = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@xpos").Value;
                widgetPosY = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@ypos").Value;

                qgt.setWidgetWidth(widgetPosWidth);
                qgt.setWidgetHeight(widgetPosHeight);
                qgt.setWidgetXpos(widgetPosX);
                qgt.setWidgetYpos(widgetPosY);


                //ToCode
                toCode = textBoxItr.Current.SelectSingleNode("child::ToCode").Value;
                qgt.ToCode = toCode;



                XPathNavigator qSetKey = textBoxItr.Current.SelectSingleNode("child::SetKey");
                if (qSetKey != null)
                {
                    qgt.SetKey = qSetKey.Value;

                }

                //process
                process = textBoxItr.Current.SelectSingleNode("child::Process").Value;
                qgt.setProcess(process);

                //section
                section = textBoxItr.Current.SelectSingleNode("child::Section").Value;
                qgt.Section = section;

                //ifSetting
                XPathNavigator ifSettingKeyNode = textBoxItr.Current.SelectSingleNode("child::IfSetting/@key");
                if (ifSettingKeyNode != null)
                {
                    qgt.IfSettingKey = ifSettingKeyNode.Value;

                }
                else
                {
                    qgt.IfSettingKey = null;

                }

                XPathNavigator ifSettingValNode = textBoxItr.Current.SelectSingleNode("child::IfSetting/@val");
                if (ifSettingValNode != null)
                {
                    qgt.IfSettingVal = ifSettingValNode.Value;

                }
                else
                {
                    qgt.IfSettingVal = null;

                }





                //save the object in the hash
                questionHash.Add(thisCode, qgt);







            }


            // Select the TextBoxDouble nodes
            textBoxItr = nav.Select("//QuestionTextDouble");


            while (textBoxItr.MoveNext())
            {

                //a new QuestionText object
                qtd = new QuestionTextDouble(form, bigMessageBox, gs, specialDataStore, this);



                //get the code from the current node
                thisCode = textBoxItr.Current.SelectSingleNode("@code").Value;
                qtd.Code = thisCode;


                //get the text of the question
                thisQuestionText = textBoxItr.Current.SelectSingleNode("@val").Value;
                qtd.Val = thisQuestionText;

                //get the child node: WidgetPos
                widgetPosWidth = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@width").Value;
                widgetPosHeight = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@height").Value;
                widgetPosX = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@xpos").Value;
                widgetPosY = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@ypos").Value;

                qtd.setWidgetWidth(widgetPosWidth);
                qtd.setWidgetHeight(widgetPosHeight);
                qtd.setWidgetXpos(widgetPosX);
                qtd.setWidgetYpos(widgetPosY);


                //ToCode
                toCode = textBoxItr.Current.SelectSingleNode("child::ToCode").Value;
                qtd.ToCode = toCode;

                //FromCode: optional
                gen = textBoxItr.Current.SelectSingleNode("child::FromCode");
                if (gen != null)
                {
                    fromCode = gen.Value;

                }
                else
                {
                    fromCode = null;
                }

                qtd.FromCode = fromCode;







                XPathNavigator qCompare = textBoxItr.Current.SelectSingleNode("child::OnErrorQuestionCompare");
                if (qCompare != null)
                {
                    qtd.OnErrorQuestionCompare = qCompare.Value;

                }

                XPathNavigator qSetKey = textBoxItr.Current.SelectSingleNode("child::SetKey");
                if (qSetKey != null)
                {
                    qtd.SetKey = qSetKey.Value;

                }




                //validation
                validation = textBoxItr.Current.SelectSingleNode("child::Validation").Value;
                qtd.setValidation(validation);

                //process
                process = textBoxItr.Current.SelectSingleNode("child::Process").Value;
                qtd.setProcess(process);

                //section
                section = textBoxItr.Current.SelectSingleNode("child::Section").Value;
                qtd.Section = section;

                //ifSetting
                XPathNavigator ifSettingKeyNode = textBoxItr.Current.SelectSingleNode("child::IfSetting/@key");
                if (ifSettingKeyNode != null)
                {
                    qtd.IfSettingKey = ifSettingKeyNode.Value;

                }
                else
                {
                    qtd.IfSettingKey = null;

                }

                XPathNavigator ifSettingValNode = textBoxItr.Current.SelectSingleNode("child::IfSetting/@val");
                if (ifSettingValNode != null)
                {
                    qtd.IfSettingVal = ifSettingValNode.Value;

                }
                else
                {
                    qtd.IfSettingVal = null;

                }





                //save the object in the hash
                questionHash.Add(thisCode, qtd);







            }

            // Select the TextBoxOneCheck nodes, i.e. widgets which have a textbox and a single checkbox
            textBoxItr = nav.Select("//QuestionTextOneCheck");


            while (textBoxItr.MoveNext())
            {

                //a new QuestionText object
                qtc = new QuestionTextOneCheck(form, bigMessageBox, gs, specialDataStore, this);



                //get the code from the current node
                thisCode = textBoxItr.Current.SelectSingleNode("@code").Value;
                qtc.Code = thisCode;


                //get the text of the question
                thisQuestionText = textBoxItr.Current.SelectSingleNode("@val").Value;
                qtc.Val = thisQuestionText;

                //get the child node: WidgetPos
                widgetPosWidth = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@width").Value;
                widgetPosHeight = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@height").Value;
                widgetPosX = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@xpos").Value;
                widgetPosY = textBoxItr.Current.SelectSingleNode("child::WidgetPos/@ypos").Value;

                qtc.setWidgetWidth(widgetPosWidth);
                qtc.setWidgetHeight(widgetPosHeight);
                qtc.setWidgetXpos(widgetPosX);
                qtc.setWidgetYpos(widgetPosY);


                //ToCode
                toCode = textBoxItr.Current.SelectSingleNode("child::ToCode").Value;
                qtc.ToCode = toCode;

                //FromCode: optional
                gen = textBoxItr.Current.SelectSingleNode("child::FromCode");
                if (gen != null)
                {
                    fromCode = gen.Value;

                }
                else
                {
                    fromCode = null;
                }

                qtc.FromCode = fromCode;






                XPathNavigator qCompare = textBoxItr.Current.SelectSingleNode("child::OnErrorQuestionCompare");
                if (qCompare != null)
                {
                    qtc.OnErrorQuestionCompare = qCompare.Value;

                }

                XPathNavigator qSetKey = textBoxItr.Current.SelectSingleNode("child::SetKey");
                if (qSetKey != null)
                {
                    qtc.SetKey = qSetKey.Value;

                }




                //validation
                validation = textBoxItr.Current.SelectSingleNode("child::Validation").Value;
                qtc.setValidation(validation);

                //process
                process = textBoxItr.Current.SelectSingleNode("child::Process").Value;
                qtc.setProcess(process);

                //section
                section = textBoxItr.Current.SelectSingleNode("child::Section").Value;
                qtc.Section = section;

                //ifSetting
                XPathNavigator ifSettingKeyNode = textBoxItr.Current.SelectSingleNode("child::IfSetting/@key");
                if (ifSettingKeyNode != null)
                {
                    qtc.IfSettingKey = ifSettingKeyNode.Value;

                }
                else
                {
                    qtc.IfSettingKey = null;

                }

                XPathNavigator ifSettingValNode = textBoxItr.Current.SelectSingleNode("child::IfSetting/@val");
                if (ifSettingValNode != null)
                {
                    qtc.IfSettingVal = ifSettingValNode.Value;

                }
                else
                {
                    qtc.IfSettingVal = null;

                }




                //label for the checkbox

                qtc.CheckBoxLabel = textBoxItr.Current.SelectSingleNode("child::CheckBoxLabel").Value;

                //code to insert as the data if the checkbox is checked

                qtc.CheckBoxCheckCode = textBoxItr.Current.SelectSingleNode("child::CheckBoxCheckCode").Value;

                //save the object in the hash
                questionHash.Add(thisCode, qtc);







            }

            //process the Radio Questions

            radioItr = nav.Select("//QuestionRadio");

            while (radioItr.MoveNext())
            {


                qr = new QuestionRadio(form, bigMessageBox, gs, specialDataStore, this);

                //fields that appear once per radio

                //get the code from the current node
                thisCode = radioItr.Current.SelectSingleNode("@code").Value;
                qr.Code = thisCode;


                //get the text of the question
                //thisQuestionText = radioItr.Current.SelectSingleNode("@val").Value;
                //qr.Val = thisQuestionText;

                qr.Val = getTitle(radioItr, "Title");



                //get the child node: WidgetPos
                widgetPosWidth = radioItr.Current.SelectSingleNode("child::WidgetPos/@width").Value;
                widgetPosHeight = radioItr.Current.SelectSingleNode("child::WidgetPos/@height").Value;
                widgetPosX = radioItr.Current.SelectSingleNode("child::WidgetPos/@xpos").Value;
                widgetPosY = radioItr.Current.SelectSingleNode("child::WidgetPos/@ypos").Value;

                qr.setWidgetWidth(widgetPosWidth);
                qr.setWidgetHeight(widgetPosHeight);
                qr.setWidgetXpos(widgetPosX);
                qr.setWidgetYpos(widgetPosY);

                //ToCode
                toCode = radioItr.Current.SelectSingleNode("child::ToCode").Value;
                qr.ToCode = toCode;

                //LabelToGroupBoxGap : optional
                string labelToGroupBoxGap = null;
                XPathNavigator ltgbgNode = radioItr.Current.SelectSingleNode("child::LabelToGroupBoxGap");
                if (ltgbgNode != null)
                {

                    labelToGroupBoxGap = ltgbgNode.Value;


                }
                qr.LabelToGroupBoxGap = labelToGroupBoxGap;





                //FromCode: optional
                gen = radioItr.Current.SelectSingleNode("child::FromCode");
                if (gen != null)
                {
                    fromCode = gen.Value;

                }
                else
                {
                    fromCode = null;
                }

                qr.FromCode = fromCode;


                //setKey : optional

                XPathNavigator setKeyNode = radioItr.Current.SelectSingleNode("child::SetKey");
                if (setKeyNode == null)
                {
                    qr.SetKey = null;

                }
                else
                {
                    qr.SetKey = setKeyNode.Value;


                }

                //setKey = radioItr.Current.SelectSingleNode("child::SetKey").Value;
                //qr.SetKey = setKey;

                //section
                section = radioItr.Current.SelectSingleNode("child::Section").Value;
                qr.Section = section;

                //ifSetting
                XPathNavigator ifSettingKeyNode = radioItr.Current.SelectSingleNode("child::IfSetting/@key");
                if (ifSettingKeyNode != null)
                {
                    qr.IfSettingKey = ifSettingKeyNode.Value;

                }
                else
                {
                    qr.IfSettingKey = null;

                }

                XPathNavigator ifSettingValNode = radioItr.Current.SelectSingleNode("child::IfSetting/@val");
                if (ifSettingValNode != null)
                {
                    qr.IfSettingVal = ifSettingValNode.Value;

                }
                else
                {
                    qr.IfSettingVal = null;

                }

                //recording
                qr.RecordThisQ = false;

                XPathNavigator recNode = radioItr.Current.SelectSingleNode("child::Recording");
                if (recNode != null)
                {
                    string recState = recNode.Value;

                    if (recState == "ON")
                    {
                        qr.RecordThisQ = true;


                    }

                }


                //each option node
                optionItr = radioItr.Current.Select("OptionRadio");

                while (optionItr.MoveNext())
                {


                    //value
                    optionValue = optionItr.Current.SelectSingleNode("child::Value").Value;

                    //text
                    //optionText = optionItr.Current.SelectSingleNode("child::Text").Value;

                    optionText = getTitle(optionItr, "Title");



                    //toCode: optional
                    XPathNavigator toCodeNode = optionItr.Current.SelectSingleNode("child::ToCode");
                    if (toCodeNode != null)
                    {
                        optionToCode = toCodeNode.Value;

                    }
                    else
                    {
                        optionToCode = null;
                    }


                    //ToCodeAndError: optional

                    XPathNavigator toCodeErrNode = optionItr.Current.SelectSingleNode("child::ToCodeAndError");
                    if (toCodeErrNode != null)
                    {
                        optionToCodeErr = toCodeErrNode.Value;

                    }
                    else
                    {
                        optionToCodeErr = null;
                    }



                    //get the child node: WidgetPos
                    widgetPosWidth = optionItr.Current.SelectSingleNode("child::WidgetPos/@width").Value;
                    widgetPosHeight = optionItr.Current.SelectSingleNode("child::WidgetPos/@height").Value;
                    widgetPosX = optionItr.Current.SelectSingleNode("child::WidgetPos/@xpos").Value;
                    widgetPosY = optionItr.Current.SelectSingleNode("child::WidgetPos/@ypos").Value;



                    op = new Option(optionValue, optionText);

                    op.setWidgetWidth(widgetPosWidth);
                    op.setWidgetHeight(widgetPosHeight);
                    op.setWidgetXpos(widgetPosX);
                    op.setWidgetYpos(widgetPosY);

                    op.ToCode = optionToCode;
                    op.ToCodeErr = optionToCodeErr;

                    //is this option the default ?
                    var defaultOption = optionItr.Current.SelectSingleNode("child::Default");

                    if (defaultOption == null)
                    {
                        op.Default = false;
                    }
                    else
                    {
                        op.Default = true;

                    }


                    qr.addOption(op);


                }

                //save the object in the hash
                questionHash.Add(thisCode, qr);

            }


            radioItr = nav.Select("//QuestionRadioDynamicLabel");

            while (radioItr.MoveNext())
            {


                qrd = new QuestionRadioDynamicLabel(form, bigMessageBox, gs, specialDataStore, this);

                //fields that appear once per radio

                //get the code from the current node
                thisCode = radioItr.Current.SelectSingleNode("@code").Value;
                qrd.Code = thisCode;


                //get the text of the question
                thisQuestionText = radioItr.Current.SelectSingleNode("@val").Value;
                qrd.Val = thisQuestionText;

                //get the child node: WidgetPos
                widgetPosWidth = radioItr.Current.SelectSingleNode("child::WidgetPos/@width").Value;
                widgetPosHeight = radioItr.Current.SelectSingleNode("child::WidgetPos/@height").Value;
                widgetPosX = radioItr.Current.SelectSingleNode("child::WidgetPos/@xpos").Value;
                widgetPosY = radioItr.Current.SelectSingleNode("child::WidgetPos/@ypos").Value;

                qrd.setWidgetWidth(widgetPosWidth);
                qrd.setWidgetHeight(widgetPosHeight);
                qrd.setWidgetXpos(widgetPosX);
                qrd.setWidgetYpos(widgetPosY);

                //ToCode
                toCode = radioItr.Current.SelectSingleNode("child::ToCode").Value;
                qrd.ToCode = toCode;

                //label generator func
                string labelGenFunc = radioItr.Current.SelectSingleNode("child::LabelGeneratorFunc").Value;
                qrd.LabelGeneratorFunc = labelGenFunc;


                //LabelToGroupBoxGap : optional
                string labelToGroupBoxGap = null;
                XPathNavigator ltgbgNode = radioItr.Current.SelectSingleNode("child::LabelToGroupBoxGap");
                if (ltgbgNode != null)
                {

                    labelToGroupBoxGap = ltgbgNode.Value;


                }
                qrd.LabelToGroupBoxGap = labelToGroupBoxGap;





                //FromCode: optional
                gen = radioItr.Current.SelectSingleNode("child::FromCode");
                if (gen != null)
                {
                    fromCode = gen.Value;

                }
                else
                {
                    fromCode = null;
                }

                qrd.FromCode = fromCode;


                //setKey : optional

                XPathNavigator setKeyNode = radioItr.Current.SelectSingleNode("child::SetKey");
                if (setKeyNode == null)
                {
                    qrd.SetKey = null;

                }
                else
                {
                    qrd.SetKey = setKeyNode.Value;


                }

                //setKey = radioItr.Current.SelectSingleNode("child::SetKey").Value;
                //qr.SetKey = setKey;

                //section
                section = radioItr.Current.SelectSingleNode("child::Section").Value;
                qrd.Section = section;

                //ifSetting
                XPathNavigator ifSettingKeyNode = radioItr.Current.SelectSingleNode("child::IfSetting/@key");
                if (ifSettingKeyNode != null)
                {
                    qrd.IfSettingKey = ifSettingKeyNode.Value;

                }
                else
                {
                    qrd.IfSettingKey = null;

                }

                XPathNavigator ifSettingValNode = radioItr.Current.SelectSingleNode("child::IfSetting/@val");
                if (ifSettingValNode != null)
                {
                    qrd.IfSettingVal = ifSettingValNode.Value;

                }
                else
                {
                    qrd.IfSettingVal = null;

                }




                //each option node
                optionItr = radioItr.Current.Select("OptionRadio");

                while (optionItr.MoveNext())
                {


                    //value
                    optionValue = optionItr.Current.SelectSingleNode("child::Value").Value;

                    //text
                    optionText = optionItr.Current.SelectSingleNode("child::Text").Value;

                    //toCode: optional
                    XPathNavigator toCodeNode = optionItr.Current.SelectSingleNode("child::ToCode");
                    if (toCodeNode != null)
                    {
                        optionToCode = toCodeNode.Value;

                    }
                    else
                    {
                        optionToCode = null;
                    }


                    //ToCodeAndError: optional

                    XPathNavigator toCodeErrNode = optionItr.Current.SelectSingleNode("child::ToCodeAndError");
                    if (toCodeErrNode != null)
                    {
                        optionToCodeErr = toCodeErrNode.Value;

                    }
                    else
                    {
                        optionToCodeErr = null;
                    }



                    //get the child node: WidgetPos
                    widgetPosWidth = optionItr.Current.SelectSingleNode("child::WidgetPos/@width").Value;
                    widgetPosHeight = optionItr.Current.SelectSingleNode("child::WidgetPos/@height").Value;
                    widgetPosX = optionItr.Current.SelectSingleNode("child::WidgetPos/@xpos").Value;
                    widgetPosY = optionItr.Current.SelectSingleNode("child::WidgetPos/@ypos").Value;



                    op = new Option(optionValue, optionText);

                    op.setWidgetWidth(widgetPosWidth);
                    op.setWidgetHeight(widgetPosHeight);
                    op.setWidgetXpos(widgetPosX);
                    op.setWidgetYpos(widgetPosY);

                    op.ToCode = optionToCode;
                    op.ToCodeErr = optionToCodeErr;

                    //is this option the default ?
                    var defaultOption = optionItr.Current.SelectSingleNode("child::Default");

                    if (defaultOption == null)
                    {
                        op.Default = false;
                    }
                    else
                    {
                        op.Default = true;

                    }


                    qrd.addOption(op);


                }

                //save the object in the hash
                questionHash.Add(thisCode, qrd);

            }


            radioItr = nav.Select("//QuestionSelect");

            while (radioItr.MoveNext())
            {


                qs = new QuestionSelect(form, bigMessageBox, gs, specialDataStore, this);

                //fields that appear once per radio

                //get the code from the current node
                thisCode = radioItr.Current.SelectSingleNode("@code").Value;
                qs.Code = thisCode;


                //get the text of the question
                //thisQuestionText = radioItr.Current.SelectSingleNode("@val").Value;
                qs.Val = getTitle(radioItr, "Title");

                //qs.Val = thisQuestionText;

                //get the child node: WidgetPos
                widgetPosWidth = radioItr.Current.SelectSingleNode("child::WidgetPos/@width").Value;
                widgetPosHeight = radioItr.Current.SelectSingleNode("child::WidgetPos/@height").Value;
                widgetPosX = radioItr.Current.SelectSingleNode("child::WidgetPos/@xpos").Value;
                widgetPosY = radioItr.Current.SelectSingleNode("child::WidgetPos/@ypos").Value;

                qs.setWidgetWidth(widgetPosWidth);
                qs.setWidgetHeight(widgetPosHeight);
                qs.setWidgetXpos(widgetPosX);
                qs.setWidgetYpos(widgetPosY);

                //ToCode
                toCode = radioItr.Current.SelectSingleNode("child::ToCode").Value;
                qs.ToCode = toCode;

                //SelectLength
                qs.SelectLength = Convert.ToInt32(radioItr.Current.SelectSingleNode("child::SelectLength").Value);

                
                //FromCode: optional
                gen = radioItr.Current.SelectSingleNode("child::FromCode");
                if (gen != null)
                {
                    fromCode = gen.Value;

                }
                else
                {
                    fromCode = null;
                }

                qs.FromCode = fromCode;


                //setKey : optional

                XPathNavigator setKeyNode = radioItr.Current.SelectSingleNode("child::SetKey");
                if (setKeyNode == null)
                {
                    qs.SetKey = null;

                }
                else
                {
                    qs.SetKey = setKeyNode.Value;


                }

                

                //section
                section = radioItr.Current.SelectSingleNode("child::Section").Value;
                qs.Section = section;

                //ifSetting
                XPathNavigator ifSettingKeyNode = radioItr.Current.SelectSingleNode("child::IfSetting/@key");
                if (ifSettingKeyNode != null)
                {
                    qs.IfSettingKey = ifSettingKeyNode.Value;

                }
                else
                {
                    qs.IfSettingKey = null;

                }

                XPathNavigator ifSettingValNode = radioItr.Current.SelectSingleNode("child::IfSetting/@val");
                if (ifSettingValNode != null)
                {
                    qs.IfSettingVal = ifSettingValNode.Value;

                }
                else
                {
                    qs.IfSettingVal = null;

                }

                //recording
                qs.RecordThisQ = false;

                XPathNavigator recNode = radioItr.Current.SelectSingleNode("child::Recording");
                if (recNode != null)
                {
                    string recState = recNode.Value;

                    if (recState == "ON")
                    {
                        qs.RecordThisQ = true;


                    }

                }


                //each option node
                optionItr = radioItr.Current.Select("OptionSelect");

                while (optionItr.MoveNext())
                {


                    //value
                    optionValue = optionItr.Current.SelectSingleNode("child::Value").Value;

                    //text
                    //optionText = optionItr.Current.SelectSingleNode("child::Text").Value;
                    optionText = getTitle(optionItr, "Title");

                    //toCode: optional
                    XPathNavigator toCodeNode = optionItr.Current.SelectSingleNode("child::ToCode");
                    if (toCodeNode != null)
                    {
                        optionToCode = toCodeNode.Value;

                    }
                    else
                    {
                        optionToCode = null;
                    }


                    //ToCodeAndError: optional

                    XPathNavigator toCodeErrNode = optionItr.Current.SelectSingleNode("child::ToCodeAndError");
                    if (toCodeErrNode != null)
                    {
                        optionToCodeErr = toCodeErrNode.Value;

                    }
                    else
                    {
                        optionToCodeErr = null;
                    }



                    op = new Option(optionValue, optionText);

                    

                    op.ToCode = optionToCode;
                    op.ToCodeErr = optionToCodeErr;

                    //is this option the default ?
                    var defaultOption = optionItr.Current.SelectSingleNode("child::Default");

                    if (defaultOption == null)
                    {
                        op.Default = false;
                    }
                    else
                    {
                        op.Default = true;

                    }


                    qs.addOption(op);


                }

                //save the object in the hash
                questionHash.Add(thisCode, qs);

            }


            radioItr = nav.Select("//QuestionRank");

            while (radioItr.MoveNext())
            {


                qrk = new QuestionRank(form, bigMessageBox, gs, specialDataStore, this);

                //fields that appear once per radio

                //get the code from the current node
                thisCode = radioItr.Current.SelectSingleNode("@code").Value;
                qrk.Code = thisCode;


                //get the text of the question
                //thisQuestionText = radioItr.Current.SelectSingleNode("@val").Value;
                qrk.Val = getTitle(radioItr, "Title");

                //qs.Val = thisQuestionText;

                //get the child node: WidgetPos
                widgetPosWidth = radioItr.Current.SelectSingleNode("child::WidgetPos/@width").Value;
                widgetPosHeight = radioItr.Current.SelectSingleNode("child::WidgetPos/@height").Value;
                widgetPosX = radioItr.Current.SelectSingleNode("child::WidgetPos/@xpos").Value;
                widgetPosY = radioItr.Current.SelectSingleNode("child::WidgetPos/@ypos").Value;

                qrk.setWidgetWidth(widgetPosWidth);
                qrk.setWidgetHeight(widgetPosHeight);
                qrk.setWidgetXpos(widgetPosX);
                qrk.setWidgetYpos(widgetPosY);

                //ToCode
                toCode = radioItr.Current.SelectSingleNode("child::ToCode").Value;
                qrk.ToCode = toCode;

                //SelectLength
                qrk.SelectLength = Convert.ToInt32(radioItr.Current.SelectSingleNode("child::SelectLength").Value);


                //FromCode: optional
                gen = radioItr.Current.SelectSingleNode("child::FromCode");
                if (gen != null)
                {
                    fromCode = gen.Value;

                }
                else
                {
                    fromCode = null;
                }

                qrk.FromCode = fromCode;


                //setKey : optional

                XPathNavigator setKeyNode = radioItr.Current.SelectSingleNode("child::SetKey");
                if (setKeyNode == null)
                {
                    qrk.SetKey = null;

                }
                else
                {
                    qrk.SetKey = setKeyNode.Value;


                }



                //section
                section = radioItr.Current.SelectSingleNode("child::Section").Value;
                qrk.Section = section;

                //ifSetting
                XPathNavigator ifSettingKeyNode = radioItr.Current.SelectSingleNode("child::IfSetting/@key");
                if (ifSettingKeyNode != null)
                {
                    qrk.IfSettingKey = ifSettingKeyNode.Value;

                }
                else
                {
                    qrk.IfSettingKey = null;

                }

                XPathNavigator ifSettingValNode = radioItr.Current.SelectSingleNode("child::IfSetting/@val");
                if (ifSettingValNode != null)
                {
                    qrk.IfSettingVal = ifSettingValNode.Value;

                }
                else
                {
                    qrk.IfSettingVal = null;

                }

                //recording
                qrk.RecordThisQ = false;

                XPathNavigator recNode = radioItr.Current.SelectSingleNode("child::Recording");
                if (recNode != null)
                {
                    string recState = recNode.Value;

                    if (recState == "ON")
                    {
                        qrk.RecordThisQ = true;


                    }

                }


                //each option node
                optionItr = radioItr.Current.Select("OptionSelect");

                while (optionItr.MoveNext())
                {


                    //value
                    optionValue = optionItr.Current.SelectSingleNode("child::Value").Value;

                    //text
                    //optionText = optionItr.Current.SelectSingleNode("child::Text").Value;
                    optionText = getTitle(optionItr, "Title");

                    //toCode: optional
                    XPathNavigator toCodeNode = optionItr.Current.SelectSingleNode("child::ToCode");
                    if (toCodeNode != null)
                    {
                        optionToCode = toCodeNode.Value;

                    }
                    else
                    {
                        optionToCode = null;
                    }


                    //ToCodeAndError: optional

                    XPathNavigator toCodeErrNode = optionItr.Current.SelectSingleNode("child::ToCodeAndError");
                    if (toCodeErrNode != null)
                    {
                        optionToCodeErr = toCodeErrNode.Value;

                    }
                    else
                    {
                        optionToCodeErr = null;
                    }



                    op = new Option(optionValue, optionText);



                    op.ToCode = optionToCode;
                    op.ToCodeErr = optionToCodeErr;

                    //is this option the default ?
                    var defaultOption = optionItr.Current.SelectSingleNode("child::Default");

                    if (defaultOption == null)
                    {
                        op.Default = false;
                    }
                    else
                    {
                        op.Default = true;

                    }


                    qrk.addOption(op);


                }

                //save the object in the hash
                questionHash.Add(thisCode, qrk);

            }




            //process the labelQuestions:

            labelItr = nav.Select("//QuestionLabel");

            while (labelItr.MoveNext())
            {


                ql = new QuestionLabel(form, bigMessageBox, gs, specialDataStore, this);

                //fields that appear once per radio

                //get the code from the current node
                thisCode = labelItr.Current.SelectSingleNode("@code").Value;
                ql.Code = thisCode;


                //get the text of the question
                //thisQuestionText = labelItr.Current.SelectSingleNode("@val").Value;
                //ql.Val = thisQuestionText;

                ql.Val = getTitle(labelItr, "Title");

                //get the child node: WidgetPos
                widgetPosWidth = labelItr.Current.SelectSingleNode("child::WidgetPos/@width").Value;
                widgetPosHeight = labelItr.Current.SelectSingleNode("child::WidgetPos/@height").Value;
                widgetPosX = labelItr.Current.SelectSingleNode("child::WidgetPos/@xpos").Value;
                widgetPosY = labelItr.Current.SelectSingleNode("child::WidgetPos/@ypos").Value;

                ql.setWidgetWidth(widgetPosWidth);
                ql.setWidgetHeight(widgetPosHeight);
                ql.setWidgetXpos(widgetPosX);
                ql.setWidgetYpos(widgetPosY);

                //ToCode
                toCode = labelItr.Current.SelectSingleNode("child::ToCode").Value;
                ql.ToCode = toCode;

                //FromCode: optional
                gen = labelItr.Current.SelectSingleNode("child::FromCode");
                if (gen != null)
                {
                    fromCode = gen.Value;

                }
                else
                {
                    fromCode = null;
                }

                ql.FromCode = fromCode;

                //section
                section = labelItr.Current.SelectSingleNode("child::Section").Value;
                ql.Section = section;


                //recording
                ql.RecordThisQ = false;

                XPathNavigator recNode = labelItr.Current.SelectSingleNode("child::Recording");
                if (recNode != null)
                {
                    string recState = recNode.Value;

                    if (recState == "ON")
                    {
                        ql.RecordThisQ = true;


                    }
                    
                }
                


                //ifSetting
                XPathNavigator ifSettingKeyNode = labelItr.Current.SelectSingleNode("child::IfSetting/@key");
                if (ifSettingKeyNode != null)
                {
                    ql.IfSettingKey = ifSettingKeyNode.Value;

                }
                else
                {
                    ql.IfSettingKey = null;

                }

                XPathNavigator ifSettingValNode = labelItr.Current.SelectSingleNode("child::IfSetting/@val");
                if (ifSettingValNode != null)
                {
                    ql.IfSettingVal = ifSettingValNode.Value;

                }
                else
                {
                    ql.IfSettingVal = null;

                }


                //ifSettingKey = labelItr.Current.SelectSingleNode("child::IfSetting/@key").Value;
                //ifSettingVal = labelItr.Current.SelectSingleNode("child::IfSetting/@val").Value;
                //ql.IfSettingKey = ifSettingKey;
                //ql.IfSettingVal = ifSettingVal;



                //save the object in the hash
                questionHash.Add(thisCode, ql);

            }

            //process TextRadio questions (have a textbox plus a set of radio buttons)

            textRadioItr = nav.Select("//QuestionTextRadio");


            while (textRadioItr.MoveNext())
            {

                //a new QuestionText object
                qtr = new QuestionTextRadio(form, bigMessageBox, gs, specialDataStore, this);



                //get the code from the current node
                thisCode = textRadioItr.Current.SelectSingleNode("@code").Value;
                qtr.Code = thisCode;


                //get the text of the question
               // thisQuestionText = textRadioItr.Current.SelectSingleNode("@val").Value;
                //qtr.Val = thisQuestionText;

                qtr.Val = getTitle(textRadioItr, "Title");

                //get the child node: WidgetPos
                widgetPosWidth = textRadioItr.Current.SelectSingleNode("child::WidgetPos/@width").Value;
                widgetPosHeight = textRadioItr.Current.SelectSingleNode("child::WidgetPos/@height").Value;
                widgetPosX = textRadioItr.Current.SelectSingleNode("child::WidgetPos/@xpos").Value;
                widgetPosY = textRadioItr.Current.SelectSingleNode("child::WidgetPos/@ypos").Value;

                qtr.setWidgetWidth(widgetPosWidth);
                qtr.setWidgetHeight(widgetPosHeight);
                qtr.setWidgetXpos(widgetPosX);
                qtr.setWidgetYpos(widgetPosY);


                //ToCode
                toCode = textRadioItr.Current.SelectSingleNode("child::ToCode").Value;
                qtr.ToCode = toCode;

                //FromCode: optional
                gen = textRadioItr.Current.SelectSingleNode("child::FromCode");
                if (gen != null)
                {
                    fromCode = gen.Value;

                }
                else
                {
                    fromCode = null;
                }

                qtr.FromCode = fromCode;





                XPathNavigator qCompare = textRadioItr.Current.SelectSingleNode("child::OnErrorQuestionCompare");
                if (qCompare != null)
                {
                    qtr.OnErrorQuestionCompare = qCompare.Value;

                }


                XPathNavigator checkDontKnow = textRadioItr.Current.SelectSingleNode("child::CheckPreviousDontKnow");
                if (checkDontKnow != null)
                {
                    qtr.CheckPreviousDontKnow = true;


                }
                else
                {
                    qtr.CheckPreviousDontKnow = false;

                }



                //validation
                validation = textRadioItr.Current.SelectSingleNode("child::Validation").Value;
                qtr.setValidation(validation);

                //process
                process = textRadioItr.Current.SelectSingleNode("child::Process").Value;
                qtr.setProcess(process);

                //section
                section = textRadioItr.Current.SelectSingleNode("child::Section").Value;
                qtr.Section = section;

                //ifSetting
                XPathNavigator ifSettingKeyNode = textRadioItr.Current.SelectSingleNode("child::IfSetting/@key");
                if (ifSettingKeyNode != null)
                {
                    qtr.IfSettingKey = ifSettingKeyNode.Value;

                }
                else
                {
                    qtr.IfSettingKey = null;

                }

                XPathNavigator ifSettingValNode = textRadioItr.Current.SelectSingleNode("child::IfSetting/@val");
                if (ifSettingValNode != null)
                {
                    qtr.IfSettingVal = ifSettingValNode.Value;

                }
                else
                {
                    qtr.IfSettingVal = null;

                }


                //setKey

                //setKey = textRadioItr.Current.SelectSingleNode("child::SetKey").Value;
                //qtr.SetKey = setKey;

                XPathNavigator setKeyNav = textRadioItr.Current.SelectSingleNode("child::SetKey");
                if (setKeyNav != null)
                {
                    qtr.SetKey = setKeyNav.Value;

                }





                //label for the radio-set
                //string radioLabel = textRadioItr.Current.SelectSingleNode("child::RadioLabel").Value;
                //qtr.RadioLabel = radioLabel;

                qtr.RadioLabel = getTitle(textRadioItr, "Subtitle");

                //recording
                qtr.RecordThisQ = false;

                XPathNavigator recNode = textRadioItr.Current.SelectSingleNode("child::Recording");
                if (recNode != null)
                {
                    string recState = recNode.Value;

                    if (recState == "ON")
                    {
                        qtr.RecordThisQ = true;


                    }

                }
              

                //each option node
                optionItr = textRadioItr.Current.Select("OptionRadio");

                while (optionItr.MoveNext())
                {


                    //value
                    optionValue = optionItr.Current.SelectSingleNode("child::Value").Value;

                    //text
                    //optionText = optionItr.Current.SelectSingleNode("child::Text").Value;
                    optionText= getTitle(optionItr, "Title");



                    //get the child node: WidgetPos
                    widgetPosWidth = optionItr.Current.SelectSingleNode("child::WidgetPos/@width").Value;
                    widgetPosHeight = optionItr.Current.SelectSingleNode("child::WidgetPos/@height").Value;
                    widgetPosX = optionItr.Current.SelectSingleNode("child::WidgetPos/@xpos").Value;
                    widgetPosY = optionItr.Current.SelectSingleNode("child::WidgetPos/@ypos").Value;



                    op = new Option(optionValue, optionText);

                    op.setWidgetWidth(widgetPosWidth);
                    op.setWidgetHeight(widgetPosHeight);
                    op.setWidgetXpos(widgetPosX);
                    op.setWidgetYpos(widgetPosY);



                    //is this option the default ?
                    var defaultOption = optionItr.Current.SelectSingleNode("child::Default");

                    if (defaultOption == null)
                    {
                        op.Default = false;
                    }
                    else
                    {
                        op.Default = true;

                    }

                    //toCode: optional
                    XPathNavigator toCodeNode = optionItr.Current.SelectSingleNode("child::ToCode");
                    if (toCodeNode != null)
                    {
                        op.ToCode = toCodeNode.Value;

                    }
                    else
                    {
                        op.ToCode = null;
                    }


                    qtr.addOption(op);


                }



                //save the object in the hash
                questionHash.Add(thisCode, qtr);
            }


            textRadioItr = nav.Select("//QuestionSelectText");


            while (textRadioItr.MoveNext())
            {

                //a new QuestionText object
                qst = new QuestionSelectText(form, bigMessageBox, gs, specialDataStore, this);



                //get the code from the current node
                thisCode = textRadioItr.Current.SelectSingleNode("@code").Value;
                qst.Code = thisCode;


                //get the text of the question
                
                qst.Val = getTitle(textRadioItr, "Title");


                //SelectLength
                qst.SelectLength = Convert.ToInt32(textRadioItr.Current.SelectSingleNode("child::SelectLength").Value);

                //get the child node: WidgetPos
                widgetPosWidth = textRadioItr.Current.SelectSingleNode("child::WidgetPos/@width").Value;
                widgetPosHeight = textRadioItr.Current.SelectSingleNode("child::WidgetPos/@height").Value;
                widgetPosX = textRadioItr.Current.SelectSingleNode("child::WidgetPos/@xpos").Value;
                widgetPosY = textRadioItr.Current.SelectSingleNode("child::WidgetPos/@ypos").Value;

                qst.setWidgetWidth(widgetPosWidth);
                qst.setWidgetHeight(widgetPosHeight);
                qst.setWidgetXpos(widgetPosX);
                qst.setWidgetYpos(widgetPosY);


                //ToCode
                toCode = textRadioItr.Current.SelectSingleNode("child::ToCode").Value;
                qst.ToCode = toCode;

                //FromCode: optional
                gen = textRadioItr.Current.SelectSingleNode("child::FromCode");
                if (gen != null)
                {
                    fromCode = gen.Value;

                }
                else
                {
                    fromCode = null;
                }

                qst.FromCode = fromCode;





                XPathNavigator qCompare = textRadioItr.Current.SelectSingleNode("child::OnErrorQuestionCompare");
                if (qCompare != null)
                {
                    qst.OnErrorQuestionCompare = qCompare.Value;

                }


                XPathNavigator checkDontKnow = textRadioItr.Current.SelectSingleNode("child::CheckPreviousDontKnow");
                if (checkDontKnow != null)
                {
                    qst.CheckPreviousDontKnow = true;


                }
                else
                {
                    qst.CheckPreviousDontKnow = false;

                }



                //validation
                validation = textRadioItr.Current.SelectSingleNode("child::Validation").Value;
                qst.setValidation(validation);

                //process
                process = textRadioItr.Current.SelectSingleNode("child::Process").Value;
                qst.setProcess(process);

                //section
                section = textRadioItr.Current.SelectSingleNode("child::Section").Value;
                qst.Section = section;

                //ifSetting
                XPathNavigator ifSettingKeyNode = textRadioItr.Current.SelectSingleNode("child::IfSetting/@key");
                if (ifSettingKeyNode != null)
                {
                    qst.IfSettingKey = ifSettingKeyNode.Value;

                }
                else
                {
                    qst.IfSettingKey = null;

                }

                XPathNavigator ifSettingValNode = textRadioItr.Current.SelectSingleNode("child::IfSetting/@val");
                if (ifSettingValNode != null)
                {
                    qst.IfSettingVal = ifSettingValNode.Value;

                }
                else
                {
                    qst.IfSettingVal = null;

                }


               


                //setKey

                //setKey = textRadioItr.Current.SelectSingleNode("child::SetKey").Value;
                XPathNavigator setKeyNav = textRadioItr.Current.SelectSingleNode("child::SetKey");
                if (setKeyNav != null)
                {
                    qst.SetKey = setKeyNav.Value;

                }

                

                //label for the radio-set
                //string radioLabel = textRadioItr.Current.SelectSingleNode("child::TextLabel").Value;
                //qtr.RadioLabel = radioLabel;


                qst.TextLabel = getTitle(textRadioItr, "Subtitle");

                //recording
                qst.RecordThisQ = false;

                XPathNavigator recNode = textRadioItr.Current.SelectSingleNode("child::Recording");
                if (recNode != null)
                {
                    string recState = recNode.Value;

                    if (recState == "ON")
                    {
                        qst.RecordThisQ = true;


                    }

                }


                //each option node
                optionItr = textRadioItr.Current.Select("OptionSelect");

                while (optionItr.MoveNext())
                {


                    //value
                    optionValue = optionItr.Current.SelectSingleNode("child::Value").Value;

                    //text
                    //optionText = optionItr.Current.SelectSingleNode("child::Text").Value;
                    optionText = getTitle(optionItr, "Title");

                    //toCode: optional
                    XPathNavigator toCodeNode = optionItr.Current.SelectSingleNode("child::ToCode");
                    if (toCodeNode != null)
                    {
                        optionToCode = toCodeNode.Value;

                    }
                    else
                    {
                        optionToCode = null;
                    }


                    //ToCodeAndError: optional

                    XPathNavigator toCodeErrNode = optionItr.Current.SelectSingleNode("child::ToCodeAndError");
                    if (toCodeErrNode != null)
                    {
                        optionToCodeErr = toCodeErrNode.Value;

                    }
                    else
                    {
                        optionToCodeErr = null;
                    }



                    op = new Option(optionValue, optionText);



                    op.ToCode = optionToCode;
                    op.ToCodeErr = optionToCodeErr;

                    //is this option the default ?
                    var defaultOption = optionItr.Current.SelectSingleNode("child::Default");

                    if (defaultOption == null)
                    {
                        op.Default = false;
                    }
                    else
                    {
                        op.Default = true;

                    }


                    qst.addOption(op);


                }



                //save the object in the hash
                questionHash.Add(thisCode, qst);
            }







            textRadioItr = nav.Select("//QuestionTextRadioButton");


            while (textRadioItr.MoveNext())
            {

                //a new QuestionText object
                qtrb = new QuestionTextRadioButton(form, bigMessageBox, gs, specialDataStore, this);



                //get the code from the current node
                thisCode = textRadioItr.Current.SelectSingleNode("@code").Value;
                qtrb.Code = thisCode;


                //get the text of the question
                thisQuestionText = textRadioItr.Current.SelectSingleNode("@val").Value;
                qtrb.Val = thisQuestionText;

                //get the child node: WidgetPos
                widgetPosWidth = textRadioItr.Current.SelectSingleNode("child::WidgetPos/@width").Value;
                widgetPosHeight = textRadioItr.Current.SelectSingleNode("child::WidgetPos/@height").Value;
                widgetPosX = textRadioItr.Current.SelectSingleNode("child::WidgetPos/@xpos").Value;
                widgetPosY = textRadioItr.Current.SelectSingleNode("child::WidgetPos/@ypos").Value;

                qtrb.setWidgetWidth(widgetPosWidth);
                qtrb.setWidgetHeight(widgetPosHeight);
                qtrb.setWidgetXpos(widgetPosX);
                qtrb.setWidgetYpos(widgetPosY);


                //ToCode
                toCode = textRadioItr.Current.SelectSingleNode("child::ToCode").Value;
                qtrb.ToCode = toCode;

                //FromCode: optional
                gen = textRadioItr.Current.SelectSingleNode("child::FromCode");
                if (gen != null)
                {
                    fromCode = gen.Value;

                }
                else
                {
                    fromCode = null;
                }

                qtrb.FromCode = fromCode;





                gen = textRadioItr.Current.SelectSingleNode("child::OnErrorQuestionCompare");
                if (gen != null)
                {
                    qtrb.OnErrorQuestionCompare = gen.Value;

                }


                gen = textRadioItr.Current.SelectSingleNode("child::CheckPreviousDontKnow");
                if (gen != null)
                {
                    qtrb.CheckPreviousDontKnow = true;


                }
                else
                {
                    qtrb.CheckPreviousDontKnow = false;

                }



                //validation
                validation = textRadioItr.Current.SelectSingleNode("child::Validation").Value;
                qtrb.setValidation(validation);

                //process
                process = textRadioItr.Current.SelectSingleNode("child::Process").Value;
                qtrb.setProcess(process);

                //section
                section = textRadioItr.Current.SelectSingleNode("child::Section").Value;
                qtrb.Section = section;

                //ifSetting
                gen = textRadioItr.Current.SelectSingleNode("child::IfSetting/@key");
                if (gen != null)
                {
                    qtrb.IfSettingKey = gen.Value;

                }
                else
                {
                    qtrb.IfSettingKey = null;

                }

                gen = textRadioItr.Current.SelectSingleNode("child::IfSetting/@val");
                if (gen != null)
                {
                    qtrb.IfSettingVal = gen.Value;

                }
                else
                {
                    qtrb.IfSettingVal = null;

                }




                //setKey

                setKey = textRadioItr.Current.SelectSingleNode("child::SetKey").Value;
                qtrb.SetKey = setKey;

                //label for the radio-set
                //radioLabel = textRadioItr.Current.SelectSingleNode("child::RadioLabel").Value;
                qtrb.RadioLabel = textRadioItr.Current.SelectSingleNode("child::RadioLabel").Value;


                //validation that runs when the special button is pressed
                string buttonValidationCode = textRadioItr.Current.SelectSingleNode("child::ValidationButton").Value;
                qtrb.setValidationButton(buttonValidationCode);





                //each option node
                optionItr = textRadioItr.Current.Select("OptionRadio");

                while (optionItr.MoveNext())
                {


                    //value
                    optionValue = optionItr.Current.SelectSingleNode("child::Value").Value;

                    //text
                    optionText = optionItr.Current.SelectSingleNode("child::Text").Value;



                    //get the child node: WidgetPos
                    widgetPosWidth = optionItr.Current.SelectSingleNode("child::WidgetPos/@width").Value;
                    widgetPosHeight = optionItr.Current.SelectSingleNode("child::WidgetPos/@height").Value;
                    widgetPosX = optionItr.Current.SelectSingleNode("child::WidgetPos/@xpos").Value;
                    widgetPosY = optionItr.Current.SelectSingleNode("child::WidgetPos/@ypos").Value;



                    op = new Option(optionValue, optionText);

                    op.setWidgetWidth(widgetPosWidth);
                    op.setWidgetHeight(widgetPosHeight);
                    op.setWidgetXpos(widgetPosX);
                    op.setWidgetYpos(widgetPosY);



                    //is this option the default ?
                    var defaultOption = optionItr.Current.SelectSingleNode("child::Default");

                    if (defaultOption == null)
                    {
                        op.Default = false;
                    }
                    else
                    {
                        op.Default = true;

                    }


                    qtrb.addOption(op);


                }



                //save the object in the hash
                questionHash.Add(thisCode, qtrb);







            }


        }
         */




    }
}
