using Unity.Entities;
using UnityEngine;

namespace RN.Network
{
    //
    public abstract class ServerBootstrap : WorldBootstrap
    {
        public static World world { get; protected set; }
        public override void InitializeWorld(World world)
        {
            ServerBootstrap.world = world;
        }
    }

    //
    public abstract class ClientBootstrap : WorldBootstrap
    {
        public int worldIndex;

        //
        public static void SetClientWorldCount(int c)
        {
            worlds = new World[c];
        }
        public static World[] worlds { get; protected set; }

        public override void InitializeWorld(World world)
        {
            worlds[worldIndex] = world;
        }
    }
}