using StardewModdingAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FelixDev.StardewMods.FeTK.Framework.Services
{
    interface IMailManager
    {
        void Add(string modId, string mailId, SDate arrivalDay);

        void RegisterMailSender(string modId, IMailSender mailSender);

        bool HasMailInMailbox(string modId, string mailId);
    }
}
