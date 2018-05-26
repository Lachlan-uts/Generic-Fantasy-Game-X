/**********************************************
* Power Grid Inventory
* Copyright 2015-2016 James Clark
**********************************************/
using UnityEngine;
using System.Collections;

namespace PowerGridInventory.Demo
{
    
    public class PlaySoundEvent : MonoBehaviour
    {
        public AudioSource OutputSource;
        public AudioClip PickupClip;
        public AudioClip DropClip;
        public AudioClip EquipClip;
        public AudioClip UnequipClip;

        public void Pickup(PGISlotItem item, PGIModel model)
        {
            OutputSource.clip = PickupClip;
            OutputSource.Play();
        }

        public void Drop(PGISlotItem item, PGIModel model)
        {
            OutputSource.clip = DropClip;
            OutputSource.Play();
        }

        public void Equip(PGISlotItem item, PGIModel model, PGISlot slot)
        {
            OutputSource.clip = EquipClip;
            OutputSource.Play();
        }

        public void Unequip(PGISlotItem item, PGIModel model, PGISlot slot)
        {
            OutputSource.clip = UnequipClip;
            OutputSource.Play();
        }

        public void BeginDrag(PGISlotItem item, PGIModel model, PGISlot slot)
        {
            OutputSource.clip = UnequipClip;
            OutputSource.Play();
        }

        public void EndDrag(PGISlotItem item, PGIModel model, PGISlot slot)
        {
            OutputSource.clip = EquipClip;
            OutputSource.Play();
        }
    }
}
