using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGameToolkit
{
    internal class ContactListener
    {
        private List<KeyValuePair<ContactInfo, ContactInfo>> contacts = new List<KeyValuePair<ContactInfo, ContactInfo>>();

        internal ContactListener(ContactManager manager)
        {
            manager.BeginContact += BeginContact;
            manager.EndContact += EndContact;
            manager.PostSolve += PostSolve;
            manager.PreSolve += PreSolve;
        }

        internal bool BeginContact(Contact contact)
        {
            ContactInfo a = new ContactInfo();
            a.Obj = (BaseObject)contact.FixtureA.Body.UserData;
            a.Manifold = contact.Manifold;
            a.Type = ContactType.Begin;

            ContactInfo b = new ContactInfo();
            b.Obj = (BaseObject)contact.FixtureB.Body.UserData;
            b.Manifold = contact.Manifold;
            b.Type = ContactType.Begin;

            contacts.Add(new KeyValuePair<ContactInfo, ContactInfo>(a, b));
            return true;
        }

        internal void EndContact(Contact contact)
        {
            ContactInfo a = new ContactInfo();
            a.Obj = (BaseObject)contact.FixtureA.Body.UserData;
            a.Manifold = contact.Manifold;
            a.Type = ContactType.End;

            ContactInfo b = new ContactInfo();
            b.Obj = (BaseObject)contact.FixtureB.Body.UserData;
            b.Manifold = contact.Manifold;
            b.Type = ContactType.End;

            contacts.Add(new KeyValuePair<ContactInfo, ContactInfo>(a, b));
        }

        internal void PostSolve(Contact contact, ContactVelocityConstraint cvc)
        {
            ContactInfo a = new ContactInfo();
            a.Obj = (BaseObject)contact.FixtureA.Body.UserData;
            a.Manifold = contact.Manifold;
            a.Type = ContactType.PostSolve;

            ContactInfo b = new ContactInfo();
            b.Obj = (BaseObject)contact.FixtureB.Body.UserData;
            b.Manifold = contact.Manifold;
            b.Type = ContactType.PostSolve;

            contacts.Add(new KeyValuePair<ContactInfo, ContactInfo>(a, b));
        }

        internal void PreSolve(Contact contact, ref Manifold oldManifold)
        {
            ContactInfo a = new ContactInfo();
            a.Obj = (BaseObject)contact.FixtureA.Body.UserData;
            a.Manifold = contact.Manifold;
            a.Type = ContactType.PreSolve;

            ContactInfo b = new ContactInfo();
            b.Obj = (BaseObject)contact.FixtureB.Body.UserData;
            b.Manifold = contact.Manifold;
            b.Type = ContactType.PreSolve;

            contacts.Add(new KeyValuePair<ContactInfo, ContactInfo>(a, b));
        }


        internal void NotifyGameObjects()
        {
            foreach (KeyValuePair<ContactInfo, ContactInfo> contact in contacts)
            {
                if (contact.Key.Type == ContactType.Begin && contact.Value.Type == ContactType.Begin)
                {
                    if (contact.Value.Obj.PhysicsEnabled)
                        contact.Key.Obj.OnContactInternal(contact.Value.Obj);
                    if (contact.Key.Obj.PhysicsEnabled)
                        contact.Value.Obj.OnContactInternal(contact.Key.Obj);
                }
                else if (contact.Key.Type == ContactType.End && contact.Value.Type == ContactType.End)
                {
                    if (contact.Value.Obj.PhysicsEnabled)
                        contact.Key.Obj.OnSeperatedInternal(contact.Value.Obj);
                    if (contact.Key.Obj.PhysicsEnabled)
                        contact.Value.Obj.OnSeperatedInternal(contact.Key.Obj);
                }
                else if (contact.Key.Type == ContactType.PreSolve && contact.Value.Type == ContactType.PreSolve)
                {
                    contact.Key.Obj.OnPreSolveInternal(contact.Value);
                    contact.Value.Obj.OnPreSolveInternal(contact.Key);
                }
                else if (contact.Key.Type == ContactType.PostSolve && contact.Value.Type == ContactType.PostSolve)
                {
                    contact.Key.Obj.OnPostSolveInternal(contact.Value);
                    contact.Value.Obj.OnPostSolveInternal(contact.Key);
                }
            }
            contacts.Clear();
        }
    }
}
