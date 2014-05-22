
/*
Copyright (c) 2014 Genome Research Ltd.
Author: Stephen Rice <sr7@sanger.ac.uk>
Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:
1. Redistributions of source code must retain the above copyright notice,
this list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright
notice, this list of conditions and the following disclaimer in the
documentation and/or other materials provided with the distribution.
3. Neither the names Genome Research Ltd and Wellcome Trust Sanger
Institute nor the names of its contributors may be used to endorse or promote
products derived from this software without specific prior written permission.
THIS SOFTWARE IS PROVIDED BY GENOME RESEARCH LTD AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL GENOME RESEARCH LTD OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;



namespace BHS_questionnaire_demo
{
    class QuestionTextRadioButton : QuestionTextRadio
    {

        private string validationButtonCodeLabel;

        private Button button;



        //this is the same as a QuestionTextRadio, except we have a button to perform an extra check
        //constructor
        public QuestionTextRadioButton(Form form, Form bigMessageBox, GlobalStore gs, GlobalStore specialDataStore, QuestionManager qm)
            : base(form, bigMessageBox, gs, specialDataStore, qm)
        {



        }


        public void setValidationButton(string validation)
        {
            validationButtonCodeLabel = validation;

        }

        public override void removeControls()
        {

            //call base
            base.removeControls();

            //remove button
            getQM().getPanel().Controls.Remove(button);
            button.Dispose();




        }

        public override void configureControls(UserDirection direction)
        {
            //call the base
            base.configureControls(direction);

            //add the button
            button = new Button();
            button.Text = "Press to check barcode is correct";

            //set font size
            setFontSize(button);

            int labelXpos = getWidgetXpos();
            int labelYpos = getWidgetYpos();

            button.Location = new Point(labelXpos + 580, labelYpos + 110);
            //button.Size = new Size(250, 50);
            button.AutoSize = true;
            button.BackColor = GlobalColours.mainButtonColour;
            button.Click += new EventHandler(button_click);


            getQM().getPanel().Controls.Add(button);





        }

        public void button_click(object sender, EventArgs e)
        {
            //called when the user clicks the button to check if a barcode is correct.
            if (validationButtonCodeLabel == "TestBlood" || validationButtonCodeLabel == "TestBloodSerum" || validationButtonCodeLabel == "TestBloodEDTA" || validationButtonCodeLabel == "TestBloodNAF")
            {
                //this is the barcode from the serum tube: we need to check that it matches the same group as the master lab barcode
                string typeSuffix="";

                //get barcode from textbox
                string barcode = textbox.Text;

                bool barcodeOK= false;
                string errorMessage= null;



                if (validationButtonCodeLabel == "TestBloodSerum")
                {
                    typeSuffix = "S1";

                }
                else if (validationButtonCodeLabel == "TestBloodEDTA")
                {

                    typeSuffix = "E1";

                }
                else if (validationButtonCodeLabel == "TestBloodNAF")
                {
                    //TestBloodNAF
                    typeSuffix = "G1";


                }

                //Note: for TestBlood, there is no typesuffix, i.e. the barcode must match the master barcode exactly.

                string masterBarCode = getGS().Get("BLOODMASTER");

                if (masterBarCode == null)
                {

                   

                    errorMessage = "Warning: Can't compare with the master barcode, which was not entered";
                    barcodeOK = false;



                }
                else
                {

                    //e.g. master BGZ100
                    //the serum tube should be <master>S1

                    if (string.IsNullOrWhiteSpace(barcode))
                    {
                        //no entry

                        errorMessage = "Please scan a barcode";
                        barcodeOK = false;


                    }
                    else
                    {
                        //compare with master
                        string expectedBarcode = masterBarCode + typeSuffix;

                        if (expectedBarcode == barcode)
                        {
                            barcodeOK = true;

                        }
                        else
                        {
                            barcodeOK = false;
                            errorMessage = "You have entered the wrong barcode";


                        }



                    }

                }

                if (barcodeOK)
                {

                    Form3 warningBox = getQM().getWarningBox();

                    warningBox.setLabel("Barcode is Correct");
                    warningBox.ShowDialog();

                }
                else
                {

                    Form3 warningBox = getQM().getWarningBox();

                    warningBox.setLabel(errorMessage);
                    warningBox.ShowDialog();

                    //wipe the textbox contents
                    textbox.Text = "";
                    textbox.Focus();

                }


            }


        }
    }



}
