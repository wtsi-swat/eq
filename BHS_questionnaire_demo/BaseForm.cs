using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.IO;
using System.Text.RegularExpressions;




namespace BHS_questionnaire_demo
{
    public partial class BaseForm : Form
    {

        //do we have any Qforms open ?
        bool qFormIsOpen = false;

        //current status of the logger
        //can be 'on', 'off', 'suspended' (suspended= was on, but has been disabled while the user has a questionnaire window open
        private string loggerStatus = "off";


        //current form for the survey
        Form1 currentSurvey = null;

        //current survey opened for checking completeness but not editing
        CompletenessForm currentSurveyComp = null;

        //the participant ID for the currently open Questionnaire
        string openParticipantID = null;


        //dir for storing all questionnaires
        private string dataDir;

        //a message box for warnings
        private Form3 warningMessageBox;

        //an info box
        private MessageForm stdMessageBox;

        //error box
        private Form2 errorBox;

        //a confirm (yes/no) box
        private ConfirmForm confirmForm;

        //private string portNum;
        //private string baudRate;
        //private string gpsCountry;

        //Registry Key
        private string keyName = @"Software\EQuestionnaire\Settings";

        private string rawAudioFile = "raw_audio_data.wav";       //wav file

        private Timer testTimer = new Timer();

        private AudioRecordingTest ar = null;

        private Dictionary<string, Qconfig> configMap;      //maps qType to parsed config object

        private Qconfig currentConfig = null;


        //used by confirm window to send back result
        public string confirmResult
        {
            get;
            set;

        }



        public BaseForm()
        {
            InitializeComponent();

            configMap = new Dictionary<string, Qconfig>();






        }

        private void BaseForm_Load(object sender, EventArgs e)
        {


            testTimer.Tick += new EventHandler(timer_Tick); // Everytime timer ticks, timer_Tick will be called
            testTimer.Interval = 3000;
            testTimer.Enabled = false;



            //set English as default language
            //listBox1.SelectedItem = "English";




            //set active cursor on the partID box
            textBox1.Focus();


            //init dialogs
            warningMessageBox = new Form3();
            stdMessageBox = new MessageForm();
            errorBox = new Form2();
            confirmForm = new ConfirmForm();



            //get saved values from registry if they exist
            //readonly access
            RegistryKey regKey = Registry.CurrentUser.OpenSubKey(keyName);
            if (regKey != null)
            {

                //get values
                dataDir = (string)regKey.GetValue("DATA_DIR");
                //portNum = (string)regKey.GetValue("GPS_PORT");
                //baudRate = (string)regKey.GetValue("GPS_BAUD");
                //gpsCountry = (string)regKey.GetValue("GPS_COUNTRY");

                //string GPSenabled = (string)regKey.GetValue("GPS_ENABLED");




                if (dataDir != null)
                {
                    label7.Text = dataDir;


                }





                regKey.Close();


            }

            if (dataDir != null)
            {

                //does the base dir still exist, i.e. its possible that the user has moved or deleted it.
                if (Directory.Exists(dataDir))
                {
                    //populate the current questionnaires, if any
                    updateExistingQuestionList(null);

                    //load help text file
                    loadHelp();


                    //find all questionnaire types in the data dir and show in the combo box.


                    DirectoryInfo di = new DirectoryInfo(dataDir);


                    string fileName;
                    string qType;   //questionnaire type

                    foreach (FileInfo file in di.GetFiles())
                    {
                        fileName = file.Name;

                        Match match = Regex.Match(fileName, "^EQ_(.+).xml$");

                        if (match.Success)
                        {

                            qType = match.Groups[1].Value;

                            //add this to the select box
                            comboBox4.Items.Add(qType);

                            //add to the select box for exporting data
                            comboBox1.Items.Add(qType);

                            //parse the config file
                            parseConfig(qType);


                        }

                    }


                }
                else
                {
                    warningMessageBox.setLabel("Warning: The Data Directory has been changed: please update the settings");
                    warningMessageBox.ShowDialog();


                }



            }



            //remove country chooser: set as invisible
            label13.Visible = false;
            comboBox2.Visible = false;





        }


        private void parseConfig(string qType)
        {

            //parse the config file and store it in a Qconfig object in a dict where key is qType
            //read these from the config file

            //fetch the config file for this Qtype and find the languages that are supported
            string configFilePath = dataDir + "\\" + "EQ_" + qType + ".conf";


            Qconfig conf = new Qconfig(errorBox);
            conf.parseConfig(configFilePath);

            configMap[qType] = conf;


        }




        private void loadHelp()
        {

            //load the help text into the help tab
            StreamReader dh = null;

            try
            {
                string helpFileName = dataDir + "\\EQ help.txt";

                dh = new StreamReader(helpFileName);

                StringBuilder sb = new StringBuilder();

                while (dh.EndOfStream == false)
                {
                    string line = dh.ReadLine();

                    sb.Append(line);
                    sb.Append("\n");


                }

                dh.Close();

                label8.Text = sb.ToString();



            }
            catch (Exception e)
            {
                warningMessageBox.setLabel("Warning: Could not load the help Text file, although this is not essential");
                warningMessageBox.ShowDialog();

                //MessageBox.Show("error:" + e.Message + e.StackTrace);


            }
            finally
            {

                if (dh != null)
                {
                    dh.Close();

                }


            }




        }

        private void button8_Click(object sender, EventArgs e)
        {

            //user has clicked the set data directory button

            //open a file dialog to get the name of the dir

            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "Select the folder to store questionnaire data";

            //set root to My Documents
            //folderBrowserDialog.RootFolder = Environment.SpecialFolder.Personal;

            // Show the FolderBrowserDialog.
            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                dataDir = folderBrowserDialog.SelectedPath;

                //update label:
                label7.Text = dataDir;


            }







        }

        private void button7_Click(object sender, EventArgs e)
        {
            //test GPS connection

        }

