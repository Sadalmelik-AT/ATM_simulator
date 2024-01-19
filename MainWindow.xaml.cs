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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.Entity; 


namespace SilvioBaldwinTsangaCadet_PP1_Projet
{
   
    public partial class MainWindow : Window
    {
        PP1Entities PP1Entities = new PP1Entities(); // entite PP1
        bool ouvert = false; // verifie ouverture de fenetre
        int essaie = 0;// pour sauver le nombre de tentative de connection
        Guichet selectedGuichet;// pour sauver le guichet selectionne
        public MainWindow()
        {
            InitializeComponent();
            btnConnect.IsEnabled = false;// desactive bouton connection
            ouvert = true; // fenetre ouverte
            txtBoxEmail.Focus();//met le focus sur email pour lsa saisie                     
            PP1Entities.Guichets.Load(); //Charge les donnee
            ComboBoxGuichet.DataContext = PP1Entities.Guichets.Local;// etablie le data context
        }

        
        private void txtBoxEmail_TextChanged(object sender, TextChangedEventArgs e) //Modification text dans txtBoxEmail
        {   
            if (PasswordBoxNIP.Password != String.Empty)// Si le mot de passe n'est pas vide
            {
                btnConnect.IsEnabled = true; // active la bouton connecter
            }

            if (txtBoxEmail.Text == String.Empty)// si l'email est vide
            {
                btnConnect.IsEnabled = false;//desactive le bouton connecter
            }
        }

        private void BoxMotPasse_PasswordChanged(object sender, RoutedEventArgs e)//Modification text dans BoxMotPasse
        {
            if (txtBoxEmail.Text != String.Empty)//Si le nom utilisateur n'est pas vide
            {
                btnConnect.IsEnabled = true;// active la bouton connecter
            }

            if (PasswordBoxNIP.Password == String.Empty)
            {
                btnConnect.IsEnabled = false;//desactive le bouton connecter
            }
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {  
            try
            {
                int nip;// pour sauver le string converti
                bool isNipValid = int.TryParse(PasswordBoxNIP.Password, out nip);//convertie en int
                

                if (isNipValid)//si conversion reussite
                {
                    Credentials cred = new Credentials(txtBoxEmail.Text, nip);// creer objet credential

                    //Verifie si un enregistrement existe

                    try
                    {
                        bool isEmailMatching = PP1Entities.Clients.Any(client => client.email == cred.EmailUtilisateur); // verfie email
                        bool isNIPMatching = PP1Entities.Clients.Any(client => client.NIP == cred.NIP);//verifie nip
                        if (!isEmailMatching && !isNIPMatching)//aucune information est correcte
                        {
                            MessageBox.Show("Les informations de connexion ne sont pas reconnues");//Affiche l'erreure

                        }
                        else if (!isEmailMatching)// Email n'existe pas dans DB
                        {
                            MessageBox.Show("L'email n'est pas reconnu");//Affiche l'erreure

                        }
                        else if (isEmailMatching && !isNIPMatching)//email correcte mais nip incorrecte
                        {
                            MessageBox.Show("Le NIP est incorrect");//Affiche l'erreure
                            essaie++;
                            if (essaie == 3)//si le nombre d'essaie atteind 3
                            {
                                try
                                {
                                    MessageBox.Show("Votre compte a ete verouille apres trop d'echec, svp contacter un administrateur");//avertissement compte blocke
                                                                                                                                        //recupere client a partit de l'email
                                    Client clientBlocke = PP1Entities.Clients.
                                        Where(client => client.email == cred.EmailUtilisateur).FirstOrDefault();
                                    clientBlocke.Blocke = true;//block le client
                                    PP1Entities.SaveChanges();//enregistre la modifications
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.Message);
                                }
                                finally
                                {
                                    essaie = 0; //reinitalie nombre d'essaie
                                }
                            }
                        }
                        else //email et NIP existe
                        {
                            try
                            {
                                //recupere le client enregistre
                                Client utilisateur = PP1Entities.Clients.Where(client => client.email == cred.EmailUtilisateur
                                                                            && client.NIP == cred.NIP).FirstOrDefault();
                                if (utilisateur != null && utilisateur.Blocke != true)//Si enregistrement existe et compte pas blocke
                                {
                                    Window1 ClientUI = new Window1(utilisateur, selectedGuichet); // creer la prochaine fenetre
                                    MessageBox.Show($"Bienvenue { utilisateur.Prenom } {utilisateur.Nom }");//Message de bienvenue
                                    ClientUI.Show();// ouvre la fenetre ClientUI
                                    this.Close();//Ferme la fenetre de login
                                    ouvert = false;//fenetre fermee
                                }
                                else if (utilisateur != null && utilisateur.Blocke == true)//Si enregistrement existe et compte est blocke
                                {
                                    MessageBox.Show("Votre compte a ete verouille apres trop d'echec, svp contacter un administrateur");//Affiche message verouillage
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
                else//si conversion echoue
                {
                    MessageBox.Show("Le NIP n'est pas un numero valide");//Affiche l'erreure
                    throw new FormatException("Le NIP n'est pas un numero valide");//lance exception nip n'est pas un chiffre
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);//Affiche l'erreure
            }
        }

        private void ComboBoxGuichet_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedGuichet = ComboBoxGuichet.SelectedItem as Guichet;  //sauve le guichet selectionne dans ComboBox
        }

        private void btnAdmin_Click(object sender, RoutedEventArgs e)
        {
            adminLogin adminLoginUI = new adminLogin();//creer fenetre admin
            adminLoginUI.Show();//ouvre la fenetre admi
            this.Close();//ferme fenetre login
        }
    }
}
