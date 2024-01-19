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
    /// Interaction logic for paiementUI.xaml
    /// </summary>
    public partial class paiementUI : Window
    {
        PP1Entities PP1Entities = new PP1Entities(); // entite PP1
        bool ouvert = false; // verifie ouverture de fenetre
        Client Client;//sauve le client 
        const string typeTransaction = "paiement";// variable pour type de transaction
        private Window1 parentWindow;//sauve la fenetre mere
        int montant;//pour sauver le montant entre
        Compte compteChequeClient;//pour sauver le compte checque du client
        const double fraisPaiement = 1.25;//fraie de paiement
        public paiementUI(Client client,Window1 window)
        {
            InitializeComponent();
            try
            {
                Client = client;//sauve le client pour les fenetres transactions
                parentWindow = window; //sauve la fenetre mere
                ouvert = true; //fenetre ouverte
                btnPaiement.IsEnabled = false;//desactive le bouton
                compteChequeClient = PP1Entities.Comptes.Where(compte => compte.CodeClient == client.CodeClient
                                             && compte.TypeCompte == "checque").FirstOrDefault();//recupere le compte checque
                soldeCompte.Content = string.Format("{0:C2}", compteChequeClient.Solde);//affiche le solde du compte checque
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void txtBoxEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            var montantValid = int.TryParse(txtBoxMontantTransfert.Text, out montant);// verifie si montant est un chiffre
            if (montantValid)//si le montant est une valeur numerique
            {
                if (montant < 0)//si montant negatif
                {
                    messageErreur.Content = "svp entrer une valeur numerique positive";
                    btnPaiement.IsEnabled = false;//desactive le bouton
                }
                else//si montant positif
                {
                    btnPaiement.IsEnabled = true;//active le bouton
                    messageErreur.Content = String.Empty;//retire le message
                }
            }
            else//sinon
            {
                messageErreur.Content = "svp entrer des valeur numerique";
                btnPaiement.IsEnabled = false;//desactive le bouton
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("Frais de 1.25$ sera applique pour le paiement.Voulez-vous poursuivre?", "Confirmation", MessageBoxButton.YesNo);//affiche message
                if (result == MessageBoxResult.Yes)//si reponse positive
                {
                    compteChequeClient.Solde -= montant;//retire le montant pour paiement
                    compteChequeClient.Solde -= (int)fraisPaiement;//cast int car double ne peut etre operande
                    var newTransaction = new Transaction
                    {
                        CodeClient = Client.CodeClient,
                        TypeTransaction = typeTransaction,
                        De = compteChequeClient.CodeCompte,
                        À = null,// null car paiment
                        TransactionDate = DateTime.Now,
                        Montant = montant
                    };
                    PP1Entities.Transactions.Add(newTransaction);//sauve la transaction
                    PP1Entities.SaveChanges();//sauve les changements
                    System.Windows.MessageBox.Show($"Montant de {string.Format("{0:C2}", montant)} a ete paye a partir du compte {compteChequeClient.CodeCompte}");//message de confirmation
                    parentWindow.RefreshUIElements();
                    this.Close();//ferme la fenetre
                    ouvert = false;//fenetre fermee
                }
                else
                {
                    //rien faire
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }
    }
}