        private void button9_Click(object sender, EventArgs e)
        {

            //save gps and data dir values to the registry

            RegistryKey regKey = Registry.CurrentUser.OpenSubKey(keyName, RegistryKeyPermissionCheck.ReadWriteSubTree);
            if (regKey == null)
            {
                //not present: create the key
                regKey = Registry.CurrentUser;
                regKey = regKey.CreateSubKey(keyName);

            }


            //save data in this key


            if (dataDir != null)
            {
                regKey.SetValue("DATA_DIR", dataDir);

            }



            regKey.Close();


            //restart the application so we can cleanly reload from the new datadir
            Application.Restart();




        }

        public void questionFormClosing()
        {

            // a questionnaire is calling this to tell us it is closing
            qFormIsOpen = false;

            //this might have been a new question, so rebuild our list of existing Qs
            updateExistingQuestionList(null);

            openParticipantID = null;

            //turn GPS logging on if it was on previously:
            /*
            if (loggerStatus == "suspended")
            {

                portNum = (string)comboBox1.SelectedItem;
                baudRate = (string)comboBox2.SelectedItem;

                if ((portNum == null) || (baudRate == null))
                {

                    //show fail
                    warningMessageBox.setLabel("Warning: Please select Port and Baud Rate");
                    warningMessageBox.ShowDialog();


                }
                else
                {
                    //test serial port connection

                    serialPort1.PortName = portNum;

                    //comm port for laptop
                    //serialPort1.PortName = "COM4";

                    serialPort1.BaudRate = Convert.ToInt32(baudRate);

                    //start the timer which will update the GPS data each half second
                    timer1.Enabled = true;


                    //try and open the serial port for GPS comms
                    try
                    {
                        serialPort1.Open();

                        label11.Text = "Starting...";
                        label12.Text = "Starting...";

                        loggerStatus = "on";


                    }
                    catch
                    {

                        //show warning screen
                        timer1.Enabled = false;

                        warningMessageBox.setLabel("Warning: Can't open serial port connection to the GPS unit. Make sure the unit is plugged in.");
                        warningMessageBox.ShowDialog();

                    }


                }




            }
             * */


        }




        private void button1_Click(object sender, EventArgs e)
        {

            //new questionnaire

            //do we have a questionnaire open already ?
            if (qFormIsOpen)
            {
                //still open
                errorBox.setLabel("Error: You have a Questionnaire already open: you must close it first.");
                errorBox.ShowDialog();
                return;



            }

            //has the user selected a language and participant ID

            //how many languages are present?
            int numLangs = listBox1.Items.Count;
            string language;

            if (numLangs == 1)
            {
                language = (string)listBox1.Items[0];

            }
            else
            {

                language = (string)listBox1.SelectedItem;

                if (language == null)
                {
                    errorBox.setLabel("Error: Please select language");
                    errorBox.ShowDialog();
                    return;



                }

            }



            //what qType did they select?
            string qType = (string)comboBox4.SelectedItem;

            if (qType == null)
            {

                errorBox.setLabel("Error: Please select Questionnaire type");
                errorBox.ShowDialog();
                return;

            }


            //do we have a dir for this qType?
            string qTypeDir = dataDir + "\\" + qType;

            if (!Directory.Exists(qTypeDir))
            {
                //No, create the dir
                try
                {
                    Directory.CreateDirectory(qTypeDir);
                }
                catch
                {

                    errorBox.setLabel("Error: Could not create directory for this questionaire type. You may not have permission to write to this location");
                    errorBox.ShowDialog();
                    return;

                }

            }



            string xmlFilePath;

            //what is the filePath for the xml config file
            xmlFilePath = dataDir + "\\EQ_" + qType + ".xml";


            //check that the xml file exists
            if (!File.Exists(xmlFilePath))
            {

                errorBox.setLabel("Error: Cannot find expected configuration file:" + xmlFilePath);
                errorBox.ShowDialog();
                return;


            }




            //participantID
            string partID = textBox1.Text;

            if (string.IsNullOrWhiteSpace(partID))
            {

                errorBox.setLabel("Error: Please enter a Participant ID");
                errorBox.ShowDialog();
                return;


            }

            //check that the partID does not contain any underscore characters (which will cause problems as its gets embedded into dir/file names which use underscores as separators
            if (partID.Contains("_"))
            {

                errorBox.setLabel("Error: The Participant ID contains underscore character(s), which is not allowed.");
                errorBox.ShowDialog();
                return;

            }


            //does this participant already exist (in which case they should be using the existing participant page)?
            string globalPartID = qType + ":" + partID;

            foreach (Participant part in listBox2.Items)
            {
                //if (partID == part.getID())
                if (globalPartID == part.getGlobalID())
                {

                    errorBox.setLabel("Error: This participant already exists");
                    errorBox.ShowDialog();
                    return;

                }




            }





            if (dataDir == null)
            {
                errorBox.setLabel("Error: Please set the data directory (settings)");
                errorBox.ShowDialog();
                return;

            }



            //check that this participant does not already exist, i.e. that the new datadir does not exist

            //string partDataDir = dataDir + "\\participant_data_" + qType + "_" + partID;
            string partDataDir = dataDir + "\\" + qType + "\\participant_data_" + partID;



            if (Directory.Exists(partDataDir))
            {
                //user already exists
                errorBox.setLabel("Error: This Participant already exists.");
                errorBox.ShowDialog();
                return;


            }


            //create the new dir.
            try
            {

                Directory.CreateDirectory(partDataDir);


            }
            catch
            {

                errorBox.setLabel("Error: Could not create directory for this participant. You may not have permission to write to this location");
                errorBox.ShowDialog();
                return;

            }


            




            //get the config object for this qtype
            Qconfig config = configMap[qType];

            string selectedCountryName = null;


            //set the selected country for forms that support multiple countries
            if (config.hasMultipleCountries())
            {

                selectedCountryName = (string)comboBox2.SelectedItem;

                if (selectedCountryName == null)
                {

                    errorBox.setLabel("Error: Please select a Country");
                    errorBox.ShowDialog();
                    return;

                }
                else
                {

                    config.selectedCountryName = selectedCountryName;


                }



            }




            //create a config file for this user, which will contain the selected language
            TextWriter tw = null;
            string userConfigFileName = partDataDir + "\\questionnaire_config.txt";

            try
            {

                tw = new StreamWriter(userConfigFileName);

                tw.WriteLine("language~" + language);

                if (selectedCountryName != null)
                {
                    tw.WriteLine("country~" + selectedCountryName);

                }


            }
            catch
            {
                errorBox.setLabel("Error: Could not create config file for this participant. You may not have permission to write to this location");
                errorBox.ShowDialog();
                return;

            }
            finally
            {
                if (tw != null)
                {
                    tw.Dispose();


                }


            }






            //open the form
            currentSurvey = new Form1();

            try
            {

                //start the survey
                //currentSurvey.startSurvey(xmlFilePath, partDataDir, partID, true, portNum, baudRate, this, gpsCountry, language, dataDir, config, userConfigFileName);

                currentSurvey.startSurvey(xmlFilePath, partDataDir, partID, true, null, null, this, null, language, dataDir, config, userConfigFileName);

                currentSurvey.Show();

                qFormIsOpen = true;

                openParticipantID = partID;




            }
            catch (ObjectDisposedException e2)
            {

                //something went wrong with startup, e.g. XML parsing exception

                qFormIsOpen = false;


            }



        }


