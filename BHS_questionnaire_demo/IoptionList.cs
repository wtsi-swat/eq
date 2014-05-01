using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BHS_questionnaire_demo
{
    interface IoptionList
    {

        void addOption(Option op);

        bool testQuestionRefs(Dictionary<string, Question> questionHash);
        






    }
}
