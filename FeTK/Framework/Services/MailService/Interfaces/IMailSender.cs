using StardewModdingAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FelixDev.StardewMods.FeTK.Framework.Services
{
    internal interface IMailSender : IMailService, IMailObserver
    {
        Mail GetMailFromId(string mailId, SDate arrivalDay);
    }
}
