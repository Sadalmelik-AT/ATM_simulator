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
    /// Interaction logic for transfertUI.xaml
    /// </summary>
    public partial class transfertUI : Window
    {
        PP1Entities PP1Entities = new PP1Entities(); // entite PP1
        bool ouvert = false; // verifie ouverture de fenetre
        Compte selectedCompte;//sauve le compte choisi dans comboBox
        Client Client;//sauve le client 
        Guichet Guichet;//sauve le guichet
        const string typeTransaction = "transfert";// variable pour type de transaction
        private Window1 parentWindow;//sauve la fenetre mere
        int montant;//pour sauver le montant entre
        Compte compteChequeClient;//pour sauver le compte checque du client
        public transfertUI(Client client, Guichet guichet, Window1 window)
        {
            InitializeComponent();
            try
            {
                var clientAccounts = PP1Entities.Comptes
                    .Where(compte => compte.CodeClient == client.CodeClient && compte.TypeCompte != "checque")
                        .ToList();//recupere les comptes du clients excepte checque
                ComboBoxTypeCompte.DataContext = clientAccounts;// etablie le data context
                Client = client;//sauve le client pour les fenetres transactions
                Guichet = guichet;//sauve le guichet pour les fenetres tranactions
                parentWindow = window; //sauve la fenetre mere
                ouvert = true; //fenetre ouverte
                btnRetrait.IsEnabled = false;//desactive le bouton
                compteChequeClient = PP1Entities.Comptes.Where(compte => compte.CodeClient == client.CodeClient
                                             && compte.TypeCompte == "checque").FirstOrDefault();//recupere le compte checque
                soldeCompte.Content = string.Format("{0:C2}", compteChequeClient.Solde);//afficje le solde du compte checque
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ComboBoxTypeCompte_SelectionChanged(object sender, SelectionChangedEventArgs e)//selection de compte dans combo box
        {
            selectedCompte = ComboBoxTypeCompte.SelectedItem as Compte;//recupere le compte selectionne
            soldeCompte.Content = selectedCompte.Solde.ToString();//affiche le solde du compte selectionne
        }

        private void txtBoxEmail_TextChanged(object sender, TextChangedEventArgs e)//changement de text dand zone de saisie
        {
            var montantValid = int.TryParse(txtBoxMontantTransfert.Text, out montant);// verifie si montant est un chiffre
            if (montantValid)//si le montant est une valeur numerique
            {
               if(montant < 0)//si montant negatif
                {
                    messageErreur.Content = "svp entrer une valeur numerique positive";
                    btnRetrait.IsEnabled = false;//desactive le bouton
                }else//si montant positif
                {
                    btnRetrait.IsEnabled = true;//active le bouton
                    messageErreur.Content = String.Empty;//retire le message
                }
            }else//sinon
            {
                messageErreur.Content = "svp entrer des valeur numerique";
                btnRetrait.IsEnabled = false;//desactive le bouton
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (selectedCompte.TypeCompte)
                {
                    case "epargne"://transfert vers compte epargne
                        compteChequeClient.Solde -= montant;//retire montant du compte checque
                        selectedCompte.Solde += montant;//ajoute montant au compte epargne
                                                        //objet transaction 
                        var newTransaction = new Transaction
                        {
                            CodeClient = Client.CodeClient,
                            TypeTransaction = typeTransaction,
                            De = compteChequeClient.CodeCompte,
                            À = selectedCompte.CodeCompte,
                            TransactionDate = DateTime.Now,
                            Montant = montant//fond suffisant pris dans compte selectionne
                        };
                        PP1Entities.Transactions.Add(newTransaction);//sauve la transaction
                        PP1Entities.SaveChanges();
                        System.Windows.MessageBox.Show($"Montant de {string.Format("{0:C2}", montant)} a ete transfere du compte {compteChequeClient.CodeCompte} au compte {selectedCompte.CodeCompte}");//message de confirmation
                        parentWindow.RefreshUIElements();
                        this.Close();//ferme la fenetre
                        ouvert = false;//fenetre ferme
                        break;
                    case "credit":
                        compteChequeClient.Solde -= montant;//retire montant du compte checque
                        selectedCompte.Solde -= montant;//ajoute montant au compte credit (diminue la marge)
                                                        //objet transaction 
                        var newTransactionCredit = new Transaction
                        {
                            CodeClient = Client.CodeClient,
                            TypeTransaction = typeTransaction,
                            De = compteChequeClient.CodeCompte,
                            À = selectedCompte.CodeCompte,// null car retrait
                            TransactionDate = DateTime.Now,
                            Montant = montant//fond suffisant pris dans compte selectionne
                        };
                        PP1Entities.Transactions.Add(newTransactionCredit);//sauve la transaction
                        PP1Entities.SaveChanges();
                        System.Windows.MessageBox.Show($"Montant de {string.Format("{0:C2}", montant)} a ete transfere du compte {compteChequeClient.CodeCompte} au compte { selectedCompte.CodeCompte}");//message de confirmation
                        parentWindow.RefreshUIElements();
                        this.Close();//ferme la fenetre
                        ouvert = false;//fenetre ferme
                        break;
                    case "hypotheque":
                        compteChequeClient.Solde -= montant;//retire montant du compte checque
                        selectedCompte.Solde -= montant;//ajoute montant au compte credit (diminue la marge)
                                                        //objet transaction 
                        var newTransactionHypo = new Transaction
                        {
                            CodeClient = Client.CodeClient,
                            TypeTransaction = typeTransaction,
                            De = compteChequeClient.CodeCompte,
                            À = selectedCompte.CodeCompte,// null car retrait
                            TransactionDate = DateTime.Now,
                            Montant = montant//fond suffisant pris dans compte selectionne
                        };
                        PP1Entities.Transactions.Add(newTransactionHypo);//sauve la transaction
                        PP1Entities.SaveChanges();
                        System.Windows.MessageBox.Show($"Montant de {string.Format("{0:C2}", montant)} a ete transfere du compte {compteChequeClient.CodeCompte} au compte " +
                            $"{selectedCompte.CodeCompte}");//message de confirmation
                        parentWindow.RefreshUIElements();
                        this.Close();//ferme la fenetre
                        ouvert = false;//fenetre ferme
                        break;
                }
            }
            catch(Exception ex){
                MessageBox.Show(ex.Message);

            }
        }
    }
}
