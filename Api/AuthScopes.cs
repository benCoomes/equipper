using System;

namespace Coomes.Equipper
{
    public class AuthScopes
    {
        public bool ReadPublic { get; set; }
        public bool ReadAll { get; set; }
        public bool ProfileReadAll { get; set; }
        public bool ProfileWrite { get; set; }
        public bool ActivityRead { get; set; }
        public bool ActivityReadAll { get; set; }
        public bool ActivityWrite { get; set; }

        public bool CanSetGear() => ReadPublic && ActivityRead && ActivityWrite;
    }
}