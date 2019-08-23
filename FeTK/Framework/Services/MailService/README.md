# Mail API Overview

The Mail API of the framework exposes a couple of features modders can use:
* Easy and powerful way to add mails to the game
* updating the mail content and get feedback on how the player interacted with the mail content
* More visualization options for the mail's textual content

Note: Not every feature requires mails to be added to the game via the framework!

## Easy and powerful way to add mails to the game
The Mail API provides an easy and powerful way to add mails to the game:
* Mails can be added to the player's mailbox either *instantly* or at the *begin* of a new day.
* Mail IDs are *unique* to each mod. Multiple mods can add mails with the same IDs without any ID conflicts arrising between those mods.

Let's take a first look at code adding a mail to the game.
```cs
private void AddMailExample()
{
   // Obtain an exclusive mail service for this mod.
   IMailService mailService = ServiceFactory.GetFactory("YourModID").GetMailService();

   // Create a mail to send to the player. Here we create a mail with some text content and an attached axe.
   var mail = new ItemMail("MyItemMailID", "Some text content", new Axe());

   // Send the mail to the player. It will arrive in the player's mailbox at the beginning of the next day. 
   mailService.AddMail(mail, 1 /* mail for tomorrow */);
}
```
Let's dissect the code and talk about the different parts in more detail.

First off, obtain an ```IMailService``` instance unique to your mod. 
```cs
IMailService mailService = ServiceFactory.GetFactory("YourModID").GetMailService();
```
We will use this instance to add mails to the game in a mod-isolated way, that is multiple mods can add mails with the _same_ ID to the game 
without the risk of conflicts.

Next, we create an instance of the mail we want to send to the player.
```cs
var mail = new ItemMail("MyItemMailID", "Some text content", new Axe());
```
Here, we create an instance of an ```ItemMail```. ```MyItemMailID``` is the ID of our mail, ```Some text content``` is the text content of the mail and ```new Axe()``` is the attached item to the mail.

Finally, we tell our previously obtained mail service to add the mail to the game.
```cs
mailService.AddMail(mail, 1 /* mail for tomorrow */);
```
With the above line, we specify that our above created item mail will arrive in the player's mailbox in the morning of the next day. If, instead, we want to *instantly* send the mail we would have to write:
```cs
mailService.AddMail(mail, 0 /* instant mailbox arrival */);
```
Please note that a mod cannot have multiple mails with the same ID scheduled for the same day in the player's mailbox *at the same time*.

### Supported Mail Types
Below is a list of all supported mail types:

| Mail Type       | Description                                             | Remarks                                                     |
|:---------------:|---------------------------------------------------------| ----------------------------------------------------------- |
| Mail            | Mail with text content only.                            | Text content can be the _empty_ string (for all mail types).|
| ItemMail        | Mail with text content and zero or more attached items. |                                                             |
| MoneyMail       | Mail with text content and attached money.              | Supported currencies: *Money*, *Star Tokens*, *Qi coins*    |
| QuestMail       | Mail with text content and zero or one attached quest.  | Attached quest can be accepted *automatically* or *manually.*  |
| RecipeMail      | Mail with text content and zero or one attached recipe. | Supported recipe types: *Cooking*, *Crafting*               |

## Update Mail Content and receive Player Interaction feedback
This framework provides two events consuming mods can use to update mail content before the mail is actually shown and to receive feedback about how the player interacted with the mail content when the mail has been closed. These events are exposed by the `IMailService` and are named `MailOpening` and `MailClosed`. Let's take a closer look:

