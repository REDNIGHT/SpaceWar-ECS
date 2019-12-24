using System;
using Unity.Entities;

namespace RN.Network
{
    public partial struct PlayerActorArray
    {
        public Entity shipEntity;

        public Entity weaponEntity0;
        public Entity weaponEntity1;
        public Entity weaponEntity2;
        public Entity weaponEntity3;
        public Entity weaponEntity4;
        public Entity weaponEntity5;
        public Entity weaponEntity6;

        public Entity assistEntity0;
        public Entity assistEntity1;
        public Entity assistEntity2;

        public int maxCount => 11;

        public Entity this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return shipEntity;
                    //case 1: return shieldWeaponEntity;

                    case 1: return weaponEntity0;
                    case 2: return weaponEntity1;
                    case 3: return weaponEntity2;
                    case 4: return weaponEntity3;
                    case 5: return weaponEntity4;
                    case 6: return weaponEntity5;
                    case 7: return weaponEntity6;

                    case 8: return assistEntity0;
                    case 9: return assistEntity1;
                    case 10: return assistEntity2;
                }

#if ENABLE_UNITY_COLLECTIONS_CHECKS
                throw new IndexOutOfRangeException();
#else
                return Entity.Null;
#endif
            }

            set
            {
                switch (index)
                {
                    case 0: shipEntity = value; return;
                    //case 1: shieldWeaponEntity = value; return;

                    case 1: weaponEntity0 = value; return;
                    case 2: weaponEntity1 = value; return;
                    case 3: weaponEntity2 = value; return;
                    case 4: weaponEntity3 = value; return;
                    case 5: weaponEntity4 = value; return;
                    case 6: weaponEntity5 = value; return;
                    case 7: weaponEntity6 = value; return;

                    case 8: assistEntity0 = value; return;
                    case 9: assistEntity1 = value; return;
                    case 10: assistEntity2 = value; return;
                }

#if ENABLE_UNITY_COLLECTIONS_CHECKS
                throw new IndexOutOfRangeException();
#endif
            }
        }

        //
        public Entity mainActorEntity
        {
            get => shipEntity;
            set => shipEntity = value;
        }


        //
        public const int WeaponMaxCount = 7;
        public Entity GetWeaponEntity(int slotIndex)
        {
            switch (slotIndex)
            {
                case 0: return weaponEntity0;
                case 1: return weaponEntity1;
                case 2: return weaponEntity2;
                case 3: return weaponEntity3;
                case 4: return weaponEntity4;
                case 5: return weaponEntity5;
                case 6: return weaponEntity6;
            }
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            throw new IndexOutOfRangeException();
#else
            return Entity.Null;
#endif
        }
        public void SetWeaponEntity(int slotIndex, Entity weaponEntity)
        {
            switch (slotIndex)
            {
                case 0: weaponEntity0 = weaponEntity; return;
                case 1: weaponEntity1 = weaponEntity; return;
                case 2: weaponEntity2 = weaponEntity; return;
                case 3: weaponEntity3 = weaponEntity; return;
                case 4: weaponEntity4 = weaponEntity; return;
                case 5: weaponEntity5 = weaponEntity; return;
                case 6: weaponEntity6 = weaponEntity; return;
            }
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            throw new IndexOutOfRangeException();
#endif
        }

        //
        public const int AssistWeaponMaxCount = 3;
        public Entity GetAssistWeaponEntity(int slotIndex)
        {
            switch (slotIndex)
            {
                case 0: return assistEntity0;
                case 1: return assistEntity1;
                case 2: return assistEntity2;
            }
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            throw new IndexOutOfRangeException($"slotIndex={slotIndex}");
#else
            return Entity.Null;
#endif
        }
        public void SetAssistWeaponEntity(int slotIndex, Entity assistEntity)
        {
            switch (slotIndex)
            {
                case 0: assistEntity0 = assistEntity; return;
                case 1: assistEntity1 = assistEntity; return;
                case 2: assistEntity2 = assistEntity; return;
            }
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            throw new IndexOutOfRangeException();
#endif
        }



        //
        public Entity curShieldWeaponEntity;
        public Entity shieldWeaponEntity0;
        public Entity shieldWeaponEntity1;
        public Entity shieldWeaponEntity2;


        public const int ShieldWeaponMaxCount = 3;
        public Entity GetShieldWeaponEntity(int slotIndex)
        {
            switch (slotIndex)
            {
                case 0: return shieldWeaponEntity0;
                case 1: return shieldWeaponEntity1;
                case 2: return shieldWeaponEntity2;
            }
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            throw new IndexOutOfRangeException();
#else
            return Entity.Null;
#endif
        }
        public void SetShieldWeaponEntity(int slotIndex, Entity assistEntity)
        {
            switch (slotIndex)
            {
                case 0: shieldWeaponEntity0 = assistEntity; return;
                case 1: shieldWeaponEntity1 = assistEntity; return;
                case 2: shieldWeaponEntity2 = assistEntity; return;
            }
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            throw new IndexOutOfRangeException();
#endif
        }
    }

}
