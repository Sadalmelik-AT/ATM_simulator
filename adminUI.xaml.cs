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
using System.Text.RegularExpressions;

namespace SilvioBaldwinTsangaCadet_PP1_Projet
{
    /// <summary>
    /// Interaction logic for adminUI.xaml
    /// </summary>
    /// 
    public partial class adminUI : Window
    {
        public class ClientItem//objet client pour  cmbCodeClient
        {
            public Client Client { get; set; }
            public string FullName { get; set; }
            public bool bloque { get; set; }
        }
        public class TransactionInfo // objet transaction pour liste transaction
        {
            public string Column1 { get; set; }
            public string Column2 { get; set; }
            public string Column3 { get; set; }
            public decimal? Column4 { get; set; }
            public DateTime? Column5 { get; set; }
            public string Column6 { get; set; } 
        }
        PP1Entities PP1Entities = new PP1Entities(); // entite PP1
        Admin administrateur;//pour sauver admin
        bool ouvert = false; // verifie ouverture de fenetre
        ClientItem selectedClientAjout;//pour sauver client dans tab ajout compte
        TypesCompte selectedClientTypeComptes;//pour sauver type compte tab aout
        public adminUI(Admin admin)
        {
            InitializeComponent();
            try
            {
                administrateur = admin;//recupere l'administrateur connecte
                ouvert = true;// fenetre ouverte
                LabelAdmin.Content = "Bienvenue " + administrateur.Prenom + " " + administrateur.Nom;

                ///////////// tab ajouter compte  /////////////
                var clientAccounts = PP1Entities.Clients.ToList();//recupere les clients
                var typesComptes = PP1Entities.TypesComptes.ToList();//recupere les type de comptes

                List<ClientItem> clientObjects = clientAccounts.Select(client => new ClientItem //creer object pout comboxclient
                {
                    Client = client,//objet client
                    FullName = $"{client.Nom} {client.Prenom}",//string memberpath
                    bloque = (bool)client.Blocke//pour tab bloque/debloque
                }).ToList();

                cmbCodeClient.ItemsSource = clientObjects;//creer data source
                cmbCodeClient.DisplayMemberPath = "FullName";//affiche memberpath
                cmbTypeCompte.ItemsSource = typesComptes;//creer data source
                cmbTypeCompte.DisplayMemberPath = "TypeCompte";//affiche memberpath

                ///////////// tab liste transactions  ////////////

                var transactionList = PP1Entities.Transactions
             .Select(transaction => new TransactionInfo //selectionne toutes les transactions et creer objet pour data context
         {
                 Column1 = transaction.TypeTransaction,
                 Column2 = transaction.De,
                 Column3 = transaction.À,
                 Column4 = transaction.Montant,
                 Column5 = transaction.TransactionDate,
                 Column6 = PP1Entities.Comptes
                     .Where(compte => compte.CodeClient == transaction.CodeClient)
                     .Select(compte => string.Concat(compte.Client.Prenom, " ", compte.Client.Nom))
                     .FirstOrDefault()
             }).ToList();
                listeTransactions.DataContext = transactionList;//etablie data context
                                                                ///////////// tab bloque/debloque  ////////////
                cmbCompteBlocke.ItemsSource = clientObjects;
                cmbCompteBlocke.DisplayMemberPath = "FullName";
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
         
        }

        ////////////////////////////////////////////////////  TAB Ajouter Client ////////////////////////////////////////////////////
        private void txtCodeClient_TextChanged(object sender, TextChangedEventArgs e)//verification code compte
        {
            try
            {
                var inputCode = 0;//Pour sauver code client
                int.TryParse(txtCodeClient.Text, out inputCode);//converstion string => int
                var existingCode = PP1Entities.Clients.Where(client => client.CodeClient == inputCode).FirstOrDefault();//Cherche pour code compte existant
                if (existingCode != null)//si code client existe
                {
                    lblCodeClient.Content = "code compte existe deja";//affiche message erreur
                    btnAjoutClient.IsEnabled = false; //desactive bouton
                }
                else//si code client existe pas
                {
                    lblCodeClient.Content = "ex:001";
                    btnAjoutClient.IsEnabled = true;//active bouton
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void txtEmail_TextChanged(object sender, TextChangedEventArgs e)//verification email
        {
            try
            {
                var existingEmail = PP1Entities.Clients.Where(client => client.email == txtEmail.Text).FirstOrDefault();//Cherche pour email existant
                if (existingEmail != null)//si email existe
                {
                    lblEmail.Content = "email existe deja";//affiche message erreur
                    btnAjoutClient.IsEnabled = false;//desactive bouton
                }
                else//si email existe pas
                {
                    lblEmail.Content = "ex@example.com";
                    btnAjoutClient.IsEnabled = true;//active bouton
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnAjoutClient_Click_1(object sender, RoutedEventArgs e)
        {
            switch (checkInput())//logique execution apres verification
            {
                case "check"://verification reussite
                    try
                    {
                        int cdClient = 0;//pour sauver input code client
                        int nipClient = 0;//pour sauver input nip client
                        int.TryParse(txtCodeClient.Text, out cdClient);//covertie code client
                        int.TryParse(txtNIP.Text, out nipClient);//convertie nip client
                        var newClient = new Client// creer nouvel objet client
                        {
                            CodeClient = cdClient,
                            Nom = txtNom.Text,
                            Prenom = txtPrenom.Text,
                            telephone = txtTelephone.Text,
                            email = txtEmail.Text,
                            NIP = nipClient,
                            NbrCompte = 1,
                            Blocke = chkBlocked.IsChecked
                        };
                        string formattedCode = $"chcq{cdClient:D3}";//format code client
                        var newClientCompte = new Compte//creer compte checque du client
                        {
                            CodeCompte = formattedCode,
                            CodeClient = cdClient,
                            TypeCompte = "checque",
                            Solde = 0
                        };
                        PP1Entities.Clients.Add(newClient);//ajoute nouveau client
                        PP1Entities.Comptes.Add(newClientCompte);//ajoute compte checque client
                        MessageBox.Show($"Nouveau client {newClient.Nom} {newClient.Prenom} a ete cree.");//affiche message erreur
                        PP1Entities.SaveChanges();//applique changement
                                                  //refresh UI elements in adminUI
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);

                    }

                    break;
                case "inputVide"://au moins 1 input vide
                    MessageBox.Show("veuillez remplir toute les informations");//affiche message erreur
                    break;
                case "email.X"://email invalide
                    MessageBox.Show("veuillez entrer un email valide");//affiche message erreur
                    break;
                case "nip.X"://nip invalide
                    MessageBox.Show("veuillez entrer un NIP valide");//affiche message erreur
                    break;
                case "phone.X"://telephone invalide
                    MessageBox.Show("veuillez entrer un # de telephone valide");//affiche message erreur
                    break;
            }
        }
        private string checkInput()//verification des input
        {
            if (string.IsNullOrEmpty(txtCodeClient.Text) ||//si un des input est vide
                string.IsNullOrEmpty(txtNom.Text) ||
                string.IsNullOrEmpty(txtPrenom.Text) ||
                string.IsNullOrEmpty(txtTelephone.Text) ||
                string.IsNullOrEmpty(txtEmail.Text) ||
                string.IsNullOrEmpty(txtNIP.Text))
            {
                return "inputVide"; //verification echouee
            }
            bool checkEmail = ValidateEmailFormat(txtEmail.Text);//verifie format email
            bool checkPhone = ValidatePhoneNumberFormat(txtTelephone.Text);//verifie format telephone
            bool checkNIP = ValidateNIPFormat(txtNIP.Text);//verifie format nip
            if (checkEmail == false)//si format non respecte
            {
                return "email.X";//verification echouee
            }
            
            if (checkPhone == false)//si format non respecte
            {
                return "phone.X";//verification echoue
            }
            
            if(checkNIP == false)//si format non respect    
            {
                return "nip.X";//verification echouee
            }

            return "check"; //verification reussite
        }
        private bool ValidateEmailFormat(string email)//verification format email
        {
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";//"ex@example.com"
            return Regex.IsMatch(email, pattern);
        }
        private bool ValidatePhoneNumberFormat(string phoneNumber)
        {
            string pattern = @"^\d{3}-\d{3}-\d{4}$";
            return Regex.IsMatch(phoneNumber, pattern);//"111-111-1111"
        }
        private bool ValidateNIPFormat(string nip)
        {
            string pattern = @"\d{4}$";//1111
            return Regex.IsMatch(nip, pattern);
        }

        ////////////////////////////////////////////////////  TAB Ajouter Compte ////////////////////////////////////////////////////
        private void cmbCodeClient_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedClientAjout = cmbCodeClient.SelectedItem as ClientItem;//recupere l'objet selectionne
        }

        private void cmbTypeCompte_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedClientTypeComptes = (TypesCompte)cmbTypeCompte.SelectedItem;//recupere le type de compte
        }

        private void btnAjoutCompte_Click(object sender, RoutedEventArgs e)
        {try
            {
                var compteExiste = PP1Entities.Comptes.Where(compte => compte.CodeClient == selectedClientAjout.Client.CodeClient
                                               && compte.TypeCompte == selectedClientTypeComptes.TypeCompte).FirstOrDefault();//cherche pour compte existant
                if (compteExiste != null)//si compte existe
                {
                    MessageBox.Show($"Le client possede deja un compte {selectedClientTypeComptes.TypeCompte}");//affiche message
                }
                else//compte n'existe pas
                {
                    string formattedCode = $"{selectedClientTypeComptes.Code}{selectedClientAjout.Client.CodeClient:D3}";//format code client
                    var newClientCompte = new Compte//creer compte checque du client
                    {
                        CodeCompte = formattedCode,
                        CodeClient = selectedClientAjout.Client.CodeClient,
                        TypeCompte = selectedClientTypeComptes.TypeCompte,
                        Solde = 0
                    };
                    PP1Entities.Comptes.Add(newClientCompte);//ajoute le nouveau compte
                    PP1Entities.Clients.Where(client => client.CodeClient == selectedClientAjout.Client.CodeClient)
                        .FirstOrDefault().NbrCompte += 1;//augmente le nombre de compte du client 
                    PP1Entities.SaveChanges();//sauve les changement
                    MessageBox.Show($"Un compte {selectedClientTypeComptes.TypeCompte} a ete ajoute a {selectedClientAjout.Client.Prenom} {selectedClientAjout.Client.Nom}");//message confirmation
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
        ////////////////////////////////////////////////////  TAB bloquer/debloque ////////////////////////////////////////////////////
        ///
        private void cmbCompteBlocke_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           var currentSelection = cmbCompteBlocke.SelectedItem as ClientItem;//sauve le client selectione
            if(currentSelection.Client.Blocke == true)//si client selectionne est bloque
            {
                checkBoxCompteBlocke.IsChecked = true;//coche la case
            }
            else//sinon 
            {
                checkBoxCompteBlocke.IsChecked = false;//decoche la case
            }
        }

        private void btnBloqueDebloque_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var currentSelection = cmbCompteBlocke.SelectedItem as ClientItem;//sauve le client selectione
                if (currentSelection.Client.Blocke == true)//si client selectionne est bloque
                {
                    MessageBoxResult result = MessageBox.Show("Voulez-vous bloquer le client " + currentSelection.Client.Prenom + " " + currentSelection.Client.Nom + "?", "Confirmation", MessageBoxButton.YesNo);

                    if (result == MessageBoxResult.Yes)
                    {
                        PP1Entities.Clients.Where(client => client.CodeClient == currentSelection.Client.CodeClient)
                       .FirstOrDefault().Blocke = false;//debloquer le client
                    }
                    else
                    {
                        //rien faire
                    }

                }
                else//sinon 
                {
                    MessageBoxResult result = MessageBox.Show($"Voulez-vous debloquer le client {currentSelection.Client.Prenom} {currentSelection.Client.Nom}?", "Confirmation", MessageBoxButton.YesNo);

                    if (result == MessageBoxResult.Yes)
                    {
                        PP1Entities.Clients.Where(client => client.CodeClient == currentSelection.Client.CodeClient)
                        .FirstOrDefault().Blocke = true;//bloquer le client
                    }
                    else
                    {
                        //rien faire
                    }
                }
                PP1Entities.SaveChanges();//sauve les changement
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void checkBoxCompteBlocke_Checked(object sender, RoutedEventArgs e)
        {

        }
        ////////////// Ajouter argent guichet /////////////////////////
        private void btnSoldeGuichet_Click(object sender, RoutedEventArgs e)//pour ouvrir fenetre ajout dans le guichet
        {
            ajoutGuichetUI ajoutGuichetUI = new ajoutGuichetUI();
            ajoutGuichetUI.ShowDialog();
        }
        ////////////// Quitter apllication /////////////////////////
        private void btnQuitter_Click(object sender, RoutedEventArgs e)//pour quitter l'application
        {
                MessageBoxResult result = MessageBox.Show("Voulez-vous quitter l'application ?", "Confirmation", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    Application.Current.Shutdown(); //ferme l'application
                }
        }

        ////////////// Payer interet epargne /////////////////////////
        private void btnInteretEpargne_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var compteEpargeList = PP1Entities.Comptes.Where(compte => compte.TypeCompte == "epargne").ToList();//recupere tout les comptes epargne
                foreach (Compte compte in compteEpargeList)//pour chaque compte epargne
                {
                    compte.Solde += compte.Solde / 100;//augment solde de 1%
                }
                MessageBox.Show("1% d'interet a ete paye a tous les comptes epargne");//message confirmation
                PP1Entities.SaveChanges();//sauve les changemets
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        ////////////// Charger interet credit /////////////////////////
        ///
        private void btnMargeCredit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var compteCreditList = PP1Entities.Comptes.Where(compte => compte.TypeCompte == "credit").ToList();//recupere tout les comptes credit
                foreach (Compte compte in compteCreditList)//pour chaque compte credit
                {
                    compte.Solde += 5 * (compte.Solde / 100);//charge 5%
                }
                MessageBox.Show("5% d'interet ont ete charge a tous les comptes credit");//message confirmation
                PP1Entities.SaveChanges();//sauve les changemets
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        ////////////// Prelevement hypothecaire /////////////////////////
        ///

        private void btnHypotheque_Click(object sender, RoutedEventArgs e)
        {
            prelevementHyptcUI prelevementHyptcUI = new prelevementHyptcUI();
            prelevementHyptcUI.ShowDialog();
        }
    }
}