        private void button2_Click(object sender, EventArgs e)
        {
            //edit selected questionnaire
            //do we have a questionnaire open already ?
            if (qFormIsOpen)
            {
                //still open
                errorBox.setLabel("Error: You have a Questionnaire already open: you must close it first.");
                errorBox.ShowDialog();
                return;



            }

            Participant selectedPart = (Participant)listBox2.SelectedItem;

            if (selectedPart == null)
            {

                errorBox.setLabel("Error: Please select a Participant from the list");
                errorBox.ShowDialog();
                return;



            }

            //is the Part locked
            if (selectedPart.Locked)
            {
                errorBox.setLabel("Error: Cannot edit a locked Participant");
                errorBox.ShowDialog();
                return;

            }

            if (dataDir == null)
            {
                errorBox.setLabel("Error: Please set the data directory (settings)");
                errorBox.ShowDialog();
                return;

            }

            //get the selected language
            string lang = (string)comboBox5.SelectedItem;
            string qType = selectedPart.qType;
            string partID = selectedPart.getID();


            //xml file path
            string xmlFilePath = dataDir + "\\EQ_" + qType + ".xml";

            //participant data dir
            string partDataDir = dataDir + "\\" + qType + "\\participant_data_" + partID;

            string userConfigFileName = partDataDir + "\\questionnaire_config.txt";


            //check that the xml file exists
            if (!File.Exists(xmlFilePath))
            {

                errorBox.setLabel("Error: Cannot find expected configuration file:" + xmlFilePath);
                errorBox.ShowDialog();
                return;


            }

            //get the config object for this qtype
            Qconfig config = configMap[qType];


            //set the selected country for forms that support multiple countries
            if (config.hasMultipleCountries())
            {
                
                
                //get selected country from the config file
                string selectedCountryName = getAttributeFromParticipantConfig(dataDir + "\\" + qType + "\\participant_data_" + partID + "\\questionnaire_config.txt", "country");


                if (selectedCountryName == null)
                {

                    errorBox.setLabel("Error: No Country was found for this participant");
                    errorBox.ShowDialog();
                    return;

                }
                else
                {

                    config.selectedCountryName = selectedCountryName;


                }



            }









            currentSurvey = new Form1();

            try
            {

                //start the survey
                //currentSurvey.startSurvey(xmlFilePath, partDataDir, partID, false, null, null, this, gpsCountry, lang, dataDir, config, userConfigFileName);
                currentSurvey.startSurvey(xmlFilePath, partDataDir, partID, false, null, null, this, null, lang, dataDir, config, userConfigFileName);

                currentSurvey.Show();

                qFormIsOpen = true;

                openParticipantID = partID;




            }
            catch (ObjectDisposedException e2)
            {

                //something went wrong with startup, e.g. XML parsing exception

                qFormIsOpen = false;


            }







        }




        /*

        private void button2_Click(object sender, EventArgs e)
        {
            //edit selected questionnaire

            //do we have a questionnaire open already ?
            if (qFormIsOpen)
            {
                //still open
                errorBox.setLabel("Error: You have a Questionnaire already open: you must close it first.");
                errorBox.ShowDialog();
                return;



            }

            Participant selectedPart = (Participant)listBox2.SelectedItem;

            if (selectedPart == null)
            {

                errorBox.setLabel("Error: Please select a Participant from the list");
                errorBox.ShowDialog();
                return;



            }

            //is the Part locked
            if (selectedPart.Locked)
            {
                errorBox.setLabel("Error: Cannot edit a locked Participant");
                errorBox.ShowDialog();
                return;

            }

            if (dataDir == null)
            {
                errorBox.setLabel("Error: Please set the data directory (settings)");
                errorBox.ShowDialog();
                return;

            }


            //open the form
            

            //has the user selected a language and participant ID

            string language = selectedPart.getLanguage();

            string xmlFilePath;

            //what is the filePath for the xml config file
            if (language == "English")
            {

                xmlFilePath = dataDir + "\\EQuestionnaire_English.xml";

            }
            else if (language == "Chewa")
            {

                xmlFilePath = dataDir + "\\EQuestionnaire_Chewa.xml";


            }
            else
            {

                //language not supported
                errorBox.setLabel("Error: Language '" + language + " is not currently supported");
                errorBox.ShowDialog();
                return;


            }


            //check that the xml file exists
            if (!File.Exists(xmlFilePath))
            {

                errorBox.setLabel("Error: Cannot find expected configuration file:" + xmlFilePath);
                errorBox.ShowDialog();
                return;


            }




            //participantID
            string partID = selectedPart.getID();

           


            //has the user set the GPS baud rate, port and the main data dir ?

            portNum = (string)comboBox1.SelectedItem;
            baudRate = (string)comboBox2.SelectedItem;
            gpsCountry = (string)comboBox3.SelectedItem;


            if (portNum == null && Utils.GPSenabled)
            {

                errorBox.setLabel("Error: Please set the GPS port Number (settings)");
                errorBox.ShowDialog();
                return;

            }

            if (baudRate == null && Utils.GPSenabled)
            {

                errorBox.setLabel("Error: Please set the GPS baudrate (settings)");
                errorBox.ShowDialog();
                return;

            }

            if (gpsCountry == null && Utils.GPSenabled)
            {
                errorBox.setLabel("Error: Please set the GPS country (settings)");
                errorBox.ShowDialog();
                return;

            }


            //check that this participant does not already exist, i.e. that the new datadir does not exist

            string partDataDir = selectedPart.getPath();


            
            


            //open the form
            currentSurvey = new Form1();


           

            try
            {

                //start the survey
                currentSurvey.startSurvey(xmlFilePath, partDataDir, partID, false, portNum, baudRate, this, gpsCountry);

                currentSurvey.Show();

                qFormIsOpen = true;

                openParticipantID = partID;




            }
            catch(ObjectDisposedException e2){

                //something went wrong with startup, e.g. XML parsing exception
                
                qFormIsOpen = false;


            }

            
            


        }
*/

