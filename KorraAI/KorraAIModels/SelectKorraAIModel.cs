using Companion.KorraAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


//different usings depending on the model

namespace Companion.KorraAI.Models
{
    public static class SelectKorraAIModel
    {
        public static IKorraAIModel GetModel()
        {
            return new Companion.KorraAI.Models.Joi.JoiModel();
            //return new Companion.KorraAI.Models.April.AprilModel();
        }
    }
}
