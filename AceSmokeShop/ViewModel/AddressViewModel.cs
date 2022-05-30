using AceSmokeShop.Models;
using System.Collections.Generic;

namespace AceSmokeShop.ViewModel
{
    public class AddressViewModel
    {
        public AddressViewModel()
        {
            AddressList = new List<Addresses>();
            newAddress = new Addresses();
            StateList = new List<State>();
        }

        public List<Addresses> AddressList { get; set; }

        public Addresses newAddress { get; set; } 

        public List<State> StateList { get; set; }

    }
}
