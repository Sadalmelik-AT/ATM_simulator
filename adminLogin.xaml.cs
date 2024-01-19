using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SilvioBaldwinTsangaCadet_PP1_Projet
{
    /// <summary>
    /// Interaction logic for adminLogin.xaml
    /// </summary>
    public partial class adminLogin : Window
    {
        bool ouvert = false; // verifie ouverture de fenetre
        PP1Entities PP1Entities = new PP1Entities(); // entite PP1
        public adminLogin()
        {
            InitializeComponent();
            btnConnect.IsEnabled = false;//desactive le bouton connecter
        }

        private void txtBoxEmail_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (PasswordBox.Password != String.Empty)// Si le mot de passe n'est pas vide
            {
                btnConnect.IsEnabled = true; // active la bouton connecter
            }

            if (txtBoxEmail.Text == String.Empty)// si l'email est vide
            {
                btnConnect.IsEnabled = false;//desactive le bouton connecter
            }
        }

        private void BoxMotPasse_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (txtBoxEmail.Text != String.Empty)//Si le nom utilisateur n'est pas vide
            {
                btnConnect.IsEnabled = true;// active la bouton connecter
            }

            if (PasswordBox.Password == String.Empty)
            {
                btnConnect.IsEnabled = false;//desactive le bouton connecter
            }
        }
        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Credentials cred = new Credentials(txtBoxEmail.Text, PasswordBox.Password);// creer objet credential

                //Verifie si un enregistrement existe

                bool isEmailMatching = PP1Entities.Admins.Any(admin => admin.email == cred.EmailUtilisateur);//verifie email
                bool isPasswordMatching = PP1Entities.Admins.Any(admin => admin.MotDePasse == cred.password);//verifie mot de passe
                if (!isEmailMatching && !isPasswordMatching)//aucune information est correcte
                {
                    MessageBox.Show("Les informations de connexion ne sont pas reconnues");//Affiche l'erreure

                }
                else if (!isEmailMatching)// Email n'existe pas dans DB
                {
                    MessageBox.Show("L'email n'est pas reconnu");//Affiche l'erreure


                }
                else if (isEmailMatching && !isPasswordMatching)//email correcte mais nip incorrecte
                {
                    MessageBox.Show("Le mot de passe est incorrect");//Affiche l'erreure

                }
                else //email et password existe
                {
                    try
                    {
                        //recupere le client enregistre
                        Admin administrateur = PP1Entities.Admins.Where(admin => admin.email == cred.EmailUtilisateur
                                                                     && admin.MotDePasse == cred.password).FirstOrDefault();
                        if (administrateur != null)//Si administrateur existe
                        {
                            string message = $"Bienvenue { administrateur.Prenom } {administrateur.Nom}";
                            MessageBox.Show(message);//Message de bienvenue
                            adminUI adminUI = new adminUI(administrateur); // creer la prochaine fenetre

                            adminUI.Show();// ouvre la fenetre ClientUI
                            this.Close();//Ferme la fenetre de login
                            ouvert = false;//fenetre fermee
                        }
                        else
                        {
                            MessageBox.Show("Compte administrateur supprime ou innexistant");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

    }
}
