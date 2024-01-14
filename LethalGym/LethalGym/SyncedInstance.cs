using LethalLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Unity.Netcode;

namespace LethalGym
{
    [Serializable]
    public class SyncedInstance<T>
    {
        public static CustomMessagingManager MessageManager => NetworkManager.Singleton.CustomMessagingManager;
        public static bool IsClient => NetworkManager.Singleton.IsClient;
        public static bool IsHost => NetworkManager.Singleton.IsHost;

        [NonSerialized] protected static int IntSize = 4;
        [NonSerialized] static readonly DataContractSerializer serializer = new(typeof(T));

        internal static T Default { get; private set; }
        internal static T Instance { get; private set; }

        internal static bool Synced;

        [NonSerialized]
        static int MAX_BUFFER_SIZE = 1300;

        protected void InitInstance(T instance, int maxSize = 1024)
        {
            Default = instance;
            Instance = instance;

            // Ensures the size of an integer is correct for the current system.
            IntSize = sizeof(int);

            // Limit to size of a single packet where fragmenting is required.
            if (maxSize < 1300)
            {
                MAX_BUFFER_SIZE = maxSize;
            }
        }

        internal static void SyncInstance(byte[] data)
        {
            Instance = DeserializeFromBytes(data);
            Synced = true;
        }

        internal static void RevertSync()
        {
            Instance = Default;
            Synced = false;
        }

        public static byte[] SerializeToBytes(T val)
        {
            using MemoryStream stream = new();

            try
            {
                serializer.WriteObject(stream, val);
                return stream.ToArray();
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError($"Error serializing instance: {e}");
                return null;
            }
        }

        public static T DeserializeFromBytes(byte[] data)
        {
            using MemoryStream stream = new(data);

            try
            {
                return (T)serializer.ReadObject(stream);
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError($"Error deserializing instance: {e}");
                return default;
            }
        }

        internal static void SendMessage(string label, ulong clientId, FastBufferWriter stream)
        {
            bool fragment = stream.Capacity > MAX_BUFFER_SIZE;
            NetworkDelivery delivery = fragment ? NetworkDelivery.ReliableFragmentedSequenced : NetworkDelivery.Reliable;

            if (fragment)
            {
                Plugin.Logger.LogDebug(
                    $"Size of stream ({stream.Capacity}) was past the max buffer size.\n" +
                    "Config instance will be sent in fragments to avoid overflowing the buffer."
                );
            }

            MessageManager.SendNamedMessage(label, clientId, stream, delivery);
        }
    }
}