### Mail-Opening Event
The mail-opening event is raised when the mail is about to be displayed to the player. It allows to change the mail's content (such as text, attached items/money/quest/recipe). Here is some example code:
```cs
private void BirthdayMailExample()
{
   IMailService mailService = ServiceFactory.GetFactory("YourModID").GetMailService();

   // Add an event handler for the mail-opening event.
   mailService.MailOpening += OnMailOpening;

   // Create a mail with an attached birthday cake.
   var birthdayCake = new SObject(Vector2.Zero, 221 /* pink cake ID */, 1);
   var mail = new ItemMail("BirthdayMail", "Hey Player!^^Here is a birthday cake for you to enjoy :)^Happy Birthday!", birthdayCake);

   mailService.AddMail(mail, 1);
}

private void OnMailOpening(object sender, MailOpeningEventArgs e)
{
   // Only proceed to change the mail content when the closed mail is our birthday cake mail.
   if (e.Id == "BirthdayMail")
   {
       // If the cake has already been attached to the mail for two days or longer we replace it with a trash item
       // (cake is now wasted).
       if (SDate.Now() >= e.ArrivalDay.AddDays(2))
       {
           // Get the changeable mail content for a mail.
           var itemMailContent = (ItemMailContent)e.Content;
           
           // Create the new trash item replacing our original birthday cake.
           var trash = new SObject(Vector2.Zero, 168 /* trash item ID */, 1);

           // Replace the attached birthday cake item with a "wasted cake" and add
           // some explanation message to the mail's text content for the player.
           itemMailContent.Text += "^^(Unfortunately, two or more days have passed since you have received this cake...the cake is now wasted!)";
           itemMailContent.AttachedItems = new List<Item>() { trash };
       }
   }
}
```
What does this code do? It sends the following mail to the player to celebrate their birthday:

![](../../../docs/images/mail-service-mail-opening-example-1.png)

When the player opens the mail from the mailbox, we check how much time has passed since the mail has been added to the player's mailbox, in other words, how long the cake has been wasting away attached to the mail without being put in a freezer. If two or more days have since passed when the player opens said mail, we update the mail content to replace the birthday cake with a now wasted cake. The result is the following:

![](../../../docs/images/mail-service-mail-opening-example-2.png)

Now let's dissect the code again:
As seen previously, we first obtain a mail service for our mod to use. Once obtained, we add a listener to the `MailOpening` event:
```cs
// Add an event handler for the mail-opening event.
mailService.MailOpening += OnMailOpening;
```
We then proceed to add a mail to the game as usual. The interesting part of this examaple happens in our specified event listener `OnMailOpening`:

First off, we check if the ID of the closed mail matches the ID we gave our birthday mail.
```cs
// Only proceed to change the mail content when the closed mail is our birthday cake mail.
if (e.Id == "BirthdayMail")
{
```
The `MailClosedEventArgs` exposes the ID of the closed mail in its `Id` property. Now that we know the closed mail is our birthday cake mail, we next have to find out how much time the arrival of this mail in the player's mailbox has since passed before the player actually opened it.
```cs
// If the cake has already been attached to the mail for two days or longer we replace it with a trash item
// (cake is now wasted).
if (SDate.Now() >= e.ArrivalDay.AddDays(2))
{
```
Again, the event data contains just the information we need! Its property `ArrivalDay` contains the actual in-game date when the mail was added to the player's mailbox. In the case that two or more days have since passed, we now proceed to replace the original birthday cake with a wasted version:
```cs
// Get the changeable mail content for a mail.
var itemMailContent = (ItemMailContent)e.Content;

// Create the new trash item replacing our original birthday cake.
var trash = new SObject(Vector2.Zero, 168 /* trash item ID */, 1);

// Replace the attached birthday cake item with a "wasted cake" and add
// some explanation message to the mail's text content for the player.
itemMailContent.Text += "^^(Unfortunately, two or more days have passed since you have received this cake...the cake is now wasted!)";
itemMailContent.AttachedItems = new List<Item>() { trash };
```
The content of a mail which can be changed is provided by the `Content` property of the event data. Since our birthday cake mail is of type `ItemMail`, the changeable content is of type `ItemMailContent`. (For other mail types, the same naming schema is used, i.e. `RecipeMailContent`, `QuestMailContent`,....). In the case of an `ItemMail` both the text content as well as the attatched items of a mail can be changed. That way, we can easily update our birthday cake mail to now contain a wasted birthday cake instead of the originally fresh and tasty cake!

#### Changeable Mail Content
Below is a table describing the mail content which can be changed for each mail type:

| Mail Type       | Changeable Content                                                                               |
|:---------------:|--------------------------------------------------------------------------------------------------|
| Mail            | &bull; Mail Text                                                                                 |
| ItemMail        | &bull; Mail Text <br/> &bull; Attached Items                                                     | 
| MoneyMail       | &bull; Mail Text <br/> &bull; Monetary Value <br/> &bull; Currency of Monetary Value             |
| QuestMail       | &bull; Mail Text <br/> &bull; Quest ID <br/> &bull; Quest Type <br/> &bull; Quest Acception Type |
| RecipeMail      | &bull; Mail Text <br/> &bull; Recipe Name <br/> &bull; Recipe Type                               |


