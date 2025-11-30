namespace TestChatSignalR.Contracts
{
   // public record class UserConnection(int UserId, int ChatId);
   public class UserConnection
   {
       public int UserId { get; set; }
       public int ChatId { get; set; }
       public string ConnectionId { get; set; }=string.Empty;
    }

}
