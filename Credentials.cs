using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilvioBaldwinTsangaCadet_PP1_Projet
{
    class Credentials
    {
        public Credentials(string email, int nip)
        {
            EmailUtilisateur = email;
            NIP = nip;

        }
        public Credentials(string email, string mdp)
        {
            EmailUtilisateur = email;
            password = mdp;
        }
        public string EmailUtilisateur { get; set; }
        public int NIP { get; set; }
        public string password { get; set; }
    }
}