        private void updateExistingQuestionList(string dirPathIgnore)
        {
            //update the list of questions shown in the listbox
            //if the paranm is not null, then ignore that dir

            if (dataDir != null)
            {

                //delete current contents of the listbox
                listBox2.Items.Clear();

                List<Participant> partList = new List<Participant>();

                //get all the directories from the base dir of the form: participant_data_
                DirectoryInfo di = new DirectoryInfo(dataDir);


                string subDirName;
                string subDirPath;
                string partID;
                string language;
                string qType;
                string qDir;    //dir for a questionnaire type
                Dictionary<string, string> partConf = null;


                foreach (DirectoryInfo subDir in di.GetDirectories())
                {

                    //dir for each qType

                    qType = subDir.Name;
                    qDir = dataDir + "\\" + qType;

                    foreach (DirectoryInfo partDir in subDir.GetDirectories())
                    {
                        //dir for each participant

                        //should we ignore this?
                        if (dirPathIgnore != null)
                        {
                            if (dirPathIgnore == partDir.FullName)
                            {
                                continue;

                            }


                        }

                        //get partID

                        subDirName = partDir.Name;

                        Match match = Regex.Match(subDirName, "^participant_data_(.+)$");

                        if (match.Success)
                        {
                            partID = match.Groups[1].Value;
                            subDirPath = partDir.FullName;

                            //read the config file for this Part.

                            string userConfigFileName = subDirPath + "\\questionnaire_config.txt";

                            partConf = Utils.readUserConfigFile(userConfigFileName);



                            /*
                            //does the lockfile exist, i.e. meaning the Participant should be treated as locked
                            string lockFileName = subDirPath + "\\lockfile.txt";

                            bool partIsLocked;

                            if (File.Exists(lockFileName))
                            {
                                partIsLocked = true;

                            }
                            else
                            {
                                partIsLocked = false;

                            }
                             */



                            Participant part = new Participant(subDirPath, partID, null);
                            //part.Locked = partIsLocked;
                            part.qType = qType;

                            part.setConfig(partConf, userConfigFileName);



                            //a valid participant dir
                            //listBox2.Items.Add(part);

                            partList.Add(part);


                        }


                    }
                }





                //order as strings
                IEnumerable<Participant> partListSorted = null;
                partListSorted = partList.OrderBy(n => n.getID());


                //add these to the listBox
                foreach (Participant part in partListSorted)
                {

                    listBox2.Items.Add(part);

                }




            }



        }

        private void button3_Click(object sender, EventArgs e)
        {

            //delete the selected Questionnaire

            Participant selectedPart = (Participant)listBox2.SelectedItem;

            if (selectedPart == null)
            {

                errorBox.setLabel("Error: Please select a Participant from the list");
                errorBox.ShowDialog();
                return;



            }

            //is the Part locked
            if (selectedPart.Locked)
            {
                errorBox.setLabel("Error: Cannot delete a locked Participant");
                errorBox.ShowDialog();
                return;

            }




            //do we have a questionnaire open already ?
            //if we have an open questionnaire, make sure it is not the one we want to delete
            if (qFormIsOpen)
            {

                //is this the same as the one we want to delete
                if (openParticipantID == selectedPart.getID())
                {
                    //yes: stop deletion
                    errorBox.setLabel("Error: You must close a Questionnaire before you can delete it.");
                    errorBox.ShowDialog();
                    return;



                }



            }


            //make sure that the user really does want to delete this
            confirmForm.setFormLabel("Are you sure you want to delete this participant ?", this);
            confirmForm.ShowDialog();

            if (confirmResult == "yes")
            {
                //get the path to the dir
                string dirPath = selectedPart.getPath();

                //delete the dir
                try
                {
                    Directory.Delete(dirPath, true);

                    //refresh the list of Parts: ignore the dir we just deleted
                    //Note: the delete function may return BEFORE the dir has been completed removed, therefore we should ignore this dir if it shows up in the next fn

                    updateExistingQuestionList(dirPath);

                    //stdMessageBox.setMainLabel("Participant deleted");
                    //stdMessageBox.ShowDialog();


                }
                catch
                {
                    errorBox.setLabel("Error: The Participant could not be deleted");
                    errorBox.ShowDialog();

                }






            }









        }

