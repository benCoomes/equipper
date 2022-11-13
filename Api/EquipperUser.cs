namespace Coomes.Equipper
{
    public class EquipperUser
    {
        public readonly string UserId;
        public readonly string DisplayName; 
        public readonly bool Authenticated;


        internal EquipperUser(string id, string name, bool authenticated) {
          UserId = id;
          DisplayName = name;
          Authenticated = authenticated;
        }
    }

    public class LoggedInUser : EquipperUser {
      public LoggedInUser(string id, string name) : base(id, name, true) 
      { }
    }

    public class AnonymousUser : EquipperUser {
      public AnonymousUser() : base(null, null, false)
      { }
    }
}