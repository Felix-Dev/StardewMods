using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FelixDev.StardewMods.FeTK.Framework.Services
{
    /// <summary>
    /// The <see cref="MailExtensions"/> class provides an API to update the content of a <see cref="Mail"/> instance.
    /// </summary>
    internal static class MailExtensions
    {
        /// <summary>
        /// Get the editable content of a mail.
        /// </summary>
        /// <param name="mail">The mail to get its editable content for.</param>
        /// <returns>A <see cref="MailContent"/> instance with the editable content.</returns>
        public static MailContent GetMailContent(this Mail mail)
        {
            switch (mail)
            {
                case ItemMail itemMail:
                    return new ItemMailContent(itemMail.Text, itemMail.AttachedItems);
                case MoneyMail moneyMail:
                    return new MoneyMailContent(moneyMail.Text, moneyMail.AttachedMoney);
                case RecipeMail recipeMail:
                    return new RecipeMailContent(recipeMail.Text, recipeMail.RecipeName);
                case QuestMail questMail:
                    return new QuestMailContent(questMail.Text, questMail.QuestId);
                default:
                    return new MailContent(mail.Text);
            }
        }

        /// <summary>
        /// Update the content of a mail.
        /// </summary>
        /// <param name="mail">The mail to update.</param>
        /// <param name="mailContent">The new content of the mail.</param>
        /// <exception cref="ArgumentNullException">The specified <paramref name="mailContent"/> is <c>null</c>.</exception>
        public static void UpdateMailContent(this Mail mail, MailContent mailContent)
        {
            if (mailContent == null)
            {
                throw new ArgumentNullException(nameof(mailContent));
            }

            switch (mail)
            {
                case ItemMail itemMail:
                    itemMail.Text = mailContent.Text;
                    itemMail.AttachedItems = ((ItemMailContent)mailContent).AttachedItems;
                    break;
                case MoneyMail moneyMail:
                    moneyMail.Text = mailContent.Text;
                    moneyMail.AttachedMoney = ((MoneyMailContent)mailContent).AttachedMoney;
                    break;
                case RecipeMail recipeMail:
                    recipeMail.Text = mailContent.Text;
                    recipeMail.RecipeName = ((RecipeMailContent)mailContent).RecipeName;
                    break;
                case QuestMail questMail:
                    questMail.Text = mailContent.Text;
                    questMail.QuestId = ((QuestMailContent)mailContent).QuestId;
                    break;
                default:
                    mail.Text = mailContent.Text;
                    break;
            }
        }
    }
}