        public void lockQuestionnaire()
        {



            //user wants to lock the selected Questionnaire
            Participant selectedPart = (Participant)listBox2.SelectedItem;

            if (selectedPart == null)
            {

                errorBox.setLabel("Error: Please select a Participant from the list");
                errorBox.ShowDialog();
                return;



            }

            //is it locked already ?

            if (selectedPart.Locked)
            {

                errorBox.setLabel("Error: This Participant is locked already");
                errorBox.ShowDialog();
                return;


            }


            //do we have a questionnaire open already ?
            //if we have an open questionnaire, make sure it is not the one we want to lock
            if (qFormIsOpen)
            {

                //is this the same as the one we want to delete
                if (openParticipantID == selectedPart.getID())
                {
                    //yes: stop deletion
                    errorBox.setLabel("Error: You must close a Questionnaire before you can lock it.");
                    errorBox.ShowDialog();
                    return;



                }



            }


            //save the lock-status to the user config file

            string qType = selectedPart.qType;
            string partID = selectedPart.getID();

            //show status of the selected Q, i.e. display the completion state.

            //get the config file
            string partDataDir = dataDir + "\\" + qType + "\\participant_data_" + partID;
            string userConfigFileName = partDataDir + "\\questionnaire_config.txt";


            //are all the sections complete?
            //we only lock when all are complete

            Dictionary<string, bool> completeness = Utils.readCompletionData(userConfigFileName);
            //check that each section is complete

            foreach (bool completed in completeness.Values)
            {
                if (!completed)
                {

                    errorBox.setLabel("Error: You must complete a Questionnaire before you can lock it.");
                    errorBox.ShowDialog();
                    return;

                }

            }

            Utils.setPartLocked(userConfigFileName);


            /*
            //place a lockfile inside the dir for this participant as a marker
            string partDir = selectedPart.getPath();
            string lockFileName = partDir + "\\lockfile.txt";

            try
            {

                FileStream fh = File.Create(lockFileName);
                fh.Close();



            }
            catch
            {

                errorBox.setLabel("Error: Could not lock this Participant");
                errorBox.ShowDialog();
                return;

            }
             */






            //redraw the list
            updateExistingQuestionList(null);



        }



        private void button4_Click(object sender, EventArgs e)
        {







        }

        private void button5_Click(object sender, EventArgs e)
        {

            //unlock the selected Participant
            Participant selectedPart = (Participant)listBox2.SelectedItem;

            if (selectedPart == null)
            {

                errorBox.setLabel("Error: Please select a Participant from the list");
                errorBox.ShowDialog();
                return;



            }

            //is the particpant locked
            if (!selectedPart.Locked)
            {
                errorBox.setLabel("Error: This Participant is NOT locked");
                errorBox.ShowDialog();
                return;


            }

            //delete the  lockfile 

            selectedPart.unlock();





            string partDir = selectedPart.getPath();
            string lockFileName = partDir + "\\lockfile.txt";

            try
            {

                File.Delete(lockFileName);



            }
            catch
            {

                errorBox.setLabel("Error: Could not unlock this Participant");
                errorBox.ShowDialog();
                return;

            }


            //redraw the list
            updateExistingQuestionList(null);








        }

