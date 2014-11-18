Instructions for building the "EQ" program, for running an Electronic Questionnaire. 

This program has been designed for computers/tablets running versions of the Microsoft Windows operating system. It is known to work with Windows XP, Windows 7 and Windows 8. The .Net framework from Microsoft
must be installed on a computer before using EQ. See this link for more information: http://www.microsoft.com/en-gb/download/details.aspx?id=30653

1. Install Microsoft Visual Studio for Windows Desktop (denoted hereafter as "VS". Note: the free "Express" version works as of this writing, i.e. the 2013 version)
2. Open VS and choose File -> Open Project
3. Open file: Projects -> BHS_questionaire_demo -> BHS_questionaire_demo.sln
4. Click "Start" in the main menu and VS should compile and run the project.
5. In the window that appears, click on the "settings" tab, then click the "Set Data Directory" button. Choose the directory where the configuration file (EQuestionnaire_English.xml) is located.
6. Click the "Save Settings" button. This will save the Data Directory location in the Windows Registry.
7. To start a new questionnaire, go to the "New Questionnaire" tab, enter a participant ID and click the "Start" button. 
8. A new window will appear. Click the "Last Question" and "Next Question" buttons to navigate between questions. Answers are saved when the "Next Question" button is clicked.
9. To exit the questionnaire form, close the window. The form can be re-opened for further editing by clicking the "Existing Questionnaire" tab, selecting the questionnaire from the list of participant IDs, then clicking the "Edit Selected Questionnaire" button.
10. Data is stored in the Data Directory. A separate directory is created for each participant with the name: participant_data_English_<participant-id>
11. It is important not to interfere with the files in these directories, as doing so may stop EQ from working properly.
12. There are a number of files in these directories, but the most important from a user perspective is the file "final_data_<participant-id>.txt".
13. This file contains the saved data for the questionnaire. Each line is formatted as <Question-code>\t<answer>

Note that EQ makes use of the following programs/libraries for sound recording:

NAudio: http://naudio.codeplex.com/
Lame: http://lame.sourceforge.net/index.php

The following files should be present in the same directory as the BHS_questionaire_demo.exe file
NAudio.dll (known to work with version 1.6.0.0)
lame.exe (known to work with version 3.99.2.5)
lame_enc.dll

NAudio.dll is included here, but will need to be moved to the Data Directory (see above).
Lame should be downloaded separately and copied to the Data Directory.

The questions shown in a questionnaire are encoded in XML in a file where the file-name contains the questionnaire-name. For each questionnaire there is also a CONF file,
which contains various other kinds of settings, such as supported languages. For example, the questionniare "Example", would have files EQ_Example.xml, EQ_Example.conf, both in the data directory.
The file name must be of the format: EQ_<questionnaire name>.xml, EQ_<questionnaire name>.conf

Check the accompanying files: EQ_EXAMPLE.xml, EQ_EXAMPLE.conf


1. The usage of a range of years within a copyright statement contained within
this distribution should be interpreted as being equivalent to a list of years
including the first and last year specified and all consecutive years between
them. For example, a copyright statement that reads 'Copyright (c) 2005, 2007-
2009, 2011-2012' should be interpreted as being identical to a statement that
reads 'Copyright (c) 2005, 2007, 2008, 2009, 2011, 2012' and a copyright
statement that reads "Copyright (c) 2005-2012' should be interpreted as being
identical to a statement that reads 'Copyright (c) 2005, 2006, 2007, 2008,
2009, 2010, 2011, 2012'."