
using FarseerPhysics.Collision;

namespace MonoGameToolkit
{
    public enum ContactType
    {
        Begin,
        End,
        PreSolve,
        PostSolve
    }

    public struct ContactInfo
    {
        public BaseObject Obj { get; internal set; }
        public Manifold Manifold { get; internal set; }
        public ContactType Type { get; internal set; }

    }
}
