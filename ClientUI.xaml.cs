using System;
using System.Collections.Generic;
using System.Data.Entity;
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
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        PP1Entities PP1Entities = new PP1Entities(); // entite PP1
        bool ouvert = false; // verifie ouverture de fenetre
        int soldeGuichet;//pour sauver le solde totalt en argent papier
        Compte selectedClientCompte;//pour sauver le compte du client selectionne
        Client Client;//sauve le client 
        Guichet Guichet;//sauve le guichet
        public Window1(Client client, Guichet guichet)
        {
            InitializeComponent();
            try
            {
                PP1Entities.TypesComptes.Load(); //Charge les donnee
                var clientAccounts = PP1Entities.Comptes.Where(compte => compte.CodeClient == client.CodeClient).ToList();//recupere les comptes du clients
                ComboBoxTypeCompte.DataContext = clientAccounts;// etablie le data context
                ouvert = true; // fenetre ouverte
                LabelGuichet.Content = guichet.CodeGuichet.ToString();//affiche le guichet choise
                soldeGuichet = (int)guichet.Solde;//recupere le solde du guichet
                LabelClient.Content = $"Bienvenue {client.Prenom} {client.Nom}";//affiche nom et prenom du client
                Client = client;//sauve le client pour les fenetres transactions
                Guichet = guichet;//sauve le guicher pour les fenetres tranactions
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }


        }
     
        private void ComboBoxTypeCompte_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                selectedClientCompte = ComboBoxTypeCompte.SelectedItem as Compte;//sauve le compte selectionne dans comboBox
                var selectedClientSolde = PP1Entities.Comptes.Where(compte => compte.TypeCompte == selectedClientCompte.TypeCompte
                && compte.CodeClient == selectedClientCompte.CodeClient).Select(compte => compte.Solde).FirstOrDefault();
                soldeCompte.Content = string.Format("{0:C2}", selectedClientSolde);//affiche le solde avec 2 decimales
                try
                {
                    //recupere toutes transactions associe au compte selectionne
                    var transactionList = PP1Entities.Transactions
                            .Where(transaction => transaction.De == selectedClientCompte.CodeCompte || transaction.À == selectedClientCompte.CodeCompte)
                            .Select(transaction => new {//selectionne les donnees suivantes
                            Column1 = transaction.TypeTransaction,//type transaction
                            Column2 = transaction.De,//compte depart
                            Column3 = transaction.À,//compte final
                            Column4 = transaction.Montant,//Montant
                            Column5 = transaction.TransactionDate//Date
                        }).ToList();

                    listeTransactions.DataContext = transactionList;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void btnDepot_Click(object sender, RoutedEventArgs e)//ouverture fenetre depot
        {
            Window2 depotUI = new Window2(Client, Guichet, this);//creer fenetre pour depot de fond
            depotUI.ShowDialog();//ouvre fenetre depot en parallele
        }

        private void btnRetrait_Click(object sender, RoutedEventArgs e)//ouverture fenetre retrait
        {
            retraitUI retraitUI = new retraitUI(Client, Guichet, this);//creer fenetre pour depot de fond
            retraitUI.ShowDialog();//ouvre fenetre depot en parallele
        }
        private void btnTransfert_Click(object sender, RoutedEventArgs e)
        {
            transfertUI transfertUI = new transfertUI(Client, Guichet, this);//creer la fenetre pour transfert de fond
            transfertUI.ShowDialog();//ouvre fenetre pour transfert de fond
        }
        private void btnPaiement_Click(object sender, RoutedEventArgs e)
        {
            paiementUI paiementUI = new paiementUI(Client, this);//creer fenetre pour paiement
            paiementUI.ShowDialog();//ouvre fenetre pour paiement
        }
        public void RefreshUIElements()//pour ragraichir UI avec nouvelle donnee
        {
            try
            {

                var clientAccounts = PP1Entities.Comptes.Where(compte => compte.CodeClient == Client.CodeClient).ToList();
                ComboBoxTypeCompte.DataContext = clientAccounts;
                var selectedClientSolde = PP1Entities.Comptes.Where(compte => compte.TypeCompte == selectedClientCompte.TypeCompte
               && compte.CodeClient == selectedClientCompte.CodeClient).Select(compte => compte.Solde).FirstOrDefault();
                soldeCompte.Content = string.Format("{0:C2}", selectedClientSolde);//affiche le solde avec 2 decimales

                if (ComboBoxTypeCompte.SelectedItem != null)
                {
                    selectedClientCompte = ComboBoxTypeCompte.SelectedItem as Compte;
                    try
                    {
                        var transactionList = PP1Entities.Transactions
                            .Where(transaction => transaction.De == selectedClientCompte.CodeCompte || transaction.À == selectedClientCompte.CodeCompte)
                            .Select(transaction => new
                            {
                                Column1 = transaction.TypeTransaction,
                                Column2 = transaction.De,
                                Column3 = transaction.À,
                                Column4 = transaction.Montant,
                                Column5 = transaction.TransactionDate
                            }).ToList();

                        listeTransactions.DataContext = transactionList;
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

        private void BtnDeconnection_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = System.Windows.MessageBox.Show("Voulez-vous vous deconnecter?", "Confirmation", MessageBoxButton.YesNo);//affiche message deconnection
            if(result == MessageBoxResult.Yes)//si oui
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();//ferme clien UI
                //reouvre fenetre principale
                
            }
            else
            {
                //rien faire
            }
        }
    }
}