### Mail-Closed Event
The mail-closed event is raised when the player closes a mail. It exposes information about how the player interacted with a mail's content, i.e. what were the attached items the player selected?/did the player accept a quest?, etc....Again, here is some example code: //TODO

## More visualization options for the mail's textual content
The Mail API introduces a **Text Coloring API** to improve the visual representation of a mail's content. This API is available both for mails added via the framework and mails added via other frameworks, such as [Content Patcher](https://github.com/Pathoschild/StardewMods/tree/develop/ContentPatcher).

The Text Coloring API provides a Markup-language based syntax to be used together with the actual mail content and thus requires no programming skills or special compatibility by the used frameworks to add mails to the game.

### Syntax

Enclose the text you want to color with the `<color></color>` tags. See the following example:
```
<color=COLOR_VALUE>some text</color>
```
The string "some text" will be rendered with the color specified by `COLOR_VALUE`. Color values can be specified using the following color representations:
1. A hexadecimal color-code specified as `#RRGGBB`, where the RR (red), GG (green) and BB (blue) hexadecimal integers specify the components of the color. All values must be between 00 (lowest value) and FF (highest value) and the values are *case-insensitive*.
2. A HTML color name. See [this color table](https://htmlcolorcodes.com/color-names/) for a list of all valid color names. Names are *case-insensitive*.

If we want to color the above text "some text" in red, we thus can write it as follows:
```
<color=#FF0000>some text</color>
``` 
-or- 
```
<color=Red>some text</color>
```

Note: Any leading, trailing or inner white-space characters in `COLOR_VALUE` will be ingored.

Every mail content can contain zero or more `<color></color>` tag pairs. Any mail content which is not enclosed by such a pair, will be 
colored in the game's default text color (based on the mail background). You can have multiple `<color></color>` tags side-by-side and you can even use nested `<color>` start tags (`<color>` tags inside other `<color>` tags).

A valid text-coloring syntax thus is defined as the following (instead of a color value in the format `#[A-Fa-f0-9]{6}` a HTML color name can also be used):
```
...<color=#[A-Fa-f0-9]{6}>...</color>...
```
where the three dots `...` can stand for optional text and `<color></color>` tags. If an invalid color value is specified, the default text color will be used (typically the in-game default text color). If there is a mismatch between the color start tags and the color end tags, the text will not be displayed correctly.

### Examples

Below are a couple of examples showcasing different levels of complexity in the use of this API and how they will look in the actual game.

Example 1:
```
"Tomorrow we will all meet at the <color=#0000FF">Town Center</color> to celebrate the end of harvest season!"
```
![](../../../docs/images/mail-service-text-coloring-api-example-1.png)

Example 2:
```
"Some <color=#FF0000>red</color>, some <color=Green>green</color> and some <color=#0000FF>blue</color>."
```
![](../../../docs/images/mail-service-text-coloring-api-example-2.png)

Example 3:
```
"<color=#000000>Some small <color=#C47902>light source</color> surrounded by darkness.</color>"
```
![](../../../docs/images/mail-service-text-coloring-api-example-3.png)

And below is a personal favorite of mine:
```
"A colorful <color=#FF0000>R</color><color=#FF7F00>A</color><color=#FFFF00>I</color><color=#00FF00>N</color>"<color=#0000FF>B</color><color=#3F00FF>O</color><color=#7F00FF>W</color> in a letter."
```
![](../../../docs/images/mail-service-text-coloring-api-example-rainbow.png)

### How to use the API in other mod frameworks?

Below is an example how to use the Text Coloring API in Content Patcher by editing the content of an already existing mail (applying the 
API to a new custom mail will work in the same way):

Create the following content.json file (based on CP 1.9 which was the most recent CP version available at the time of this writing):
```js
{
   "Format": "1.9",
   "Changes": [
    {
      "Action": "EditData",
      "Target": "Data/Mail",
      "Entries": {
        "fall_15": "Dear @,^I just want to remind you that the <color=#0000FF>Stardew Valley Fair</color> is happening tomorrow.^ Don't forget to bring your 9 items for the grange display.^ Remember, the fair starts at 9AM in the town square. See you there!^   -Mayor Lewis"
      }
    }
   ]
}
```
As you can see, we enclosed the words "Stardew Valley Fair" with `<color=#0000FF></color>`. This will result in the following mail content when a player named "Player" will open the mail with the ID "fall_15":

![](../../../docs/images/mail-service-cp-edit-example.png)

As simple as that!