        private void button6_Click(object sender, EventArgs e)
        {
            //copy all data


            //user should first select the type of form to use


            if (comboBox1.SelectedItem == null)
            {

                errorBox.setLabel("Error: please select form-type to copy from first");
                errorBox.ShowDialog();
                return;

            }

            string qType = comboBox1.SelectedItem.ToString();



            //ask the user to specify a dir to copy to (e.g. on a memory stick)

            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "Select the folder to copy data files to";

            string toDataDir;



            // Show the FolderBrowserDialog.
            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                toDataDir = folderBrowserDialog.SelectedPath;


                string formDir = dataDir + "\\" + qType;
                string fileName;
                string finalDataSourcePath; //location to copy from
                string finalDataDestPath;   //location to copy to
                string partDirName;
                string partID;
                string finalDataFileName;

                //get all dirs in this Questionnaire type
                DirectoryInfo di = new DirectoryInfo(formDir);

                foreach (DirectoryInfo partDir in di.GetDirectories())
                {

                    //capture the participant ID from the dir name
                    partDirName = partDir.Name;

                    Match match = Regex.Match(partDirName, @"^participant_data_(.+)$");

                    if (match.Success)
                    {

                        partID = match.Groups[1].Value;
                        finalDataFileName = "final_data_" + partID + ".txt";

                    }
                    else
                    {
                        //ignore
                        continue;

                    }




                    //get a list of files in this dir
                    foreach (FileInfo file in partDir.GetFiles())
                    {
                        fileName = file.Name;

                        //is this the final data file?
                        if (fileName == finalDataFileName)
                        {
                            //copy text data
                            finalDataSourcePath = file.FullName;
                            //finalDataDestPath = toDataDir + "\\" + fileName;

                            finalDataDestPath = toDataDir + "\\" + "participant_data_" + partID + "_" + qType + ".txt";


                            copyFile(finalDataSourcePath, finalDataDestPath);

                        }
                        else
                        {

                            //is this an audio file?

                            match = Regex.Match(fileName, @"^audioRecording(\d+)\.mp3$");

                            if (match.Success)
                            {

                                //copy text data
                                finalDataSourcePath = file.FullName;
                                finalDataDestPath = toDataDir + "\\audioRecording_" + partID + "_" + match.Groups[1].Value + "_" + qType + ".mp3";
                                copyFile(finalDataSourcePath, finalDataDestPath);


                            }




                        }


                    }


                }

                stdMessageBox.setMainLabel("Copying Finished");
                stdMessageBox.ShowDialog();

            }





        }

        private void copyFile(string sourceFile, string destFile)
        {
            try
            {

                File.Copy(sourceFile, destFile, true);

            }
            //ignore problems where the source file does not exist, which can sometimes happen for legitimate reasons
            catch (FileNotFoundException e2) { }

            catch (Exception e3)
            {

                errorBox.setLabel("Error: Could not copy file:" + e3);
                errorBox.ShowDialog();
                return;

            }


        }


        /*

        private void button10_Click(object sender, EventArgs e)
        {

            //start the GPS logger

            //check its not already running or in use by the Questionnaire window
            if (loggerStatus == "on")
            {

                errorBox.setLabel("Error: The logger is already in use");
                errorBox.ShowDialog();
                return;


            }


            if (loggerStatus == "suspended")
            {

                errorBox.setLabel("Error: The GPS connection is already in use (Questionnaire Window)");
                errorBox.ShowDialog();
                return;


            }

            if (qFormIsOpen)
            {
                errorBox.setLabel("Error: The GPS connection is already in use (Questionnaire Window)");
                errorBox.ShowDialog();
                return;


            }

            //check that the data directory is set and exists

            if (dataDir == null || (!Directory.Exists(dataDir)))
            {
                errorBox.setLabel("Error: Please set the data directory (settings)");
                errorBox.ShowDialog();
                return;

            }



            portNum = (string)comboBox1.SelectedItem;
            baudRate = (string)comboBox2.SelectedItem;

            if ((portNum == null) || (baudRate == null))
            {

                //show fail
                warningMessageBox.setLabel("Warning: Please select Port and Baud Rate");
                warningMessageBox.ShowDialog();


            }
            else
            {
                //test serial port connection

                serialPort1.PortName = portNum;

                //comm port for laptop
                //serialPort1.PortName = "COM4";

                serialPort1.BaudRate = Convert.ToInt32(baudRate);

                //start the timer which will update the GPS data each half second
                timer1.Enabled = true;


                //try and open the serial port for GPS comms
                try
                {
                    serialPort1.Open();

                    label11.Text = "Starting...";
                    label12.Text = "Starting...";

                    loggerStatus = "on";






                }
                catch
                {

                    //show warning screen
                    timer1.Enabled = false;

                    warningMessageBox.setLabel("Warning: Can't open serial port connection to the GPS unit. Make sure the unit is plugged in.");
                    warningMessageBox.ShowDialog();

                }


            }


        }
         * 
         */






        private void writeLog(string data)
        {
            //check the logfile:

            string logfile = dataDir + "\\gps_logger.txt";

            //append to log file
            if (!File.Exists(logfile))
            {


                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(logfile))
                    try
                    {
                        //StreamWriter sw = File.CreateText(logfile);
                        sw.WriteLine(data);
                    }
                    catch (Exception ee)
                    {
                        MessageBox.Show(ee.StackTrace);

                    }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(logfile))
                {
                    sw.WriteLine(data);



                }



            }




        }




        /*
        private void button11_Click(object sender, EventArgs e)
        {
            //stop logging

            //disable the timer
            timer1.Enabled = false;

            //close the serial port
            if (serialPort1.IsOpen)
            {
                serialPort1.Close();

            }

            label11.Text = "disconnected";
            label12.Text = "disconnected";

            loggerStatus = "off";



        }
         */

        private void button12_Click(object sender, EventArgs e)
        {
            //copy GPS logging data.

            //make sure logging is disabled

            if (loggerStatus == "on")
            {
                warningMessageBox.setLabel("Warning: Turn off logging before copying data");

                warningMessageBox.ShowDialog();

                return;



            }

            //list of GPSposition objects
            List<GPSposition> posList = new List<GPSposition>();


            //read the data into memory
            string logfile = dataDir + "\\gps_logger.txt";

            char[] splitChars = { '\t' };

            if (File.Exists(logfile))
            {
                StreamReader dh = null;
                StreamWriter sr = null;

                try
                {

                    //read the data from the logfile

                    dh = new StreamReader(logfile);

                    while (dh.EndOfStream == false)
                    {
                        string line = dh.ReadLine();

                        //ignore unavaiable lines
                        if (line != "unavailable")
                        {

                            string[] items = line.Split(splitChars);

                            //first will be latitude, second will be longitude
                            posList.Add(new GPSposition(items[0], items[1]));


                        }



                    }

                    dh.Close();

                    //ask the user for a file to write the kml to

                    string saveToFile = null;

                    SaveFileDialog sfd = new SaveFileDialog();

                    //must be a kml file
                    sfd.Filter = "kml files (*.kml)|*.kml";

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {

                        saveToFile = sfd.FileName;



                    }


                    //write the KML
                    sr = new StreamWriter(saveToFile);

                    //KML headers

                    sr.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    sr.WriteLine("<kml xmlns=\"http://www.opengis.net/kml/2.2\">");
                    sr.WriteLine("<Document>");
                    sr.WriteLine("<name>Points with TimeStamps</name>");

                    //icons
                    sr.WriteLine("<Style id=\"hiker-icon\">");
                    sr.WriteLine("<IconStyle>");
                    sr.WriteLine("<Icon>");
                    sr.WriteLine("<href>http://maps.google.com/mapfiles/ms/icons/hiker.png</href>");
                    sr.WriteLine("</Icon>");
                    sr.WriteLine("<hotSpot x=\"0\" y=\".5\" xunits=\"fraction\" yunits=\"fraction\"/>");
                    sr.WriteLine("</IconStyle>");
                    sr.WriteLine("</Style>");

                    //placemarks

                    foreach (GPSposition pos in posList)
                    {

                        sr.WriteLine(pos.getXMLposition("#hiker-icon"));

                    }


                    // final elements

                    sr.WriteLine("</Document></kml>");





                }
                catch (Exception ex)
                {

                    errorBox.setLabel("Error: Could not load log data");
                    errorBox.ShowDialog();

                    MessageBox.Show(ex.Message + ": " + ex.StackTrace);




                }

                finally
                {

                    if (dh != null)
                    {
                        dh.Close();


                    }

                    if (sr != null)
                    {

                        sr.Close();

                    }

                }





            }
            else
            {

                warningMessageBox.setLabel("Warning: No logging data was found.");

                warningMessageBox.ShowDialog();


            }






        }

        /*

        public void checkCompleteness(bool lockingEnabled, bool newUser, string xmlFilePath, Qconfig config)
        {
            //locking should only be enabled if this is called from the baseform, but not from the Qform

            //check the completeness of the selected questionnaire
            //do we have a questionnaire open already ?

            

            Participant selectedPart = null;

            if (newUser)
            {

                //we might be using a new participant, which won't be selected or even in this list

                
                    //update the question list
                    updateExistingQuestionList(null);

                    //get the participant from the list
                    foreach (Participant part in listBox2.Items)
                    {

                        //MessageBox.Show(part.getID());


                        if (openParticipantID == part.getID())
                        {
                            selectedPart = part;
                            break;



                        }

                    }

                    if (selectedPart == null)
                    {
                        //something has gone wrong
                        errorBox.setLabel("Error: Can't show status: " + openParticipantID);
                        errorBox.ShowDialog();
                        return;



                    }



            }
            else
            {

                selectedPart = (Participant)listBox2.SelectedItem;

                if (selectedPart == null)
                {

                    errorBox.setLabel("Error: Please select a Participant from the list");
                    errorBox.ShowDialog();
                    return;


                }



            }



            if (dataDir == null)
            {
                errorBox.setLabel("Error: Please set the data directory (settings)");
                errorBox.ShowDialog();
                return;

            }


            //open the form


            //has the user selected a language and participant ID
            




            //participantID
            string partID = selectedPart.getID();



            //check that this participant does not already exist, i.e. that the new datadir does not exist

            string partDataDir = selectedPart.getPath();


            //open the form
            currentSurveyComp = new CompletenessForm(lockingEnabled);




            try
            {

                //start the survey
                currentSurveyComp.startCheck(xmlFilePath, partDataDir, partID, this, config);

                currentSurveyComp.Show();

                //qFormIsOpen = true;

                //openParticipantID = partID;




            }
            catch (ObjectDisposedException e2)
            {

                //something went wrong with startup, e.g. XML parsing exception

                //qFormIsOpen = false;


            }




        }
*/



        private void button13_Click(object sender, EventArgs e)
        {
            //lock selected Q.


            lockQuestionnaire();





        }

        private void button4_Click_1(object sender, EventArgs e)
        {

            //delete any data present in the log file by deleting the file.
            string logfile = dataDir + "\\gps_logger.txt";

            //check that the data directory is set and exists

            if (dataDir == null || (!Directory.Exists(dataDir)))
            {
                errorBox.setLabel("Error: Please set the data directory (settings)");
                errorBox.ShowDialog();
                return;

            }

            //is the user sure ?



            confirmForm.setFormLabel("Are you sure you want to delete all previous locations ?", this);
            confirmForm.ShowDialog();

            if (confirmResult == "yes")
            {
                //delete the file
                try
                {
                    File.Delete(logfile);


                }
                catch
                {

                    errorBox.setLabel("Error: Could not delete locations");
                    errorBox.ShowDialog();
                    return;

                }





            }





        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {



        }


        private List<string> getLanguagesForFormType(string qType)
        {

            //look up the config object
            Qconfig config = configMap[qType];

            return config.getLanguageList();




        }











        /*
        private List<string> getLanguagesForFormType(string qType){

            //read these from the config file

            //fetch the config file for this Qtype and find the languages that are supported
            string configFilePath = dataDir + "\\" + "EQ_" + qType + ".conf";

            //open this file and fetch each lang line.

            List<string> langList = new List<string>();


            StreamReader reader = null;

            try
            {

                reader = new StreamReader(configFilePath);
                Char[] delim = new Char[] { '~' };

                while (reader.EndOfStream == false)
                {
                    string line = reader.ReadLine();
                    string[] parts = line.Split(delim);

                    if (parts[0] == "language")
                    {

                        //add this as an option in the language box.
                        langList.Add(parts[1]);


                    }


                }



            }
            catch
            {
                errorBox.setLabel("Error: Could not load config file");
                errorBox.ShowDialog();
                return null;


            }
            finally
            {

                if (reader != null)
                {
                    reader.Close();

                }



            }

            return langList;





        }
         */












        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {


            //remove anything already in the language box.
            listBox1.Items.Clear();

            string selectedQtype = (string)comboBox4.SelectedItem;


            List<string> langList = getLanguagesForFormType(selectedQtype);

            foreach (string lang in langList)
            {
                listBox1.Items.Add(lang);

            }

            //show logos
            //delete logos if previous Q was selected

            if (currentConfig != null)
            {
                currentConfig.hideLogos(this);

            }

            Qconfig conf = configMap[selectedQtype];

            currentConfig = conf;

            conf.displayLogos(this);

            //does this form support multiple countries
            if (conf.hasMultipleCountries())
            {
                //show country chooser
                label13.Visible = true;
                comboBox2.Visible = true;

                //remove any previous items
                comboBox2.Items.Clear();

                //populate countries
                comboBox2.Items.AddRange(conf.getCountryNames());


            }
            else
            {

                //remove country chooser
                label13.Visible = false;
                comboBox2.Visible = false;



            }



        }


        public void addLogo(PictureBox pb)
        {

            tabControl1.TabPages[0].Controls.Add(pb);




        }

        public void removeLogo(PictureBox pb)
        {

            tabControl1.TabPages[0].Controls.Remove(pb);



        }


        private void listBox2_SelectedValueChanged(object sender, EventArgs e)
        {

            //when user clicks on existing form:user

            //get the form type

            Participant part = (Participant)listBox2.SelectedItem;

            if (part == null)
            {
                return;

            }

            string qType = part.qType;
            string partID = part.getID();

            //what languages are available?

            List<string> langList = getLanguagesForFormType(qType);

            //add these to the select box.
            comboBox5.Items.Clear();

            foreach (string lang in langList)
            {
                comboBox5.Items.Add(lang);


            }

            //what is the language that was last used with this Q?
            //get from the config file.

            string currentLang = getAttributeFromParticipantConfig(dataDir + "\\" + qType + "\\participant_data_" + partID + "\\questionnaire_config.txt", "language");

            //MessageBox.Show("current lang:" + currentLang);

            //set the selected language
            comboBox5.SelectedItem = currentLang;














        }

        /*
        private string getSelectedLangFromConfig(string configFilePath)
        {

            //read this file and get the language setting
            StreamReader reader = null;

            try
            {

                reader = new StreamReader(configFilePath);
                Char[] delim = new Char[] { '~' };

                while (reader.EndOfStream == false)
                {
                    string line = reader.ReadLine();
                    string[] parts = line.Split(delim);

                    if (parts[0] == "language")
                    {

                        return parts[1];

                    }

                }


            }
            catch
            {
                errorBox.setLabel("Error: Could not load config file");
                errorBox.ShowDialog();
                return null;


            }
            finally
            {

                if (reader != null)
                {
                    reader.Close();

                }



            }

            return null;




        }
         */ 



        private string getAttributeFromParticipantConfig(string configFilePath, string attrName)
        {

            //read this file and get the language setting
            StreamReader reader = null;

            try
            {

                reader = new StreamReader(configFilePath);
                Char[] delim = new Char[] { '~' };

                while (reader.EndOfStream == false)
                {
                    string line = reader.ReadLine();
                    string[] parts = line.Split(delim);

                    if (parts[0] == attrName)
                    {

                        return parts[1];

                    }

                }


            }
            catch
            {
                errorBox.setLabel("Error: Could not load config file");
                errorBox.ShowDialog();
                return null;


            }
            finally
            {

                if (reader != null)
                {
                    reader.Close();

                }



            }

            return null;




        }





        private void button4_Click_2(object sender, EventArgs e)
        {

            if (dataDir == null)
            {
                errorBox.setLabel("Error: Please set the data directory (settings)");
                errorBox.ShowDialog();
                return;

            }


            string audioFile = dataDir + "\\" + rawAudioFile;


            //record user for 3 sec
            ar = new AudioRecordingTest(audioFile);

            ar.startRecording();

            label11.Text = "Recording";
            label11.Visible = true;

            testTimer.Enabled = true;
            testTimer.Start();



        }

        private void timer_Tick(object sender, EventArgs e)
        {

            //stop the timer
            testTimer.Stop();

            ar.stopRecording();

            label11.Text = "Playing";


            //play file


            AudioPlayer ap = new AudioPlayer(dataDir + "\\" + rawAudioFile, this);
            ap.playFile();

            //label11.Visible = false;
            //label11.Text = "Recording";



        }


        public void playbackFinished()
        {

            label11.Visible = false;

        }

        private void BaseForm_FormClosing(object sender, FormClosingEventArgs e)
        {

            testTimer.Stop();
            testTimer.Dispose();






        }

        private void button7_Click_1(object sender, EventArgs e)
        {

            //delete all Questionnaires

            confirmForm.setFormLabel("Are you sure you want to delete ALL DATA?", this);
            confirmForm.ShowDialog();

            if (confirmResult == "yes")
            {
                //make sure none are open first.
                if (qFormIsOpen)
                {

                    errorBox.setLabel("Error: You must close an open Questionnaire before you can delete.");
                    errorBox.ShowDialog();
                    return;


                }

                if (dataDir != null)
                {

                    //does the base dir still exist, i.e. its possible that the user has moved or deleted it.
                    if (Directory.Exists(dataDir))
                    {


                        DirectoryInfo di = new DirectoryInfo(dataDir);
                        DirectoryInfo diQ = null;


                        string fileName;
                        string qType;   //questionnaire type
                        string qDir;    //dir of the specific Qtype
                        string debug = "";

                        foreach (FileInfo file in di.GetFiles())
                        {
                            fileName = file.Name;

                            Match match = Regex.Match(fileName, "^EQ_(.+).xml$");

                            if (match.Success)
                            {

                                qType = match.Groups[1].Value;
                                qDir = dataDir + "\\" + qType;

                                if (Directory.Exists(qDir))
                                {

                                    //get the dir with the same name
                                    diQ = new DirectoryInfo(qDir);



                                    //delete all the Dirs/files inside this Dir.
                                    foreach (DirectoryInfo dir in diQ.GetDirectories())
                                    {


                                        try
                                        {


                                            Directory.Delete(dir.FullName, true);

                                            //debug += "Deleting dir:" + dir.FullName + "\n";

                                        }
                                        catch (Exception ex)
                                        {
                                            //errorBox.setLabel("Error: Directory " +  dir.FullName + " could not be deleted");
                                            errorBox.setLabel("Error: " + ex);
                                            errorBox.ShowDialog();

                                        }



                                    }



                                }



                            }

                        }


                        //MessageBox.Show(debug);


                        //clear the existing question list
                        listBox2.Items.Clear();



                    }
                    else
                    {
                        warningMessageBox.setLabel("Warning: The Data Directory has been changed or not set: please update the settings");
                        warningMessageBox.ShowDialog();


                    }



                }



            }



        }

        private void button10_Click(object sender, EventArgs e)
        {

            Participant selectedPart = (Participant)listBox2.SelectedItem;

            if (selectedPart == null)
            {

                errorBox.setLabel("Error: Please select a Participant from the list");
                errorBox.ShowDialog();
                return;



            }

            string qType = selectedPart.qType;
            string partID = selectedPart.getID();

            //show status of the selected Q, i.e. display the completion state.

            //get the config file
            string partDataDir = dataDir + "\\" + qType + "\\participant_data_" + partID;
            string userConfigFileName = partDataDir + "\\questionnaire_config.txt";



            //read the completion data from the config file
            Dictionary<string, bool> compDict = Utils.readCompletionData(userConfigFileName);


            //pass to the completion window
            Utils.displayCompletionWindow(compDict);





        }









    }
}
