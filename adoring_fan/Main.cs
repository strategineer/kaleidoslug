using XRL;
using XRL.Core;
using XRL.World;

[PlayerMutator]
public class MyPlayerMutator : IPlayerMutator
{
    public void mutate(GameObject player)
    {
        // modify the player object when a New Game begins
        // for example, add a custom part to the player:
        //player.AddPart<MyCustomPart>();
        XRL.Messages.MessageQueue.AddPlayerMessage("Hello, Im a mutate message!");
    }
}

[HasCallAfterGameLoadedAttribute]
public class MyLoadGameHandler
{
    [CallAfterGameLoadedAttribute]
    public static void MyLoadGameCallback()
    {
        // Called whenever loading a save game
        GameObject player = XRLCore.Core?.Game?.Player?.Body;
        if (player != null)
        {
            //player.RequirePart<MyCustomPart>(); //RequirePart will add the part only if the player doesn't already have it. This ensures your part only gets added once, even after multiple save loads.
            XRL.Messages.MessageQueue.AddPlayerMessage("Hello, Im a message!");
        }
    }
}